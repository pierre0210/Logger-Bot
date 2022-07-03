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
            await RespondAsync(text);
        }
    }
}
