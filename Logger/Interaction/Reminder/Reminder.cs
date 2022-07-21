using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Logger;
using Logger.Database;
using Logger.Database.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction.Reminder
{
    [Group("reminder", "reminder command")]
    public class Reminder
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _service;
        private readonly InteractionService _interaction;

        public Reminder(DiscordSocketClient client, IServiceProvider service, InteractionService interaction)
        {
            _client = client;
            _service = service;
            _interaction = interaction;
        }

        [SlashCommand("add", "add new reminder")]
        public async Task AddAsync([Summary(description: "小時")]int hours, [Summary(description: "分鐘")]int minutes, string content)
        {
            RedisUtility utility = new RedisUtility(Program.RedisDb);
        }
    }
}
