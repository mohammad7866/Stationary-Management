using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using System.Reflection;
using System.IO; // <- only needed if ImplicitUsings is off

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger + XML comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// DB (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Global error handler → ProblemDetails (place early)
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// Migrate first, then seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Initialize(db);
}

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
