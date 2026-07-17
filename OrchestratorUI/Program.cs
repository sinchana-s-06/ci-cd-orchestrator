var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// ✅ ADD THIS (required for login)
builder.Services.AddSession();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // ✅ REQUIRED
app.UseRouting();

// ✅ ADD THIS (before endpoints)
app.UseSession();

app.UseAuthorization();

// ✅ CLEAN ROUTING (REMOVE static assets stuff)
app.MapRazorPages();

app.Run();