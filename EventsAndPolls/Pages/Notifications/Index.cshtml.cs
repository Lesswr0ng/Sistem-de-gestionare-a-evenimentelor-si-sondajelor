using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventsAndPolls.Pages.Notifications;

[Authorize]
public class IndexModel : PageModel
{
     public void OnGet() { }
}
