using ExecutorService.Executor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Services.AddScoped<IExecutorService, ExecutorService.Executor.ExecutorService>();
builder.Services.AddScoped<IExecutorRepository, ExecutorRepository>();
builder.Services.AddSingleton<IExecutorConfig, ExecutorConfig>();

app.UseHttpsRedirection();

app.MapGet("/index", () => "Hello world");

app.Run();