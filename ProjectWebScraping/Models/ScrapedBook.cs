using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWebScraping.Models
{
  public class ScrapedBook
  {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PriceWithoutTax { get; set; }
    public decimal Tax { get; set; }
    public int StarRating { get; set; }
    public decimal AvailableInStock { get; set; }
    public string UPC {  get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string NumberOfReviews { get; set; } = string.Empty;
    public string Category {  get; set; } = string.Empty;
  }
}
