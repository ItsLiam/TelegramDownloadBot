using System;

namespace TelegramDownloadBot.Bot.Data
{
    public class SearchResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Uploaded { get; set; }
        public uint Size { get; set; }
        public uint NumOfFiles { get; set; }
        public uint Seeders { get; set; }
        public uint Peers { get; set; }
        public string MagnetUrl { get; set; }
    }
}