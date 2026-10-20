using DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<UserDB>(provider =>
    new UserDB(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// קריאת מחרוזת החיבור מ-User Secrets
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in configuration.");
}

// אתחול בסיס הנתונים והטבלאות בהזנקה
await DatabaseInitializer.InitializeAsync(connectionString);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
 
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
