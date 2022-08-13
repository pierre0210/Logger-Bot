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
	public class Message : InteractionModuleBase<SocketInteractionContext>
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
			using(var db = new SQLiteContext())
			{
				var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
				if(row == null)
				{
					await RespondAsync(text: "此伺服器未設定紀錄頻道", ephemeral: true);
				}
				else
				{
					row.MessageLog = true;
					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", row);
					db.SaveChanges();
					await RespondAsync(text: "開啟文字記錄功能");
				}
			}
		}

		[SlashCommand("disable", "disable message logging")]
		public async Task DisableAsync()
		{
			using (var db = new SQLiteContext())
			{
				var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
				if (row == null)
				{
					await RespondAsync(text: "此伺服器未設定紀錄頻道", ephemeral: true);
				}
				else
				{
					row.MessageLog = false;
					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", row);
					db.SaveChanges();
					await RespondAsync(text: "關閉文字記錄功能");
				}
			}
		}
	}
}
