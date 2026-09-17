var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ABCRetailWeb.Services.TableStorageService>();

builder.Services.AddSingleton<ABCRetailWeb.Services.BlobStorageService>();

builder.Services.AddSingleton<ABCRetailWeb.Services.QueueStorageService>();

builder.Services.AddSingleton<ABCRetailWeb.Services.FileShareLoggingService>();

// Register a global HttpClient connection mapping for serverless triggers
builder.Services.AddHttpClient();



// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
