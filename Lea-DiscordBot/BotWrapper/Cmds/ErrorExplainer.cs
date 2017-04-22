using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class ErrorExplainer
    {
        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message, params string[] splittedMsg)
        {
            if (splittedMsg.Length > 1)
                for (int i = 1; i < splittedMsg.Length; i++)
                    if (Leayal.NumberHelper.TryParse(splittedMsg[i], out var theCode))
                    {
                        EmbedBuilder errorExplainationbuilder = new EmbedBuilder()
                        {
                            Description = GetErrorExplaination(theCode),
                            Title = string.Format("Error Code: {0}", theCode)
                        };
                        await message.Channel.SendMessageAsync("", false, errorExplainationbuilder.Build());
                    }
        }

        private static string GetErrorExplaination(int errorCode)
        {
            switch (errorCode)
            {
                case 630:
                    return "This mean your connection to the game server has been terminated due to some certain reasons.";
                case 816:
                    return "This mean you have been temporarily banned. Either contact SEGA or waiting for miracle. Wish best luck to you~!";
                case 817:
                    return "This mean you have been permanent banned. Dudu once said: \"Your luck is extraordinary terrible\".";
                default:
                    return "Unknown error or laiwhgliawgilawhg";
            }
        }
    }
}
