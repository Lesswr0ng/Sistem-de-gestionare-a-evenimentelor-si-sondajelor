using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Pages.Account;

public class RegisterModel : PageModel
{
     private readonly UserManager<ApplicationUser> _userManager;
     private readonly SignInManager<ApplicationUser> _signInManager;

     public RegisterModel(
         UserManager<ApplicationUser> userManager,
         SignInManager<ApplicationUser> signInManager)
     {
          _userManager = userManager;
          _signInManager = signInManager;
     }

     [BindProperty]
     public RegisterInputModel Input { get; set; } = new();

     public class RegisterInputModel
     {
          [Required(ErrorMessage = "Numele este obligatoriu")]
          [Display(Name = "Nume afișat")]
          public string DisplayName { get; set; } = string.Empty;

          [Required(ErrorMessage = "Email-ul este obligatoriu")]
          [EmailAddress(ErrorMessage = "Email invalid")]
          [Display(Name = "Email")]
          public string Email { get; set; } = string.Empty;

          [Required(ErrorMessage = "Parola este obligatorie")]
          [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere")]
          [DataType(DataType.Password)]
          [Display(Name = "Parolă")]
          public string Password { get; set; } = string.Empty;

          [Required(ErrorMessage = "Confirmarea parolei este obligatorie")]
          [DataType(DataType.Password)]
          [Compare("Password", ErrorMessage = "Parolele nu coincid")]
          [Display(Name = "Confirmă parola")]
          public string ConfirmPassword { get; set; } = string.Empty;
     }

     public async Task<IActionResult> OnPostAsync()
     {
          if (!ModelState.IsValid)
               return Page();

          var user = new ApplicationUser
          {
               UserName = Input.Email,
               Email = Input.Email,
               DisplayName = Input.DisplayName,
               EmailConfirmed = true
          };

          var result = await _userManager.CreateAsync(user, Input.Password);

          if (result.Succeeded)
          {
               // All new registrations get User role — promote to Organizer manually
               await _userManager.AddToRoleAsync(user, Roles.User);
               await _userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("DisplayName", user.DisplayName));
               await _signInManager.SignInAsync(user, isPersistent: false);
               return RedirectToPage("/Events/Index");
          }

          foreach (var error in result.Errors)
               ModelState.AddModelError(string.Empty, error.Description);

          return Page();
     }
}