using CommentService.Api.Clients;
using CommentService.Api.Clients.Handlers;
using CommentService.Api.Clients.Identity.Configuration;
using CommentService.Api.Consumers;
using CommentService.Api.Data;
using CommentService.Api.Extensions;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Services;
using CourseService.Contracts.Clients;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Text;
using VideoService.Contracts.Clients;
namespace CommentService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            AuthenticationSettings authenticationSettings = new AuthenticationSettings();
            configuration.GetSection("JWT").Bind(authenticationSettings);
            var connectionString = builder.Configuration.GetConnectionString("CommentDb");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
             options.UseNpgsql(connectionString));
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAppAuthorization();
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",

                    builder =>
                    {

                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .WithExposedHeaders("www-authenticate");
                    });
            });
            builder.Services.AddSingleton(authenticationSettings);
            builder.Services.AddScoped<IVideoCourseService, Services.VideoCourseService>();
            builder.Services.AddScoped<ICommentService, Services.CommentService>();
            builder.Services.AddScoped<IReactionService, ReactionService>();
            builder.Services.AddScoped<ICoursePermissionsService, CoursePermissionsService>();
            builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection("IdentitySettings"));
            builder.Services.AddHttpClient<IIdentityTokenService, IdentityTokenService>();
            builder.Services.AddTransient<AuthenticationDelegatingHandler>();
            builder.Services.AddHttpClient<ICourseServiceClient, CourseServiceClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<IdentitySettings>>().Value;
                client.BaseAddress = new Uri(settings.Authority);
            })
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            builder.Services.AddHttpClient<IVideoServiceClient, VideoServiceClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<IdentitySettings>>().Value;
                client.BaseAddress = new Uri(settings.Authority);
            })
            
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            builder.Services.AddMassTransit(
                options => {
                    options.SetKebabCaseEndpointNameFormatter();
                    options.AddConsumer<VideoCreatedEventConsumer>();
                    options.AddConsumer<UserCoursePermissionsUpdatedConsumer>();
                    options.UsingRabbitMq((context, cfg) =>
                    {
                        var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
                        cfg.Host(new Uri(connectionString));
                        cfg.ReceiveEndpoint("comment-service", e =>
                        {
                            e.ConfigureConsumer<VideoCreatedEventConsumer>(context);
                            e.ConfigureConsumer<UserCoursePermissionsUpdatedConsumer>(context);
                        });
                        cfg.ConfigureEndpoints(context);
                    });
                });

            builder.Services.AddFluentValidationAutoValidation(configuration =>
            {
                configuration.DisableBuiltInModelValidation = true;

                configuration.ValidationStrategy = SharpGrip.FluentValidation.AutoValidation.Mvc.Enums.ValidationStrategy.All;
                configuration.EnableBodyBindingSourceAutomaticValidation = true;

                configuration.EnableFormBindingSourceAutomaticValidation = true;

                configuration.EnableQueryBindingSourceAutomaticValidation = true;

                configuration.EnablePathBindingSourceAutomaticValidation = true;

                configuration.EnableCustomBindingSourceAutomaticValidation = true;

            });
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


            builder.Services.AddOpenApiDocument();

            builder.Services.AddControllers();
             ;
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuer = authenticationSettings.Issuer,
                    ValidAudience = authenticationSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.Key))
                };
            });
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });
            builder.Host.UseSerilog((ctx, lc) =>
                lc.ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new[] { new Uri(builder.Configuration["Elastic:Uri"]) }, opts =>
                {
                    opts.DataStream = new DataStreamName("logs", "comment-service");

                }
                , transport =>
                {
                    transport.Authentication(new BasicAuthentication(
                        "elastic",
                        builder.Configuration["Elastic:Password"]));
                }
                )
               .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
             );
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    options.RoutePrefix = string.Empty;
                });
                DatabaseManagmentService.MigrationInitialisation(app);
            }
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
