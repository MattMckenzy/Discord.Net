using System;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    public partial class DiscordShardedClient
    {
        #region General

        /// <summary> Fired when a shard is connected to the Discord gateway. </summary>
        public event EventHandler<ShardConnectedArguments> ShardConnected;
        public class ShardConnectedArguments
        {
            public DiscordSocketClient DiscordSocketClient { get; set; }
        }

        /// <summary> Fired when a shard is disconnected from the Discord gateway. </summary>
        public event EventHandler<ShardDisconnectedArguments> ShardDisconnected;
        public class ShardDisconnectedArguments
        {
            public Exception Exception { get; set; }
            public DiscordSocketClient Client { get; set; }
        }

        /// <summary> Fired when a guild data for a shard has finished downloading. </summary>
        public event EventHandler<ShardReadyArguments> ShardReady;
        public class ShardReadyArguments
        {
            public DiscordSocketClient DiscordSocketClient { get; set; }
        }

        /// <summary> Fired when a shard receives a heartbeat from the Discord gateway. </summary>
        public event EventHandler<ShardLatencyUpdatedArguments> ShardLatencyUpdated;
        public class ShardLatencyUpdatedArguments
        {
            public int OriginalLatency { get; set; }
            public int UpdatedLatency { get; set; }
            public DiscordSocketClient Client { get; set; }
        }

        #endregion
    }
}
