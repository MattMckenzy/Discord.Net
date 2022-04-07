using Discord.API;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;

namespace Discord.WebSocket
{
    public partial class DiscordShardedClient : BaseSocketClient, IDiscordClient
    {
        #region DiscordShardedClient
        private readonly DiscordSocketConfig _baseConfig;
        private readonly Dictionary<int, int> _shardIdsToIndex;
        private readonly bool _automaticShards;
        private int[] _shardIds;
        private DiscordSocketClient[] _shards;
        private ImmutableArray<StickerPack<SocketSticker>> _defaultStickers;
        private int _totalShards;
        private SemaphoreSlim[] _identifySemaphores;
        private object _semaphoreResetLock;
        private Task _semaphoreResetTask;

        private bool _isDisposed;

        /// <inheritdoc />
        public override int Latency { get => GetLatency(); protected set { } }
        /// <inheritdoc />
        public override UserStatus Status { get => _shards[0].Status; protected set { } }
        /// <inheritdoc />
        public override IActivity Activity { get => _shards[0].Activity; protected set { } }

        internal new DiscordSocketApiClient ApiClient
        {
            get
            {
                if (base.ApiClient.CurrentUserId == null)
                    base.ApiClient.CurrentUserId = CurrentUser?.Id;

                return base.ApiClient;
            }
        }
        /// <inheritdoc/>
        public override IReadOnlyCollection<StickerPack<SocketSticker>> DefaultStickerPacks
            => _defaultStickers.ToReadOnlyCollection();

        /// <inheritdoc />
        public override IReadOnlyCollection<SocketGuild> Guilds => GetGuilds().ToReadOnlyCollection(GetGuildCount);
        /// <inheritdoc />
        public override IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels => GetPrivateChannels().ToReadOnlyCollection(GetPrivateChannelCount);
        public IReadOnlyCollection<DiscordSocketClient> Shards => _shards;

        /// <summary>
        ///     Provides access to a REST-only client with a shared state from this client.
        /// </summary>
        public override DiscordSocketRestClient Rest => _shards?[0].Rest;

        public override SocketSelfUser CurrentUser { get => _shards?.FirstOrDefault(x => x.CurrentUser != null)?.CurrentUser; protected set => throw new InvalidOperationException(); }

        /// <summary> Creates a new REST/WebSocket Discord client. </summary>
        public DiscordShardedClient() : this(null, new DiscordSocketConfig()) { }
        /// <summary> Creates a new REST/WebSocket Discord client. </summary>
#pragma warning disable IDISP004
        public DiscordShardedClient(DiscordSocketConfig config) : this(null, config, CreateApiClient(config)) { }
#pragma warning restore IDISP004
        /// <summary> Creates a new REST/WebSocket Discord client. </summary>
        public DiscordShardedClient(int[] ids) : this(ids, new DiscordSocketConfig()) { }
        /// <summary> Creates a new REST/WebSocket Discord client. </summary>
#pragma warning disable IDISP004
        public DiscordShardedClient(int[] ids, DiscordSocketConfig config) : this(ids, config, CreateApiClient(config)) { }
#pragma warning restore IDISP004
        private DiscordShardedClient(int[] ids, DiscordSocketConfig config, API.DiscordSocketApiClient client)
            : base(config, client)
        {
            if (config.ShardId != null)
                throw new ArgumentException($"{nameof(config.ShardId)} must not be set.");
            if (ids != null && config.TotalShards == null)
                throw new ArgumentException($"Custom ids are not supported when {nameof(config.TotalShards)} is not specified.");

            _semaphoreResetLock = new object();
            _shardIdsToIndex = new Dictionary<int, int>();
            config.DisplayInitialLog = false;
            _baseConfig = config;
            _defaultStickers = ImmutableArray.Create<StickerPack<SocketSticker>>();

            if (config.TotalShards == null)
                _automaticShards = true;
            else
            {
                _totalShards = config.TotalShards.Value;
                _shardIds = ids ?? Enumerable.Range(0, _totalShards).ToArray();
                _shards = new DiscordSocketClient[_shardIds.Length];
                _identifySemaphores = new SemaphoreSlim[config.IdentifyMaxConcurrency];
                for (int i = 0; i < config.IdentifyMaxConcurrency; i++)
                    _identifySemaphores[i] = new SemaphoreSlim(1, 1);
                for (int i = 0; i < _shardIds.Length; i++)
                {
                    _shardIdsToIndex.Add(_shardIds[i], i);
                    var newConfig = config.Clone();
                    newConfig.ShardId = _shardIds[i];
                    _shards[i] = new DiscordSocketClient(newConfig, this, i != 0 ? _shards[0] : null);
                    RegisterEvents(_shards[i], i == 0);
                }
            }
        }
        private static API.DiscordSocketApiClient CreateApiClient(DiscordSocketConfig config)
            => new DiscordSocketApiClient(config.RestClientProvider, config.WebSocketProvider, DiscordRestConfig.UserAgent, config.GatewayHost,
                useSystemClock: config.UseSystemClock, defaultRatelimitCallback: config.DefaultRatelimitCallback);

