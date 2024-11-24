using Microsoft.AspNetCore.Http.HttpResults;
using Siparis.Models;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace Siparis.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string? ImgUrl { get; set; }
        public IFormFile Image { get; set; }
        public int ProductId { get; set; }

       

    }
}
