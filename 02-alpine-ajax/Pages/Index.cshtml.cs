using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuotesApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Quotes/Index");
}
