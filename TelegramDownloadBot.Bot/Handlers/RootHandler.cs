using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using TelegramDownloadBot.Bot.Services;
using Humanizer.Bytes;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramDownloadBot.Bot.Data;
using MonoTorrent.Client;

namespace TelegramDownloadBot.Bot.Handlers
{
    public class RootHandler
    {
        private readonly TorrentService _torrent;
        private readonly IServiceScopeFactory _scopeFactory;

        public RootHandler(TorrentService torrent, IServiceScopeFactory scopeFactory)
        {
            _torrent = torrent;
            _scopeFactory = scopeFactory;
        }
        
        public async void Handle(MessageEventArgs args, TelegramBotClient client)
        {
            var split = args.Message.Text.Split(" ");
            
            switch (split.First())
            {
                case "/start":
                    await client.SendTextMessageAsync(args.Message.Chat.Id, "Hello! Welcome to my bot :D", replyToMessageId: args.Message.MessageId);
                    break;
                case "/search":
                {
                    //sanitizing input string
                    var query = split.ToList();
                    query.RemoveAt(0);
                    var sanitised = string.Join(' ', query);
                    
                    await client.SendTextMessageAsync(args.Message.Chat.Id, $"Searching for {sanitised}");
                    var response = (await _torrent.Search(sanitised));
                    
                    response.Sort((a, b) => b.Seeders.CompareTo(a.Seeders));

                    using var scope = _scopeFactory.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    var count = 0;
                    var stringed = response.Take(15).Select(r =>
                    {
                        count++;

                        if (r.Seeders < 1) return null;

                        var cached = new CachedSearchResponse()
                        {
                            SearchResponse = r,
                            OptionNumber = count,
                            ChatId = args.Message.Chat.Id
                        };

                        if (db == null) throw new Exception("db is null");
                        
                        db.CachedSearchResponses.Add(cached);
                        db.SaveChanges();

                        return $"Option {count} | {r.Title}\n{r.Seeders} seeders";
                    });

                    var message = await client.SendTextMessageAsync(args.Message.Chat.Id, $"Search results for: {sanitised}\n\n{string.Join("\n\n", stringed.Where(x => !string.IsNullOrEmpty(x)))}\n\nTo download one, type /download with the option number");
                    
                    
                    break;
                }
                case "/download":
                {
                    //sanitizing input string
                    var query = split.ToList();
                    query.RemoveAt(0);
                    var sanitised = string.Join(' ', query);

                    var canParse = int.TryParse(sanitised, out var id);

                    using var scope = _scopeFactory.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    var selectedOption = db.CachedSearchResponses.Include(c => c.SearchResponse)
                        .OrderBy(q => q.Id).LastOrDefault(c => (c.ChatId == args.Message.Chat.Id) && (c.OptionNumber == id));

                    if (!canParse || selectedOption == null)
                    {
                        await client.SendTextMessageAsync(args.Message.Chat.Id, "Invalid option");
                        return;
                    }

                    
                    await client.SendTextMessageAsync(args.Message.Chat.Id, $"Downloading {selectedOption.SearchResponse.Title}");

                    _torrent.Download(selectedOption.SearchResponse.MagnetUrl);
                    
                    break;
                }
            }
        }   
    }
}