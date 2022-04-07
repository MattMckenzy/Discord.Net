using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Audio
{
    public class LatencyUpdatedArguments
    {
        public int OriginalLatency { get; set; }
        public int UpdatedLatency { get; set; }
    }

    public class StreamCreatedArguments
    {
        public ulong UserId { get; set; }
        public AudioInStream Stream { get; set; }
    }

    public class SpeakingUpdatedArguments
    {
        public ulong UserId { get; set; }
        public bool IsSpeaking { get; set; }
    }

    public interface IAudioClient : IDisposable
    {
        event EventHandler Connected;
        event EventHandler<Exception> Disconnected;
        event EventHandler<LatencyUpdatedArguments> LatencyUpdated;
        event EventHandler<LatencyUpdatedArguments> UdpLatencyUpdated;
        event EventHandler<StreamCreatedArguments> StreamCreated;
        event EventHandler<ulong> StreamDestroyed;
        event EventHandler<SpeakingUpdatedArguments> SpeakingUpdated;

        /// <summary> Gets the current connection state of this client. </summary>
        ConnectionState ConnectionState { get; }
        /// <summary> Gets the estimated round-trip latency, in milliseconds, to the voice WebSocket server. </summary>
        int Latency { get; }
        /// <summary> Gets the estimated round-trip latency, in milliseconds, to the voice UDP server. </summary>
        int UdpLatency { get; }

        /// <summary>Gets the current audio streams.</summary>
        IReadOnlyDictionary<ulong, AudioInStream> GetStreams();

        Task StopAsync();
        Task SetSpeakingAsync(bool value);

        /// <summary>Creates a new outgoing stream accepting Opus-encoded data.</summary>
        AudioOutStream CreateOpusStream(int bufferMillis = 1000);
        /// <summary>Creates a new outgoing stream accepting Opus-encoded data. This is a direct stream with no internal timer.</summary>
        AudioOutStream CreateDirectOpusStream();
        /// <summary>Creates a new outgoing stream accepting PCM (raw) data.</summary>
        AudioOutStream CreatePCMStream(AudioApplication application, int? bitrate = null, int bufferMillis = 1000, int packetLoss = 30);
        /// <summary>Creates a new direct outgoing stream accepting PCM (raw) data. This is a direct stream with no internal timer.</summary>
        AudioOutStream CreateDirectPCMStream(AudioApplication application, int? bitrate = null, int packetLoss = 30);
    }
}
