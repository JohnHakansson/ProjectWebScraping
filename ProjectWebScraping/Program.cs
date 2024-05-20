namespace ProjectWebScraping
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      await new WebScraper().Scrape();
    }
  }
}
