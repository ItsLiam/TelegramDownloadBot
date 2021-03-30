using System;

namespace TelegramDownloadBot.Bot.Data
{
    public class ActiveTorrent
    {
        public int Id { get; set; }
        public int TorrentId { get; set; }
        public bool IsFinished { get; set; }
        public long ChatId { get; set; }
    }
}