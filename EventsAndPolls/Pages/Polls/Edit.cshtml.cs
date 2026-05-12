using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventsAndPolls.Infrastructure.Identity;

namespace EventsAndPolls.Pages.Polls;

[Authorize(Roles = Roles.Organizer)]
public class EditModel : PageModel
{
     public void OnGet() { }
}