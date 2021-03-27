using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramDownloadBot.Bot.Data;
using TelegramDownloadBot.Bot.Handlers;
using TelegramDownloadBot.Bot.Services;

namespace TelegramDownloadBot.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddDbContext<DatabaseContext>(options => options.UseSqlite("Data Source=app.db"));
                    services.AddSingleton<TelegramService>();
                    services.AddSingleton<TorrentService>();
                    services.AddSingleton<RootHandler>();
                });
    }
}