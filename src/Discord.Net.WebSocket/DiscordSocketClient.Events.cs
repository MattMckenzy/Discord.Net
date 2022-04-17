using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.WebSocket
{
    public partial class DiscordSocketClient
    {
        #region General
        /// <summary> Fired when connected to the Discord gateway. </summary>
        public event EventHandler Connected;
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary> Fired when disconnected to the Discord gateway. </summary>
        public event EventHandler<DisconnectedArguments> Disconnected;
        public class DisconnectedArguments
        {
            public Exception Exception { get; set; }
        }
        protected virtual void OnDisconnected(DisconnectedArguments eventArgs)
        {
            Disconnected?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when guild data has finished downloading.
        /// </summary>
        /// <remarks>
        ///     It is possible that some guilds might be unsynced if <see cref="DiscordSocketConfig.MaxWaitBetweenGuildAvailablesBeforeReady" />
        ///     was not long enough to receive all GUILD_AVAILABLEs before READY.
        /// </remarks>
        public event EventHandler Ready;
        protected virtual void OnReady()
        {
            Ready?.Invoke(this, EventArgs.Empty);
        }

        /// <summary> Fired when a heartbeat is received from the Discord gateway. </summary>
        public event EventHandler<LatencyUpdatedArguments> LatencyUpdated;
        public class LatencyUpdatedArguments
        {
            public int OriginalLatency { get; set; }
            public int UpdatedLatency { get; set; }
        }
        protected virtual void OnLatencyUpdated(LatencyUpdatedArguments eventArgs)
        {
            LatencyUpdated?.Invoke(this, eventArgs);
        }

        internal DiscordSocketClient(DiscordSocketConfig config, DiscordRestApiClient client) : base(config, client)
        {
        }
        #endregion
    }
}
