using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    public partial class BaseSocketClient
    {
        #region Channels

        /// <summary> Fired when a channel is created. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a generic channel has been created. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketChannel"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The newly created channel is passed into the event handler parameter. The given channel type may
        ///         include, but not limited to, Private Channels (DM, Group), Guild Channels (Text, Voice, Category);
        ///         see the derived classes of <see cref="SocketChannel"/> for more details.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code language="cs" region="ChannelCreated"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs"/>
        /// </example>
        public event EventHandler<SocketChannel> ChannelCreated;
        protected virtual void OnChannelCreated(SocketChannel eventArgs)
        {
            ChannelCreated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a channel is destroyed. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a generic channel has been destroyed. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketChannel"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The destroyed channel is passed into the event handler parameter. The given channel type may
        ///         include, but not limited to, Private Channels (DM, Group), Guild Channels (Text, Voice, Category);
        ///         see the derived classes of <see cref="SocketChannel"/> for more details.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code language="cs" region="ChannelDestroyed"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs"/>
        /// </example>
        public event EventHandler<SocketChannel> ChannelDestroyed;
        protected virtual void OnChannelDestroyed(SocketChannel eventArgs)
        {
            ChannelDestroyed?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a channel is updated. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a generic channel has been destroyed. The event handler must return a
        ///         <see cref="Task"/> and accept 2 <see cref="SocketChannel"/> as its parameters.
        ///     </para>
        ///     <para>
        ///         The original (prior to update) channel is passed into the first <see cref="SocketChannel"/>, while
        ///         the updated channel is passed into the second. The given channel type may include, but not limited
        ///         to, Private Channels (DM, Group), Guild Channels (Text, Voice, Category); see the derived classes of
        ///         <see cref="SocketChannel"/> for more details.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code language="cs" region="ChannelUpdated"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs"/>
        /// </example>
        public event EventHandler<SocketChannelUpdated> ChannelUpdated;
        protected virtual void OnChannelUpdated(SocketChannelUpdated eventArgs)
        {
            ChannelUpdated?.Invoke(this, eventArgs);
        }
        public class SocketChannelUpdated
        {
            public SocketChannel OldSocketChannel { get; set; }
            public SocketChannel NewSocketChannel { get; set; }
        }

        #endregion

        #region Messages

        /// <summary> Fired when a message is received. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a message is received. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketMessage"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The message that is sent to the client is passed into the event handler parameter as
        ///         <see cref="SocketMessage"/>. This message may be a system message (i.e.
        ///         <see cref="SocketSystemMessage"/>) or a user message (i.e. <see cref="SocketUserMessage"/>. See the
        ///         derived classes of <see cref="SocketMessage"/> for more details.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <para>The example below checks if the newly received message contains the target user.</para>
        ///     <code language="cs" region="MessageReceived"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs"/>
        /// </example>
        public event EventHandler<SocketMessage> MessageReceived;
        protected virtual void OnMessageReceived(SocketMessage eventArgs)
        {
            MessageReceived?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a message is deleted. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a message is deleted. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="Cacheable{TEntity,TId}"/> and 
        ///         <see cref="ISocketMessageChannel"/> as its parameters.
        ///     </para>
        ///     <para>
        ///         <note type="important">
        ///             It is not possible to retrieve the message via
        ///             <see cref="Cacheable{TEntity,TId}.DownloadAsync"/>; the message cannot be retrieved by Discord
        ///             after the message has been deleted.
        ///         </note>
        ///         If caching is enabled via <see cref="DiscordSocketConfig"/>, the
        ///         <see cref="Cacheable{TEntity,TId}"/> entity will contain the deleted message; otherwise, in event
        ///         that the message cannot be retrieved, the snowflake ID of the message is preserved in the 
        ///         <see cref="ulong"/>.
        ///     </para>
        ///     <para>
        ///         The source channel of the removed message will be passed into the 
        ///         <see cref="ISocketMessageChannel"/> parameter.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code language="cs" region="MessageDeleted"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs" />
        /// </example>

        public event EventHandler<MessageDeletedArguments> MessageDeleted;
        protected virtual void OnMessageDeleted(MessageDeletedArguments eventArgs)
        {
            MessageDeleted?.Invoke(this, eventArgs);
        }
        public class MessageDeletedArguments
        {
            public Cacheable<IMessage, ulong> Message { get; set; }
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
        }

        /// <summary> Fired when multiple messages are bulk deleted. </summary>
        /// <remarks>
        ///     <note>
        ///         The <see cref="MessageDeleted"/> event will not be fired for individual messages contained in this event.
        ///     </note>
        ///     <para>
        ///         This event is fired when multiple messages are bulk deleted. The event handler must return a
        ///         <see cref="Task"/> and accept an <see cref="IReadOnlyCollection{Cacheable}"/> and 
        ///         <see cref="ISocketMessageChannel"/> as its parameters.
        ///     </para>
        ///     <para>
        ///         <note type="important">
        ///             It is not possible to retrieve the message via
        ///             <see cref="Cacheable{TEntity,TId}.DownloadAsync"/>; the message cannot be retrieved by Discord
        ///             after the message has been deleted.
        ///         </note>
        ///         If caching is enabled via <see cref="DiscordSocketConfig"/>, the
        ///         <see cref="Cacheable{TEntity,TId}"/> entity will contain the deleted message; otherwise, in event
        ///         that the message cannot be retrieved, the snowflake ID of the message is preserved in the 
        ///         <see cref="ulong"/>.
        ///     </para>
        ///     <para>
        ///         The source channel of the removed message will be passed into the 
        ///         <see cref="ISocketMessageChannel"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<MessagesBulkDeletedArguments> MessagesBulkDeleted;
        protected virtual void OnMessagesBulkDeleted(MessagesBulkDeletedArguments eventArgs)
        {
            MessagesBulkDeleted?.Invoke(this, eventArgs);
        }
        public class MessagesBulkDeletedArguments
        {
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
            public IReadOnlyCollection<Cacheable<IMessage, ulong>> Messages { get; set; }
        }

        /// <summary> Fired when a message is updated. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a message is updated. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="Cacheable{TEntity,TId}"/>, <see cref="SocketMessage"/>,
        ///         and <see cref="ISocketMessageChannel"/> as its parameters.
        ///     </para>
        ///     <para>
        ///         If caching is enabled via <see cref="DiscordSocketConfig"/>, the
        ///         <see cref="Cacheable{TEntity,TId}"/> entity will contain the original message; otherwise, in event
        ///         that the message cannot be retrieved, the snowflake ID of the message is preserved in the 
        ///         <see cref="ulong"/>.
        ///     </para>
        ///     <para>
        ///         The updated message will be passed into the <see cref="SocketMessage"/> parameter.
        ///     </para>
        ///     <para>
        ///         The source channel of the updated message will be passed into the 
        ///         <see cref="ISocketMessageChannel"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<MessageUpdatedArguments> MessageUpdated;
        public class MessageUpdatedArguments
        {
            public Cacheable<IMessage, ulong> OriginalMessage { get; set; }
            public SocketMessage UpdatedMessage { get; set; }
            public ISocketMessageChannel MessageChannel { get; set; }
        }
        protected virtual void OnMessageUpdated(MessageUpdatedArguments eventArgs)
        {
            MessageUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a reaction is added to a message. </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when a reaction is added to a user message. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="Cacheable{TEntity,TId}"/>, an 
        ///         <see cref="ISocketMessageChannel"/>, and a <see cref="SocketReaction"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         If caching is enabled via <see cref="DiscordSocketConfig"/>, the
        ///         <see cref="Cacheable{TEntity,TId}"/> entity will contain the original message; otherwise, in event
        ///         that the message cannot be retrieved, the snowflake ID of the message is preserved in the 
        ///         <see cref="ulong"/>.
        ///     </para>
        ///     <para>
        ///         The source channel of the reaction addition will be passed into the 
        ///         <see cref="ISocketMessageChannel"/> parameter.
        ///     </para>
        ///     <para>
        ///         The reaction that was added will be passed into the <see cref="SocketReaction"/> parameter.
        ///     </para>
        ///     <note>
        ///         When fetching the reaction from this event, a user may not be provided under 
        ///         <see cref="SocketReaction.User"/>. Please see the documentation of the property for more
        ///         information.
        ///     </note>
        /// </remarks>
        /// <example>
        ///     <code language="cs" region="ReactionAdded"
        ///           source="..\Discord.Net.Examples\WebSocket\BaseSocketClient.Events.Examples.cs"/>
        /// </example>
        public event EventHandler<ReactionAddedArguments> ReactionAdded;
        public class ReactionAddedArguments
        {
            public Cacheable<IUserMessage, ulong> Message { get; set; }
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
            public SocketReaction AddedReaction { get; set; }
        }
        protected virtual void OnReactionAdded(ReactionAddedArguments eventArgs)
        {
            ReactionAdded?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a reaction is removed from a message. </summary>
        public event EventHandler<ReactionRemovedArguments> ReactionRemoved;
        public class ReactionRemovedArguments
        {
            public Cacheable<IUserMessage, ulong> Message { get; set; }
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
            public SocketReaction RemovedReaction { get; set; }
        }
        protected virtual void OnReactionRemoved(ReactionRemovedArguments eventArgs)
        {
            ReactionRemoved?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when all reactions to a message are cleared. </summary>
        public event EventHandler<ReactionsClearedArguments> ReactionsCleared;
        public class ReactionsClearedArguments
        {
            public Cacheable<IUserMessage, ulong> Message { get; set; }
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
        }
        protected virtual void OnReactionsCleared(ReactionsClearedArguments eventArgs)
        {
            ReactionsCleared?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when all reactions to a message with a specific emote are removed.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when all reactions to a message with a specific emote are removed.
        ///         The event handler must return a <see cref="Task"/> and accept a <see cref="ISocketMessageChannel"/> and
        ///         a <see cref="IEmote"/> as its parameters.
        ///     </para>
        ///     <para>
        ///         The channel where this message was sent will be passed into the <see cref="ISocketMessageChannel"/> parameter.
        ///     </para>
        ///     <para>
        ///         The emoji that all reactions had and were removed will be passed into the <see cref="IEmote"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<ReactionsRemovedForEmoteArguments> ReactionsRemovedForEmote;
        public class ReactionsRemovedForEmoteArguments
        {
            public Cacheable<IUserMessage, ulong> Message { get; set; }
            public Cacheable<IMessageChannel, ulong> MessageChannel { get; set; }
            public IEmote RemovedEmote { get; set; }
        }
        protected virtual void OnReactionsRemovedForEmote(ReactionsRemovedForEmoteArguments eventArgs)
        {
            ReactionsRemovedForEmote?.Invoke(this, eventArgs);
        }

        #endregion

        #region Roles

        /// <summary> Fired when a role is created. </summary>
        public event EventHandler<SocketRole> RoleCreated;
        protected virtual void OnRoleCreated(SocketRole eventArgs)
        {
            RoleCreated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a role is deleted. </summary>
        public event EventHandler<SocketRole> RoleDeleted;
        protected virtual void OnRoleDeleted(SocketRole eventArgs)
        {
            RoleDeleted?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a role is updated. </summary>
        public event EventHandler<RoleUpdatedArguments> RoleUpdated;
        public class RoleUpdatedArguments
        {
            public SocketRole OriginalRole { get; set; }
            public SocketRole UpdatedRole { get; set; }
        }
        protected virtual void OnRoleUpdated(RoleUpdatedArguments eventArgs)
        {
            RoleUpdated?.Invoke(this, eventArgs);
        }

        #endregion

        #region Guilds

        /// <summary> Fired when the connected account joins a guild. </summary>
        public event EventHandler<SocketGuild> JoinedGuild;
        protected virtual void OnJoinedGuild(SocketGuild eventArgs)
        {
            JoinedGuild?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when the connected account leaves a guild. </summary>
        public event EventHandler<SocketGuild> LeftGuild;
        protected virtual void OnLeftGuild(SocketGuild eventArgs)
        {
            LeftGuild?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a guild becomes available. </summary>
        public event EventHandler<SocketGuild> GuildAvailable;
        protected virtual void OnGuildAvailable(SocketGuild eventArgs)
        {
            GuildAvailable?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a guild becomes unavailable. </summary>
        public event EventHandler<SocketGuild> GuildUnavailable;
        protected virtual void OnGuildUnavailable(SocketGuild eventArgs)
        {
            GuildUnavailable?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when offline guild members are downloaded. </summary>
        public event EventHandler<SocketGuild> GuildMembersDownloaded;
        protected virtual void OnGuildMembersDownloaded(SocketGuild eventArgs)
        {
            GuildMembersDownloaded?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a guild is updated. </summary>
        public event EventHandler<GuildUpdatedArguments> GuildUpdated;
        public class GuildUpdatedArguments
        {
            public SocketGuild OriginalGuild { get; set; }
            public SocketGuild UpdatedGuild { get; set; }
        }
        protected virtual void OnGuildUpdated(GuildUpdatedArguments eventArgs)
        {
            GuildUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>Fired when a user leaves without agreeing to the member screening </summary>
        public event EventHandler<GuildJoinRequestDeletedArguments> GuildJoinRequestDeleted;
        public class GuildJoinRequestDeletedArguments
        {
            public Cacheable<SocketGuildUser, ulong> GuildUser { get; set; }
            public SocketGuild Guild { get; set; }
        }
        protected virtual void OnGuildJoinRequestDeleted(GuildJoinRequestDeletedArguments eventArgs)
        {
            GuildJoinRequestDeleted?.Invoke(this, eventArgs);
        }

        #endregion

        #region Guild Events

        /// <summary>
        ///     Fired when a guild event is created.
        /// </summary>
        public event EventHandler<SocketGuildEvent> GuildScheduledEventCreated;
        protected virtual void OnGuildScheduledEventCreated(SocketGuildEvent eventArgs)
        {
            GuildScheduledEventCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a guild event is updated.
        /// </summary>
        public event EventHandler<GuildScheduledEventUpdatedArguments> GuildScheduledEventUpdated;
        public class GuildScheduledEventUpdatedArguments
        {
            public Cacheable<SocketGuildEvent, ulong> OriginalGuildEvent { get; set; }
            public SocketGuildEvent UpdatedGuildEvent { get; set; }
        }
        protected virtual void OnGuildScheduledEventUpdated(GuildScheduledEventUpdatedArguments eventArgs)
        {
            GuildScheduledEventUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a guild event is cancelled.
        /// </summary>
        public event EventHandler<SocketGuildEvent> GuildScheduledEventCancelled;
        protected virtual void OnGuildScheduledEventCancelled(SocketGuildEvent eventArgs)
        {
            GuildScheduledEventCancelled?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a guild event is completed.
        /// </summary>
        public event EventHandler<SocketGuildEvent> GuildScheduledEventCompleted;
        protected virtual void OnGuildScheduledEventCompleted(SocketGuildEvent eventArgs)
        {
            GuildScheduledEventCompleted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a guild event is started.
        /// </summary>
        public event EventHandler<SocketGuildEvent> GuildScheduledEventStarted;
        protected virtual void OnGuildScheduledEventStarted(SocketGuildEvent eventArgs)
        {
            GuildScheduledEventStarted?.Invoke(this, eventArgs);
        }

        public event EventHandler<GuildScheduledEventUserAddArguments> GuildScheduledEventUserAdd;
        public class GuildScheduledEventUserAddArguments
        {
            public Cacheable<SocketUser, RestUser, IUser, ulong> AddedGuildEventUser { get; set; }
            public SocketGuildEvent GuildEvent { get; set; }
        }
        protected virtual void OnGuildScheduledEventUserAdd(GuildScheduledEventUserAddArguments eventArgs)
        {
            GuildScheduledEventUserAdd?.Invoke(this, eventArgs);
        }

        public event EventHandler<GuildScheduledEventUserRemoveArguments> GuildScheduledEventUserRemove;
        public class GuildScheduledEventUserRemoveArguments
        {
            public Cacheable<SocketUser, RestUser, IUser, ulong> RemovedGuildEventUser { get; set; }
            public SocketGuildEvent GuildEvent { get; set; }
        }
        protected virtual void OnGuildScheduledEventUserRemove(GuildScheduledEventUserRemoveArguments eventArgs)
        {
            GuildScheduledEventUserRemove?.Invoke(this, eventArgs);
        }

        #endregion

        #region Integrations

        /// <summary> Fired when an integration is created. </summary>
        public event EventHandler<IIntegration> IntegrationCreated;
        protected virtual void OnIntegrationCreated(IIntegration eventArgs)
        {
            IntegrationCreated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when an integration is updated. </summary>
        public event EventHandler<IIntegration> IntegrationUpdated;
        protected virtual void OnIntegrationUpdated(IIntegration eventArgs)
        {
            IntegrationUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when an integration is deleted. </summary>
        public event EventHandler<IntegrationDeletedArguments> IntegrationDeleted;
        public class IntegrationDeletedArguments
        {
            public IGuild Guild { get; set; }
            public ulong IntegrationId { get; set; }
            public Optional<ulong> ApplicationId { get; set; }
        }
        protected virtual void OnIntegrationDeleted(IntegrationDeletedArguments eventArgs)
        {
            IntegrationDeleted?.Invoke(this, eventArgs);
        }

        #endregion

        #region Users

        /// <summary> Fired when a user joins a guild. </summary>
        public event EventHandler<SocketGuildUser> UserJoined;
        protected virtual void OnUserJoined(SocketGuildUser eventArgs)
        {
            UserJoined?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user leaves a guild. </summary>
        public event EventHandler<UserLeftArguments> UserLeft;
        public class UserLeftArguments
        {
            public SocketGuild Guild { get; set; }
            public SocketUser User { get; set; }
        }
        protected virtual void OnUserLeft(UserLeftArguments eventArgs)
        {
            UserLeft?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user is banned from a guild. </summary>
        public event EventHandler<UserBannedArguments> UserBanned;
        public class UserBannedArguments
        {
            public SocketUser User { get; set; }
            public SocketGuild Guild { get; set; }
        }
        protected virtual void OnUserBanned(UserBannedArguments eventArgs)
        {
            UserBanned?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user is unbanned from a guild. </summary>
        public event EventHandler<UserUnbannedArguments> UserUnbanned;
        public class UserUnbannedArguments
        {
            public SocketUser User { get; set; }
            public SocketGuild Guild { get; set; }
        }
        protected virtual void OnUserUnbanned(UserUnbannedArguments eventArgs)
        {
            UserUnbanned?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user is updated. </summary>
        public event EventHandler<UserUpdatedArguments> UserUpdated;
        public class UserUpdatedArguments
        {
            public SocketUser OriginalUser { get; set; }
            public SocketUser UpdatedUser { get; set; }
        }
        protected virtual void OnUserUpdated(UserUpdatedArguments eventArgs)
        {
            UserUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a guild member is updated. </summary>
        public event EventHandler<GuildMemberUpdatedArguments> GuildMemberUpdated;
        public class GuildMemberUpdatedArguments
        {
            public Cacheable<SocketGuildUser, ulong> OriginalGuildUser { get; set; }
            public SocketGuildUser UpdatedGuildUser { get; set; }
        }
        protected virtual void OnGuildMemberUpdated(GuildMemberUpdatedArguments eventArgs)
        {
            GuildMemberUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user joins, leaves, or moves voice channels. </summary>
        public event EventHandler<UserVoiceStateUpdatedArguments> UserVoiceStateUpdated;
        public class UserVoiceStateUpdatedArguments
        {
            public SocketUser User { get; set; }
            public SocketVoiceState OriginalVoiceState { get; set; }
            public SocketVoiceState UpdatedVoiceState { get; set; }
        }
        protected virtual void OnUserVoiceStateUpdated(UserVoiceStateUpdatedArguments eventArgs)
        {
            UserVoiceStateUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when the bot connects to a Discord voice server. </summary>
        public event EventHandler<SocketVoiceServer> VoiceServerUpdated;
        protected virtual void OnVoiceServerUpdated(SocketVoiceServer eventArgs)
        {
            VoiceServerUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when the connected account is updated. </summary>
        public event EventHandler<CurrentUserUpdatedArguments> CurrentUserUpdated;
        public class CurrentUserUpdatedArguments
        {
            public SocketSelfUser OriginalSelfUser { get; set; }
            public SocketSelfUser UpdatedSelfUser { get; set; }
        }
        protected virtual void OnCurrentUserUpdated(CurrentUserUpdatedArguments eventArgs)
        {
            CurrentUserUpdated?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user starts typing. </summary>
        public event EventHandler<UserIsTypingArguments> UserIsTyping;
        public class UserIsTypingArguments
        {
            public Cacheable<IUser, ulong> User { get; set; }
            public Cacheable<IMessageChannel, ulong> Channel { get; set; }
        }
        protected virtual void OnUserIsTyping(UserIsTypingArguments eventArgs)
        {
            UserIsTyping?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user joins a group channel. </summary>
        public event EventHandler<SocketGroupUser> RecipientAdded;
        protected virtual void OnRecipientAdded(SocketGroupUser eventArgs)
        {
            RecipientAdded?.Invoke(this, eventArgs);
        }

        /// <summary> Fired when a user is removed from a group channel. </summary>
        public event EventHandler<SocketGroupUser> RecipientRemoved;
        protected virtual void OnRecipientRemoved(SocketGroupUser eventArgs)
        {
            RecipientRemoved?.Invoke(this, eventArgs);
        }

        #endregion

        #region Presence

        /// <summary> Fired when a users presence is updated. </summary>
        public event EventHandler<PresenceUpdatedArguments> PresenceUpdated;
        public class PresenceUpdatedArguments
        {
            public SocketUser User { get; set; }
            public SocketPresence OriginalPresence { get; set; }
            public SocketPresence UpdatedPresence { get; set; }
        }
        protected virtual void OnPresenceUpdated(PresenceUpdatedArguments eventArgs)
        {
            PresenceUpdated?.Invoke(this, eventArgs);
        }

        #endregion

        #region Invites

        /// <summary>
        ///     Fired when an invite is created.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when an invite is created. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketInvite"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The invite created will be passed into the <see cref="SocketInvite"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<SocketInvite> InviteCreated;
        protected virtual void OnInviteCreated(SocketInvite eventArgs)
        {
            InviteCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when an invite is deleted.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when an invite is deleted. The event handler must return
        ///         a <see cref="Task"/> and accept a <see cref="SocketGuildChannel"/> and
        ///         <see cref="string"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The channel where this invite was created will be passed into the <see cref="SocketGuildChannel"/> parameter.
        ///     </para>
        ///     <para>
        ///         The code of the deleted invite will be passed into the <see cref="string"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<InviteDeletedArguments> InviteDeleted;
        public class InviteDeletedArguments
        {
            public SocketGuildChannel GuildChannel { get; set; }
            public string InviteCode { get; set; }
        }
        protected virtual void OnInviteDeleted(InviteDeletedArguments eventArgs)
        {
            InviteDeleted?.Invoke(this, eventArgs);
        }

        #endregion

        #region Interactions

        /// <summary>
        ///     Fired when an Interaction is created. This event covers all types of interactions including but not limited to: buttons, select menus, slash commands, autocompletes.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when an interaction is created. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketInteraction"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The interaction created will be passed into the <see cref="SocketInteraction"/> parameter.
        ///     </para>
        /// </remarks>
        public event EventHandler<SocketInteraction> InteractionCreated;
        protected virtual void OnInteractionCreated(SocketInteraction eventArgs)
        {
            InteractionCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a button is clicked and its interaction is received.
        /// </summary>
        public event EventHandler<SocketMessageComponent> ButtonExecuted;
        protected virtual void OnButtonExecuted(SocketMessageComponent eventArgs)
        {
            ButtonExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a select menu is used and its interaction is received.
        /// </summary>
        public event EventHandler<SocketMessageComponent> SelectMenuExecuted;
        protected virtual void OnSelectMenuExecuted(SocketMessageComponent eventArgs)
        {
            SelectMenuExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a slash command is used and its interaction is received.
        /// </summary>
        public event EventHandler<SocketSlashCommand> SlashCommandExecuted;
        protected virtual void OnSlashCommandExecuted(SocketSlashCommand eventArgs)
        {
            SlashCommandExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a user command is used and its interaction is received.
        /// </summary>
        public event EventHandler<SocketUserCommand> UserCommandExecuted;
        protected virtual void OnUserCommandExecuted(SocketUserCommand eventArgs)
        {
            UserCommandExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a message command is used and its interaction is received.
        /// </summary>
        public event EventHandler<SocketMessageCommand> MessageCommandExecuted;
        protected virtual void OnMessageCommandExecuted(SocketMessageCommand eventArgs)
        {
            MessageCommandExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when an autocomplete is used and its interaction is received.
        /// </summary>
        public event EventHandler<SocketAutocompleteInteraction> AutocompleteExecuted;
        protected virtual void OnAutocompleteExecuted(SocketAutocompleteInteraction eventArgs)
        {
            AutocompleteExecuted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a modal is submitted.
        /// </summary>
        public event EventHandler<SocketModal> ModalSubmitted;
        protected virtual void OnModalSubmitted(SocketModal eventArgs)
        {
            ModalSubmitted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a guild application command is created.
        ///</summary>
        ///<remarks>
        ///     <para>
        ///         This event is fired when an application command is created. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketApplicationCommand"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The command that was deleted will be passed into the <see cref="SocketApplicationCommand"/> parameter.
        ///     </para>
        ///     <note>
        ///         <b>This event is an undocumented discord event and may break at any time, its not recommended to rely on this event</b>
        ///     </note>
        /// </remarks>
        public event EventHandler<SocketApplicationCommand> ApplicationCommandCreated;
        protected virtual void OnApplicationCommandCreated(SocketApplicationCommand eventArgs)
        {
            ApplicationCommandCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///      Fired when a guild application command is updated.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when an application command is updated. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketApplicationCommand"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The command that was deleted will be passed into the <see cref="SocketApplicationCommand"/> parameter.
        ///     </para>
        ///     <note>
        ///         <b>This event is an undocumented discord event and may break at any time, its not recommended to rely on this event</b>
        ///     </note>
        /// </remarks>
        public event EventHandler<SocketApplicationCommand> ApplicationCommandUpdated;
        protected virtual void OnApplicationCommandUpdated(SocketApplicationCommand eventArgs)
        {
            ApplicationCommandUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///      Fired when a guild application command is deleted.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This event is fired when an application command is deleted. The event handler must return a
        ///         <see cref="Task"/> and accept a <see cref="SocketApplicationCommand"/> as its parameter.
        ///     </para>
        ///     <para>
        ///         The command that was deleted will be passed into the <see cref="SocketApplicationCommand"/> parameter.
        ///     </para>
        ///     <note>
        ///         <b>This event is an undocumented discord event and may break at any time, its not recommended to rely on this event</b>
        ///     </note>
        /// </remarks>
        public event EventHandler<SocketApplicationCommand> ApplicationCommandDeleted;
        protected virtual void OnApplicationCommandDeleted(SocketApplicationCommand eventArgs)
        {
            ApplicationCommandDeleted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a thread is created within a guild, or when the current user is added to a thread.
        /// </summary>
        public event EventHandler<SocketThreadChannel> ThreadCreated;
        protected virtual void OnThreadCreated(SocketThreadChannel eventArgs)
        {
            ThreadCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a thread is updated within a guild.
        /// </summary>
        public event EventHandler<ThreadUpdatedArguments> ThreadUpdated;
        public class ThreadUpdatedArguments
        {
            public Cacheable<SocketThreadChannel, ulong> OriginalThreadChannel { get; set; }
            public SocketThreadChannel UpdatedThreadChannel { get; set; }
        }
        protected virtual void OnThreadUpdated(ThreadUpdatedArguments eventArgs)
        {
            ThreadUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a thread is deleted.
        /// </summary>
        public event EventHandler<Cacheable<SocketThreadChannel, ulong>> ThreadDeleted;
        protected virtual void OnThreadDeleted(Cacheable<SocketThreadChannel, ulong> eventArgs)
        {
            ThreadDeleted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a user joins a thread
        /// </summary>
        public event EventHandler<SocketThreadUser> ThreadMemberJoined;
        protected virtual void OnThreadMemberJoined(SocketThreadUser eventArgs)
        {
            ThreadMemberJoined?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a user leaves a thread
        /// </summary>
        public event EventHandler<SocketThreadUser> ThreadMemberLeft;
        protected virtual void OnThreadMemberLeft(SocketThreadUser eventArgs)
        {
            ThreadMemberLeft?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a stage is started.
        /// </summary>
        public event EventHandler<SocketStageChannel> StageStarted;
        protected virtual void OnStageStarted(SocketStageChannel eventArgs)
        {
            StageStarted?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a stage ends.
        /// </summary>
        public event EventHandler<SocketStageChannel> StageEnded;
        protected virtual void OnStageEnded(SocketStageChannel eventArgs)
        {
            StageEnded?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a stage is updated.
        /// </summary>
        public event EventHandler<StageUpdatedArguments> StageUpdated;
        public class StageUpdatedArguments
        {
            public SocketStageChannel OriginalStage { get; set; }
            public SocketStageChannel UpdatedStage { get; set; }
        }
        protected virtual void OnStageUpdated(StageUpdatedArguments eventArgs)
        {
            StageUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a user requests to speak within a stage channel.
        /// </summary>
        public event EventHandler<RequestToSpeakArguments> RequestToSpeak;
        public class RequestToSpeakArguments
        {
            public SocketStageChannel StageChannel { get; set; }
            public SocketGuildUser GuildUser { get; set; }
        }
        protected virtual void OnRequestToSpeak(RequestToSpeakArguments eventArgs)
        {
            RequestToSpeak?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a speaker is added in a stage channel.
        /// </summary>
        public event EventHandler<SpeakerAddedArguments> SpeakerAdded;
        public class SpeakerAddedArguments
        {
            public SocketStageChannel StageChannel { get; set; }
            public SocketGuildUser GuildUser { get; set; }
        }
        protected virtual void OnSpeakerAdded(SpeakerAddedArguments eventArgs)
        {
            SpeakerAdded?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a speaker is removed from a stage channel.
        /// </summary>
        public event EventHandler<SpeakerRemovedArguments> SpeakerRemoved;
        public class SpeakerRemovedArguments
        {
            public SocketStageChannel StageChannel { get; set; }
            public SocketGuildUser GuildUser { get; set; }
        }
        protected virtual void OnSpeakerRemoved(SpeakerRemovedArguments eventArgs)
        {
            SpeakerRemoved?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a sticker in a guild is created.
        /// </summary>
        public event EventHandler<SocketCustomSticker> GuildStickerCreated;
        protected virtual void OnGuildStickerCreated(SocketCustomSticker eventArgs)
        {
            GuildStickerCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a sticker in a guild is updated.
        /// </summary>
        public event EventHandler<GuildStickerUpdatedArguments> GuildStickerUpdated;
        public class GuildStickerUpdatedArguments
        {
            public SocketCustomSticker OriginalCustomSticker { get; set; }
            public SocketCustomSticker UpdatedCustomSticker { get; set; }
        }
        protected virtual void OnGuildStickerUpdated(GuildStickerUpdatedArguments eventArgs)
        {
            GuildStickerUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Fired when a sticker in a guild is deleted.
        /// </summary>
        public event EventHandler<SocketCustomSticker> GuildStickerDeleted;
        protected virtual void OnGuildStickerDeleted(SocketCustomSticker eventArgs)
        {
            GuildStickerDeleted?.Invoke(this, eventArgs);
        }

        #endregion
    }
}
