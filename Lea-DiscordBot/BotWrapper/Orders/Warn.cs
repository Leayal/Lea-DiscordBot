using System;
using Microsoft.IO;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Drawing;
using System.Linq;

namespace LeaDiscordBot.BotWrapper.Orders
{
    public static class Warn
    {
        public static async Task ProcessMessage(Bot self, SocketMessage message, string msg = null)
        {
            string rawfilepath = Path.Combine("Images", "warn_raw.jpg");
            if (File.Exists(rawfilepath))
                using (RecyclableMemoryStream memStream = new RecyclableMemoryStream(Program.memoryMgr))
                {
                    using (FileStream fs = File.OpenRead(rawfilepath))
                        fs.CopyTo(memStream);
                    StringBuilder sb = new StringBuilder();
                    bool isFirstItem = true;
                    if ()
                    foreach (var mentioned in message.MentionedUsers.Where((user) =>
                    {
                        return (!user.IsBot && user.Id != self.Client.CurrentUser.Id);
                    }))
                    {
                        if (isFirstItem)
                        {
                            isFirstItem = false;
                            sb.Append(mentioned.Username);
                        }
                        else
                            sb.AppendFormat(", {0}", mentioned.Username);
                    }
                    if (string.IsNullOrWhiteSpace(sb.ToString()))
                        sb.Append("Give me a target~!");
                    else
                    {
                        sb.Insert(0, "I warn you\n");
                    }
                    using (Image myBitMap = Bitmap.FromStream(memStream))
                    using (Graphics gr = Graphics.FromImage(myBitMap))
                    using (Font font = new Font("sans-serif", 15))
                    using (RecyclableMemoryStream output = new RecyclableMemoryStream(Program.memoryMgr))
                    {
                        gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        gr.DrawString(sb.ToString(), font, Brushes.Black, new RectangleF(new PointF(229, 401), new SizeF(230, 175)));

                        myBitMap.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                        output.Position = 0;
                        await message.Channel.SendFileAsync(output, "warning.jpg");
                    }
                }
            else
            {
                Console.WriteLine(string.Format("The raw image '{0}' is not found", rawfilepath));
            }
        }
    }
}
