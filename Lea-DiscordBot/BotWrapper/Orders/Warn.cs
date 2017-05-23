using System;
using Microsoft.IO;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using ImageSharp;
using ImageSharp.Drawing;
using SixLabors.Fonts;
using System.Linq;

namespace LeaDiscordBot.BotWrapper.Orders
{
    public static class Warn
    {
        private static FontCollection fonts;
        private static FontFamily fontFamily;
        private static Font font;
        /// <summary>
        /// Why it's so python-alike?????
        /// </summary>
        /// <param name="self">No explain, it's the bot</param>
        /// <param name="message">The message received from chat</param>
        /// <returns></returns>
        public static async Task ProcessMessage(Bot self, SocketMessage message)
        {
            if (fonts == null)
            {
                fonts = new FontCollection();
                using (FileStream fs = File.OpenRead("Times_New_Roman.ttf"))
                    fonts.Install(fs);
                fontFamily = fonts.Families.First();
                font = new Font(fontFamily, 15F);
            }

            string rawfilepath = Path.Combine("Images", "warn_raw.png");
            if (File.Exists(rawfilepath))
                using (RecyclableMemoryStream memStream = new RecyclableMemoryStream(Program.memoryMgr))
                {
                    StringBuilder sb = new StringBuilder();
                    bool isFirstItem = true;
                    //if (self.CommandCooldown)
                    foreach (var mentioned in message.MentionedUsers.Where((user) =>
                    {
                        return (!user.IsBot && user.Id != self.Client.CurrentUser.Id);
                    }))
                    {
                        if (mentioned.Id == message.Author.Id)
                        {
                            sb.AppendFormat("Why would you want to warn yourself...? Are you a masochist, {0}????", mentioned.Username);
                            break;
                        }
                        else
                        {
                            if (isFirstItem)
                            {
                                isFirstItem = false;
                                sb.Append(mentioned.Username);
                            }
                            else
                                sb.AppendFormat(", {0}", mentioned.Username);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(sb.ToString()))
                        sb.Append("Give me a target~!");
                    else
                    {
                        sb.Insert(0, "I warn you\n");
                    }
                    Console.WriteLine(sb.ToString());
                    using (Image<Rgba32> image = Image.Load(rawfilepath))
                    {
                        image.DrawText(sb.ToString(), font, Rgba32.Black, new System.Numerics.Vector2(229, 401), new TextGraphicsOptions(true) { WrapTextWidth = 230, TextAlignment = TextAlignment.Left })
                            .Save(memStream); // automatic encoder selected based on extension.
                        memStream.Position = 0;
                        await message.Channel.SendFileAsync(memStream, "warning.png");
                    }
                }
            else
            {
                Console.WriteLine(string.Format("The raw image '{0}' is not found", rawfilepath));
            }
        }
    }
}
