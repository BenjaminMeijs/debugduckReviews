using System;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace debugduckReviews
{
    class Review
    {
        public string ProductID { get; set; }
        public string ReviewStars { get; set; }
        public string ReviewTitle { get; set; }
        public string ReviewContent { get; set; }
        public string ReviewerName { get; set; }
    }

    class Product
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ReviewAverage { get; set; }
        public string HTMLoutput { get; set; }
    }
    class Program
    {
        
        public static string RreviewPath = "C:\\Users\\benja\\source\\repos\\debugduckReviews\\debugduckReviews\\review.csv";
        public static string RproductPath = "C:\\Users\\benja\\source\\repos\\debugduckReviews\\debugduckReviews\\product.csv";
        public static string WproductPath = "C:\\Users\\benja\\source\\repos\\debugduckReviews\\debugduckReviews\\newproduct.csv";
        public static string RWphpProducts = "C:\\Users\\benja\\source\\repos\\debugduckReviews\\debugduckReviews\\en_products.php";
        static void Main(string[] args)
        {
            using var reviewReader = new StreamReader(RreviewPath);
            using var productReader = new StreamReader(RproductPath);
            using var reviewCSV = new CsvReader(reviewReader);
            using var productCSV = new CsvReader(productReader);
            reviewCSV.Configuration.Delimiter = "\t";
            productCSV.Configuration.Delimiter = "\t";

            List<Review> reviews = reviewCSV.GetRecords<Review>().ToList();
            List<Product> products = productCSV.GetRecords<Product>().ToList();
            //calculate the star average
            foreach (Product product in products)
            {
                product.ReviewAverage = (reviews
                    .Where(r => r.ProductID == product.ProductID)
                    .Average(r => int.Parse(r.ReviewStars))).ToString();

                string HTMLproductDescription = $@"
    <div class='productDescription'>
    {product.ProductDescription}
    </div>
";
                string averageRating = new string('★', int.Parse(product.ReviewAverage));
                string formURL = $"https://docs.google.com/forms/d/e/1FAIpQLSeyIh7aT-knRiInv46M9P48kAhQANf74R3V7lC64mIhJqNErQ/viewform?usp=pp_url&entry.117829278={product.ProductID}";
                string HTMLreviewHeader = $@"
<div class='reviewHeader'>
    <a href='{formURL}'  target='_blank'>
        Leave a review
    </a>
    <br>
    <span class='averageRatingLeader'>
      average rating:
    </span>
    <span class='averageRating'>
        {averageRating}
    </span>
</div>";
                string HTMLreviews = "";
                foreach (Review review in reviews.Where(r => r.ProductID == product.ProductID))
                {
                    string rating = new String('★', int.Parse(review.ReviewStars));
                    HTMLreviews += $@"
<div class='reviewContainer'>
<span class='reviewStars'>
  {rating}
</span>
<span class='reviewTitle'>
  {review.ReviewTitle}
</span>
<div class='reviewContent'>
  {review.ReviewContent}
</div>
<div class='reviewerName'>
  {review.ReviewerName}
</div>
</div>
";
                }
                string HTMLresult = (HTMLproductDescription + HTMLreviewHeader + HTMLreviews)
                    .Replace(Environment.NewLine,String.Empty)
                    .Replace("\t", String.Empty)
                    .Replace(";", string.Empty)
                    .Replace("{", string.Empty)
                    .Replace("}",string.Empty);
                product.HTMLoutput = HTMLresult;
            }

            using var writer = new StreamWriter(WproductPath);
            using var productWriter = new CsvWriter(writer);
            productWriter.Configuration.Delimiter = "\t";
            productWriter.WriteRecords(products);


            string phpText = File.ReadAllText(RWphpProducts);
            foreach (Product product in products)
            {
                string find = $"<p>[{product.ProductID}]</p>";
                string replace = product.HTMLoutput;
                phpText = phpText.Replace(find, replace);
            }
            
            File.WriteAllText(RWphpProducts, phpText);
        }
    }
}
