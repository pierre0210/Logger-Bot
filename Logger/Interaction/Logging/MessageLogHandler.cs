using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
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
            IMessage msg = msgCache.Value;
            IMessageChannel msgChannel = channelCache.Value;
            
            RedisUtility utility = new RedisUtility(Program.RedisDb);
            var guildChannel = msgChannel as IGuildChannel;
            var row = utility.DbGetAsync<GuildInfo>($"{guildChannel.GuildId}");
            if (row != null && msgChannel.Id != row.Result.LogChannelId)
            {
                if (row.Result.MessageLog)
                {
                    var logEmbed = new EmbedBuilder().WithTitle($"Message deleted in #{guildChannel.Name}").WithColor(Color.DarkRed)
                    .WithAuthor(msg.Author).WithDescription(msg.Content).WithTimestamp(DateTime.Now).WithFooter($"ID: {msg.Author.Id}");
                    await _client.GetGuild(guildChannel.GuildId).GetTextChannel(row.Result.LogChannelId).SendMessageAsync(embed: logEmbed.Build());
                }
            }
        }
    }
}
