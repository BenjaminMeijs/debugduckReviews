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
        public static string RreviewPath = "";
        public static string RproductPath = "";
        public static string WproductPath = "";
        static void Main(string[] args)
        {
            using var reviewReader = new StreamReader(RreviewPath);
            using var productReader = new StreamReader(RproductPath);
            using var reviewCSV = new CsvReader(reviewReader);
            using var productCSV = new CsvReader(productReader);

            List<Review> reviews = reviewCSV.GetRecords<Review>().ToList();
            List<Product> products = productCSV.GetRecords<Product>().ToList();
            //calculate the star average
            foreach (Product product in products)
            {
                product.ReviewAverage = reviews
                    .Where(r => r.ProductID == product.ProductID)
                    .Sum(r => int.Parse(r.ReviewStars)).ToString();

                string averageRating = new String('★', int.Parse(product.ReviewAverage));
                string HTMLreviewHeader = $@"
<div class='reviewHeader'>
    <a href='https://docs.google.com/forms/d/e/1FAIpQLSeyIh7aT-knRiInv46M9P48kAhQANf74R3V7lC64mIhJqNErQ/viewform?usp=pp_url&entry.117829278='  target='_blank'>
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
                product.HTMLoutput = HTMLreviewHeader + HTMLreviews;
            }

            using var writer = new StreamWriter(WproductPath);
            using var csv = new CsvWriter(writer);
            csv.WriteRecords(products);
        }
    }
}
