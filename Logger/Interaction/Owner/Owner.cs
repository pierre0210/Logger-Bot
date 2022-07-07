using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Logger.Interaction.Owner
{
    public class Owner : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private static System.Timers.Timer _timer;

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

        [RequireOwner]
        [SlashCommand("ultmute", "ultimate mute command")]
        public async Task UltMuteAsync(IGuildUser user, int minutes)
        {
            Program.BlackList.Add(user.Id);
            var post = new EmbedBuilder().WithColor(Color.DarkRed)
                .WithDescription($"<@{user.Id}> 哈哈去新疆");
            await RespondAsync(text: "Muted", ephemeral: true);
            await Context.Channel.SendMessageAsync(embed: post.Build());
            _timer = new System.Timers.Timer(minutes * 60 * 1000);
            _timer.Elapsed += (sender, args) =>
            {
                var releasePost = new EmbedBuilder().WithColor(Color.DarkGreen)
                    .WithDescription($"<@{user.Id}> 出獄");
                Context.Channel.SendMessageAsync(embed: releasePost.Build());
                Program.BlackList.Remove(user.Id);
                _timer.Stop();
                _timer.Dispose();
            };
            _timer.Enabled = true;
            
        }
    }
}