        internal async Task AcquireIdentifyLockAsync(int shardId, CancellationToken token)
        {
            int semaphoreIdx = shardId % _baseConfig.IdentifyMaxConcurrency;
            await _identifySemaphores[semaphoreIdx].WaitAsync(token).ConfigureAwait(false);
        }

        internal void ReleaseIdentifyLock()
        {
            lock (_semaphoreResetLock)
            {
                if (_semaphoreResetTask == null)
                    _semaphoreResetTask = ResetSemaphoresAsync();
            }
        }

        private async Task ResetSemaphoresAsync()
        {
            await Task.Delay(5000).ConfigureAwait(false);
            lock (_semaphoreResetLock)
            {
                foreach (var semaphore in _identifySemaphores)
                    if (semaphore.CurrentCount == 0)
                        semaphore.Release();
                _semaphoreResetTask = null;
            }
        }

        internal override async Task OnLoginAsync(TokenType tokenType, string token)
        {
            if (_automaticShards)
            {
                var botGateway = await GetBotGatewayAsync().ConfigureAwait(false);
                _shardIds = Enumerable.Range(0, botGateway.Shards).ToArray();
                _totalShards = _shardIds.Length;
                _shards = new DiscordSocketClient[_shardIds.Length];
                int maxConcurrency = botGateway.SessionStartLimit.MaxConcurrency;
                _baseConfig.IdentifyMaxConcurrency = maxConcurrency;
                _identifySemaphores = new SemaphoreSlim[maxConcurrency];
                for (int i = 0; i < maxConcurrency; i++)
                    _identifySemaphores[i] = new SemaphoreSlim(1, 1);
                for (int i = 0; i < _shardIds.Length; i++)
                {
                    _shardIdsToIndex.Add(_shardIds[i], i);
                    var newConfig = _baseConfig.Clone();
                    newConfig.ShardId = _shardIds[i];
                    newConfig.TotalShards = _totalShards;
                    _shards[i] = new DiscordSocketClient(newConfig, this, i != 0 ? _shards[0] : null);
                    RegisterEvents(_shards[i], i == 0);
                }
            }

            //Assume thread safe: already in a connection lock
            for (int i = 0; i < _shards.Length; i++)
                await _shards[i].LoginAsync(tokenType, token);

            if(_defaultStickers.Length == 0 && _baseConfig.AlwaysDownloadDefaultStickers)
                await DownloadDefaultStickersAsync().ConfigureAwait(false);

        }
        internal override async Task OnLogoutAsync()
        {
            //Assume thread safe: already in a connection lock
            if (_shards != null)
            {
                for (int i = 0; i < _shards.Length; i++)
                    await _shards[i].LogoutAsync();
            }

            if (_automaticShards)
            {
                _shardIds = new int[0];
                _shardIdsToIndex.Clear();
                _totalShards = 0;
                _shards = null;
            }
        }

        /// <inheritdoc />
        public override async Task StartAsync()
            => await Task.WhenAll(_shards.Select(x => x.StartAsync())).ConfigureAwait(false);
        /// <inheritdoc />
        public override async Task StopAsync()
            => await Task.WhenAll(_shards.Select(x => x.StopAsync())).ConfigureAwait(false);

