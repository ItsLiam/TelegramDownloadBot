using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramDownloadBot.Bot.Services;

namespace TelegramDownloadBot.Bot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TelegramService _telegram;

        public Worker(ILogger<Worker> logger, TelegramService telegramService)
        {
            _logger = logger;
            _telegram = telegramService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _telegram.Start(stoppingToken);


                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(-1, stoppingToken);
            }
        }
    }
}