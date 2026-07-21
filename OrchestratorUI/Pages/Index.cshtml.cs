using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchestratorUI.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var user = HttpContext.Session.GetString("User");

        if (string.IsNullOrEmpty(user))
        {
            return RedirectToPage("/Login");
        }

        return RedirectToPage("/Dashboard");
    }
}
