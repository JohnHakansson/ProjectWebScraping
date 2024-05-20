using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWebScraping
{
  public class WebScraper
  {
    private readonly string _baseUrl = "https://books.toscrape.com/";

    public void Scrape()
    {
      var web = new HtmlWeb();
      HtmlDocument doc = web.Load(_baseUrl);

      HtmlNode sidePanel = doc.DocumentNode.Descendants("div").Where(x => x.HasClass("side_categories")).First();

      if(sidePanel == null)
      {
        return;
      }

      List<HtmlNode> categories = sidePanel.Descendants("ul")
        .Where(node => !node.HasClass("nav nav-list"))
        .First()
        .Descendants("ul")
        .First()
        .Descendants("li")
        .Select(li => li.SelectSingleNode("a"))
        .ToList();
        
      foreach(var category in categories)
      {
        var link = category.GetAttributeValue("href", string.Empty);
        Console.WriteLine(string.Format("{0}{1}", _baseUrl, link));
      }
    }
  }
}
