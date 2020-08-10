using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Blog.Extension;

namespace Blog
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging();
			services.AddControllers().AddApplicationPart(typeof(Controllers.IndexController).Assembly);
			services.AddApiVersioning(option =>
			{
				option.DefaultApiVersion = new ApiVersion(1, 0);
				option.ReportApiVersions = true;
				option.AssumeDefaultVersionWhenUnspecified = true;
				option.ApiVersionReader = new HeaderApiVersionReader("X-Version");
			});
			services.AddSwaggerGen(option =>
			{
				option.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog Web API", Version = "v1" });
				option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey
				});
			});
			services.AddService(option =>
			{
				option.InterfaceAssembly = typeof(Service.Anchor).Assembly;
				option.AddImplementation(Extension.Emit.BuildAssembly.Build(typeof(Service.Implementation.Anchor).Assembly))
				.AddImplementation(typeof(Service.Implementation.Anchor).Assembly);
			});
			services.Configure<Config.Config>(Configuration.GetSection(Config.Config.Key));
			services.AddSingleton<Service.Test>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.Use(async (context, next) =>
		   {
			   try
			   {
				   await next();
			   }
			   catch (Exception ex)
			   {
				   Console.WriteLine("Configure");
				   Console.WriteLine(ex.ToString());
				   Console.WriteLine("InnerException");
				   Console.WriteLine(ex.InnerException?.ToString());
				   Console.WriteLine("Message");
				   Console.WriteLine(ex.Message);
				   throw;
			   }

		   });
			app.UseHttpsRedirection();
			app.UseResponseCaching();
			app.UseRouting();
			app.UseAuthorization();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
