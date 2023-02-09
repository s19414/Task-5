namespace APBD5.Models
{
    [Serializable]
    public class ProductToRestock
    {
        public ProductToRestock(int IdProduct, int IdWarehouse, int Amount, DateTime CreatedAt) {
            this.IdProduct = IdProduct;
            this.IdWarehouse = IdWarehouse;
            this.Amount = Amount;
            this.CreatedAt = CreatedAt;
        }

        public int IdProduct { get; set; }
        public int IdWarehouse { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
