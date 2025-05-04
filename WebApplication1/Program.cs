using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Modules.Executor;
using WebApplication17.Executor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IExecutorRepository, ExecutorRepositoryMock>();
builder.Services.AddScoped<IExecutorService, ExecutorService>();
builder.Services.AddSingleton<IExecutorConfig, ExecutorConfig>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:80");