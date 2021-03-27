using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using MonoTorrent;
using MonoTorrent.Client;
using TelegramDownloadBot.Bot.Data;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace TelegramDownloadBot.Bot.Services
{
    public class TorrentService
    {
        private readonly ILogger<TorrentService> _logger;


        
        private string _baseUrl =
            "http://10.1.1.3:9117/api/v2.0/indexers/all/results/torznab/api?apikey=sd66az7eqokwe8pfhc59dcu1rdav7iep&t=search&cat=&q=";

        private string _apiKey = "sd66az7eqokwe8pfhc59dcu1rdav7iep";

        private HttpClient _httpClient;

        public TorrentService(ILogger<TorrentService> logger)
        {
            _logger = logger;
            
            _httpClient = new HttpClient();
        }

        public async Task<List<SearchResponse>> Search(string query)
        {
            Console.WriteLine($"searching for {query}");
            
            var response = await _httpClient.GetAsync($"{_baseUrl}{HttpUtility.UrlEncode(query)}");

            Console.WriteLine(response.StatusCode.ToString());

            await using var sw = File.CreateText("search.xss");

            await sw.WriteLineAsync(await response.Content.ReadAsStringAsync());

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Rss));

            using var reader = new StringReader(await response.Content.ReadAsStringAsync());

            var parsed = (Rss) (xmlSerializer.Deserialize(reader));

            if (parsed == null)
            {
                Console.WriteLine("failed to deserialize search");
                return null;
            }
            
            var responses = new List<SearchResponse>();

            foreach (var item in parsed.Channel.Item)
            {
                uint.TryParse(item.Attr.FirstOrDefault(a => a.Name == "seeders")?.Value, out var seeders);
                uint.TryParse(item.Attr.FirstOrDefault(a => a.Name == "peers")?.Value, out var peers);
                uint.TryParse(item.Files, out var numFiles);
                uint.TryParse(item.Size, out var size);
                
                if (item.Enclosure.Url == null) continue;
                if (!item.Enclosure.Url.StartsWith("http")) continue;
                
                
                var search = new SearchResponse()
                {
                    Title = item.Title,
                    Size = size,
                    Uploaded = DateTime.Parse(item.PubDate),
                    NumOfFiles = numFiles,
                    Peers = peers,
                    Seeders = seeders,
                    MagnetUrl = item.Enclosure.Url
                };

                responses.Add(search);
            }

            return await Task.FromResult(responses);
        }
        public async void Download(string url)
        {
            Console.WriteLine($"being called in download with url \n{url}");
            var torrentClient = new Client("http://10.1.1.3:9091/transmission/rpc", null, "admin", "password");
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);
            
            await File.WriteAllBytesAsync("test3.torrent", await response.Content.ReadAsByteArrayAsync());

            Console.WriteLine($"torrent length {(await response.Content.ReadAsByteArrayAsync()).LongLength}");
            
            // var newTorrent = new NewTorrent()
            // {

            // };

            // await torrentClient.TorrentAddAsync(newTorrent);
        }
    }
}