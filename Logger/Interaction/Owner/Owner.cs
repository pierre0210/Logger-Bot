using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;

namespace Logger.Interaction.Owner
{
    public class Owner : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public Owner(DiscordSocketClient client)
        {
            _client = client;
        }

        [RequireOwner]
        [SlashCommand("shutdown", "shutdown the bot (bot owner only)")]
        public async Task ShutdownAsync()
        {
            await RespondAsync(text: "Shutting down...", ephemeral: true);
            Program.isBotOn = false;
        }
    }
}
