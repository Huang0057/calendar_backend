using Microsoft.EntityFrameworkCore;
using Calendar.API.Data;
using System.Text.Json.Serialization;
using Calendar.API.Services;
using Calendar.API.Services.Interfaces;
using Calendar.API.Mappings;
using Calendar.API.Repositories;
using Calendar.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 加入資料庫服務 - 簡化為只使用配置中的連接字串
// 決定使用哪個連接字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 檢查是否在 Docker 環境中 (通過環境變數判斷)
var dockerConnectionString = builder.Configuration.GetConnectionString("DockerConnection") ??
                            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// 如果在 Docker 中，則使用 Docker 的連接字串
if (!string.IsNullOrEmpty(dockerConnectionString))
{
    connectionString = dockerConnectionString;
    Console.WriteLine("使用 Docker 連接字串");
}
else
{
    Console.WriteLine("使用本地開發連接字串");
}

// 添加 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(connectionString));

// 加入 AutoMapper 服務
builder.Services.AddAutoMapper(typeof(TodoProfile));

// 註冊應用服務
builder.Services.AddApplicationServices(builder.Configuration);

// 配置JWT認證
builder.Services.AddJwtAuthentication(builder.Configuration);

// 配置CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// 註冊 TodoService
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

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

// 確保資料庫已創建並應用遷移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // 使用 context.Database.EnsureCreated() 確保資料庫已創建
        context.Database.EnsureCreated();
        
        // 或者使用 Migrate() 應用遷移（如果有）
        // context.Database.Migrate();
        
        Console.WriteLine("資料庫初始化成功");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "資料庫初始化時發生錯誤");
        Console.WriteLine($"資料庫初始化錯誤: {ex.Message}");
    }
}

// 在所有環境中啟用 Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseGlobalExceptionHandler(app.Environment);

app.Run();