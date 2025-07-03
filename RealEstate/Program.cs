using System.Text;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<RealEstateDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Cloudinary configuration
var account = new Account(
    builder.Configuration["appSettings:CloudinaryUsername"],
    builder.Configuration["appSettings:CloudinaryApiKey"],
    builder.Configuration["appSettings:CloudinarySecreteKey"]
);
var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
builder.Services.AddSingleton(cloudinary);

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<RealEstateDbContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealEstate API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = true;
        options.InvalidModelStateResponseFactory = context => {
            var details = new ValidationProblemDetails(context.ModelState);
            details.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id;
            return new BadRequestObjectResult(details);
        };
    });
// JWT Authentication
//var jwtIssuer = builder.Configuration["appSettings:EfcKey"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
//var jwtAudience = builder.Configuration["appSettings:JwtAudience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
//var jwtKey = builder.Configuration["appSettings:JwtKey"] ?? throw new InvalidOperationException("Jwt:Key missing");

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = jwtIssuer,
//            ValidAudience = jwtAudience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//        };
//    });


// CORRECTED JWT SETUP
var jwtIssuer = builder.Configuration["appSettings:JwtIssuer"]
    ?? "b@$&$g8uB9EwLc_GLp7JVgV8epLzReZr7HaA"; // From your token

var jwtAudience = builder.Configuration["appSettings:JwtAudience"]
    ?? "your_api_audience"; // Set this if you use audiences

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // Disable since token has no audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["appSettings:JwtKey"]!
            ))
        };
    });
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "RealEstate API Running ✅");

app.Run();
