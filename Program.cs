using Microsoft.EntityFrameworkCore; // Provides ORM functionality for database interactions
using TimeTracker.Data; // References the application's database context
using TimeTracker.Services; // Includes application services like AuthService and TimeLogService
using Microsoft.AspNetCore.Authentication.JwtBearer; // Enables JWT authentication middleware
using Microsoft.IdentityModel.Tokens; // Provides tools for JWT token validation
using System.Text; // Allows encoding for converting strings to bytes
using Microsoft.AspNetCore.Identity; // Provides utilities for password hashing
using TimeTracker.Models; // References application models like User

var builder = WebApplication.CreateBuilder(args); // Sets up the application builder

// Get JWT settings from configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings"); // Reads JWT-related settings from appsettings.json
var issuer = jwtSettings["Issuer"]; // Issuer of the JWT token
var audience = jwtSettings["Audience"]; // Audience for which the JWT token is intended
var secretKey = jwtSettings["SecretKey"]; // Secret key used for signing the JWT token

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
var key = Encoding.UTF8.GetBytes(secretKey); // Converts the secret key string into a byte array

// Add services to the container
builder.Services.AddEndpointsApiExplorer(); // Enables API endpoint discovery
builder.Services.AddSwaggerGen(); // Adds Swagger for API documentation
builder.Services.ConfigureSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Time Tracker", // Title for the Swagger UI
        Version = "v1" // API version
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Default authentication scheme
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Default challenge scheme
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Validates the token's issuer
        ValidateAudience = true, // Validates the token's audience
        ValidateLifetime = true, // Ensures the token is not expired
        ValidateIssuerSigningKey = true, // Validates the signing key
        ValidIssuer = issuer, // Expected issuer
        ValidAudience = audience, // Expected audience
        IssuerSigningKey = new SymmetricSecurityKey(key) // Signing key
    };

    // Read the token from the cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"]; // Reads JWT token from a cookie
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token; // Sets the token for further validation
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
            Console.WriteLine($"Authentication failed: {context.Exception.Message}"); // Logs failure messages
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token successfully validated"); // Logs successful validation
            return Task.CompletedTask;
        }
    };
});

// Register application services
builder.Services.AddControllers(); // Adds support for controller-based API endpoints
builder.Services.AddScoped<AuthService>(); // Registers AuthService with scoped lifetime
builder.Services.AddScoped<TimeLogService>(); // Registers TimeLogService with scoped lifetime
builder.Services.AddScoped<IUserService, UserService>(); // Registers IUserService with UserService implementation
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>(); // Registers password hashing for User model
builder.Services.AddScoped<IPeerReviewQuestionService, PeerReviewQuestionService>(); // PeerReviewQuestion service registration
builder.Services.AddScoped<IPeerReviewService, PeerReviewService>(); // PeerReview service registration
builder.Services.AddScoped<IPeerReviewAnswerService, PeerReviewAnswerService>(); // PeerReviewAnswer service registration

builder.Services.AddCors(options =>
{
    // Configures Cross-Origin Resource Sharing (CORS) policies
    options.AddPolicy("AllowFrontendApp",
        builder => builder.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500", "http://localhost:5501") // Allowed origins
                        .AllowAnyHeader() // Allows any headers
                        .AllowAnyMethod() // Allows any HTTP methods
                        .AllowCredentials()); // Enables sending credentials like cookies
});

// Configure the database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Reads the database connection string
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
    options => options.EnableRetryOnFailure())); // Configures the database context with retry logic

// Build the application
var app = builder.Build();

// Configure Swagger UI
app.UseCors("AllowFrontendApp"); // Applies the CORS policy
app.UseSwagger(); // Enables Swagger middleware
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); // Sets the endpoint for Swagger documentation
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) // Checks if the environment is development
{
    app.UseSwagger(); // Enables Swagger for development
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

// Enable HTTPS redirection
app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS

// Enable Authentication and Authorization middleware
app.UseAuthentication();  // Processes JWT tokens for authenticated requests
app.UseAuthorization();   // Enforces authorization policies defined in controllers

// Map controller routes
app.MapControllers(); // Maps endpoints to controllers based on routing attributes

// Configure cookie policy
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax, // Configures cookies to be accessible with Lax SameSite settings
});

// Run the application
app.Run(); // Starts the application