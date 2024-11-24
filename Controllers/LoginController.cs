using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Siparis.Models;
using Dapper;

namespace Siparis.Controllers
{
	public class LoginController : Controller
	{

		string connectionString = "";
		public IActionResult Index()
		{

            ViewData["email"] = HttpContext.Session.GetString("email");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            return View(new LoginModel());
        }
		[HttpPost]
		public IActionResult Index(LoginModel model)
		{
			if (model.Email == null || model.Password == null)
			{
				ViewData["Error"] = "Form eksik veya hatalı!";
				return View("Index", "Login");
			}
			using var connection = new SqlConnection(connectionString);
			var login = connection.Query<LoginModel>("SELECT * FROM Users").ToList();

			foreach (var user in login)
			{
				if (user.Email == model.Email && user.Password == model.Password )
				{
					ViewData["Msg"] = "Giriş Başarılı";
					HttpContext.Session.SetString("email", user.Email);
					HttpContext.Session.SetInt32("Id", user.Id);
					HttpContext.Session.SetString("UserName", user.Name);

					ViewBag.IdUser = user.Id;
					return RedirectToAction("Index", "Home");

				}
				ViewData["Msg"] = "Kullanıcı adı veya şifre yanlış";

			}
			return View("Index", model);
		}

		public IActionResult Exit()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Login");
		}

		public IActionResult SendMail(LoginModel model)
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
        
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
		public IActionResult SignUp(LoginModel model)
		{
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Msg");
            }
            using var connection = new SqlConnection(connectionString);
            var login = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users WHERE Email = @Email", new { model.Email });

            if (model.Password != model.PasswordRepeat)
            {
                ViewData["Message"] = "Sifreler uyusmuyor";
                return View("Index", model);
            }
            else if (login?.Email == model.Email)
            {
                ViewData["Message"] = "Bu mail kayıtlı";
                return View("Index", model);
            }

            else
            {
                model.Password = Helper.Hash(model.Password);

                var client = new SmtpClient("smtp.eu.mailgun.org", 587)
                {
                    Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "3b212cffce4a231e162ecd83abce45ea-911539ec-debf1d4c"),
                    EnableSsl = true
                };


                var signup = "INSERT INTO users (Name, Password, Email) VALUES (@Name, @Password, @Email)";

                var data = new
                {
                    model.Name,
                    model.Password,
                    model.Email,
                };

                var rowsAffected = connection.Execute(signup, data);

                ViewBag.Subject = "Hoş Geldiniz! Kayıt İşleminiz Başarıyla Tamamlandı";
                ViewBag.Body = $"<h1>Hoş Geldiniz, {model.Name}!</h1>\r\n            <p>Web sitemize kayıt olduğunuz için teşekkür ederiz. Kayıt işleminiz başarıyla tamamlandı.</p>\r\n            <p>Aşağıdaki bilgileri gözden geçirebilirsiniz:</p>\r\n            <ul>\r\n                <li><strong>Kullanıcı Adı:</strong> </li>\r\n                <li><strong>E-posta:</strong> {model.Email}</li>\r\n            </ul>\r\n            <p>Hesabınızı doğrulamak ve hizmetlerimizden yararlanmaya başlamak için <a href=>buraya tıklayın</a>.</p>\r\n            <p>İyi günler dileriz!</p>";
                ViewBag.MessageCssClass = "alert-success";
                ViewBag.Message = "Başarıyla kayıt olundu. Onaylamak için mail kutunuza gidin";
                SendMail(model);
                return View("Message");
            }
        }

		public IActionResult Account(string id)
		{
			using var connection = new SqlConnection(connectionString);
			var account = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });

			return View(account);
		}

		public IActionResult ConfirmAccount(int id)
		{
			using var connection = new SqlConnection(connectionString);
			var students = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users", new { Id = id });

			var sql = "UPDATE Users SET IsApproved = 1 WHERE Id = @Id";
			var affectedRows = connection.Execute(sql, new { Id = id });

			return RedirectToAction("Index");
		}

		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpPost]
		public IActionResult ForgotPassword(LoginModel model)
		{
			using var connection = new SqlConnection(connectionString);
			var login = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users WHERE Email = @Email", new { model.Email });

			if (!(login == null))
			{
				var client = new SmtpClient("smtp.eu.mailgun.org", 587)
				{
                    Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "3b212cffce4a231e162ecd83abce45ea-911539ec-debf1d4c"),
                    EnableSsl = true
                };
				var Key = Guid.NewGuid().ToString();
				var change = "UPDATE users SET ResetKey = @ResetKey WHERE Email=@Email";
				var parameters = new
				{
					Email = login.Email,
					ResetKey = Key
				};
				var affectedRows = connection.Execute(change, parameters);

				ViewBag.Subject = "Şifre Sıfırlama Talebiniz";
				ViewBag.Body = $"<p>Merhaba {model.UserName},</p>\r\n            <p>Şifrenizi sıfırlamak için bir talepte bulunduğunuzu aldık. Lütfen aşağıdaki bağlantıya tıklayarak şifrenizi sıfırlayın:</p>\r\n            <p><a href=\"https://localhost:7145/Login/ResetPassword/{Key}\" class=\"button\">Şifreyi Sıfırla</a></p>\r\n            <p>Bu bağlantı, güvenliğiniz için 24 saat geçerli olacaktır. Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>\r\n            <p>Şifrenizi sıfırlama konusunda herhangi bir sorun yaşarsanız, bizimle iletişime geçmekten çekinmeyin.</p>";
				ViewBag.MessageCssClass = "alert-success";
				ViewBag.Message = "Şifre Sıfırlama Talebiniz Başarıyla Alındı. Lütfen mail kutunuza gidin";
				ViewBag.Return = "Message";
				SendMail(model);
				return View("Msg");
			}
			else
			{
				@ViewData["Msg"] = "Bu E-Postaya ait bir kayıt bulunamadı.";
				return View();
			}
		}
        //public IActionResult ResetPassword(string id)
        //{
        //	using var connection = new SqlConnection(connectionString);
        //	var account = connection.QueryFirstOrDefault<LoginModel>("SELECT * FROM Users WHERE ResetKey = @ResetKey", new { ResetKey = id });

        //	return View(account);
        //}
        //[HttpPost]
        //public IActionResult ResetPassword(LoginModel model)
        //{
        //	using var connection = new SqlConnection(connectionString);
        //	var mail = "SELECT * FROM Users";
        //	var sql = "UPDATE Users SET Password = @Password WHERE Id=@Id";

        //	var parameters = new
        //	{
        //		model.Password,
        //		model.Id,
        //	};

        //	var affectedRows = connection.Execute(sql, parameters);
        //	ViewBag.Message = "Şifre Güncellendi.";
        //	ViewBag.MessageCssClass = "alert-success";
        //	ViewBag.Login = "Giris";
        //	return View("Msg");
        //}

        
    }
}
