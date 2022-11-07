using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var db_host = Environment.GetEnvironmentVariable("DB_HOST");
var db_name = Environment.GetEnvironmentVariable("DB_NAME");
var db_password = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");

var connectionString = $"Data Source={db_host};Initial Catalog={db_name};User ID=sa;Password={db_password};";

builder.Services.AddDbContext<PersonDbContext>(option => option.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
