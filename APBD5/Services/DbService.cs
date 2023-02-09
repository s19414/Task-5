using APBD5.Models;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace APBD5.Services
{
    public class DbService
    {
        private string connectionString;
        public DbService(IConfiguration _configuration)
        {
            connectionString = _configuration.GetConnectionString("PJATK_DB");
        }
        //in code
        public async Task<int> RegisterProduct(ProductToRestock productToRestock)
        {
            if (productToRestock.Amount <= 0)
            {
                //-1 signifies Amount argument is 0 or lower
                return -1;
            }
            Task<IEnumerable<Product>> getProductsTask = GetProductsAsync();
            Task<IEnumerable<Warehouse>> getWarehousesTask = GetWarehousesAsync();
            Task<IEnumerable<Order>> getOrdersTask = GetOrdersAsync();
            Task<IEnumerable<Product_Warehouse>> getProduct_Warehouses = GetProduct_WarehousesAsync();
            bool productExists = false;
            bool warehouseExists = false;
            bool orderExists = false;
            bool orderPreviouslyCompleted = false;
            int orderID = 0;
            Decimal priceOfProduct = 0;
            foreach (Product product in await getProductsTask)
            {
                if (product.IdProduct == productToRestock.IdProduct) {
                    priceOfProduct = product.Price;
                    productExists = true;
                }
            }
            foreach (Warehouse warehouse in await getWarehousesTask) {
                if( warehouse.IdWarehouse == productToRestock.IdWarehouse)
                {
                    warehouseExists = true;
                }
            }
            foreach (Order order in await getOrdersTask)
            {
                if(order.IdProduct == productToRestock.IdProduct &&
                    order.Amount == productToRestock.Amount &&
                    order.CreatedAt < productToRestock.CreatedAt)
                {
                    orderExists = true;
                    orderID = order.IdOrder;
                    //check if order was previously completed
                    foreach (Product_Warehouse pw in await getProduct_Warehouses)
                    {
                        if (order.IdOrder == pw.IdOrder)
                        {
                            orderPreviouslyCompleted = true;
                        }
                    }
                }
            }
            
            if (!productExists || !warehouseExists || !orderExists)
            {
                //-2 signifies product/warehouse/order don't exist
                return -2;
            }
            if(orderPreviouslyCompleted)
            {
                //-3 signifies the order was previously completed
                return -3;
            }
            //BEGIN DB OPERATIONS
            //UPDATE Order.FulfilledDate
            DateTime fulfilledDate = DateTime.Now;
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(@"UPDATE [Order] SET FulfilledAt=@fulfilledAt WHERE IdOrder=@IdOrder;"))
            {
                com.Connection = con;
                com.Parameters.AddWithValue("@fulfilledAt", fulfilledDate.ToString());
                com.Parameters.AddWithValue("@IdOrder", orderID);
                Task openAsync = con.OpenAsync();
                await openAsync;
                await com.ExecuteNonQueryAsync();
            }
            //INSERT record to Product_Warehouse
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(@"INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);"))
            {
                com.Connection = con;
                //TEMP
                com.Parameters.AddWithValue("@IdWarehouse", productToRestock.IdWarehouse);
                com.Parameters.AddWithValue("@IdProduct", productToRestock.IdProduct);
                com.Parameters.AddWithValue("@IdOrder", orderID);
                com.Parameters.AddWithValue("@Amount", productToRestock.Amount);
                com.Parameters.AddWithValue("@Price", (Decimal)(productToRestock.Amount * priceOfProduct));
                Console.WriteLine("DATE TO STRING: " + fulfilledDate.ToString() + ", long date: " + fulfilledDate.ToLongDateString());
                com.Parameters.AddWithValue("@CreatedAt", fulfilledDate.ToString());
                await con.OpenAsync();
                await com.ExecuteNonQueryAsync();
            }
            using (SqlConnection con =  new SqlConnection(connectionString))
            {
                SqlCommand com = new SqlCommand(@"SELECT IdProductWarehouse FROM Product_Warehouse WHERE CreatedAt=@fulfilledDate AND IdOrder=@IdOrder 
                                                AND IdProduct=@IdProduct AND IdWarehouse=@IdWarehouse AND Amount=@Amount", con);
                com.Parameters.AddWithValue("@fulfilledDate", fulfilledDate.ToString());
                com.Parameters.AddWithValue("@IdOrder", orderID);
                com.Parameters.AddWithValue("@IdProduct", productToRestock.IdProduct);
                com.Parameters.AddWithValue("@IdWarehouse", productToRestock.IdWarehouse);
                com.Parameters.AddWithValue("@Amount", productToRestock.Amount);
                await con.OpenAsync();
                SqlDataReader dataReader = await com.ExecuteReaderAsync();
                
                int result = -10;
                while(await dataReader.ReadAsync())
                {
                    result = (int)dataReader["IdProductWarehouse"];
                }
                return result;
            }
        }

        //with stored procedure
        public async Task<int> RegisterProductStoredProc(ProductToRestock productToRestock)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("AddProductToWarehouse"))
            {
                cmd.Connection = con;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdProduct", productToRestock.IdProduct);
                cmd.Parameters.AddWithValue("@IdWarehouse", productToRestock.IdWarehouse);
                cmd.Parameters.AddWithValue("@Amount", productToRestock.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", productToRestock.CreatedAt.ToString());
                await con.OpenAsync();
                SqlDataReader rd = await cmd.ExecuteReaderAsync();
                int result = -10;
                while (await rd.ReadAsync())
                {
                    result = (int)rd["NewId"];
                }
                return result;
                
            }
        }
        //HELPER FUNCTIONS
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand com = new SqlCommand("SELECT * FROM Product", con);
            await con.OpenAsync();
            SqlDataReader dataReader = await com.ExecuteReaderAsync();
            List<Product> result = new List<Product>();
            while (await dataReader.ReadAsync()) {
                result.Add(new Product((int)dataReader["IdProduct"],
                    dataReader["Name"].ToString(),
                    dataReader["Description"].ToString(),
                    (Decimal)dataReader["Price"]));
            }
            return result;
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesAsync()
        {
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand com = new SqlCommand("SELECT * FROM Warehouse", con);
            await con.OpenAsync();
            SqlDataReader dataReader = await com.ExecuteReaderAsync();
            List<Warehouse> result = new List<Warehouse>();
            while (await dataReader.ReadAsync())
            {
                result.Add(new Warehouse(
                    (int)dataReader["IdWarehouse"],
                    dataReader["Name"].ToString(),
                    dataReader["Address"].ToString()
                    ));
            }
            return result;
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand com = new SqlCommand("SELECT * FROM [Order]", con);
            await con.OpenAsync();
            SqlDataReader dataReader = await com.ExecuteReaderAsync();
            List<Order> result = new List<Order>();
            while (await dataReader.ReadAsync())
            {
                result.Add(new Order(
                        (int)dataReader["IdOrder"],
                        (int)dataReader["IdProduct"],
                        (int)dataReader["Amount"],
                        (DateTime)dataReader["CreatedAt"],
                        null
                        ));
                if (dataReader["FulfilledAt"].GetType() != typeof(DBNull))
                {
                    Console.WriteLine("WASSSSSUP");
                    result[result.Count - 1].FulfilledAt = (DateTime)dataReader["FulfilledAt"];
                }
            }
            return result;
        }

        public async Task<IEnumerable<Product_Warehouse>> GetProduct_WarehousesAsync()
        {
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand com = new SqlCommand("SELECT * FROM Product_Warehouse", con);
            await con.OpenAsync();
            SqlDataReader dataReader = await com.ExecuteReaderAsync();
            List<Product_Warehouse> result = new List<Product_Warehouse>();
            while (await dataReader.ReadAsync())
            {
                result.Add(new Product_Warehouse(
                    (int)dataReader["IdProductWarehouse"],
                    (int)dataReader["IdWarehouse"],
                    (int)dataReader["IdProduct"],
                    (int)dataReader["IdOrder"],
                    (int)dataReader["Amount"],
                    (Decimal)dataReader["Price"],
                    (DateTime)dataReader["CreatedAt"]
                    ));
            }
            return result;
        }
    }
}
