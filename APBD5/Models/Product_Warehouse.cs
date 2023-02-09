using System.Data.SqlTypes;

namespace APBD5.Models
{
    [Serializable]
    public class Product_Warehouse
    {

        public Product_Warehouse(int idProductWarehouse, int idWarehouse, int idProduct, int idOrder, int amount, Decimal price,DateTime createdAt)
        {
            IdProductWarehouse = idProductWarehouse;
            IdWarehouse = idWarehouse;
            IdProduct = idProduct;
            IdOrder = idOrder;
            Amount = amount;
            Price = price;
            CreatedAt = createdAt;
        }

        public int IdProductWarehouse { get; set; }
        public int IdWarehouse { get;set; }
        public int IdProduct { get; set; }
        public int IdOrder { get; set; }
        public int Amount { get; set; }
        public Decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
