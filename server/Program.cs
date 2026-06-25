namespace Artifex
{
    class MainClass
    {
        static void Main(string[] args)
        {
            DatabaseUtilClass.Init();

            var builder = WebApplication.CreateBuilder(args);

            // Controller 등록
            builder.Services.AddControllers();

            // Service 등록
            builder.Services.AddScoped<Services.ChatService>();
            // builder.Services.AddScoped<LoreService>();
            // builder.Services.AddScoped<MetaService>();

            var app = builder.Build();

            // Controller 활성화
            app.MapControllers();

            app.Run();
        }
    }
}