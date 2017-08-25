using System;
using System.Text;
using System.Threading.Tasks;

namespace LeaDiscordBot
{
    class Program
    {
        public static Microsoft.IO.RecyclableMemoryStreamManager memoryMgr = new Microsoft.IO.RecyclableMemoryStreamManager();
        private static BotWrapper.Bot myBot;
        private static Leayal.Ini.IniFile _configFile;
        internal static Leayal.Ini.IniFile ConfigFile
        {
            get
            {
                // Use current WORKING directory
                if (_configFile == null)
                    _configFile = new Leayal.Ini.IniFile(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "bot.ini"));
                return _configFile;
            }
        }

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            ShowMainMenu();
        }

        public static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("Hello User~! I am Lea-DiscordBot. I am awaiting for your order:");
            Console.WriteLine("1. Launch DiscordBot");
            Console.WriteLine("2. Settings");
            Console.WriteLine();
            Console.WriteLine("0. Quit");
            Console.Write("Press the number without pressing enter: ");
            ConsoleKeyInfo ki = AwaitingOrder(false, '0', '1', '2');
            switch (ki.KeyChar)
            {
                case '1':
                    // Launch Bot
                    Console.Clear();
                    try
                    {
                        string myKey = ConfigFile.GetValue("Bot", "Token", string.Empty);
                        if (string.IsNullOrWhiteSpace(myKey))
                            throw new ArgumentNullException("You haven't set the BOT token.");
                        else
                        {
                            string cmdPrefix = ConfigFile.GetValue("Bot", "CommandPrefix", string.Empty);
                            string launchEQPoking = ConfigFile.GetValue("Bot", "LaunchEQAfterLogin", string.Empty);
                            if (string.IsNullOrWhiteSpace(launchEQPoking))
                                launchEQPoking = "0";
                            if (string.IsNullOrWhiteSpace(cmdPrefix))
                                cmdPrefix = "'";
                            MainAsync(cmdPrefix, myKey, !Leayal.StringHelper.IsEqual(launchEQPoking, "0")).GetAwaiter().GetResult();
                            BlockExit(false);
                            Logout();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("Press any key to close");
                        AwaitingOrder(false, null);
                    }
                    break;
                case '2':
                    // Launch Settings
                    LaunchConfigPanel();
                    break;
                default:
                    Console.Clear();
                    Environment.Exit(0);
                    return;
            }
        }

        private static void BlockExit(bool intercept)
        {
            ConsoleKeyInfo theKey = Console.ReadKey(intercept);
            if (theKey.Key != ConsoleKey.Escape)
                BlockExit(intercept);
        }

        private static void LaunchConfigPanel()
        {
            Console.Clear();
            Console.WriteLine("Choose setting:");
            Console.WriteLine("1. Set Bot token");
            Console.WriteLine("2. Set Bot prefix");
            Console.WriteLine("3. Change. Launch EQ Alert along with the bot???");
            Console.WriteLine("4. Set EQ Server for the bot to leech or crawl or laihgliawhgliawhgilahwglih.");
            Console.WriteLine("5. Set UserAgent will be used for EQ's requests.");
            Console.WriteLine("6. Set Username will be used for EQ's requests.");
            Console.WriteLine("7. Set Password will be used for EQ's requests.");
            Console.WriteLine("8. Set Credits line EQ's notification.");
            Console.WriteLine();
            Console.WriteLine("0. Back to main menu");
            Console.Write("Press the number without pressing enter: ");
            ConsoleKeyInfo ki = AwaitingOrder(false, '0', '1', '2', '3', '4', '5', '6', '7', '8');
            switch (ki.KeyChar)
            {
                case '0':
                    ShowMainMenu();
                    break;
                case '1':
                    Console.Clear();
                    Console.WriteLine("Please leave the Bot's token below and then press Enter to confirm... or press Esc to cancel:");
                    //Tricky ???
                    StringBuilder sb1 = new StringBuilder();
                    if (ConsoleReadline(sb1) != null)
                    {
                        ConfigFile.SetValue("Bot", "Token", sb1.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Token settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '3':
                    Console.Clear();
                    Console.WriteLine("Please Press 1 (True) or 0 (False)... or press Esc to cancel:");
                    var key = AwaitingOrder(false, '0', '1');
                    if (key.KeyChar == '0')
                    {
                        ConfigFile.SetValue("Bot", "LaunchEQAfterLogin", new string(key.KeyChar, 1));
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        ConfigFile.SetValue("Bot", "LaunchEQAfterLogin", new string(key.KeyChar, 1));
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '2':
                    Console.Clear();
                    Console.WriteLine("Please leave the string below and then press Enter to confirm... or press Esc to cancel:");
                    //Tricky ???
                    StringBuilder sb3 = new StringBuilder();
                    if (ConsoleReadline(sb3) != null)
                    {
                        ConfigFile.SetValue("Bot", "CommandPrefix", sb3.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Command prefix settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '4':
                    Console.Clear();
                    Console.WriteLine("If you don't own the the EQ Server or use someone. Put Credits (Please read here).");
                    Console.WriteLine("Please leave the Bot's EQ server url below and then press Enter to confirm... or press Esc to cancel:");
                    //Tricky ???
                    StringBuilder sb4 = new StringBuilder();
                    if (ConsoleReadline(sb4) != null)
                    {
                        ConfigFile.SetValue("EQ", "Server", sb4.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("EQ Server settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '5':
                    Console.Clear();
                    Console.WriteLine("Please leave the Bot's useragent below and then press Enter to confirm... or press Esc to cancel. Whitespace to remove:");
                    //Tricky ???
                    StringBuilder sb5 = new StringBuilder();
                    if (ConsoleReadline(sb5) != null)
                    {
                        ConfigFile.SetValue("EQ", "UserAgent", sb5.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("EQ Server settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '6':
                    Console.Clear();
                    Console.WriteLine("Please leave the Bot's Authorize info (Username) below and then press Enter to confirm... or press Esc to cancel. Whitespace to remove:");
                    //Tricky ???
                    StringBuilder sb6 = new StringBuilder();
                    if (ConsoleReadline(sb6) != null)
                    {
                        ConfigFile.SetValue("EQ", "Username", sb6.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("EQ Server settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '7':
                    Console.Clear();
                    Console.WriteLine("Please leave the Bot's Authorize info (Password) below and then press Enter to confirm... or press Esc to cancel. Whitespace to remove:");
                    //Tricky ???
                    StringBuilder sb7 = new StringBuilder();
                    if (ConsoleReadline(sb7) != null)
                    {
                        ConfigFile.SetValue("EQ", "Password", sb7.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("EQ Server settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
                case '8':
                    Console.Clear();
                    Console.WriteLine("If you don't own the the EQ Server or use someone. Put Credits.");
                    Console.WriteLine("Please leave the credits message below and then press Enter to confirm... or press Esc to cancel. Leave whitespace to remove:");
                    //Tricky ???
                    StringBuilder sb8 = new StringBuilder();
                    if (ConsoleReadline(sb8) != null)
                    {
                        ConfigFile.SetValue("EQ", "CreditMessage", sb8.ToString());
                        ConfigFile.Save();
                        Console.Clear();
                        Console.WriteLine("Config saved.");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("EQ Server settings cancelled");
                        Console.WriteLine("Press any key to go back");
                        AwaitingOrder(false, null);
                        LaunchConfigPanel();
                    }
                    break;
            }
        }

        private static StringBuilder ConsoleReadline(StringBuilder sb)
        {
            var pressedKey = Console.ReadKey(false);
            switch(pressedKey.Key)
            {
                case ConsoleKey.Enter:
                    return sb;
                case ConsoleKey.Escape:
                    return null;
                default:
                    sb.Append(pressedKey.KeyChar);
                    return ConsoleReadline(sb);
            }

        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Logout();
        }

        private static async Task MainAsync(string cmdPrefix, string token, bool launchEQPoke)
        {
            myBot = new BotWrapper.Bot(cmdPrefix, launchEQPoke);
            await myBot.Login(token);
        }

        private static void Logout()
        {
            myBot.Logout().GetAwaiter().GetResult();
        }

        private static ConsoleKeyInfo AwaitingOrder(bool intercept, params char[] allowedChars)
        {
            ConsoleKeyInfo theKey = Console.ReadKey(intercept);
            if (allowedChars != null && allowedChars.Length > 0)
            {
                for (int i = 0; i < allowedChars.Length; i++)
                    if (allowedChars[i] == theKey.KeyChar)
                        return theKey;
                return AwaitingOrder(intercept, allowedChars);
            }
            else
                return theKey;
        }
    }
}