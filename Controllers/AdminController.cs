using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Siparis.Models;
using System.Data.SqlClient;

namespace Siparis.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = "";


        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Product>("SELECT * FROM Products" ).ToList();

            return View(posts);
        }

        public IActionResult ProductAdd()
        {
           
            return View();
        }
     
        [HttpPost]
        public IActionResult ProductAdd(Product model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            model.DateCreated = DateTime.Now;
            

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO Products (Name, Description, Price, Stock, DateCreated, Detail, ImgUrl) VALUES (@Name, @Description, @Price, @Stock, @DateCreated, @Detail, @ImgUrl)";

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);
            model.ImgUrl = imageName;
            var data = new
            {
                model.Name,
                model.Description,
                model.Price,
                model.Stock,
                model.DateCreated,
                model.Detail,
                model.ImgUrl,
            };

            var rowsAffected = connection.Execute(sql, data);


            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Eklendi.";
            return View("Message");
        }

        public bool CheckLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("email")))
            {
                return false;
            }

            return true;
        }

        public IActionResult ProductDelete(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Products WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = Id });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ProductUpdate(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var category = connection.QuerySingleOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = Id });

            return View(category);
        }
        [HttpPost]
        public IActionResult ProductUpdate(Product model)
        {
            using var connection = new SqlConnection(connectionString);

            var imageName = model.ImgUrl;
            if (model.Image != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
                using var stream = new FileStream(path, FileMode.Create);
                model.Image.CopyTo(stream);
            }

            var sql = "UPDATE Products SET Name=@Name, Description=@Description, Detail=@Detail, Price=@Price, Stock=@Stock,DateCreated=@DateCreated, ImgUrl = @ImgUrl WHERE Id = @Id";

            var parameters = new
            {
                model.Name,
                model.Description,
                model.Detail,
                model.Price,
                model.Stock,
                DateCreated = DateTime.Now,
                model.Id,
                ImgUrl = imageName
            };
            var affectedRows = connection.Execute(sql, parameters);

            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        public IActionResult CategoryList()
        {
            using var connection = new SqlConnection(connectionString);
            var category = connection.Query<Category>("SELECT * FROM Categories").ToList();

            return View(category);
        }


        public IActionResult CategoryAdd()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CategoryAdd(Category model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO Categories (CategoryName, Description, ImgUrl) VALUES (@CategoryName, @Description, @ImgUrl)";

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);
            model.ImgUrl = imageName;
            var data = new
            {
                model.CategoryName,
                model.Description,
                model.ImgUrl,
            };

            var rowsAffected = connection.Execute(sql, data);


            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Eklendi.";
            return View("Message");
        }

        public IActionResult CategoryDelete(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Categories WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = Id });

            return RedirectToAction("Index");
        }

        public IActionResult CategoryUpdate(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var post = connection.QuerySingleOrDefault<Category>("SELECT * FROM Categories WHERE Id = @Id", new { Id = Id });

            return View(post);
        }
        public IActionResult CategoryUpdate(Category model)
        {
            using var connection = new SqlConnection(connectionString);

            var imageName = model.ImgUrl;
            if (model.Image != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
                using var stream = new FileStream(path, FileMode.Create);
                model.Image.CopyTo(stream);
            }

            var sql = "UPDATE Categories SET CategoryName=@CatgoryName, Description=@Description,  ImgUrl = @ImgUrl WHERE Id = @Id";

            var parameters = new
            {
                model.CategoryName,
                model.Description,
                model.Id,
                ImgUrl = imageName
            };
            var affectedRows = connection.Execute(sql, parameters);

            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        public IActionResult CommentUpdate(int id)
        {
            if (CheckLogin())
            {
                ViewData["Id"] = HttpContext.Session.GetInt32("Id");

                using var connection = new SqlConnection(connectionString);
                var comment = connection.QuerySingleOrDefault<Product>("SELECT comments.*, Username, Name, FROM comments LEFT JOIN Users ON comments.UserId = Users.Id WHERE comments.Id = @Id", new { Id = id });

                return View(comment);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public IActionResult CommentUpdate(Product model)
        {
            if (model.Comment == null)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);

            var sql = "UPDATE comments SET Comment = @Comment, CommentDate = @CommentDate WHERE Id=@Id";

            var parameters = new
            {
                model.Comments,
                model.Id,
                CommentDate = DateTime.Now
            };

            var affectedRows = connection.Execute(sql, parameters);

            return RedirectToAction("Detail", "Home", new { id = model.Id });
        }
        public IActionResult DeleteComment(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Comments WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Detail", "Home");
        }
        public IActionResult YorumGöster(int id)//burası düzenlenecek
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "UPDATE comments SET ReportComment = 1 WHERE Id=@Id";

            var affectedRows = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index", "Home");

        }

        public IActionResult Report()
        {
            using var connection = new SqlConnection(connectionString);
            var salesReport = connection.Query<Sale>("SELECT * FROM Sales ORDER BY Quantity");

            return View(salesReport);
        }


    }
}
