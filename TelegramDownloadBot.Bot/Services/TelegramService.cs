using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using TelegramDownloadBot.Bot.Handlers;

namespace TelegramDownloadBot.Bot.Services
{
    public class TelegramService
    {
        private TelegramBotClient _client;
        private RootHandler _handler;
        private CallbackHandler _cbHandler;

        public TelegramService(RootHandler handler, CallbackHandler cbHandler)
        {
            _handler = handler;
            _cbHandler = cbHandler;

            //init
            _client = new TelegramBotClient("1679074026:AAHjWMmSNrqdElgxJAHbvAYb69sk7XgQs8Y");

            //listeners
            _client.OnMessage += OnMessageReceived;
            _client.OnCallbackQuery += OnCallbackReceived;
        }

        public TelegramBotClient Client
        {
            get => _client;
        }

        public void Start(CancellationToken token)
        {
            _client.StartReceiving(cancellationToken: token);
        }

        void OnMessageReceived(object sender, MessageEventArgs args) => _handler.Handle(args, _client);
        void OnCallbackReceived(object sender, CallbackQueryEventArgs args) => _cbHandler.Handle(args, _client);
    }
}