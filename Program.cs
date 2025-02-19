using Microsoft.EntityFrameworkCore;
using Calendar.API.Data;
using System.Text.Json.Serialization;
using Calendar.API.Services;
using Calendar.API.Mappings;


var builder = WebApplication.CreateBuilder(args);

// 加入資料庫服務
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("找不到連接字串 'DefaultConnection'");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 加入 AutoMapper 服務
builder.Services.AddAutoMapper(typeof(TodoProfile));


// 註冊 TodoService
builder.Services.AddScoped<ITodoService, TodoService>();

// 配置日誌服務
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// 加入基本服務

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// 配置開發環境
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();