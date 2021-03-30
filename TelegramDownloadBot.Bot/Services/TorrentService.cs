using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using MonoTorrent;
using MonoTorrent.Client;
using TelegramDownloadBot.Bot.Data;
using Microsoft.Extensions.DependencyInjection;
using Humanizer;
using Telegram.Bot.Types.InputFiles;
using System.Web;
using Transmission.API.RPC.Entity;

namespace TelegramDownloadBot.Bot.Services
{
    public class TorrentService
    {
        private readonly ILogger<TorrentService> _logger;

        private string _apiKey = "sd66az7eqokwe8pfhc59dcu1rdav7iep";

        private HttpClient _httpClient;
        private IServiceScopeFactory _scope;
        private MonoTorrent.Client.ClientEngine _engine;
        private Timer _timer;

        public TorrentService(ILogger<TorrentService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;

            _httpClient = new HttpClient();
            _scope = scopeFactory;

            _engine = new MonoTorrent.Client.ClientEngine();
            _timer = new Timer(_TimerCallback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
        }

        private async void _TimerCallback(object e)
        {
            var transmission = _scope.CreateScope().ServiceProvider.GetRequiredService<TransmissionService>().Client;
            var database = _scope.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            var telegram = _scope.CreateScope().ServiceProvider.GetRequiredService<TelegramService>().Client;

            if (database.ActiveTorrents.Count() < 1) return;

            var info = transmission.TorrentGet(TorrentFields.ALL_FIELDS, database.ActiveTorrents.Where(at => !at.IsFinished).Select(at => at.TorrentId).ToArray());


            foreach (var to in info.Torrents)
            {
                var speed = to.RateDownload.Bytes();
                Console.WriteLine($"\n~~ {to.ID} | {to.Name}\nstate\t{to.Status}\nspeed\t{Math.Round(speed.LargestWholeNumberValue, 1)} {speed.LargestWholeNumberFullWord}/s\ndone\t{to.IsFinished}\npdone\t{Math.Round(to.PercentDone, 1)}");

                if (!to.IsFinished && to.PercentDone == 100) continue;

                Console.WriteLine($"...downloading {to.Name} complete");

                var dbt = database.ActiveTorrents.FirstOrDefault(t => t.TorrentId == to.ID);
                dbt.IsFinished = true;

                database.SaveChanges();

                transmission.TorrentStop(new[] { (object)to.ID });

                foreach (var file in to.Files)
                {
                    Console.WriteLine($"...uploading {to.Name}");

                    using var fs = File.Open($"G:{Path.DirectorySeparatorChar}Projects{Path.DirectorySeparatorChar}TelegramDownloadBot{Path.DirectorySeparatorChar}transmission{Path.DirectorySeparatorChar}downloads{Path.DirectorySeparatorChar}complete{Path.DirectorySeparatorChar}{file.Name}", FileMode.Open, FileAccess.Read, FileShare.Read);
                    var onf = new InputOnlineFile(fs);

                    await telegram.SendDocumentAsync(dbt.ChatId, onf);
                    Console.WriteLine($"...uploaded {to.Name}");
                }
            }
        }



        public async Task<List<SearchResponse>> SearchShow(string show, int season, int episode)
        {
            var http = new HttpClient();
            var response = await http.GetAsync($"http://10.1.1.3:9117/api/v2.0/indexers/all/results/torznab/?apikey={_apiKey}&q={show}&season={season}&ep={episode}");

            var xmlse = new XmlSerializer(typeof(Rss));


            using var reader = new StringReader(await response.Content.ReadAsStringAsync());

            var rssResponse = (Rss)xmlse.Deserialize(reader);

            var responses = new List<SearchResponse>();


            foreach (var rs in rssResponse.Channel.Item)
            {
                uint.TryParse(rs.Attr.FirstOrDefault(s => s.Name == "seeders").Value, out var seeders);
                uint.TryParse(rs.Attr.FirstOrDefault(s => s.Name == "peers").Value, out var peers);
                // uint.TryParse(rs.Attr.FirstOrDefault(s => s.Name == "size").Value, out var size);

                using var hasher = new SHA1Managed();

                var hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rs.Title + rs.Size));

                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                if (!rs.Enclosure.Url.Contains("magnet")) continue;

                var guid = sb.ToString();

                var sr = new SearchResponse()
                {
                    Size = rs.Size,
                    Title = rs.Title,
                    Seeders = seeders,
                    Peers = peers,
                    MagnetUrl = rs.Enclosure.Url,
                    Guid = guid
                };

                responses.Add(sr);
            }

            return await Task.FromResult(responses);
        }
        public async void Download(SearchResponse sr, long chatId)
        {
            System.Console.WriteLine($"downloading {sr.Title}...");


            //services
            var transmission = _scope.CreateScope().ServiceProvider.GetRequiredService<TransmissionService>().Client;
            var database = _scope.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();

            System.Console.WriteLine(Directory.GetCurrentDirectory());

            var torrentSettings = new TorrentSettings();

            NewTorrent torrent;

            System.Console.WriteLine("trying with magnet");
            var metadata = await _engine.DownloadMetadataAsync(MagnetLink.Parse(sr.MagnetUrl), CancellationToken.None);
            System.Console.WriteLine("downloaded metadata");

            torrent = new NewTorrent()
            {
                Metainfo = Convert.ToBase64String(metadata)
            };


            var torrentInfo = transmission.TorrentAdd(torrent);

            if (database.ActiveTorrents.Any(at => at.TorrentId == torrentInfo.ID)) return;

            var activeTorrent = new ActiveTorrent()
            {
                ChatId = chatId,
                TorrentId = torrentInfo.ID,
                IsFinished = false,
            };

            database.ActiveTorrents.Add(activeTorrent);
            database.SaveChanges();
        }
    }
}