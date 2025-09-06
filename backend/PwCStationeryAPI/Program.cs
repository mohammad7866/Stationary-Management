// backend/PwCStationeryAPI/Program.cs
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Filters;
using PwCStationeryAPI.Models;
using PwCStationeryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ========= Services =========

// Filters
builder.Services.AddScoped<AuditLogActionFilter>();

// Audit logger
builder.Services.AddScoped<IStockMutationService, StockMutationService>();
builder.Services.AddScoped<IStockAdjustService, StockAdjustService>(); // ← add this
builder.Services.AddScoped<IAuditLogger, NoopAuditLogger>();
builder.Services.AddScoped<AuditLogger>(); // if any controller requests the concrete                                           // concrete for controllers that request it
builder.Services.AddHttpContextAccessor();                    // if AuditLogger needs it


// Issue/Return service
// services
builder.Services.AddScoped<IStockMutationService, StockMutationService>();
builder.Services.AddScoped<IStockAdjustService, StockAdjustService>(); // 👈 add this

// Controllers + JSON (ignore cycles to avoid 500s on EF graphs)
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogActionFilter>();  // global auditing
})
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // 🚩 key line
});

// Swagger (XML comments + JWT auth button + local server)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PwC Stationery API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT here (no 'Bearer ' prefix needed)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Make Swagger "Try it out" use the right base URL (adjust if your launchSettings differ)
    c.AddServer(new OpenApiServer { Url = "https://localhost:7043" });
});

// EF Core (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity + Roles
builder.Services
    .AddIdentityCore<ApplicationUser>(o =>
    {
        o.Password.RequireDigit = true;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change-me";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // dev only
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,   // set true + provide Issuer in prod
            ValidateAudience = false, // set true + provide Audience in prod
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// CORS (Vite dev server + API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:7043",
                "https://localhost:7043"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// Misc
builder.Services.AddHttpContextAccessor();

// ========= Build app =========
var app = builder.Build();

// ========= Pipeline =========
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 👈 show real errors during dev
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PwC Stationery API v1");
        c.DocumentTitle = "PwC Stationery API";
    });
}
else
{
    // Generic handler only in non-dev
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var problem = Results.Problem(
                title: "Unexpected error",
                statusCode: StatusCodes.Status500InternalServerError);
            await problem.ExecuteAsync(context);
        });
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// ========= DB migrate + seed =========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed domain data
    DbInitializer.Initialize(db);

    // Seed auth users/roles
    await AuthSeeder.SeedAsync(scope.ServiceProvider);
}

// ========= Endpoints =========
app.MapControllers();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
