namespace Siparis.Models
{
    public class Sale
    {

        public int Id { get; set; }
        public int ProductId { get; set; }  // Satılan ürünün ID'si
        public string ProductName { get; set; }  // Ürünün adı
	
		public decimal Price { get; set; }  // Satış fiyatı
        public int Quantity { get; set; }  // Satılan miktar
        public string? ImgUrl { get; set; }
        public IFormFile Image { get; set; }
        public int UserId { get; set; }
        public int Total { get; set; }

    }

 
    public class Rapor
    {
        public int Id { get; set; }
        public int ProductId { get; set; }  // Satılan ürünün ID'si
        public string ProductName { get; set; }  // Ürünün adı

        public Decimal BuyingPrice { get; set; } //Alış Fiyatı
        public decimal SellingPrice { get; set; }  // Satış fiyatı
        public int Quantity { get; set; }  // Satılan miktar
        public decimal Profit { get; set; }  // Kâr miktarı
        public DateTime SaleDate { get; set; }  // Satış tarihi
    }

    public class Cart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string EMail { get; set; }
        public string CustomerName { get; set; }
        public int CustomerAdress { get; set; }
        public string CustomerCity { get; set; }
        public int Quantity { get; set; }
        public int Subtotal { get; set; }
        public decimal Price { get; set; }
        
    }

}
