using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Args;
using TelegramDownloadBot.Bot.Data;
using Transmission.API.RPC;

namespace TelegramDownloadBot.Bot.Services
{
    public class TransmissionService
    {
        private Client _client;
        private IServiceScopeFactory _scope;

        public TransmissionService(IServiceScopeFactory scopeFactory)
        {
            //init
            _client = new Client("http://10.1.1.3:9091/transmission/rpc", null, "admin", "password");
            _scope = scopeFactory;

            //listeners
            // var timer = new Timer(TimerCallback, null, 10, 10);
        }

        public Client Client { get => _client; }
    }
}