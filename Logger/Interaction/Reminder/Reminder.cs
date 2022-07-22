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
using System.Threading;

namespace Logger.Interaction.Reminder
{
    [Group("reminder", "reminder command")]
    public class Reminder : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _service;
        private readonly InteractionService _interaction;

        private Timer _timer;

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
            Database.Table.Reminder reminder = new Database.Table.Reminder();
            reminder.UserId = Context.User.Id;
            reminder.Duration = hours*60*60 + minutes*60;
            reminder.Content = content;

            int index;
            for(index = 0; index < Program.RedisServer.Keys().Count(); index++)
            {
                if(!await utility.DbExistsAsync<Database.Table.Reminder>($"{Context.User.Id}:{index}")) break;
            }
            await utility.DbSetAsync($"{Context.User.Id}:{index}", reminder);
            await RespondAsync($"提醒 **{Context.User.Username}** {content}", ephemeral: true);
            //TimerCallback callback = new TimerCallback(_todo);
            _timer = new Timer(async x => await _todo(x, Context.User.Id, index), null, 0, reminder.Duration * 1000);
        }

        private async Task _todo(object x, ulong userId, int index)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            RedisUtility utility = new RedisUtility(Program.RedisDb);
            var row = await utility.DbGetAsync<Database.Table.Reminder>($"{userId}:{index}");
            var channel = await _client.GetDMChannelAsync(row.UserId);
            await channel.SendMessageAsync(row.Content);
            await utility.DbDelAsync<Database.Table.Reminder>($"{userId}:{index}");
        }
    }
}
