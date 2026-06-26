using Artifex.Services;

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
            builder.Services.AddScoped<ChatService>();
            // builder.Services.AddScoped<LoreService>();
            // builder.Services.AddScoped<MetaService>();

            builder.Services.AddHttpClient<LLMService>(client =>
                {
                    client.BaseAddress = new Uri("http://127.0.0.1:11434");
                }
            );

            var app = builder.Build();

            // Controller 활성화
            app.MapControllers();

            app.Run();
        }
    }
}