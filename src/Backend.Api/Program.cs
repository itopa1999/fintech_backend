using Backend.Application;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Settings;
using Backend.Api.Middleware;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Filters;
using HealthChecks.UI.Client;
using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text;
using Backend.Application.Interfaces;
using Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.Results;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        var result = new BaseResult(
            statusCode: HttpStatusCode.BadRequest,
            message: string.Join(" | ", errors)
        );

        return new ObjectResult(result)
        {
            StatusCode = 400
        };
    };
});

// CORS
var corsPolicyName = "CorsPolicy";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// Swagger

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Backend Service API",
            Version = "v1",
            Description = "Backend Service v1"
        }
    );

    options.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Title = "Backend Service API",
            Version = "v2",
            Description = "Backend Service v2"
        }
    );

    options.SwaggerDoc(
        "v3",
        new OpenApiInfo
        {
            Title = "Backend Service API",
            Version = "v3",
            Description = "Backend Service API v3"
        }
    );

    options.CustomSchemaIds(type => type.FullName);
    options.ResolveConflictingActions(c => c.First());

    options.OperationFilter<SwaggerFilter.RemoveVersionFromParameter>();
    options.DocumentFilter<SwaggerFilter.ReplaceVersionWithExactValueInPath>();
    options.SchemaFilter<SwaggerIgnoreFilter>();


    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
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


// CQRS (MediatR)
builder.Services.AddMediatR(typeof(AssemblyReference).Assembly);

// Health checks
builder.Services.AddHealthChecks();

// Redis cache
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "BackendCache:";
});

builder.Services.AddScoped<CacheService>();

// DB connection string 
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider")?.Trim().ToLowerInvariant();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider is "postgres" or "postgresql" or "npgsql")
    {
        var connection = builder.Configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("Postgres connection string is not configured.");
        }

        options.UseNpgsql(connection);
    }
    else
    {
        var connection = builder.Configuration.GetConnectionString("SqlServer")
            ?? builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("SQL Server connection string is not configured.");
        }

        options.UseSqlServer(connection);
    }
});

// Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

var key = Encoding.UTF8.GetBytes(jwtSettings!.Key);


// AUTHENTICATION
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,

        ClockSkew = TimeSpan.Zero
    };
});


builder.Services.AddAuthorization();



var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(corsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapHealthChecks(
        "/api/v1/health",
        new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }
    );

    endpoints.MapGet("/{**path}", async context =>
    {
        JObject jObject = new JObject
        {
            { "health", "Done creating, Navigate to /health to see the health status." },
            { "ready", "Navigate to /health/ready to see the readiness status." },
            { "live", "Navigate to /health/live to see the liveness status." }
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(jObject));
    });
});

app.Run();