        public DiscordSocketClient GetShard(int id)
        {
            if (_shardIdsToIndex.TryGetValue(id, out id))
                return _shards[id];
            return null;
        }
        private int GetShardIdFor(ulong guildId)
            => (int)((guildId >> 22) % (uint)_totalShards);
        public int GetShardIdFor(IGuild guild)
            => GetShardIdFor(guild?.Id ?? 0);
        private DiscordSocketClient GetShardFor(ulong guildId)
            => GetShard(GetShardIdFor(guildId));
        public DiscordSocketClient GetShardFor(IGuild guild)
            => GetShardFor(guild?.Id ?? 0);

        /// <inheritdoc />
        public override async Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null)
            => await _shards[0].GetApplicationInfoAsync(options).ConfigureAwait(false);

        /// <inheritdoc />
        public override SocketGuild GetGuild(ulong id)
            => GetShardFor(id).GetGuild(id);

        /// <inheritdoc />
        public override SocketChannel GetChannel(ulong id)
        {
            for (int i = 0; i < _shards.Length; i++)
            {
                var channel = _shards[i].GetChannel(id);
                if (channel != null)
                    return channel;
            }
            return null;
        }
        private IEnumerable<ISocketPrivateChannel> GetPrivateChannels()
        {
            for (int i = 0; i < _shards.Length; i++)
            {
                foreach (var channel in _shards[i].PrivateChannels)
                    yield return channel;
            }
        }
        private int GetPrivateChannelCount()
        {
            int result = 0;
            for (int i = 0; i < _shards.Length; i++)
                result += _shards[i].PrivateChannels.Count;
            return result;
        }

        private IEnumerable<SocketGuild> GetGuilds()
        {
            for (int i = 0; i < _shards.Length; i++)
            {
                foreach (var guild in _shards[i].Guilds)
                    yield return guild;
            }
        }
        private int GetGuildCount()
        {
            int result = 0;
            for (int i = 0; i < _shards.Length; i++)
                result += _shards[i].Guilds.Count;
            return result;
        }
        /// <inheritdoc/>
        public override async Task<SocketSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            var sticker = _defaultStickers.FirstOrDefault(x => x.Stickers.Any(y => y.Id == id))?.Stickers.FirstOrDefault(x => x.Id == id);

            if (sticker != null)
                return sticker;

            foreach (var guild in Guilds)
            {
                sticker = await guild.GetStickerAsync(id, CacheMode.CacheOnly).ConfigureAwait(false);

                if (sticker != null)
                    return sticker;
            }

            if (mode == CacheMode.CacheOnly)
                return null;

            var model = await ApiClient.GetStickerAsync(id, options).ConfigureAwait(false);

            if (model == null)
                return null;


