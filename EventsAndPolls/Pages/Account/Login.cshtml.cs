using EventsAndPolls.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Pages.Account;

public class LoginModel : PageModel
{
     private readonly SignInManager<ApplicationUser> _signInManager;

     public LoginModel(SignInManager<ApplicationUser> signInManager)
     {
          _signInManager = signInManager;
     }

     [BindProperty]
     public LoginInputModel Input { get; set; } = new();

     public string? ReturnUrl { get; set; }

     public void OnGet(string? returnUrl = null)
     {
          ReturnUrl = returnUrl ?? Url.Content("~/");
     }

     public class LoginInputModel
     {
          [Required(ErrorMessage = "Email-ul este obligatoriu")]
          [EmailAddress]
          public string Email { get; set; } = string.Empty;

          [Required(ErrorMessage = "Parola este obligatorie")]
          [DataType(DataType.Password)]
          public string Password { get; set; } = string.Empty;

          [Display(Name = "Ține-mă minte")]
          public bool RememberMe { get; set; }
     }

     public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
     {
          returnUrl ??= Url.Content("~/");

          if (!ModelState.IsValid)
               return Page();

          var result = await _signInManager.PasswordSignInAsync(
              Input.Email,
              Input.Password,
              Input.RememberMe,
              lockoutOnFailure: false);

          if (result.Succeeded)
               return LocalRedirect(returnUrl);

          ModelState.AddModelError(string.Empty, "Email sau parolă incorectă");
          return Page();
     }
}