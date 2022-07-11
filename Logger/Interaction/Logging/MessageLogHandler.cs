using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction.Logging
{
    public class MessageLogHandler
    {
        private readonly DiscordSocketClient _client;

        public MessageLogHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task SaveMessage(Cacheable<IMessage, ulong> msgCache, Cacheable<IMessageChannel, ulong> channelCache)
        {
            // Redis DB
            IMessage msg = msgCache.Value;
            IMessageChannel messageChannel = channelCache.Value;
        }
    }
}
