using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Dishes;
using dishes.Server.Features.Identity;
using dishes.Server.Features.Identity.GetProfile;
using dishes.Server.Features.Identity.UpdateProfile;
using dishes.Server.Features.Ingredients;
using dishes.Server.Features.Recipes;
using dishes.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var corsPolicy = "CorsPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityApiEndpoints<AppIdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<GetProfileHandler>();
builder.Services.AddScoped<UpdateProfileHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        if (allowedOrigins is not null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:AppConnectionString"]));

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure static files for recipe images
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(corsPolicy);
app.UseHttpsRedirection();

var userGroup = app.MapGroup("/api/user")
            .RequireAuthorization()
            .WithTags("Identity");

userGroup.MapIdentityEndpoints();
userGroup.MapIdentityApi<AppIdentityUser>();

var apiGroup = app.MapGroup("/api");

apiGroup.MapDishesEndpoints();
apiGroup.MapIngredientsEndpoints();
apiGroup.MapRecipesEndpoints();


app.MapFallbackToFile("/index.html");

app.Run();