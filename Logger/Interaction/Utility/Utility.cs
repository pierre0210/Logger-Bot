using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction.Utility
{
    [Group("utility", "some useful tools")]
    public class Utility : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public Utility(DiscordSocketClient client)
        {
            _client = client;
        }

        [SlashCommand("ping", "get latency")]
        public async Task PingAsync()
        {
            await RespondAsync($"**Latency:** `{_client.Latency.ToString()}ms`");
        }

        [DefaultMemberPermissions(GuildPermission.ManageMessages)]
        [SlashCommand("say", "say something")]
        public async Task SayAsync([Summary(description: "some text")] string text)
        {
            await RespondAsync(text: "Done", ephemeral: true);
            await Context.Channel.SendMessageAsync(text);
        }

        [SlashCommand("8ball", "8ball")]
        public async Task EightBallAsync([Summary(description: "some question")] string question)
        {
            var replies = new List<string>();
            replies.Add("當然!");
            replies.Add("顯然否");
            replies.Add("或許(?");
            replies.Add("等等再問");
            replies.Add("你為什麼不問問神奇海螺呢?");

            string answer = replies[new Random().Next(replies.Count - 1)];

            var answerEmbed = new EmbedBuilder()
                .WithColor(Color.Gold)
                .WithDescription($"**Q: {question}**\nA: {answer}");
            await RespondAsync(embed: answerEmbed.Build());
        }
    }
}
