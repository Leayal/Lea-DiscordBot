using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Leayal;

namespace LeaDiscordBot.BotWrapper
{
    public class Bot : System.IDisposable
    {
        public const string BotVersion = "0.0.1";
        private DiscordSocketClient client;
        private char commandPrefix;
        private ulong ownerID;

        public Bot(char cmdPrefix)
        {
            System.Console.WriteLine("Creating Bot instance");
            //this.ownerID = 1;
            this.commandPrefix = cmdPrefix;
            this.client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                MessageCacheSize = 1,
                LogLevel = Discord.LogSeverity.Error,
                HandlerTimeout = 5000,
                ConnectionTimeout = 3000,
                DefaultRetryMode = Discord.RetryMode.Retry502 | Discord.RetryMode.RetryTimeouts
            });
            this.client.LoggedIn += Client_LoggedIn;
            this.client.LoggedOut += Client_LoggedOut;
            this.client.MessageReceived += Client_MessageReceived;
            this.client.Log += Client_Log;
            //this.commander = new CommandService(new CommandServiceConfig() { CaseSensitiveCommands = false });
        }

        private async Task Client_LoggedIn()
        {
            System.Console.WriteLine("Bot logged in. Ready~!");
            var myInfo = await client.GetApplicationInfoAsync();
            System.Console.WriteLine(string.Format("Owner: {0}{1}", myInfo.Owner.Username, string.IsNullOrEmpty(myInfo.Owner.Discriminator) ? string.Empty : "#" + myInfo.Owner.Discriminator));
            this.ownerID = myInfo.Owner.Id;
            await Task.Yield();
        }

        public async Task Login(string botToken)
        {
            System.Console.WriteLine("Logging in");
            await this.client.LoginAsync(TokenType.Bot, botToken);
            await this.client.StartAsync();
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                if (message.Author != this.client.CurrentUser)
                {
                    foreach (var mentioned in message.MentionedUsers)
                        if (mentioned.Id == this.client.CurrentUser.Id)
                        {
                            await message.Channel.SendMessageAsync("Someone pinged me????");
                            break;
                        }
                    // Command type with command prefix ????
                    if (message.Content.IndexOf(this.commandPrefix) == 0)
                    {
                        string lowercontext = message.Content.ToLower();
                        // Remove the command prefix and start to parse the command
                        string[] splittedMsg = lowercontext.Remove(0, 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        // Someone ordered me to do something ???
                        switch(splittedMsg[0])
                        {
                            case "info":
                                // Introduce myself
                                EmbedBuilder ebuilder = new EmbedBuilder();
                                //ebuilder.Url = "";
                                ebuilder.Description = "Why would you want to know of me??? Just take it simple: I'm a bot.";
                                ebuilder.ThumbnailUrl = this.client.CurrentUser.GetAvatarUrl();
                                ebuilder.AddField("Version", BotVersion);
                                ebuilder.Title = "Lea-DiscordBot";
                                ebuilder.Url = "https://github.com/Leayal/Lea-DiscordBot/";
                                ebuilder.AddField("Language", ".NET Core/C#");
                                var myauthor = new EmbedAuthorBuilder();
                                var authorObject = this.client.GetUser(164090291421184002);
                                myauthor.Name = authorObject.Username;
                                myauthor.IconUrl = authorObject.GetAvatarUrl();
                                ebuilder.Author = myauthor;
                                //ebuilder.
                                await message.Channel.SendMessageAsync("", false, ebuilder.Build());
                                break;
                            case "shutdown":
                                if (message.Author.Id == this.ownerID)
                                {
#pragma warning disable 4014
                                    this.client.LogoutAsync();
#pragma warning restore 4014
                                    await this.client.StopAsync();                                    
                                    System.Environment.Exit(0);
                                }
                                return;
                            case "error":
                                if (splittedMsg.Length > 1)
                                    for (int i = 1; i < splittedMsg.Length; i++)
                                        if (NumberHelper.TryParse(splittedMsg[i], out var theCode))
                                        {
                                            EmbedBuilder errorExplainationbuilder = new EmbedBuilder()
                                            {
                                                Description = GetErrorExplaination(theCode),
                                                Title = string.Format("Error Code: {0}", theCode)
                                            };
                                            await message.Channel.SendMessageAsync("", false, errorExplainationbuilder.Build());
                                        }
                                break;
                            //case "wiki": let the wiki come later
                                
                            default:
                                System.Console.WriteLine(string.Format("'{0}' (ID: {1}) ordered '{2}' but this command is not existed.", message.Author.Username, message.Author.Id, splittedMsg[0]));
                                break;
                        }
                    }
                }
        }

        private string GetErrorExplaination(int errorCode)
        {
            switch(errorCode)
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

        private async Task Client_LoggedOut()
        {
            await Task.Yield();
            System.Environment.Exit(0);
        }

        private async Task Client_Log(LogMessage arg)
        {
            await Task.Yield();
        }

        public async Task Logout()
        {
            await this.client.LogoutAsync();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }
    }
}
