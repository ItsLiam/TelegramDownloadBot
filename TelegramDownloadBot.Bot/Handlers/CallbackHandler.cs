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
    public class CallbackHandler
    {
        private readonly TorrentService _torrent;
        private readonly IServiceScopeFactory _scopeFactory;

        public CallbackHandler(TorrentService torrent, IServiceScopeFactory scopeFactory)
        {
            _torrent = torrent;
            _scopeFactory = scopeFactory;
        }

        public async void Handle(CallbackQueryEventArgs args, TelegramBotClient client)
        {
            var query = args.CallbackQuery.Data.Split('/');

            var scope = _scopeFactory.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            switch (query[0])
            {
                case "download":
                    var sr = database.SearchResponses.First(s => s.Guid == query[1]);

                    _torrent.Download(sr, args.CallbackQuery.Message.Chat.Id);

                    await client.SendTextMessageAsync(args.CallbackQuery.Message.Chat.Id, $"Downloading {sr.Title}");
                    break;
            }
        }
    }
}