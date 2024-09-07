using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AuthService service) : ControllerBase
    {
        // Mock database for demonstration
        private readonly AuthService _service = service; // This creates a private field to store the AuthService called _authService

        [HttpGet("{id}")]
        public ActionResult<User> GetById(string id)
        {
            var user = _service.GetById(id);

            if(user is not null)
            {
                return user;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Create(User newUser)
        {
            var user = _service.CreateUser(newUser);
            return CreatedAtAction(nameof(GetById), new { id = user!.NetID }, user);
        }

        // [HttpPost]
        // public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        // {
        //     _context.TodoItems.Add(todoItem);
        //     await _context.SaveChangesAsync();

        //     //    return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        //     return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        // }

        // [HttpPost("register")]
    //     public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    //     {
    //         // Validate the request data
    //         if (!ModelState.IsValid)
    //         {
    //             return BadRequest(ModelState);
    //         }

    //         // Create a new user object
    //         var user = new User
    //         {
    //             NetID = request.NetID,
    //             Password = request.Password
    //         };

    //         // Save the user to the database
    //         var result = await _authService.CreateUser(user);

    //         if (result.Success)
    //         {
    //             Console.WriteLine($"User created successfully: {user.NetID}");
    //             return Ok($"User created successfully: {user.NetID}");
    //         }

    //         Console.WriteLine($"Failed to register user: {user.NetID}");
    //         return BadRequest(result.Message);
    //     }
    //     public class RegisterRequest
    //     {
    //         public required string NetID { get; set; }

    //         public required string Password { get; set; }
    //     }

    //     [HttpPost("login")]
    //     public async Task<IActionResult> Login([FromForm] LoginRequest request)
    //     {
    //         var user = await _authService.AuthenticateUser(request.NetID, request.Password);

    //         if (user != null)
    //         {
    //             Console.WriteLine($"Login successful for: {request.NetID}");
    //             return Ok();
    //         }
    //         Console.WriteLine($"Login failed for username: {request.NetID}");
    //         return Unauthorized();
    //     }
    // }

    // public class LoginRequest
    // {
    //     public string NetID { get; set; } = string.Empty; // or make it nullable with `string?`
    //     public string Password { get; set; } = string.Empty;
    }
}