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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
			services.Configure<Option.Config>(Configuration.GetSection(Option.Config.Key));
			services.Configure<Option.CAPTCHAConfig>(Configuration.GetSection(Option.CAPTCHAConfig.Key));
			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer("User", x =>
			 {
				 var signingKey = Convert.FromBase64String(Configuration["JWT:SigningSecret"]);
				 x.RequireHttpsMetadata = false;
				 x.SaveToken = true;
				 x.TokenValidationParameters = new TokenValidationParameters
				 {
					 ValidateIssuerSigningKey = true,
					 IssuerSigningKey = new SymmetricSecurityKey(signingKey),
					 ValidateIssuer = true,
					 ValidateAudience = false,
					 ValidateLifetime = false
				 };
			 }).AddJwtBearer("CaptchaCode", x =>
			 {
				 var signingKey = Convert.FromBase64String(Configuration["JWT:SigningSecret"]);
				 x.RequireHttpsMetadata = false;
				 x.SaveToken = true;
				 x.TokenValidationParameters = new TokenValidationParameters
				 {
					 ValidateIssuerSigningKey = true,
					 IssuerSigningKey = new SymmetricSecurityKey(signingKey),
					 ValidateIssuer = true,
					 ValidateAudience = false,
					 ValidateLifetime = false
				 };
			 });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
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
