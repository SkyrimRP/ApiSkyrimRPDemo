using ApiSkyrimRP.Core;
using ApiSkyrimRP.Middlewares;
using Database;
using Domain;
using Domain.Services.JwtAuthManager.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace ApiSkyrimRP
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
            ServerVersion serverVersion = ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"));

            services.AddDbContext<DatabaseContext>(options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), serverVersion));

            JwtTokenConfig jwt = Configuration.GetSection("JWT").Get<JwtTokenConfig>();
            services.AddSingleton(jwt);

            services.AddDomain();

            services.AddTransient<ServerAccessControlMiddleware>();

            services.AddSingleton(new ServersCacheService());
            services.AddHostedService<ServersCache>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Skyrim RP Api",
                    Description = "Демонстрационная версия Api. Совместимая с серверами SkyMP",
                    Version = "demo",
                    License = new() { Name = "" },
                    Contact = new() { Name = "Skyrim RP", Url = new("https://discord.gg/fa8qW29UdN") }
                });

                OpenApiSecurityScheme securityScheme = new()
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, Array.Empty<string>()}
                });

                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwt.SecretKey)),

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            IConfigurationSection EmailSection = Configuration.GetSection("Email");
            services.AddSingleton(new MailService(
                EmailSection["From"],
                EmailSection["Login"],
                EmailSection["Password"],
                EmailSection["ServerAddress"],
                ushort.Parse(EmailSection["ServerPort"])));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiSkyrimRP v1"));
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ServerAccessControlMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
