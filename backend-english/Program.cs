
using DotNetEnv;
using Infrastructure.Repository;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Npgsql;

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


            //register repositories
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<IVocabRepository, VocabRepository>();
            builder.Services.AddSingleton<IVSMeaningRepository, VSMeaningRepository>();

            builder.Services.AddSingleton<IVocabService, VocabService>();
            // Add services to the container.


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính
                options.JsonSerializerOptions.WriteIndented = true; // Format JSON dễ đọc
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
