using QuotesApp.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// In-memory data, shared for the life of the process.
builder.Services.AddSingleton<QuoteStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
