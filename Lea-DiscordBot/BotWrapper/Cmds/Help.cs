using Discord.WebSocket;
using System.Threading.Tasks;
using System.Text;
using Discord;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class Help
    {
        private class aaaaa
        {
            public string Message { get; }
            public Embed Data { get; }
            internal aaaaa(string str, Embed _data)
            {
                this.Message = str;
                this.Data = _data;
            }
        }
        private static aaaaa theHelp;

        internal static void BuildHelp(string cmdPrefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```");

            sb.AppendLine(cmdPrefix + "help - It's what you're looking at, mate.");
            sb.AppendLine(cmdPrefix + "info - Introduce myself.");
            sb.AppendLine(cmdPrefix + "error - Explain about PSO2's error code.");

            sb.Append("```");

            // Unconfigured modules
            EmbedBuilder ebuilder = new EmbedBuilder();
            ebuilder.Title = "Functions that u can't do anything but ignore it";
            ebuilder.AddInlineField(":alarm_clock: EQ Alert", "throw upcoming EQ infos to #eq-alert channel.");
            theHelp = new aaaaa(sb.ToString(), ebuilder.Build());
            sb.Clear();
        }

        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message)
        {
            await message.Channel.SendMessageAsync(theHelp.Message, false, theHelp.Data);

            /*var dmChannel = await message.Author.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync(sb.ToString(), false, ebuilder.Build());
            await dmChannel.CloseAsync();//*/
        }
    }
}
