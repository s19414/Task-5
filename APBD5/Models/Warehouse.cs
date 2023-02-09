namespace APBD5.Models
{
    [Serializable]
    public class Warehouse
    {
        public Warehouse(int idWarehouse, string name, string address)
        {
            IdWarehouse = idWarehouse;
            Name = name;
            Address = address;
        }

        public int IdWarehouse { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
