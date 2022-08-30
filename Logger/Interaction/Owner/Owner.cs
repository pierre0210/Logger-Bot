using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading;
using Logger.Database.Table;

namespace Logger.Interaction.Owner
{
	public class Owner : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly DiscordSocketClient _client;
		private static Timer _timer;

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
		public async Task UltMuteAsync(IGuildUser user, int minutes, string place = "新疆")
		{
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			BlackList blackList = new BlackList();
			blackList.UserId = user.Id;
			blackList.ReleaseTime = DateTime.Now.AddMinutes(minutes);
			if(user.VoiceChannel != null)
			{
				await user.ModifyAsync(u =>
				{
					u.Mute = true;
				});
			}
			
			if(!await utility.DbExistsAsync<BlackList>($"{user.GuildId}:{user.Id}"))
			{
				await utility.DbSetAsync($"{user.GuildId}:{user.Id}", blackList);
			}
			else
			{
				await utility.DbDelAsync<BlackList>($"{user.GuildId}:{user.Id}");
				await utility.DbSetAsync($"{user.GuildId}:{user.Id}", blackList);
			}
			var post = new EmbedBuilder().WithColor(Color.DarkRed)
				.WithDescription($"{user.Mention} 哈哈去{place} `勞改時間：{minutes} 分鐘`");
			await RespondAsync(text: "Muted", ephemeral: true);
			await Context.Channel.SendMessageAsync(embed: post.Build());
			_timer = new Timer(async x => await _unban(x, user, Context), null, minutes * 60 * 1000, Timeout.Infinite);
		}

		[RequireOwner]
		[SlashCommand("embed", "create embed message")]
		public async Task EmbedAsync(string title, string description, uint color, bool isTimestamp, IUser user)
		{
			var embed = new EmbedBuilder().WithTitle(title).WithDescription(description).WithColor(new Color(color))
				.WithAuthor(user);
			if(isTimestamp) embed.Timestamp = DateTime.Now;
			await RespondAsync(text: "Done", ephemeral: true);
			await Context.Channel.SendMessageAsync(embed: embed.Build());
		}

		private async Task _unban(object x, IGuildUser user, SocketInteractionContext context)
		{
			if (user.VoiceChannel != null)
			{
				await user.ModifyAsync(u =>
				{
					u.Mute = false;
				});
			}
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			if (await utility.DbExistsAsync<BlackList>($"{user.GuildId}:{user.Id}"))
			{
				await utility.DbDelAsync<BlackList>($"{user.GuildId}:{user.Id}");
			}

			var releasePost = new EmbedBuilder().WithColor(Color.DarkGreen)
				.WithDescription($"{user.Mention} 出獄");
			await context.Channel.SendMessageAsync(embed: releasePost.Build());
		}
	}
}
