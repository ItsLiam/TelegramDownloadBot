using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using TelegramDownloadBot.Bot.Data;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace TelegramDownloadBot.Bot.Services
{
    public class TransmissionService
    {
        private Client _client;
        private IServiceScopeFactory _scope;

        public TransmissionService(IServiceScopeFactory scopeFactory)
        {
            //init
            _client = new Client("http://localhost:9091/transmission/rpc", null, "username", "password");
            _scope = scopeFactory;

            //listeners
            // var timer = new Timer(TimerCallback, null, 10, 10);
        }

        public Client Client { get => _client; }
    }
}