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
			await RespondAsync($"提醒 **{Context.User.Username}** {content}");
			//TimerCallback callback = new TimerCallback(_todo);
			_timer = new Timer(async x => await _todo(x, Context.User.Id, index), null, reminder.Duration * 1000, Timeout.Infinite);
		}

		[RequireOwner]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		[SlashCommand("list", "list all reminder")]
		public async Task ListAsync()
		{
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			var allkeys = Program.RedisServer.Keys(pattern: $"{typeof(Database.Table.Reminder).FullName}:{Context.User.Id}:*");
			string keyList = String.Empty;
			foreach(var key in allkeys.Reverse())
			{
				string index = key.ToString().Split(":")[2]; // type:userId:index
				var row = await utility.DbGetWithFullnameAsync<Database.Table.Reminder>(key.ToString());
				keyList += $"[{index}] {row.Content}\n";
			}
			if(keyList.Length > 0)
			{
				EmbedBuilder keyEmbed = new EmbedBuilder().WithColor(Color.DarkRed)
					.WithDescription($"```\n{keyList}```").WithAuthor(Context.User).WithTimestamp(DateTime.Now);
				await RespondAsync(embed: keyEmbed.Build());
			}
			else
			{
				EmbedBuilder keyEmbed = new EmbedBuilder().WithColor(Color.DarkRed)
					.WithDescription("**Empty**").WithAuthor(Context.User).WithTimestamp(DateTime.Now);
				await RespondAsync(embed: keyEmbed.Build());
			}
		}

		[RequireOwner]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		[SlashCommand("delete", "delete reminder")]
		public async Task DelAsync([Summary(description: "編號 可用list指令查詢")]int index)
		{
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			if(await utility.DbExistsAsync<Database.Table.Reminder>($"{Context.User.Id}:{index}"))
			{
				await utility.DbDelAsync<Database.Table.Reminder>($"{Context.User.Id}:{index}");
				await RespondAsync($"提醒(編號：{index})已刪除");
			}
			else
			{
				await RespondAsync("查無此編號");
			}
		}

		private async Task _todo(object x, ulong userId, int index)
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			if(await utility.DbExistsAsync<Database.Table.Reminder>($"{userId}:{index}"))
			{
				var row = await utility.DbGetAsync<Database.Table.Reminder>($"{userId}:{index}");
				var user = await _client.GetUserAsync(userId);
				var channel = await user.CreateDMChannelAsync();
				await channel.SendMessageAsync(row.Content);
				await utility.DbDelAsync<Database.Table.Reminder>($"{userId}:{index}");
			}
		}
	}
}
