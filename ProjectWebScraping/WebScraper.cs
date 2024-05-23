using HtmlAgilityPack;
using Newtonsoft.Json;
using ProjectWebScraping.Models;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectWebScraping
{
  public class WebScraper
  {
    private readonly string _baseUrl = "https://books.toscrape.com/";
    private readonly HtmlWeb _htmlWeb = new HtmlWeb();
    private readonly string _numberAndDotMatch = "[0-9.]+";
    private readonly string _numberMatch = "[0-9]+";
    private readonly string _path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/WebScraping";
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task Scrape()
    {
      HtmlDocument doc = await _htmlWeb.LoadFromWebAsync(_baseUrl);
      var categoryTasks = new List<Task>();
      HtmlNode sidePanel = doc.DocumentNode.Descendants("div")
        .Where(x => x.HasClass("side_categories"))
        .First();

      List<HtmlNode> categories = sidePanel.Descendants("ul")
        .Where(node => !node.HasClass("nav nav-list"))
        .First()
        .Descendants("ul")
        .First()
        .Descendants("li")
        .Select(li => li.SelectSingleNode("a"))
        .ToList();



      foreach (HtmlNode category in categories)
      {
        string link = category.GetAttributeValue("href", string.Empty);
        string fullLink = string.Format("{0}{1}", _baseUrl, link);
        string categoryName = category.InnerText.Trim().DecodeAndSanitize();
        categoryTasks.Add(HandleCategory(fullLink, categoryName));
      }

      await Task.WhenAll(categoryTasks);
    }

    public async Task HandleCategory(string categoryLink, string categoryName)
    {
      Console.WriteLine(categoryName + " scraping started...");
      HtmlNode categoryPage = (await _htmlWeb.LoadFromWebAsync(categoryLink)).DocumentNode;
      Directory.CreateDirectory(string.Format("{0}/{1}", _path, categoryName));

      List<HtmlNode> books = new List<HtmlNode>();
      List<Task> bookTasks = new List<Task>();
      bool pagesExist = false;

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

      pagesExist = categoryDiv.Descendants("ul")
        .Where(ul => ul.HasClass("pager")).FirstOrDefault() != null;

      while (pagesExist)
      {
        var liNode = categoryDiv.Descendants("ul")
          .Where(ul => ul.HasClass("pager"))
          .First()
          .Descendants("li")
          .Where(li => li.HasClass("next"))
          .FirstOrDefault();

        if (liNode == null)
        {
          pagesExist = false;
        }
        else
        {
          string pageLink = liNode.Descendants("a")
            .First()
            .GetAttributeValue("href", string.Empty);

          categoryDiv = _htmlWeb.Load(categoryLink.Replace("index.html", pageLink)).DocumentNode;
          books.AddRange(categoryDiv.Descendants("section")
            .First()
            .Descendants("div")
            .Where(div => !div.HasClass("alert"))
            .First()
            .Descendants("ol")
            .First()
            .Descendants("li")
            .ToList());
        }
      }

      foreach (HtmlNode book in books)
      {
        bookTasks.Add(HandleBook(book, categoryName));
      }

      Console.WriteLine(categoryName + " scraping complete, " + books.Count + " books found.");

      await Task.WhenAll(bookTasks);
    }

    public async Task HandleBook(HtmlNode bookNode, string categoryName)
    {
      HtmlNode aNode = bookNode.Descendants("article").First()
        .Descendants("h3").First()
        .Descendants("a").First();

      var link = aNode.GetAttributeValue("href", string.Empty).Replace("../", "");
      link = string.Format("{0}{1}{2}", _baseUrl, "catalogue/", link);

      HtmlNode bookPage = (await _htmlWeb.LoadFromWebAsync(link)).DocumentNode
        .Descendants("article")
        .First();

      HtmlNode firstRow = bookPage.Descendants().Where(div => div.HasClass("row")).First();

      string imgLink = firstRow.Descendants("img").First()
        .GetAttributeValue("src", string.Empty)
        .Replace("../", "");
      imgLink = string.Format("{0}{1}", _baseUrl, imgLink);
      var book = new ScrapedBook();
      var price = Regex.Match(firstRow.Descendants("p")
        .Where(p => p.HasClass("price_color"))
        .First().InnerText, _numberAndDotMatch).Value;

      book.Title = firstRow.Descendants("h1").First().InnerText.DecodeAndSanitize();
      book.StarRating = GetNumberOfStars(string.Join(" ", firstRow.Descendants("p")
        .Where(p => p.HasClass("star-rating"))
        .First()
        .GetClasses()));
      var descriptionNode = bookPage.Descendants("div")
        .Where(div => div.Id == "product_description")
        .FirstOrDefault();
      book.Description = descriptionNode != null ? descriptionNode.NextSibling.NextSibling.InnerText.DecodeAndSanitize() : string.Empty;

      var table = bookPage.Descendants("table").First();
      var tableRows = table.Descendants("tr");

      book.UPC = tableRows.ElementAt(0).Descendants("td").First().InnerText.DecodeAndSanitize();
      book.ProductType = tableRows.ElementAt(1).Descendants("td").First().InnerText.DecodeAndSanitize();
      book.PriceWithoutTax = decimal.Parse(Regex.Match(tableRows.ElementAt(2).Descendants("td").First().InnerText, _numberAndDotMatch).Value);
      book.Price = decimal.Parse(Regex.Match(tableRows.ElementAt(3).Descendants("td").First().InnerText, _numberAndDotMatch).Value);
      book.Tax = decimal.Parse(Regex.Match(tableRows.ElementAt(4).Descendants("td").First().InnerText, _numberAndDotMatch).Value);
      book.AvailableInStock = int.Parse(Regex.Match(tableRows.ElementAt(5).Descendants("td").First().InnerText, _numberMatch).Value);
      book.NumberOfReviews = int.Parse(tableRows.ElementAt(6).Descendants("td").First().InnerText);
      book.Category = categoryName;
      string serializedJson = JsonConvert.SerializeObject(book);
      string title = book.Title.EndsWith("...") ? book.Title.Replace("...", "") : book.Title;

      var dirPath = string.Format("{0}/{1}/{2}", _path, categoryName, title);
      Directory.CreateDirectory(dirPath);

      HttpResponseMessage response = await _httpClient.GetAsync(imgLink);
      byte[] byteImg = await response.Content.ReadAsByteArrayAsync();
      File.WriteAllBytes(string.Format("{0}/{1}", dirPath, title + ".jpg"), byteImg);

      File.WriteAllText(string.Format("{0}/{1}", dirPath, title + ".json"), serializedJson);
    }

    public int GetNumberOfStars(string ratingString)
    {
      if (ratingString.Contains("One"))
      {
        return 1;
      }
      else if (ratingString.Contains("Two"))
      {
        return 2;
      }
      else if (ratingString.Contains("Three"))
      {
        return 3;
      }
      else if (ratingString.Contains("Four"))
      {
        return 4;
      }
      else
      {
        return 5;
      }
    }

  }
}
