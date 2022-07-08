using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Logger.Database;
using Logger.Database.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction.Logging
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("message-log", "message logging")]
    public class Message
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _service;
        private readonly InteractionService _interaction;

        public Message(DiscordSocketClient client, IServiceProvider service, InteractionService interaction)
        {
            _client = client;
            _service = service;
            _interaction = interaction;
        }

        [SlashCommand("enable", "enable message logging")]
        public async Task EnableAsync()
        {

        }

        [SlashCommand("disable", "disable message logging")]
        public async Task DisableAsync()
        {

        }
    }
}
