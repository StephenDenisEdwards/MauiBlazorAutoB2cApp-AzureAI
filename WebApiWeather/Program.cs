using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace WebApiWeather
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			// Add an authentication scheme
			// Add an authentication scheme
			//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			//	.AddMicrosoftIdentityWebApi(builder.Configuration);

			//builder.Services
			//	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			//	.AddMicrosoftIdentityWebApi(builder.Configuration);

			builder.Services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApi(options =>
					{
						builder.Configuration.Bind("AzureAd", options);
						options.TokenValidationParameters.NameClaimType = "name";
					},
					jwtOptions => { builder.Configuration.Bind("AzureAd", jwtOptions); });

			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("ReadScope", policy =>
					policy.RequireAuthenticatedUser()
						.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "Weather.Read"));

				options.AddPolicy("ReadWriteScope", policy =>
					policy.RequireAuthenticatedUser()
						.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "Weather.ReadWrite"));
			});



			// Add services to the container.
			builder.Services.AddControllers();

			// Add CORS services
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader();
				});
			});

			// Add Swagger support
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

			// Use CORS before authorization
			app.UseCors("AllowAll");

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}
