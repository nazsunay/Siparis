using Microsoft.AspNetCore.Http.HttpResults;
using System.Data.Common;
using System;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Siparis.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImgUrl { get; set; }
        public IFormFile Image { get; set; }
        public DateTime DateCreated { get; set; }
        public string Detail { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Yeni eklenen alanlar
        public string? Comment { get; set; }
        public List<ProductComment>? Comments { get; set; }
        public int? CommentCount { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? ProductId { get; set; }
    }

    public class Search()
    {
        public string SearchProduct { get; set; }
    }
    public class Report()
    {
        public int SaleId { get; set; }
        public string ProductName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal BuyingPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Profit { get; set; }
        public DateTime SaleDate { get; set; }
    }

   
    public class ProductComment
    {
        public int Id { get; set; }
        
        public string Text { get; set; }
       
        public int? ProductId { get; set; }
        public DateTime DateCreated { get; set; }

    }

    public class ProductModel
    {
        public int Id { get; set; }
        public bool? IsApproved { get; set; }
        public Product Product { get; set; }

        public List<Product> Products { get; set; }
        public List<ProductComment> Comments { get; set; }
    }


}
