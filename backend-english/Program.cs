
using DotNetEnv;
using Infrastructure.Repository;
using Infrastructure.Repository.Implements;
using Infrastructure.Repository.Interfaces;

namespace backend_english
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Env.Load();

            //string connect = Environment.GetEnvironmentVariable("CONNECTTION_STRING");
            //Console.WriteLine("Connect ne" + connect);
            var connectionString = builder.Configuration.GetConnectionString("Supabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("❌ Không tìm thấy connection string trong appsettings.json");
            }

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính
                options.JsonSerializerOptions.WriteIndented = true; // Format JSON dễ đọc
            });

            //register repositories
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<IVocabRepository, VocabRepository>();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



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
