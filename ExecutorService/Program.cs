using ExecutorService.Executor;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IExecutorRepository, ExecutorRepositoryMock>();
builder.Services.AddScoped<ICodeExecutorService, CodeExecutorService>();
builder.Services.AddSingleton<IExecutorConfig, ExecutorConfig>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// app.UseAuthentication();
//
// app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:1337");