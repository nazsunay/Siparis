using Microsoft.AspNetCore.Mvc;
using Siparis.Models;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace Siparis.Controllers
{
    public class BasketController : Controller
    {
        string connectionString = "";

        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);


            var product = connection.Query<Sale>(" SELECT s.*, u.Id  FROM Sales s \r\n  LEFT  JOIN Users u ON s.UserId = u.Id").ToList();

            return View(product); 
           
        }
        public IActionResult SatinAl(int id)
        {
            using var connection = new SqlConnection(connectionString);

            // Ürün bilgilerini çekme
            var product = connection.QuerySingleOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });

            if (product == null || product.Stock <= 0)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Ürün bulunamadı veya stokta yok.";
                return View("Message");
            }

            // Satış kaydını ekleme
            var sqlInsertSales = "INSERT INTO Sales (ProductName, Price, Quantity) VALUES (@Name, @Price, @Quantity)";
            var salesData = new
            {
                product.Name,
                product.Price,
                Quantity = 1
            };

            var rowsAffected = connection.Execute(sqlInsertSales, salesData);

            if (rowsAffected > 0)
            {
                // Stoktan düşme işlemi
                var sqlUpdateProduct = "UPDATE Products SET Stock = Stock - 1 WHERE Id = @Id";
                connection.Execute(sqlUpdateProduct, new { Id = id });

                // Sepete ürünü ekleme (herhangi bir kullanıcıya bağlı değil)
                var cartItem = connection.QuerySingleOrDefault<Cart>("SELECT * FROM Cart WHERE ProductId = @ProductId", new { ProductId = id });

                if (cartItem == null)
                {
                    // Sepete yeni ürün ekleme
                    var sqlInsertCart = "INSERT INTO Cart (ProductId,UserId, Quantity) VALUES (@ProductId, @UserId, @Quantity)";
                    connection.Execute(sqlInsertCart, new
                    {
                        ProductId = product.Id,
                        UserId= product.UserId,
                        
                        Quantity = 1
                    });
                }
                else
                {
                    // Var olan ürünü güncelleme
                    var sqlUpdateCart = "UPDATE Cart SET Quantity = Quantity + 1 WHERE ProductId = @ProductId";
                    connection.Execute(sqlUpdateCart, new { ProductId = id });
                }

                // Sepetteki toplam fiyatı hesaplama
                var subTotalSql = "SELECT SUM(Price * Quantity) AS Subtotal FROM Cart";
                var subTotal = connection.QueryFirstOrDefault<Cart>(subTotalSql);
                ViewBag.SubTotal = subTotal;

                ViewBag.MessageCssClass = "alert-primary";
                ViewBag.Message = "Ürün başarıyla sepete eklendi.";
                return RedirectToAction("Index", "Basket");
            }
            else
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Satın alma işlemi başarısız.";
                return View("Message");
            }
        }


        
        public IActionResult DeleteBasket(int Id)
        {
           
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Sales WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = Id });

            return RedirectToAction("Index", "Basket");
        }
        [HttpPost]
        public IActionResult TakePayment(User model)
        {
            //ID HATASI ÇÖZÜLECEK!!!!
            try
            {

                
                if (!ModelState.IsValid)
                {
                    // Hata mesajı göster
                    return View("Message");
                }

                // Veritabanı bağlantısı
                using (var connection = new SqlConnection(connectionString))
                {
                    // Kullanıcıyı doğrula
                    var user = connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Email = @Email AND Id = @Id", new { model.Email, model.Id });
                    if (user == null)
                    {
                        // Kullanıcı bulunamadı
                        ViewBag.MessageCssClass = "alert-danger";
                        ViewBag.Message = "Kullanıcı Bulunamadı Lütfen Kayıt Olunuz.";
                        return View("Message");
                    }

                    // Sepete ait ürünleri al
                    var cartItems = connection.Query<Cart>("SELECT * FROM Cart WHERE UserId = @userId", new { userId = user.Id }).ToList();

                    
                    var subTotal = cartItems.Sum(item => item.Price * item.Quantity);

                    SendMail(model, subTotal);

                    // GEREKMİYOR AMA GEREKECEK 
                    connection.Execute("DELETE FROM Cart WHERE UserId = @userId", new { userId = user.Id });

                    ViewBag.MessageCssClass = "alert-danger";
                    ViewBag.Message = "Teşekkür ederiz Siparişiniz en yakın zamanda Teslim Edilecektir.";
                    return View("Message");
                    
                }
            }
            catch (Exception ex)
            {
               
                return View("Error");
            }
        }
        //public IActionResult TakePayment(User model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.MessageCssClass = "alert-danger";
        //        ViewBag.Message = "Eksik veya hatalı işlem yaptın";
        //        return View("Message");
        //    }
        //    using var connection = new SqlConnection(connectionString);
        //    ViewData["email"] = HttpContext.Session.GetString("email");
        //    var login = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users WHERE Email = @Email AND Id = @Id", new { model.Email,model.Id });
        //    if (ViewData["email"] == null)
        //    {
        //        if (login?.Email == model.Email)
        //        {
        //            ViewData["Message"] = $"Bu mail kayıtlı. Lutfen giris <a href=\"/login/login\" > yapin <a/>";
        //            return RedirectToAction("SiparisEt", "Basket", model);
        //        }

        //    }

        //    ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
        //    var userId = ViewData["UserId"];
        //    var client = new SmtpClient("smtp.eu.mailgun.org", 587)
        //    {
        //        Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "3b212cffce4a231e162ecd83abce45ea-911539ec-debf1d4c"),
        //        EnableSsl = true
        //    };

        //    var sale = "SELECT * FROM Sales WHERE UserId = @userId ";
        //    var saleSql = connection.Query<Cart>(sale, new { userId }).ToList();

        //    foreach (var item in saleSql)
        //    {
        //        var sqlInsert = "INSERT INTO Cart (UserId,ProductId,Email,CustomerName,CustomerAddress,CustomerCity,CustomerZipCode) VALUES (@UserId,@ProductId,@Email,@CustomerName,@CustomerAddress,@CustomerCity,@CustomerZipCode)";
        //        var data = new
        //        {
        //            item.ProductId,
        //            item.UserId,
        //            model.Email,
        //            CustomerName = model.Name,
        //            CustomerAddress = model.Address,
        //            CustomerCity = model.City,
        //            CustomerZipCode = model.ZipCode,
        //        };
        //        connection.Execute(sqlInsert, data);
        //    }
        //    var subTotalSql = "SELECT SUM(Price * Quantity) AS Subtotal FROM cart WHERE UserId = @userId";
        //    var subTotal = connection.QueryFirstOrDefault<Cart>(subTotalSql, new { userId });
        //    ViewBag.SubTotal = subTotal.Subtotal;

        //    ViewBag.Subject = "Siparişiniz Başarıyla Alındı";
        //    ViewBag.Body = $"<p>Merhaba <strong>{model.Name}</strong>,</p>\r\n        <p>Fruitkha'dan alışveriş yaptığınız için teşekkür ederiz! Siparişiniz başarıyla alındı ve en kısa sürede işleme konulacaktır.</p>\r\n        \r\n        <h3>Sipariş Detayları:</h3>\r\n        <ul>\r\n<li><strong>Toplam Tutar:</strong> {ViewBag.SubTotal} TL</li>\r\n        </ul>\r\n\r\n        <h3>Teslimat Bilgileri:</h3>\r\n        <ul>\r\n            <li><strong>Alıcı Adı:</strong> {model.Name}</li>\r\n            <li><strong>Teslimat Adresi:</strong> {model.Address}</li>\r\n <p>Siparişinizin durumu hakkında sizi bilgilendirmek için e-posta göndermeye devam edeceğiz. Siparişiniz gönderildiğinde, takip numarası ve tahmini teslimat süresi hakkında bilgi alacaksınız.</p>\r\n\r\n            <p>Tekrar teşekkür ederiz ve siparişinizin keyfini çıkarmanızı dileriz!</p>\r\n\r\n            <p>Saygılarımızla,<br>TeknoMarkt</p>";
        //    ViewBag.MessageCssClass = "alert-success";
        //    ViewBag.Message = "Başarıyla kayıt olundu. Onaylamak için mail kutunuza gidin";
        //    SendMail(model);
        //    connection.Execute("DELETE FROM Cart Where UserId =@userId", new { userId });
        //    return View("ThankYou");

        //}
        public IActionResult SendMail(User model,decimal Subtotal)
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
            
            mailMessage.To.Add(new MailAddress($"{model.Email}"));

            client.Send(mailMessage);
            return View();

        }



        public IActionResult SiparisEt()
        {
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];
            using var connection = new SqlConnection(connectionString);
            var cart = connection.Query<Sale>("SELECT * FROM Sales WHERE UserId = @userId", new { userId }).ToList();
            ViewBag.CheckOut = cart;

            // Total sütununu güncellemek için sorgu
            var totalSql = "SELECT SUM(Price * Quantity) AS Total FROM Sales WHERE UserId = @userId;";
            var total = connection.QueryFirstOrDefault<Sale>(totalSql, new { userId });

            // Toplam tutarı ViewBag'e atama
            ViewBag.Total = total.Total;

            return View(new User());
        }



    }
}