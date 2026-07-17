using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace OrchestratorUI.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            if (Username == "admin" && Password == "admin")
            {
                // ✅ store session
                HttpContext.Session.SetString("User", Username);

                return RedirectToPage("/Dashboard");
            }

            ErrorMessage = "Invalid credentials";
            return Page();
        }
    }
}