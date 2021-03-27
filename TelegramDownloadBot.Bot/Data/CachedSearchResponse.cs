namespace TelegramDownloadBot.Bot.Data
{
    public class CachedSearchResponse
    {
        public int Id { get; set; }
        public SearchResponse SearchResponse { get; set; }
        public int OptionNumber { get; set; }
        public long ChatId { get; set; }
    }
}