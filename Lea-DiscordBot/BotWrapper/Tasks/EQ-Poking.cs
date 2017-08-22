using System;
using System.Collections.Generic;
using System.Threading;
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
    public class EQPoking : IDisposable
    {
        private HttpClient client;
        public Uri PokingURL { get; set; }
        private DateTime lastPokingStamp;
        private bool cancelling;
        private bool _isbusy;
        public bool IsRunning { get { return this._isbusy; } }
        private Timer myTimer;

        private static class FixedDlay
        {
            public static readonly TimeSpan WhenNormal = new TimeSpan(0, 5, 0);
            public static readonly TimeSpan WhenMaintenance = new TimeSpan(0, 10, 0);
        }

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

            this.myTimer = new Timer(new TimerCallback(Timer_Callback), null, Timeout.Infinite, this.GetFixedDelay(FixedDlay.WhenNormal.Minutes).Milliseconds);
        }

        private void Timer_Callback(object state)
        {
            Console.WriteLine(DateTime.Now.ToString());
            try
            {
                bool isInMaintenance = false;
                var theTask = this.InnerPoking();
                var internetResource = theTask.GetAwaiter().GetResult();
                if (this.cancelling)
                    return;
                if (internetResource != null && internetResource.Count > 0)
                {
                    isInMaintenance = (bool)internetResource["Maintenance"];
                    if (!isInMaintenance)
                        this.EQDataChanged?.Invoke(this.MakeEmbed(internetResource));
                }
                if (this.cancelling)
                    return;
                if (!isInMaintenance)
                    this.myTimer.Change(FixedDlay.WhenNormal, TimeSpan.Zero);
                else
                    this.myTimer.Change(FixedDlay.WhenMaintenance, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void StartPoking()
        {
            if (this.IsRunning)
                throw new InvalidOperationException();
            this._isbusy = true;
            this.cancelling = false;

            this.myTimer.Change(0, this.GetFixedDelay(FixedDlay.WhenNormal.Minutes).Milliseconds);

            Console.WriteLine("Started EQ poking.");
        }

        public event Func<EQPostBlock, Task> EQDataChanged;

        public void CancelTask()
        {
            this.cancelling = true;
            this._isbusy = false;
            this.myTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task<Dictionary<string, object>> InnerPoking()
        {
            Dictionary<string, object> result = null;
            // Imo, a WebSocket for this thing, for a SocketIO should be nice.
            Task<HttpResponseMessage> pokingresult = this.client.GetAsync(this.PokingURL, HttpCompletionOption.ResponseHeadersRead);
            HttpResponseMessage rep = await pokingresult;
            if (pokingresult.IsFaulted)
                throw pokingresult.Exception;
            else if (pokingresult.IsCanceled)
            {
                await Task.Delay(1000);
                return await this.InnerPoking();
            }
            else if (pokingresult.IsCompleted)
            {
                // HttpResponseMessage rep = pokingresult.Result;

                if (rep.IsSuccessStatusCode)
                {
                    if (rep.Content.Headers.LastModified.HasValue)
                    {
                        if (rep.Content.Headers.LastModified.Value.UtcDateTime != this.lastPokingStamp)
                        {
                            this.lastPokingStamp = rep.Content.Headers.LastModified.Value.UtcDateTime;
                            // We may not even need the JObject
                            using (var stream = await rep.Content.ReadAsStreamAsync())
                                result = this.FlattenJson(stream);
                            /* bool hasAnyValue = flattenJson
                                .Where((_keypair)=> { return (_keypair.Key != "JST" && _keypair.Key != "Maintenance"); })
                                .Any((_keypair) => { return (_keypair.Value != null); });//*/
                        }
                    }
                    else
                        using (var stream = await rep.Content.ReadAsStreamAsync())
                            result = this.FlattenJson(stream);
                }
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
            if (hour <= 0)
                return string.Empty;
            else
                return string.Format(":clock{0}:", hour);
        }

        private TimeSpan GetFixedDelay(int fixedMin)
        {
            DateTime current = DateTime.Now;
            int solan = current.Minute / fixedMin;
            int result = solan * fixedMin;
            if ((solan * fixedMin) <= current.Minute)
                return new TimeSpan(0, ((solan + 1) * fixedMin) - current.Minute, 0);
            else
                return new TimeSpan(0, result - current.Minute, 0);
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

        public void Dispose()
        {
            if (this.myTimer != null)
                this.myTimer.Dispose();
            if (this.client != null)
                this.client.Dispose();
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
