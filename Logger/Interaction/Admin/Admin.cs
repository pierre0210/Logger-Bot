using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Logger.Database;
using Logger.Database.Table;

namespace Logger.Interaction.Admin
{
	[DefaultMemberPermissions(GuildPermission.Administrator)]
	[Group("admin", "admin only command")]
	public class Admin : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly DiscordSocketClient _client;
		private readonly IServiceProvider _service;
		private readonly InteractionService _interaction;

		public Admin(DiscordSocketClient client, IServiceProvider service, InteractionService interaction)
		{
			_client = client;
			_service = service;
			_interaction = interaction;
		}

		[SlashCommand("set-log-channel", "設定記錄頻道")]
		public async Task SetLogChannelAsync([Summary(description: "logging channel")] IChannel logChannel)
		{
			using(var db = new SQLiteContext())
			{
				var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
				if(row == null)
				{
					GuildInfo info = new GuildInfo();
					info.GuildId = Context.Guild.Id;
					info.LogChannelId = logChannel.Id;

					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", info);
					db.GuildInfos.Add(info);
					db.SaveChanges();
				}
				else
				{
					row.LogChannelId = logChannel.Id;
					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", row);
					db.SaveChanges();
				}
				
				//await new RedisUtility(Program.RedisDb).DbSetAsync<GuildInfo>(key.ToString(), info);
				await RespondAsync(text: "Done", ephemeral: true);
			}
		}

		[SlashCommand("set-report-channel", "設定檢舉頻道")]
		public async Task SetReportChannelAsync([Summary(description: "report channel")] IChannel reportChannel)
		{
			using (var db = new SQLiteContext())
			{
				var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
				if (row == null)
				{
					GuildInfo info = new GuildInfo();
					info.GuildId = Context.Guild.Id;
					info.ReportChannelId = reportChannel.Id;

					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", info);
					db.GuildInfos.Add(info);
					db.SaveChanges();
				}
				else
				{
					row.ReportChannelId = reportChannel.Id;
					RedisUtility utility = new RedisUtility(Program.RedisDb);
					await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
					await utility.DbSetAsync<GuildInfo>($"{Context.Guild.Id}", row);
					db.SaveChanges();
				}

				//await new RedisUtility(Program.RedisDb).DbSetAsync<GuildInfo>(key.ToString(), info);
				await RespondAsync(text: "Done", ephemeral: true);
			}
		}

		[SlashCommand("pingbonk", "BONK!")]
		public async Task PingBonkAsync(IUser victim, int times, string msg = "")
		{
			await RespondAsync(text: "Start bonking!", ephemeral: true);
			string bonk = $"<@{victim.Id}> {msg}";

			for (int i = 0; i < times; i++)
			{
				await Context.Channel.SendMessageAsync(text: bonk);
			}
		}
	}
}
