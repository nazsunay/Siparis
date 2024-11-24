namespace Siparis.Models
{
    public class LoginModel
    {

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
        public string? Email { get; set; }
        public int? IsApproved { get; set; }
        public string? ValidKey { get; set; }
        public string? ResetKey { get; set; }
    }

    public class LoginRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ChangePassword
    {
        public int Id { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
