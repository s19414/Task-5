using APBD5.Models;
using APBD5.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD5.Controllers
{
    [Route("api/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        //take care of dependencies
        private DbService _dbService;
        public WarehousesController(DbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(ProductToRestock arg)
        {
            //error codes from 
            const int AMOUNT_IS_TOO_LOW = -1;
            const int ARG_DOESNT_EXIST = -2;
            const int ORDER_PREVIOUSLY_COMPLETED = -3;

            int returnCode = await _dbService.RegisterProduct(arg);
            switch (returnCode)
            {
                case AMOUNT_IS_TOO_LOW:
                    {
                        return BadRequest("Amount must be higher than 0!");
                    }
                case ARG_DOESNT_EXIST:
                    {
                        return NotFound("Product, Warehouse or Order doesn't exist!");
                    }
                case ORDER_PREVIOUSLY_COMPLETED:
                    {
                        return BadRequest("Order was previously completed!");
                    }
                default:
                    {
                        return Ok("New Product_Warehouse entry ID is: " + returnCode);
                    }

            }
        }
    }
}
