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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

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
            if (args?.Message?.Text == null) return;
            var split = args.Message.Text.Split(" ");

            switch (split.First())
            {
                case "/start":
                    await client.SendTextMessageAsync(args.Message.Chat.Id, "Hello! Welcome to my bot :D\nFor help type /help", replyToMessageId: args.Message.MessageId);
                    break;
                case "/help":
                    await client.SendTextMessageAsync(args.Message.Chat.Id, "COMMANDS\tEXAMPLE\n/tv {season number} {episode number}\t/tv The Blacklist S01E12");

                    break;
                case "/tv":
                    {
                        var seasonInfoRegex = new Regex(@"([Ss]?)([0-9]{1,2})([xXeE\.\-]?)([0-9]{1,2})");


                        //sanitizing input string
                        var query = split.ToList();
                        query.RemoveAt(0);
                        var sanitized = string.Join(' ', query);

                        var data = seasonInfoRegex.Matches(sanitized).First();

                        var searchingMessage = await client.SendTextMessageAsync(args.Message.Chat.Id, $"\u23F3");




                        var aRegex = new Regex(@"[a-zA-Z]+");

                        var seasonInfo = aRegex.Split(data.ToString());
                        var season = seasonInfo[1];
                        var episode = seasonInfo[2];
                        var show = sanitized.Replace(data.ToString(), null);

                        var searchResponse = await _torrent.SearchShow(show, int.Parse(season), int.Parse(episode));

                        await client.DeleteMessageAsync(searchingMessage.Chat.Id, searchingMessage.MessageId);

                        searchResponse.Sort((a, b) => b.Seeders.CompareTo(a.Seeders));

                        if (searchResponse.Count < 1) await client.SendTextMessageAsync(args.Message.Chat.Id, "No results");

                        var top = searchResponse.Take(5);

                        var scope = _scopeFactory.CreateScope();
                        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                        database.SearchResponses.AddRange(top);
                        database.SaveChanges();


                        foreach (var res in top)
                        {
                            var replyMarkup = new InlineKeyboardMarkup(
                                InlineKeyboardButton.WithCallbackData("Download", $"download/{res.Guid}")
                            );

                            var size = ((int)res.Size).Bytes();

                            await client.SendTextMessageAsync(args.Message.Chat.Id, $"{res.Title}\nSize: {Math.Round(size.Megabytes, 1)}MB\nSeeders: {res.Seeders}", replyMarkup: replyMarkup);
                        }

                        break;
                    }
                    // case "/download":
                    //     {
                    //         //sanitizing input string
                    //         var query = split.ToList();
                    //         query.RemoveAt(0);
                    //         var sanitized = string.Join(' ', query);

                    //         var canParse = int.TryParse(sanitized, out var id);

                    //         using var scope = _scopeFactory.CreateScope();
                    //         await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    //         var selectedOption = db.CachedSearchResponses.Include(c => c.SearchResponse)
                    //             .OrderBy(q => q.Id).LastOrDefault(c => (c.ChatId == args.Message.Chat.Id) && (c.OptionNumber == id));

                    //         if (!canParse || selectedOption == null)
                    //         {
                    //             await client.SendTextMessageAsync(args.Message.Chat.Id, "Invalid option");
                    //             return;
                    //         }


                    //         await client.SendTextMessageAsync(args.Message.Chat.Id, $"Downloading {selectedOption.SearchResponse.Title}");

                    //         _torrent.Download(selectedOption.SearchResponse.MagnetUrl);

                    //         break;
                    //     }
            }
        }
    }
}