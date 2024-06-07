using Microsoft.AspNetCore.Mvc;
using WebApplicationRedis.Models;
using WebApplicationRedis.Services;

namespace WebApplicationRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly RedisCacheService _redisCacheService;

        public HomeController(RedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var userData = await _redisCacheService.GetUserDataAsync(userId);

            if (userData == null)
            {
                // Simulate fetching from SQL Server
                userData = new UserData
                {
                    UserId = userId,
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 30
                    // Simulate other properties
                };

                await _redisCacheService.SetUserDataAsync(userId, userData);
            }

            return Ok(userData);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SetUser(string userId, [FromBody] UserData userData)
        {
            await _redisCacheService.SetUserDataAsync(userId, userData);
            return Ok();
        }

        [HttpPost("batch")]
        public async Task<IActionResult> SetUserBatch([FromBody] List<UserData> users)
        {
            await _redisCacheService.SetUserDetailsBatchAsync(users);
            return Ok();
        }
    }
}
