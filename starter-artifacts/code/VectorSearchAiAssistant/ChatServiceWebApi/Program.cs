using VectorSearchAiAssistant.SemanticKernel.Chat;
using VectorSearchAiAssistant.Service.Interfaces;
using VectorSearchAiAssistant.Service.Models.ConfigurationOptions;
using VectorSearchAiAssistant.Service.Services;

namespace ChatServiceWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:CosmosDB"));

            builder.Services.AddOptions<SemanticKernelRAGServiceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI"));

            builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
            builder.Services.AddSingleton<IRAGService, SemanticKernelRAGService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();

            // System prompt service backed by an Azure blob storage account
            builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:DurableSystemPrompt"));
            builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            builder.Services.AddScoped<ChatEndpoints>();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            // Map the chat REST endpoints:
            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatEndpoints>();
                service?.Map(app);
            }

            app.Run();
        }
    }
}