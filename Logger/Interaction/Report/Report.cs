using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction.Report
{
    public class Report : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public Report(DiscordSocketClient client)
        {
            _client = client;
        }

        [EnabledInDm(false)]
        [SlashCommand("report", "檢舉指令")]
        public async Task ReportAsync()
        {
            var mb = new ModalBuilder()
                .WithTitle("檢舉表單")
                .WithCustomId("report_form")
                .AddTextInput("被檢舉人名稱：", "report_user", TextInputStyle.Short, placeholder: "Troll#9487", required: true, maxLength: 256)
                .AddTextInput("被檢舉人ID：", "report_id", TextInputStyle.Short, placeholder: "123456789012345678", required: true, maxLength: 1024)
                .AddTextInput("訊息連結或截圖網址：", "report_url", TextInputStyle.Short, placeholder: "https://example.com/image.jpg", required: true, maxLength: 1024)
                .AddTextInput("檢舉事由：", "report_detail", TextInputStyle.Paragraph, required: true, maxLength: 1024);

            await RespondWithModalAsync(mb.Build());
        }
    }
}
