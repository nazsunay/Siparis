using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Siparis.Models;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace Siparis.Controllers
{
    public class HomeController : Controller
    {

        string connectionString = "";


        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var product = connection.Query<Product>("SELECT * From Products").ToList();

            return View(product);
        }
        public IActionResult Details(int id)
        {
            if (id == null) 
            {
                return RedirectToAction("Index");
            }

            ViewData["Id"] = HttpContext.Session.GetInt32("Id");
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");


            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT Products.*, Users.Name FROM Products LEFT JOIN Users ON Users.Id = products.UserId WHERE products.Id = @Id";
            var post = connection.QuerySingleOrDefault<Product>(sql, new { Id = id });


            var comments = connection.Query<Product>("SELECT comments.*, Users.Name, Name FROM comments LEFT JOIN Users ON Users.Id = comments.UserId WHERE comments.ProductId = @Id AND comments.IsApproved = 1 ", new { Id = id }).ToList();

            var count = connection.QuerySingle<int>("SELECT COUNT (ProductId) as CommentCount FROM comments Where ProductId = @id", new { id });

            ViewBag.Count = count;
            ViewBag.Comments = comments;
            ViewBag.Control = ViewData["Id"];

            return View(post);


        }

        public IActionResult Search(Search model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalý iþlem yaptýnýz";
                return View("Message");
            }

            ViewData["Id"] = HttpContext.Session.GetInt32("Id");
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");

            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM Products WHERE Name LIKE @search";
            var post = connection.QuerySingleOrDefault<Product>(sql, new { search = model.SearchProduct });

            if (post != null)
            {
                return RedirectToAction("Details", new { Id = post.Id });
            }
            else
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Ürün bulunamadý.";
                return View("Message");
            }
        }

        public IActionResult Category()
        {
            using var connection = new SqlConnection(connectionString);
            var category = connection.Query<Product>("SELECT * From Categories").ToList();

            return View(category);
        }
        public IActionResult CategoryDetail(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM Products WHERE CategoryId = @CategoryId", new { CategoryId = id }).ToList();
            return View(products);
        }
        public IActionResult SatinAl(int id)
		{
			using var connection = new SqlConnection(connectionString);
			var products = connection.QuerySingleOrDefault<Product>("SELECT p.*, s.* FROM Products p LEFT JOIN Sales s ON p.Id = s.ProductId  WHERE p.Id = @Id", new { Id = id });

			if (products == null || products.Stock <= 0)
			{
				ViewBag.MessageCssClass = "alert-danger";
				ViewBag.Message = "Ürün bulunamadý veya stokta yok.";
				return View("Message");
			}

			var sqlInsertSales = "INSERT INTO Sales (ProductName, Price, Quantity) VALUES (@Name, @Price, @Quantity)";
			var data = new
			{

				products.Name,
				products.Price,
				Quantity = 1,

			};

			var rowsAffected = connection.Execute(sqlInsertSales, data);

			if (rowsAffected > 0)
			{
				var sqlUpdateProduct = "UPDATE Products SET Stock = Stock - 1 WHERE Id = @Id";
				connection.Execute(sqlUpdateProduct, new { Id = id });

				ViewBag.MessageCssClass = "alert-primary";
				ViewBag.Message = "Ürün baþarýyla sepete eklendi.";
				return RedirectToAction("Index", "Basket");

			}
			else
			{
				ViewBag.MessageCssClass = "alert-danger";
				ViewBag.Message = "Satýn alma iþlemi baþarýsýz.";
			}

			return View("Message");
		}
      

        public IActionResult SendMail(Product model)
        {
            var client = new SmtpClient("smtp.eu.mailgun.org", 587)
            {
                Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "3b212cffce4a231e162ecd83abce45ea-911539ec-debf1d4c"),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("bildirim@siparis.com.tr", "Siparis.com"),

                Subject = ViewBag.Subject,
                Body = ViewBag.Body,
                IsBodyHtml = true,
            };

            mailMessage.ReplyToList.Add(model.Email);

            mailMessage.To.Add(new MailAddress($"{model.Email}", $"{model.UserName}"));

            client.Send(mailMessage);
            return RedirectToAction(ViewBag.Return);

        }

        [HttpPost]//sonradan ürüne yorum yapabilme ?
        public IActionResult AddComments(ProductComment model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalý iþlem yaptýn";
                return View("Message");
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO Comments (Text, DateCreated) VALUES (@Text, @DateCreated)";
            try
            {
                var data = new
                {
                    
                  model.Text,
                  model.DateCreated
                };
                var affectedRows = connection.Execute(sql, data);



                return RedirectToAction("Detail", "Home", new { id = model.ProductId });
            }
            catch
            {
                return RedirectToAction("Index");

            }
        }

        public ActionResult About() 
        {
            return View();
        
        }

      
    }
}
