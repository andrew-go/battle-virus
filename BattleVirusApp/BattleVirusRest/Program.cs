using BattleVirusRepositories.Repositories;
using BattleVirusServices.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<BattleVirusDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("BattleVirusDB")));

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddDbContext<BattleVirusDbContext>(options =>
    options.UseNpgsql(ConvertHerokuUrlToConnectionString(connectionString)));
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        b => b.WithOrigins("https://andrew-go.github.io")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddScoped<ISessionTokenService, SessionTokenService>();
builder.Services.AddScoped<ISessionTokenRepository, SessionTokenRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<IGameStateRepository, GameStateRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowSpecificOrigin"); // Use the CORS policy
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.UseAuthorization();

app.MapControllers();

app.Run();

string ConvertHerokuUrlToConnectionString(string? url)
{
    if (url is null)
    {
        throw new ArgumentNullException(nameof(url));
    }
    
    // Parse the connection URL
    var uri = new Uri(url);
    var username = uri.UserInfo.Split(':')[0];
    var password = uri.UserInfo.Split(':')[1];
    var dbHost = uri.Host;
    var dbName = uri.AbsolutePath.TrimStart('/');
    var dbPort = uri.Port;
    
    // Build the Npgsql connection string
    return $"Server={dbHost};Port={dbPort};User Id={username};Password={password};Database={dbName};SSL Mode=Require;Trust Server Certificate=True";
}
