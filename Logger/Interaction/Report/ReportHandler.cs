using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using Logger.Database;
using Logger.Database.Table;

namespace Logger.Interaction.Report
{
    public class ReportHandler
    {
        private readonly DiscordSocketClient _client;

        public ReportHandler(DiscordSocketClient client)
        {
            _client = client;
            //_client.ModalSubmitted += SendReport;
        }

        public async Task SendReport(SocketModal modal)
        {
            if (modal.Data.CustomId == "report_form")
            {
                List<SocketMessageComponentData> components = modal.Data.Components.ToList();
                string userName = components.First(x => x.CustomId == "report_user").Value;
                ulong userId = (ulong)Decimal.Parse(components.First(x => x.CustomId == "report_id").Value);
                string url = components.First(x => x.CustomId == "report_url").Value;
                string detail = components.First(x => x.CustomId == "report_detail").Value;

                string authorName = modal.User.Username;
                ulong authorId = modal.User.Id;
                string authorAvatar = modal.User.GetAvatarUrl();

                ulong guildId = (ulong)modal.GuildId;

                var messageEmbed = new EmbedBuilder()
                    .WithColor(Color.DarkRed)
                    .WithAuthor($"{authorName} ({authorId})", iconUrl: authorAvatar)
                    .WithTitle("檢舉表單")
                    //.WithDescription($"**檢舉人ID：**{authorId}")
                    .AddField("被檢舉人名稱：", $"{userName}", true).AddField("被檢舉人ID：", $"{userId}", true)
                    .AddField("檢舉事由：", detail)
                    .WithTimestamp(DateTime.Now);

                if (url.EndsWith(".jpg") || url.EndsWith(".png"))
                {
                    messageEmbed.WithImageUrl(url);
                }
                else
                {
                    messageEmbed.AddField("訊息連結：", url);
                }

                await modal.DeferAsync(ephemeral: true);

                using(var db = new SQLiteContext())
                {
                    var row = db.GuildInfos.Where(x => x.GuildId == guildId).FirstOrDefault();
                    await _client.GetGuild(guildId)
                        .GetTextChannel(row.ReportChannelId).SendMessageAsync(embed: messageEmbed.Build());
                }
            }
        }
    }
}
