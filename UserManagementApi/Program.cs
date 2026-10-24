using UserManagementApi.Configurations;
using UserManagementApi.Data.Access;
using UserManagementApi.Data.Repositories;
using UserManagementApi.Data.Seeders;
using UserManagementApi.Modules.Authentication.Filters;
using UserManagementApi.Modules.Authentication.Services;
using UserManagementApi.Modules.Users.Services;
using UserManagementApi.Security;
using UserManagementApi.Shared.Helpers;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

namespace UserManagementApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.AddPolicy("DevCors", policy =>
                    {
                        policy.WithOrigins("https://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
                }
                else
                {
                    options.AddPolicy("ProdCors", policy =>
                    {
                        var allowedOrigins = builder.Configuration
                            .GetSection("AllowedOrigins")
                            .Get<string[]>() ?? [];

                        policy.WithOrigins(allowedOrigins)          //  explicit domains only
                              .WithHeaders(                          //only headers required
                                  "Content-Type",
                                  "Authorization",
                                  "X-XSRF-TOKEN")
                              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                              .AllowCredentials();                   
                              //.SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // cache preflight
                    });
                }
            });

            builder.Services.AddDataProtection();

            builder.Services.ConfigureOptions<AuthOptionsSetup>();

            builder.Services.AddScoped<AntiforgeryValidationFilter>();

            builder.Services
                .AddApiControllers()
                .AddApiJsonOptions();

            builder.Services.AddCustomModelValidation();

            
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true; // Ensures all routes are lowercase
            });
            
            // Add services to the container
            builder.Services.AddHttpContextAccessor();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            
            builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            builder.Services.AddSingleton<IDbExecutor, DbExecutor>();
            builder.Services.AddSingleton<ISecureProtector, SecureProtector>();
            builder.Services.AddSingleton<IProtectData, ProtectData>();
            builder.Services.AddSingleton<DataSeeder>();
            

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            #region
            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(opt =>
            //    {
            //        opt.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = true,
            //            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            //            ValidateAudience = true,
            //            ValidAudience = builder.Configuration["AppSettings:Audience"],
            //            ValidateLifetime = true,
            //            IssuerSigningKey = new SymmetricSecurityKey(
            //                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            //            ValidateIssuerSigningKey = true
            //        };
            //    });

            //method 1
            //        builder.Services.AddAuthentication(options =>
            //        {
            //            options.DefaultAuthenticateScheme = "GcmAuthentication";
            //            options.DefaultChallengeScheme = "GcmAuthentication";

            //        })
            //  .AddScheme<AuthenticationSchemeOptions, GcmAuthenticationHandler>(
            //"GcmAuthentication", options => { });
            #endregion
       
            //method 2 
            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "GcmAuthentication";
                options.DefaultChallengeScheme = "GcmAuthentication";
            })
            .AddScheme<AuthenticationSchemeOptions, GcmAuthenticationHandler>("GcmAuthentication", null);

            builder.Services.AddAppAuthorization();
            //builder.Services.AddAuthorization();

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN"; // header Angular sends
                options.Cookie.Name = ".AspNetCore.Antiforgery"; // .NET's internal cookie
                options.Cookie.HttpOnly = true; // JS cannot read this one
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });

            //builder.Services.AddDistributedMemoryCache();
            //COMMANDS TO RUN BEFORE ADDING SQL SERVER
            //dotnet tool install - g dotnet - sql - cache
            //dotnet sql-cache create "Server=localhost;Database=DemoAppDb;Integrated Security=True;TrustServerCertificate=True" dbo SessionCache
            builder.Services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = builder.Configuration.GetConnectionString("DemoAppDb");
                options.SchemaName = "dbo";
                options.TableName = "SessionCache";
            });
            builder.Services.AddScoped<ISessionService, SessionService>();

            //builder.Services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(1);
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //});

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<UsersService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors("DevCors");
                app.MapOpenApi();
                app.MapScalarApiReference();

            } else
            {
                app.UseHttpsRedirection();
                app.UseHsts();
                app.UseCors("ProdCors");
            }

            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //    //Secure = CookieSecurePolicy.None,
            //    //MinimumSameSitePolicy = SameSiteMode.Lax
            //});


            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.SeedAsync();
            }

            app.UseMiddleware<ApiExceptionMiddleware>();
            //stops from loading javascript
            app.UseMiddleware<ContentSecurityPolicyMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<SessionValidationMiddleware>();
            //app.UseSession();

            app.MapControllers();

            app.Run();
        }
    }
}

//CORRECT ORDER OF CALLING 
//app.UseStaticFiles()
//app.UseRouting()
//app.UseAuthentication()(if using)
//app.UseAuthorization()(if using)
//app.UseSession() <— Must come after routing but before your endpoints.
//app.MapControllers() / app.MapRazorPages()
