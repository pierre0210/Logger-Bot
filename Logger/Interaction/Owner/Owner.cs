using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading;

namespace Logger.Interaction.Owner
{
    public class Owner : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private static Timer _timer;

        public Owner(DiscordSocketClient client)
        {
            _client = client;
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireOwner]
        [SlashCommand("shutdown", "shutdown the bot (bot owner only)")]
        public async Task ShutdownAsync()
        {
            await RespondAsync(text: "Shutting down...", ephemeral: true);
            Program.isBotOn = false;
        }

        //[DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireOwner]
        [SlashCommand("ultmute", "ultimate mute command")]
        public async Task UltMuteAsync(IGuildUser user, int minutes)
        {
            Program.BlackList.Add(user.Id);
            var post = new EmbedBuilder().WithColor(Color.DarkRed)
                .WithDescription($"<@{user.Id}> 哈哈去新疆");
            await RespondAsync(text: "Muted", ephemeral: true);
            await Context.Channel.SendMessageAsync(embed: post.Build());
            _timer = new Timer(async x => await _unban(x, user.Id, Context), null, minutes * 60 * 1000, Timeout.Infinite);
        }

        [RequireOwner]
        [SlashCommand("embed", "create embed message")]
        public async Task EmbedAsync(string title, string description, UInt32 color, bool isTimestamp, IUser user)
        {
            var embed = new EmbedBuilder().WithTitle(title).WithDescription(description).WithColor(new Color(color))
                .WithAuthor(user);
            if(isTimestamp) embed.Timestamp = DateTime.Now;
            await RespondAsync(text: "Done", ephemeral: true);
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        private async Task _unban(object x, ulong userId, SocketInteractionContext context)
        {
            var releasePost = new EmbedBuilder().WithColor(Color.DarkGreen)
                .WithDescription($"<@{userId}> 出獄");
            await context.Channel.SendMessageAsync(embed: releasePost.Build());
            Program.BlackList.Remove(userId);
        }
    }
}
