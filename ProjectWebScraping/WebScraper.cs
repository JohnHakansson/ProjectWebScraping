using HtmlAgilityPack;
using ProjectWebScraping.Models;
using System.Diagnostics;

namespace ProjectWebScraping
{
  public class WebScraper
  {
    private readonly string _baseUrl = "https://books.toscrape.com/";
    private readonly HtmlWeb _htmlWeb = new HtmlWeb();

    public void Scrape()
    {
      Stopwatch sw = Stopwatch.StartNew();
      HtmlDocument doc = _htmlWeb.Load(_baseUrl);

      HtmlNode sidePanel = doc.DocumentNode.Descendants("div")
        .Where(x => x.HasClass("side_categories"))
        .First();

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
        
      foreach(HtmlNode category in categories)
      {
        string link = category.GetAttributeValue("href", string.Empty);
        string fullLink = string.Format("{0}{1}", _baseUrl, link);
        HandleThing(fullLink);
      }

      sw.Stop();
      Console.WriteLine(sw.Elapsed.ToString());
    }

    public Task HandleThing(string categoryLink)
    {
      HtmlNode categoryPage = _htmlWeb.Load(categoryLink).DocumentNode;
      List<HtmlNode> books = new List<HtmlNode>();
      var categoryDiv = categoryPage.Descendants("section")
        .First()
        .Descendants("div")
        .Where(div => !div.HasClass("alert"))
        .First();

      books.AddRange(categoryDiv
        .Descendants("ol")
        .First()
        .Descendants("li")
        .ToList());

      var pager1 = categoryDiv.Descendants("ul").Where(ul => ul.HasClass("pager"));


      var pager = categoryDiv.Descendants("ul")
        .Where(ul => ul.HasClass("pager")).Count() > 0;

      if(pager)
      {
        HtmlNode categoryPage2 = _htmlWeb.Load(categoryLink.Replace("index.html", "page-2.html")).DocumentNode;

        books.AddRange(categoryPage2.Descendants("section")
          .First()
          .Descendants("div")
          .Where(div => !div.HasClass("alert"))
          .First()
          .Descendants("ol")
          .First()
          .Descendants("li")
          .ToList());
      }

      Console.WriteLine(books.Count);

      foreach (HtmlNode book in books)
      {
        HandleBook(book);
      }

      return null;
    }

    public Task HandleBook(HtmlNode bookNode)
    {
      HtmlNode aNode = bookNode.Descendants("article").First()
        .Descendants("h3").First()
        .Descendants("a").First();

      var link = aNode.GetAttributeValue("href", string.Empty).Replace("../", "");
      link = string.Format("{0}{1}{2}", _baseUrl, "catalogue/", link);

      HtmlNode bookPage = _htmlWeb.Load(link).DocumentNode;
      var book = new ScrapedBook();

      return null;
    }
  }
}
