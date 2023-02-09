using System.Data.SqlTypes;

namespace APBD5.Models
{
    [Serializable]
    public class Product
    {
        public Product(int IdProduct, string Name, string Description, Decimal Price)
        {
            this.IdProduct = IdProduct;
            this.Name = Name;
            this.Description = Description;
            this.Price = Price;
        }

        public int IdProduct { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Decimal Price { get; set; }
    }
}
