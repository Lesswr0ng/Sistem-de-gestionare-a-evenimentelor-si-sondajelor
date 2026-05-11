using EventsAndPolls.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventsAndPolls.Pages.Account;

public class LogoutModel : PageModel
{
     private readonly SignInManager<ApplicationUser> _signInManager;
     private readonly ILogger<LogoutModel> _logger;

     public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
     {
          _signInManager = signInManager;
          _logger = logger;
     }

     // GET — redirect away, logout only via POST to prevent CSRF
     public IActionResult OnGet()
     {
          return RedirectToPage("/Index");
     }

     public async Task<IActionResult> OnPostAsync()
     {
          await _signInManager.SignOutAsync();
          _logger.LogInformation("User logged out");

          // Clear the cookie explicitly to ensure the session is gone
          Response.Cookies.Delete(".AspNetCore.Identity.Application");

          return RedirectToPage("/Index");
     }
}