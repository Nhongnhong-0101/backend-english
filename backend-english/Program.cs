
using DotNetEnv;
using Infrastructure.Repository;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace backend_english
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Supabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("❌ Không tìm thấy connection string trong appsettings.json");
            }

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000); // chỉ HTTP, không có HTTPS
            });
            //register repositories
            builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
            builder.Services.AddSingleton<IVocabRepository, VocabRepository>();
            builder.Services.AddSingleton<IVSMeaningRepository, VSMeaningRepository>();
            builder.Services.AddSingleton<IWSRepository, WSRepository>();
            builder.Services.AddSingleton<IEmailTemplateRepository, EmailTemplateRepository>();
            builder.Services.AddSingleton<IUSResultRepository, USResultRepository>();
            builder.Services.AddSingleton<ISQuestionRepository, SQuestionRepository>();


            builder.Services.AddSingleton<IVocabService, VocabService>();
            builder.Services.AddSingleton<IWSService, WSService>();
            builder.Services.AddSingleton<IAccountService, AccountService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IValueService, ValueService>();
            builder.Services.AddSingleton<IChatbotService, ChatbotService>();
            builder.Services.AddSingleton<IUSResultService, USResultService>();
            builder.Services.AddSingleton<ISQuestionService,SQuestionService>();

            // Add services to the container.


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính
                options.JsonSerializerOptions.WriteIndented = true; // Format JSON dễ đọc
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "backend_english", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Nhập token ở đây. Ví dụ: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters 
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
                    };

                }

            );
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