            if (model.GuildId.IsSpecified)
            {
                var guild = GetGuild(model.GuildId.Value);
                sticker = guild.AddOrUpdateSticker(model);
                return sticker;
            }
            else
            {
                return SocketSticker.Create(_shards[0], model);
            }
        }
        private async Task DownloadDefaultStickersAsync()
        {
            var models = await ApiClient.ListNitroStickerPacksAsync().ConfigureAwait(false);

            var builder = ImmutableArray.CreateBuilder<StickerPack<SocketSticker>>();

            foreach (var model in models.StickerPacks)
            {
                var stickers = model.Stickers.Select(x => SocketSticker.Create(_shards[0], x));

                var pack = new StickerPack<SocketSticker>(
                    model.Name,
                    model.Id,
                    model.SkuId,
                    model.CoverStickerId.ToNullable(),
                    model.Description,
                    model.BannerAssetId,
                    stickers
                );

                builder.Add(pack);
            }

            _defaultStickers = builder.ToImmutable();
        }

        /// <inheritdoc />
        public override SocketUser GetUser(ulong id)
        {
            for (int i = 0; i < _shards.Length; i++)
            {
                var user = _shards[i].GetUser(id);
                if (user != null)
                    return user;
            }
            return null;
        }
        /// <inheritdoc />
        public override SocketUser GetUser(string username, string discriminator)
        {
            for (int i = 0; i < _shards.Length; i++)
            {
                var user = _shards[i].GetUser(username, discriminator);
                if (user != null)
                    return user;
            }
            return null;
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
        {
            return await _shards[0].GetVoiceRegionsAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async ValueTask<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
        {
            return await _shards[0].GetVoiceRegionAsync(id, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="guilds"/> is <see langword="null"/></exception>
        public override async Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
        {
            if (guilds == null) throw new ArgumentNullException(nameof(guilds));
            for (int i = 0; i < _shards.Length; i++)
            {
                int id = _shardIds[i];
                var arr = guilds.Where(x => GetShardIdFor(x) == id).ToArray();
                if (arr.Length > 0)
                    await _shards[i].DownloadUsersAsync(arr).ConfigureAwait(false);
            }
        }

        private int GetLatency()
        {
            int total = 0;
            for (int i = 0; i < _shards.Length; i++)
                total += _shards[i].Latency;
            return (int)Math.Round(total / (double)_shards.Length);
        }

        /// <inheritdoc />
        public override async Task SetStatusAsync(UserStatus status)
        {
            for (int i = 0; i < _shards.Length; i++)
                await _shards[i].SetStatusAsync(status).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public override async Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
        {
            IActivity activity = null;
            if (!string.IsNullOrEmpty(streamUrl))
                activity = new StreamingGame(name, streamUrl);
            else if (!string.IsNullOrEmpty(name))
                activity = new Game(name, type);
            await SetActivityAsync(activity).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public override async Task SetActivityAsync(IActivity activity)
        {
            for (int i = 0; i < _shards.Length; i++)
                await _shards[i].SetActivityAsync(activity).ConfigureAwait(false);
        }

        private void RegisterEvents(DiscordSocketClient client, bool isPrimary)
        {
            client.LoggedOut += (sender, args) =>
            {
                var state = LoginState;
                if (state == LoginState.LoggedIn || state == LoginState.LoggingIn)
                {
                    //Should only happen if token is changed
                    var _ = LogoutAsync(); //Signal the logout, fire and forget
                }
            };
        }
        #endregion

        #region IDiscordClient
        /// <inheritdoc />
        ISelfUser IDiscordClient.CurrentUser => CurrentUser;

        /// <inheritdoc />
        async Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
            => await GetApplicationInfoAsync().ConfigureAwait(false);

        /// <inheritdoc />
        Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
            => Task.FromResult<IChannel>(GetChannel(id));
        /// <inheritdoc />
        Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
            => Task.FromResult<IReadOnlyCollection<IPrivateChannel>>(PrivateChannels);

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
            => await GetConnectionsAsync().ConfigureAwait(false);

        /// <inheritdoc />
        async Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
            => await GetInviteAsync(inviteId, options).ConfigureAwait(false);

        /// <inheritdoc />
        Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
            => Task.FromResult<IGuild>(GetGuild(id));
        /// <inheritdoc />
        Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
            => Task.FromResult<IReadOnlyCollection<IGuild>>(Guilds);
        /// <inheritdoc />
        async Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
            => await CreateGuildAsync(name, region, jpegIcon).ConfigureAwait(false);

        /// <inheritdoc />
        async Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            var user = GetUser(id);
            if (user is not null || mode == CacheMode.CacheOnly)
                return user;

            return await Rest.GetUserAsync(id, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
            => Task.FromResult<IUser>(GetUser(username, discriminator));

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
        {
            return await GetVoiceRegionsAsync().ConfigureAwait(false);
        }
        /// <inheritdoc />
        async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
        {
            return await GetVoiceRegionAsync(id).ConfigureAwait(false);
        }
        #endregion

        #region Dispose
        internal override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_shards != null)
                    {
                        foreach (var client in _shards)
                            client?.Dispose();
                    }
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        internal override ValueTask DisposeAsync(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_shards != null)
                    {
                        foreach (var client in _shards)
                            client?.Dispose();
                    }
                }

                _isDisposed = true;
            }

            return base.DisposeAsync(disposing);
        }
        #endregion
    }
}
