using Microsoft.AspNetCore.Mvc.RazorPages;
using QuotesApp.Domain;

namespace QuotesApp.Pages.Quotes;

public class IndexModel(QuoteStore store) : PageModel
{
    public IReadOnlyList<Quote> Quotes { get; private set; } = [];

    public void OnGet() => Quotes = store.All();
}
