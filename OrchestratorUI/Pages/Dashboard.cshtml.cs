using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace OrchestratorUI.Pages
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
{
    Console.WriteLine("🔥 Dashboard OnGet HIT");

    var user = HttpContext.Session.GetString("User");
    Console.WriteLine("User: " + user);

    if (string.IsNullOrEmpty(user))
    {
        Console.WriteLine("❌ Redirecting to Login");
        return RedirectToPage("/Login");
    }

    Console.WriteLine("✅ Access granted");
    return Page();
}
    }
}