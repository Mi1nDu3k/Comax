using Comax.Business.Interfaces;
using Comax.Common.DTOs.User;
using Comax.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterDTO dto)
        {
            var user = await _userService.RegisterAsync(dto);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDTO>> Login([FromBody] LoginDTO dto)
        {
            var result = await _userService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
    }
}
