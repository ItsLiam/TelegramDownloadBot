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
        
        public TelegramService(RootHandler handler)
        {
            _handler = handler;
            //init
            _client = new TelegramBotClient("1679074026:AAE8qQPzAQC_4sNzfDNS6Qut0mkJU7Jq7ZE");
            
            //listeners
            _client.OnMessage += OnMessageReceived;
        }

        public void Start(CancellationToken token)
        {
            _client.StartReceiving(cancellationToken: token);
        }

        void OnMessageReceived(object? sender, MessageEventArgs args) => _handler.Handle(args, _client);
    }
}