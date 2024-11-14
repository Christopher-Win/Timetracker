using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TimeTracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Get JWT settings from configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var secretKey = jwtSettings["SecretKey"];

// Ensure none of the settings are null or empty
if (string.IsNullOrEmpty(secretKey))
{
    throw new ArgumentNullException(nameof(secretKey), "SecretKey cannot be null or empty.");
}

if (string.IsNullOrEmpty(issuer))
{
    throw new ArgumentNullException(nameof(issuer), "Issuer cannot be null or empty.");
}

if (string.IsNullOrEmpty(audience))
{
    throw new ArgumentNullException(nameof(audience), "Audience cannot be null or empty.");
}

// Convert the secret key to bytes
var key = Encoding.UTF8.GetBytes(secretKey);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Time Tracker",
        Version = "v1"
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Read the token from the cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
                Console.WriteLine("JWT token found in cookie");
            }
            else
            {
                Console.WriteLine("JWT token not found in cookie");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token successfully validated");
            return Task.CompletedTask;
        }
    };
});

// Register application services
builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();  
builder.Services.AddScoped<TimeLogService>();  
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPeerReviewQuestionService, PeerReviewQuestionService>();
builder.Services.AddScoped<IPeerReviewService, PeerReviewService>();
builder.Services.AddScoped<IPeerReviewAnswerService, PeerReviewAnswerService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp",
        builder => builder.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500", "http://localhost:5501")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());  // Use if credentials (cookies) are being sent  // Use if credentials (cookies) are being sent
});

// Configure the database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
    options => options.EnableRetryOnFailure()));

// Build the application
var app = builder.Build();

// Configure Swagger UI
app.UseCors("AllowFrontendApp");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable Authentication and Authorization middleware
app.UseAuthentication();  // Process JWT tokens
app.UseAuthorization();   // Enforce [Authorize] policies

// Map controller routes
app.MapControllers();

// Configure cookie policy
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
});

// Run the application
app.Run();
