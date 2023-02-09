using APBD5.Services;
using APBD5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD5.Controllers
{
    [Route("api/warehouses2")]
    [ApiController]
    public class Warehouses2Controller : ControllerBase
    {
        private DbService _dbService;
        public Warehouses2Controller(DbService dbService)
        {
            _dbService = dbService;
        }
        [HttpPost]
        public async Task<IActionResult> AddStock(ProductToRestock arg)
        {
            int result = await _dbService.RegisterProductStoredProc(arg);
            return Ok("ID of new Product_Warehouse is: " + result);
        }
    }
}
