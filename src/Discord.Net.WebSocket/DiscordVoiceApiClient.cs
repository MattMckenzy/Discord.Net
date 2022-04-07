using Discord.API;
using Discord.API.Voice;
using Discord.Net.Converters;
using Discord.Net.Udp;
using Discord.Net.WebSockets;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Audio
{
    internal class DiscordVoiceAPIClient : IDisposable
    {
        #region DiscordVoiceAPIClient
        public const int MaxBitrate = 128 * 1024;
        public const string Mode = "xsalsa20_poly1305";

        public event EventHandler<SentRequestArguments> SentRequest;
        public class SentRequestArguments
        {
            public string Method { get; set; }
            public string Endpoint { get; set; }
            public double MillisecondsTaken { get; set; }
        }
        public event EventHandler<VoiceOpCode> SentGatewayMessage;
        public event EventHandler SentDiscovery;
        public event EventHandler<int> SentData;
        public event EventHandler<ReceivedEventArguments> ReceivedEvent;
        public class ReceivedEventArguments
        {
            public VoiceOpCode Operation { get; set; }
            public object Payload { get; set; }
        }
        public event EventHandler<byte[]> ReceivedPacket;
        public event EventHandler<Exception> Disconnected;

        private readonly JsonSerializer _serializer;
        private readonly SemaphoreSlim _connectionLock;
        private readonly IUdpSocket _udp;
        private CancellationTokenSource _connectCancelToken;
        private bool _isDisposed;
        private ulong _nextKeepalive;

        public ulong GuildId { get; }
        internal IWebSocketClient WebSocketClient { get; }
        public ConnectionState ConnectionState { get; private set; }

        public ushort UdpPort => _udp.Port;

        internal DiscordVoiceAPIClient(ulong guildId, WebSocketProvider webSocketProvider, UdpSocketProvider udpSocketProvider, JsonSerializer serializer = null)
        {
            GuildId = guildId;
            _connectionLock = new SemaphoreSlim(1, 1);
            _udp = udpSocketProvider();
            _udp.ReceivedDatagram += async (data, index, count) =>
            {
                if (index != 0 || count != data.Length)
                {
                    var newData = new byte[count];
                    Buffer.BlockCopy(data, index, newData, 0, count);
                    data = newData;
                }
                ReceivedPacket.Invoke(this, data);
            };

            WebSocketClient = webSocketProvider();
            //_gatewayClient.SetHeader("user-agent", DiscordConfig.UserAgent); //(Causes issues in .Net 4.6+)
            WebSocketClient.BinaryMessage += async (data, index, count) =>
            {
                using (var compressed = new MemoryStream(data, index + 2, count - 2))
                using (var decompressed = new MemoryStream())
                {
                    using (var zlib = new DeflateStream(compressed, CompressionMode.Decompress))
                        zlib.CopyTo(decompressed);
                    decompressed.Position = 0;
                    using (var reader = new StreamReader(decompressed))
                    {
                        var msg = JsonConvert.DeserializeObject<SocketFrame>(reader.ReadToEnd());
                        ReceivedEvent.Invoke(this, new ReceivedEventArguments { Operation = (VoiceOpCode)msg.Operation, Payload = msg.Payload });
                    }
                }
            };
            WebSocketClient.TextMessage += async text =>
            {
                var msg = JsonConvert.DeserializeObject<SocketFrame>(text);

                ReceivedEvent.Invoke(this, new ReceivedEventArguments { Operation = (VoiceOpCode)msg.Operation, Payload = msg.Payload });
            };
            WebSocketClient.Closed += async ex =>
            {
                await DisconnectAsync().ConfigureAwait(false);
                Disconnected.Invoke(this, ex);
            };

            _serializer = serializer ?? new JsonSerializer { ContractResolver = new DiscordContractResolver() };
        }
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _connectCancelToken?.Dispose();
                    _udp?.Dispose();
                    WebSocketClient?.Dispose();
                    _connectionLock?.Dispose();
                }
                _isDisposed = true;
            }
        }
        public void Dispose() => Dispose(true);

        public async Task SendAsync(VoiceOpCode opCode, object payload, RequestOptions options = null)
        {
            byte[] bytes = null;
            payload = new SocketFrame { Operation = (int)opCode, Payload = payload };
            if (payload != null)
                bytes = Encoding.UTF8.GetBytes(SerializeJson(payload));
            await WebSocketClient.SendAsync(bytes, 0, bytes.Length, true).ConfigureAwait(false);
            SentGatewayMessage.Invoke(this, opCode);
        }
        public async Task SendAsync(byte[] data, int offset, int bytes)
        {
            await _udp.SendAsync(data, offset, bytes).ConfigureAwait(false);
            SentData.Invoke(this, bytes);
        }
        #endregion

        #region WebSocket
        public async Task SendHeartbeatAsync(RequestOptions options = null)
        {
            await SendAsync(VoiceOpCode.Heartbeat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), options: options).ConfigureAwait(false);
        }
        public async Task SendIdentityAsync(ulong userId, string sessionId, string token)
        {
            await SendAsync(VoiceOpCode.Identify, new IdentifyParams
            {
                GuildId = GuildId,
                UserId = userId,
                SessionId = sessionId,
                Token = token
            }).ConfigureAwait(false);
        }
        public async Task SendSelectProtocol(string externalIp, int externalPort)
        {
            await SendAsync(VoiceOpCode.SelectProtocol, new SelectProtocolParams
            {
                Protocol = "udp",
                Data = new UdpProtocolInfo
                {
                    Address = externalIp,
                    Port = externalPort,
                    Mode = Mode
                }
            }).ConfigureAwait(false);
        }
        public async Task SendSetSpeaking(bool value)
        {
            await SendAsync(VoiceOpCode.Speaking, new SpeakingParams
            {
                IsSpeaking = value,
                Delay = 0
            }).ConfigureAwait(false);
        }

        public async Task ConnectAsync(string url)
        {
            await _connectionLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectInternalAsync(url).ConfigureAwait(false);
            }
            finally { _connectionLock.Release(); }
        }
        private async Task ConnectInternalAsync(string url)
        {
            ConnectionState = ConnectionState.Connecting;
            try
            {
                _connectCancelToken?.Dispose();
                _connectCancelToken = new CancellationTokenSource();
                var cancelToken = _connectCancelToken.Token;

                WebSocketClient.SetCancelToken(cancelToken);
                await WebSocketClient.ConnectAsync(url).ConfigureAwait(false);

                _udp.SetCancelToken(cancelToken);
                await _udp.StartAsync().ConfigureAwait(false);

                ConnectionState = ConnectionState.Connected;
            }
            catch
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
            }
            finally { _connectionLock.Release(); }
        }
        private async Task DisconnectInternalAsync()
        {
            if (ConnectionState == ConnectionState.Disconnected)
                return;
            ConnectionState = ConnectionState.Disconnecting;

            try
            { _connectCancelToken?.Cancel(false); }
            catch { }

            //Wait for tasks to complete
            await _udp.StopAsync().ConfigureAwait(false);
            await WebSocketClient.DisconnectAsync().ConfigureAwait(false);

            ConnectionState = ConnectionState.Disconnected;
        }
        #endregion

        #region Udp
        public async Task SendDiscoveryAsync(uint ssrc)
        {
            var packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);
            await SendAsync(packet, 0, 70).ConfigureAwait(false);
            SentDiscovery.Invoke(this, EventArgs.Empty);
        }
        public async Task<ulong> SendKeepaliveAsync()
        {
            var value = _nextKeepalive++;
            var packet = new byte[8];
            packet[0] = (byte)(value >> 0);
            packet[1] = (byte)(value >> 8);
            packet[2] = (byte)(value >> 16);
            packet[3] = (byte)(value >> 24);
            packet[4] = (byte)(value >> 32);
            packet[5] = (byte)(value >> 40);
            packet[6] = (byte)(value >> 48);
            packet[7] = (byte)(value >> 56);
            await SendAsync(packet, 0, 8).ConfigureAwait(false);
            return value;
        }

        public void SetUdpEndpoint(string ip, int port)
        {
            _udp.SetDestination(ip, port);
        }
        #endregion

        #region Helpers
        private static double ToMilliseconds(Stopwatch stopwatch) => Math.Round((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0, 2);
        private string SerializeJson(object value)
        {
            var sb = new StringBuilder(256);
            using (TextWriter text = new StringWriter(sb, CultureInfo.InvariantCulture))
            using (JsonWriter writer = new JsonTextWriter(text))
                _serializer.Serialize(writer, value);
            return sb.ToString();
        }
        private T DeserializeJson<T>(Stream jsonStream)
        {
            using (TextReader text = new StreamReader(jsonStream))
            using (JsonReader reader = new JsonTextReader(text))
                return _serializer.Deserialize<T>(reader);
        }
        #endregion
    }
}
