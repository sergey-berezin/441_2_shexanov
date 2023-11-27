using ObjectDetectionServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//builder.Services.AddScoped<IObjectDetectionService, ObjectDetectionService>();
//builder.Services.AddSingleton<ObjectDetectionService>();
builder.Services.AddSingleton<IObjectDetectionService, ObjectDetectionService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("Cors", builder =>
{
    builder
        .WithOrigins("http://127.0.0.1:5501")
        .WithHeaders("*")
        .WithMethods("*")
        .AllowCredentials();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Cors");

app.MapControllers();

app.Run();

public partial class Program { }