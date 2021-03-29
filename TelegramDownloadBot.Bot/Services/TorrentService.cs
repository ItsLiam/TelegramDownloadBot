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

namespace TelegramDownloadBot.Bot.Services
{
    public class TorrentService
    {
        private readonly ILogger<TorrentService> _logger;

        private string _apiKey = "sd66az7eqokwe8pfhc59dcu1rdav7iep";

        private HttpClient _httpClient;
        private IServiceScopeFactory _scope;
        private MonoTorrent.Client.ClientEngine _engine;

        public TorrentService(ILogger<TorrentService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;

            _httpClient = new HttpClient();
            _scope = scopeFactory;

            _engine = new MonoTorrent.Client.ClientEngine();
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


            var torrentSettings = new TorrentSettings();

            Torrent torrent;

            if (sr.MagnetUrl.Contains("magnet"))
            {
                System.Console.WriteLine("trying with magnet");
                var metadata = await _engine.DownloadMetadataAsync(MagnetLink.Parse(sr.MagnetUrl), CancellationToken.None);

                torrent = MonoTorrent.Torrent.Load(metadata);
            }
            else
            {
                torrent = MonoTorrent.Torrent.Load(new Uri(sr.MagnetUrl), "./torrents/data");
            }

            var torrentManager = new TorrentManager(torrent, ".\\torrents", torrentSettings);

            torrentManager.TorrentStateChanged += async (object e, TorrentStateChangedEventArgs args) =>
            {
                var speed = (args.TorrentManager.Engine.TotalDownloadSpeed).Bytes();

                System.Console.WriteLine($"prog\t{Math.Round(args.TorrentManager.PartialProgress, 1)}%");
                System.Console.WriteLine($"speed\t{speed.LargestWholeNumberValue} {speed.LargestWholeNumberFullWord}/s");
                System.Console.WriteLine($"state\t{args.TorrentManager.State}");

                if (args.TorrentManager.State == TorrentState.Seeding)
                {
                    await args.TorrentManager.StopAsync();
                    var telegram = _scope.CreateScope().ServiceProvider.GetRequiredService<TelegramService>();

                    string[] mediaExtensions = {
                        ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
                        ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
                        ".AVI", ".MP4", ".DIVX", ".WMV", ".MKV" //etc
                    };

                    foreach (var file in args.TorrentManager.Torrent.Files)
                    {
                        if (!mediaExtensions.Any(me => file.FullPath.ToUpper().Contains(me))) continue;

                        using var fs = File.Open(file.FullPath, FileMode.Open);

                        var upload = new InputOnlineFile(fs);

                        await telegram.Client.SendDocumentAsync(chatId, upload);
                    }
                }
            };

            torrentManager.PieceHashed += (object e, PieceHashedEventArgs args) =>
            {
                var speed = (args.TorrentManager.Engine.TotalDownloadSpeed).Bytes();

                var date = DateTime.Now.ToShortTimeString();

                System.Console.WriteLine($"{date}\tspeed\t{Math.Round(speed.LargestWholeNumberValue, 1)} {speed.LargestWholeNumberFullWord}/s");
                System.Console.WriteLine($"{date}\tprog\t{Math.Round(args.TorrentManager.PartialProgress, 1)}%");
            };

            await _engine.Register(torrentManager);
            await _engine.StartAllAsync();

            var lastCheck = DateTime.Now;

            _engine.StatsUpdate += (object e, StatsUpdateEventArgs args) =>
            {
                // if ((lastCheck - DateTime.Now).TotalSeconds < 1) return;

                var speed = (_engine.TotalDownloadSpeed).Bytes();

                var date = DateTime.Now.ToShortTimeString();

                lastCheck = DateTime.Now;

                System.Console.WriteLine($"{date}\tspeed\t{Math.Round(speed.LargestWholeNumberValue, 1)} {speed.LargestWholeNumberFullWord}/s");
            };
        }
    }
}