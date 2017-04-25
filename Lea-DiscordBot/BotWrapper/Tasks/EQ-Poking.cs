using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord;
using System.Linq;

namespace LeaDiscordBot.BotWrapper.Tasks
{
    /// <summary>
    /// This class is for the game Phantasy Star Online 2's EQ Notification. If you don't know about the game or no need to use this. You can use the bot settings to disable.
    /// </summary>
    public class EQPoking
    {
        private HttpClient client;
        public Uri PokingURL { get; set; }
        private DateTime lastPokingStamp;
        private bool cancelling;
        private bool _isbusy;
        public bool IsRunning { get { return this._isbusy; } }

        public EQPoking()
        {
            this._isbusy = false;
            this.client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                Proxy = null,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });
            this.PokingURL = new Uri("https://pso2.acf.me.uk/api/eq.json");
            this.client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoCache = true
            };
            this.client.DefaultRequestHeaders.UserAgent.ParseAdd("KetgirlsOnryWithPermission");
            this.lastPokingStamp = DateTime.MinValue;
        }

        public async Task StartPoking()
        {
            if (this.IsRunning)
                throw new InvalidOperationException();
            this._isbusy = true;
            this.cancelling = false;
            await Task.Factory.StartNew(async delegate {
                while (!this.cancelling)
                {
                    var internetResource = await this.InnerPoking();
                    if (this.cancelling)
                        break;
                    if (internetResource != null)
                    {
                        bool isInMaintenance = (bool)internetResource["Maintenance"];
                        if (!isInMaintenance)
                            this.EQDataChanged?.Invoke(this.MakeEmbed(internetResource));
                    }
                    if (this.cancelling)
                        break;
                    await Task.Delay(this.GetFixedDelay());
                }
                this._isbusy = false;
            }, TaskCreationOptions.LongRunning);
            Console.WriteLine("Started EQ poking.");
        }

        public event Func<EQPostBlock, Task> EQDataChanged;

        public void CancelTask()
        {
            this.cancelling = true;
        }

        private async Task<Dictionary<string, object>> InnerPoking()
        {
            Dictionary<string, object> result = null;
            using (var pokingresult = await this.client.GetAsync(this.PokingURL, HttpCompletionOption.ResponseHeadersRead))
                if (pokingresult.IsSuccessStatusCode)
                {
                    if (pokingresult.Content.Headers.LastModified.HasValue)
                    {
                        if (pokingresult.Content.Headers.LastModified.Value.UtcDateTime != this.lastPokingStamp)
                        {
                            this.lastPokingStamp = pokingresult.Content.Headers.LastModified.Value.UtcDateTime;
                            // We may not even need the JObject
                            using (var stream = await pokingresult.Content.ReadAsStreamAsync())
                                result = this.FlattenJson(stream);
                            /* bool hasAnyValue = flattenJson
                                .Where((_keypair)=> { return (_keypair.Key != "JST" && _keypair.Key != "Maintenance"); })
                                .Any((_keypair) => { return (_keypair.Value != null); });//*/
                        }
                    }
                    else
                        using (var stream = await pokingresult.Content.ReadAsStreamAsync())
                            result = this.FlattenJson(stream);
                }
            return result;
        }

        private EQPostBlock MakeEmbed(Dictionary<string, object> rawlist)
        {
            //EmbedBuilder budiler = new EmbedBuilder();
            // Let's hope the markdown work here
            StringBuilder sb = new StringBuilder();
            string value;
            // OK, rewrite this, or not
            int offsetHeader = 0;
            value = rawlist["Now"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendFormat("\n:on: Currently on going: ```{0}```", value);
                offsetHeader = sb.Length;
            }
            DateTime anotherCurrentDateTime = DateTime.UtcNow;
            // 00:30m
            value = rawlist["HalfHour"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddMinutes(30d);
                sb.AppendFormat("\n:soon: In half hour: `{0}`", value);
            }
            // 01:00m
            value = rawlist["OneLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(1d);
                sb.AppendFormat("\n{0} In one hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }
            // 01:30m
            value = rawlist["OneHalfLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(1d);
                anotherCurrentDateTime.AddMinutes(30d);
                sb.AppendFormat("\n{0} In one and half hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }
            // 02:00m
            value = rawlist["TwoLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(2d);
                sb.AppendFormat("\n{0} In two hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }
            // 02:30m
            value = rawlist["TwoHalfLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(2d);
                anotherCurrentDateTime.AddMinutes(30d);
                sb.AppendFormat("\n{0} In two and half hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }
            // 03:00m
            value = rawlist["ThreeLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(3d);
                sb.AppendFormat("\n{0} In three hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }
            // 03:30m
            value = rawlist["ThreeHalfLater"] as string;
            if (!string.IsNullOrEmpty(value))
            {
                anotherCurrentDateTime.AddHours(3d);
                anotherCurrentDateTime.AddMinutes(30d);
                sb.AppendFormat("\n{0} In three and half hour: `{1}`", this.GetClockEmoji(anotherCurrentDateTime), value);
            }

            // JST mean next nearest EQ will gonna happen at

            if (!string.IsNullOrWhiteSpace(sb.ToString()) && offsetHeader < sb.Length)
            {
                if (offsetHeader == 0)
                    sb.Insert(0, "Up coming EQ for all ships:");
                else
                    sb.Insert(offsetHeader, "\nUp coming EQ for all ships:");
            }
            else
            {
                bool firstline = true;
                for (int i = 1; i <= 10; i++)
                {
                    value = rawlist["Ship" + i.ToString()] as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (firstline)
                        {
                            firstline = false;
                            sb.AppendFormat("**Ship{0}**: `{1}`", i, value);
                        }
                        else
                            sb.AppendFormat("\n**Ship{0}**: `{1}`", i, value);
                    }
                }
                if (!string.IsNullOrWhiteSpace(sb.ToString()) && !firstline)
                    sb.Insert(offsetHeader, "Up coming random EQ at " + (string)rawlist["JST"] + " JST:\n");
            }

            return new EQPostBlock(sb.ToString());
        }

        private string GetClockEmoji(DateTime dt)
        {
            int hour = dt.Hour;
            if (dt.Hour > 12)
                hour = dt.Hour - 12;
            return string.Format(":clock{0}:", hour);
        }

        private TimeSpan GetFixedDelay()
        {
            // Let's just hard-coded then
            DateTime current = DateTime.Now;
            if (current.Minute == 0)
                return new TimeSpan(0, 5, 0);
            else if (current.Minute < 5)
                return new TimeSpan(0, 5 - current.Minute, 0);
            else if (current.Minute < 10)
                return new TimeSpan(0, 10 - current.Minute, 0);
            else if (current.Minute < 15)
                return new TimeSpan(0, 15 - current.Minute, 0);
            else if (current.Minute < 20)
                return new TimeSpan(0, 20 - current.Minute, 0);
            else if (current.Minute < 25)
                return new TimeSpan(0, 25 - current.Minute, 0);
            else if (current.Minute < 30)
                return new TimeSpan(0, 30 - current.Minute, 0);
            else if (current.Minute < 35)
                return new TimeSpan(0, 35 - current.Minute, 0);
            else if (current.Minute < 40)
                return new TimeSpan(0, 40 - current.Minute, 0);
            else if (current.Minute < 45)
                return new TimeSpan(0, 45 - current.Minute, 0);
            else if (current.Minute < 50)
                return new TimeSpan(0, 50 - current.Minute, 0);
            else if (current.Minute < 55)
                return new TimeSpan(0, 55 - current.Minute, 0);
            else
                return new TimeSpan(0, 5, 0);

        }

        private Dictionary<string, object> FlattenJson(Stream jsonStream)
        {
            using (StreamReader sr = new StreamReader(jsonStream))
            using (JsonTextReader jtr = new JsonTextReader(sr))
                return this.FlattenJson(jtr);
        }

        private Dictionary<string, object> FlattenJson(string jsonContent)
        {
            using (StringReader sr = new StringReader(jsonContent))
            using (JsonTextReader jtr = new JsonTextReader(sr))
                return this.FlattenJson(jtr);
        }

        private Dictionary<string, object> FlattenJson(JsonReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            string currentPropertyname = null;
            while (reader.Read())
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    // Since this is a flat JSON, this is safe
                    currentPropertyname = reader.Value as string;
                    if (reader.Read())
                        result.Add(currentPropertyname, reader.Value);
                }
            return result;
        }

        public class EQDataChangedEventArgs : EventArgs
        {
            public EQPostBlock Data { get; }

            public EQDataChangedEventArgs(EQPostBlock _data) : base()
            {
                this.Data = _data;
            }
        }

        public class EQPostBlock
        {
            public Embed EmbedData { get; }
            public string Message { get; }
            internal EQPostBlock(string msg, Embed data)
            {
                this.Message = msg;
                this.EmbedData = data;
            }

            internal EQPostBlock(string msg) : this(msg, null) { }
        }
    }
}
