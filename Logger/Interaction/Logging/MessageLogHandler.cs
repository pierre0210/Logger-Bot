using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger.Database.Table;

namespace Logger.Interaction.Logging
{
	public class MessageLogHandler
	{
		private readonly DiscordSocketClient _client;

		public MessageLogHandler(DiscordSocketClient client)
		{
			_client = client;
		}

		public async Task LogDeleteMessage(Cacheable<IMessage, ulong> msgCache, Cacheable<IMessageChannel, ulong> channelCache)
		{
			if(!msgCache.HasValue) return;
			IMessage msg = msgCache.Value;
			IMessageChannel msgChannel = channelCache.Value;
			
			RedisUtility utility = new RedisUtility(Program.RedisDb);
			var guildChannel = msgChannel as IGuildChannel;
			var row = await utility.DbGetAsync<GuildInfo>($"{guildChannel.GuildId}");
			if (row != null && msgChannel.Id != row.LogChannelId)
			{
				if (row.MessageLog)
				{
					if(msg.Content.Length > 0)
					{
						var textEmbed = new EmbedBuilder().WithTitle($"Message deleted in #{guildChannel.Name}").WithColor(Color.DarkRed)
							.WithAuthor(msg.Author).WithDescription(msg.Content).WithTimestamp(DateTime.Now).WithFooter($"ID: {msg.Author.Id}");
						await _client.GetGuild(guildChannel.GuildId).GetTextChannel(row.LogChannelId).SendMessageAsync(embed: textEmbed.Build());
					}
					
					if(msg.Attachments != null)
					{
						using(var http = new HttpClient())
						{
							foreach(var file in msg.Attachments)
							{
								var uri = new Uri(file.Url);
								var response = await http.GetStreamAsync(uri);
								string fileName = file.Filename;
								string filePath = Path.Join(Program.TempFolderPath, fileName);
								int fileSize = file.Size;

								var fileEmbed = new EmbedBuilder().WithTitle($"Attachment deleted in #{guildChannel.Name}").WithColor(Color.DarkRed)
									.WithAuthor(msg.Author).WithDescription($"File Name: {fileName}\nFile size: {fileSize} Bytes").WithTimestamp(DateTime.Now).WithFooter($"ID: {msg.Author.Id}");
								
								MemoryStream ms = new MemoryStream();
								await response.CopyToAsync(ms);
								await _client.GetGuild(guildChannel.GuildId)
									.GetTextChannel(row.LogChannelId)
									.SendFileAsync(new FileAttachment(ms, fileName), text: fileName, embed: fileEmbed.Build());
/*
								using (var fs = new FileStream(filePath, FileMode.Create))
								{
									await response.CopyToAsync(fs);
									await _client.GetGuild(guildChannel.GuildId)
										.GetTextChannel(row.LogChannelId)
										.SendFileAsync(text: fileName, filePath: filePath, embed: fileEmbed.Build());
									//File.Delete(filePath);
									await fs.FlushAsync();
								}
*/
							}
						}
					}
				}
			}
		}

		public async Task LogUpdateMessage(Cacheable<IMessage, ulong> msgCache, SocketMessage msg, ISocketMessageChannel channel)
		{
			if(!msgCache.HasValue) return;
			IMessage oldMsg = msgCache.Value;
			var guildChannel = channel as IGuildChannel;

			RedisUtility utility = new RedisUtility(Program.RedisDb);
			var row = await utility.DbGetAsync<GuildInfo>($"{guildChannel.GuildId}");

			if(row != null && channel.Id != row.LogChannelId && !oldMsg.Content.Equals(msg.Content))
			{
				if(row.MessageLog)
				{
					if(msg.Content.Length > 0)
					{
						var textEmbed = new EmbedBuilder().WithTitle($"Message edited in #{guildChannel.Name}").WithColor(Color.DarkRed)
							.WithAuthor(msg.Author).WithDescription($"❎ {oldMsg.Content}\n✅ {msg.Content}")
							.WithTimestamp(DateTime.Now).WithFooter($"ID: {msg.Author.Id}");
						await _client.GetGuild(guildChannel.GuildId).GetTextChannel(row.LogChannelId).SendMessageAsync(embed: textEmbed.Build());
					}
				}
			}
		}
	}
}
