using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();  // Register AuthService here



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // This should get the connection string from the appsettings.json file
builder.Services.AddDbContext<ApplicationDBContext>(options => // This should register the DbContext with the DI container which will allow it to be injected into other classes
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options => options.EnableRetryOnFailure()));

var app = builder.Build(); 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
    
}

app.UseHttpsRedirection();


app.MapControllers(); // This should map my Controllers apis
app.Run();
