using System.Net;
using AonFreelancing.Contexts;
using AonFreelancing.Enums;
using AonFreelancing.Hubs;
using AonFreelancing.Middlewares;
using AonFreelancing.Models;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using AonFreelancing.Configs;
using AonFreelancing.Jobs;
using AonFreelancing.Models.Documents;
using ZainCash.Net.Extensions;
using ZainCash.Net.Services;

namespace AonFreelancing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var conf = builder.Configuration;

            var kestrelEndpointsConfig = conf.GetSection("Kestrel:Endpoints");
            string? httpUrl = kestrelEndpointsConfig["Http:Url"];
            string? httpsUrl = kestrelEndpointsConfig["Https:Url"];

            if (httpUrl == null || httpsUrl == null)
                throw new InvalidOperationException(
                    "Missing required Kestrel endpoint configuration. Http and Https urls must be provided in configuration.");
            
            builder.WebHost.UseUrls(httpUrl, httpsUrl); // Allow all interfaces

            bool isSslEnabled = kestrelEndpointsConfig.GetValue<bool>("Https:Ssl:Enabled");
            if (isSslEnabled)
            {
                string? certPath = kestrelEndpointsConfig["Https:Ssl:CertPath"];
                string? certPassword = kestrelEndpointsConfig["Https:Ssl:Password"];
                if (certPath == null || certPassword == null)
                    throw new InvalidOperationException(
                        "SSL certificate configuration is missing. CertPath and Password must be provided in configuration.");
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Listen(IPAddress.Any, new Uri(httpUrl).Port); 
                    options.Listen(IPAddress.Any, new Uri(httpsUrl).Port,
                    listenOptions => { listenOptions.UseHttps(certPath, certPassword); });
                    options.Configure(config: conf);
                });
            }

            builder.Services.AddControllers(o => o.SuppressAsyncSuffixInActionNames = false)
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals);

            builder.Services.AddSingleton<OtpManager>();
            builder.Services.AddSingleton<JwtService>();
            builder.Services.AddSingleton<FileStorageService>();
            builder.Services.AddSingleton<InMemorySignalRUserConnectionService>();
            builder.Services.AddScoped<PushNotificationService>();
            builder.Services.AddScoped<OTPService>();
            builder.Services.AddScoped<TempUserService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RoleService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<ProjectLikeService>();
            builder.Services.AddScoped<ProjectService>();
            builder.Services.AddScoped<RatingService>();
            builder.Services.AddScoped<TaskService>();
            builder.Services.AddScoped<SkillsService>();
            builder.Services.AddScoped<BidService>();
            builder.Services.AddScoped<FreelancerService>();
            builder.Services.AddScoped<ActivitiesService>();
            builder.Services.AddScoped<CommentService>();
            builder.Services.AddScoped<ProfileService>();
            builder.Services.AddScoped<SubscriptionsService>();
            builder.Services.AddScoped<ElasticService<UserDocument>>();
            builder.Services.AddScoped<ElasticService<Project>>();
            builder.Services.AddScoped<SignalingService>();

            builder.Services.AddScoped<ZainCashService>();
            builder.Services.AddZainCashConfig("ZainCash", builder.Configuration);

            builder.Services.AddHostedService<ElsSetupJob>();
            builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticSettings"));

            builder.Services.AddDbContext<MainAppContext>(options =>
                options.UseSqlServer(conf.GetConnectionString("Default")));
            builder.Services.AddIdentity<User, ApplicationRole>()
                .AddEntityFrameworkStores<MainAppContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Configuration.AddJsonFile("appsettings.json");
            // JWT Authentication configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty);

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            // If the request is for a hub
                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/Hubs")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            int preFlightMaxAge = int.Parse(builder.Configuration.GetSection("Cors")["PreFlightMaxAge"]);
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    //builder.WithMethods();
                    //builder.AllowCredentials();
                    builder.SetPreflightMaxAge(TimeSpan.FromMinutes(preFlightMaxAge));
                });
            });

            var app = builder.Build();

            //seed roles to the database
            using (var serviceScope = app.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                SeedRoles(roleManager).GetAwaiter().GetResult();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // just for testing
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(FileStorageService.ROOT),
                RequestPath = "/images"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/html")),
                RequestPath = "/pages"
            });
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationsHub>("/Hubs/Notifications");
            app.MapHub<SignalingHub>("/Hubs/Signaling");
            app.Run();
        }

        static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            foreach (string roleName in Enum.GetNames(typeof(UserRoles)))
            {
                string normalizedRoleName = roleName.ToLower();
                if (!await roleManager.RoleExistsAsync(normalizedRoleName))
                    await roleManager.CreateAsync(new ApplicationRole { Name = normalizedRoleName });
            }
        }
    }
}