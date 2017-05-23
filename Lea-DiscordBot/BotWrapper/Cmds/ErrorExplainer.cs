using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class ErrorExplainer
    {
        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message, string cmdPrefix, params string[] splittedMsg)
        {
            if (splittedMsg.Length > 1)
            {
                for (int i = 1; i < splittedMsg.Length; i++)
                    if (Leayal.NumberHelper.TryParse(splittedMsg[i], out var theCode))
                    {
                        var hohoho = GetErrorExplaination(theCode);
                        EmbedBuilder errorExplainationbuilder = new EmbedBuilder()
                        {
                            Description = hohoho.Explaination,
                            Title = string.Format("Error Code: {0}", hohoho.ErrorCode)
                        };
                        await message.Channel.SendMessageAsync("", false, errorExplainationbuilder.Build());
                    }
            }
            else
            {
                await message.Channel.SendMessageAsync("```\n" + 
                    "Usage:\n" +
                    cmdPrefix + "error [errorCode1] [errorCode2]\n" + 
                    "Ex:" + cmdPrefix + "error 630 816\n" +
                    "   " + cmdPrefix + "error 817\n" +
                    "```");
            }
        }

        struct ErrorExplaination
        {
            public string Explaination { get; }
            public string ErrorCode { get; }

            public ErrorExplaination(string code, string explain)
            {
                this.Explaination = explain;
                this.ErrorCode = code;
            }
        }

        private static ErrorExplaination GetErrorExplaination(int errorCode)
        {
            switch (errorCode)
            {
                case 630:
                    return new ErrorExplaination(errorCode.ToString(), "This mean your connection to the game server has been terminated due to some certain reasons.");
                case 816:
                    return new ErrorExplaination(errorCode.ToString(), "This mean you have been temporarily banned. Either contact SEGA or waiting for miracle. Wish best luck to you~!");
                case 817:
                    return new ErrorExplaination(errorCode.ToString(), "This mean you have been permanent banned. Dudu once said: \"Your luck is extraordinary terrible\".");
                default:
                    if (errorCode > 0)
                    {
                        if (errorCode < 100) return new ErrorExplaination("Unknown", "Unknown.");
                        else if (errorCode < 200) return new ErrorExplaination("1xx", "Unknown.");
                        else if (errorCode < 300) return new ErrorExplaination("2xx", "Unknown.");
                        else if (errorCode < 400) return new ErrorExplaination("3xx", "Unknown.");
                        else if (errorCode < 500) return new ErrorExplaination("4xx", "Unknown.");
                        else if (errorCode < 600) return new ErrorExplaination("5xx", "Unknown.");
                        else if (errorCode < 700)
                            return new ErrorExplaination("6xx", "The error 6xx seem to be related to connection issues.");
                        else if (errorCode < 800) return new ErrorExplaination("7xx", "Unknown.");
                        else if (errorCode < 900)
                            return new ErrorExplaination("8xx", "The error 8xx seem to be related to account issues.");
                        else
                            return new ErrorExplaination(errorCode.ToString(), "This error code seem to be invalid .....");
                    }
                    else
                        return new ErrorExplaination("Unknown", "This error code seem to be invalid .....");
            }
        }
    }
}
