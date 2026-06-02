using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuotesApp.Domain;

namespace QuotesApp.Pages.Quotes;

public class DetailsModel(QuoteStore store) : PageModel
{
    public Quote Quote { get; private set; } = null!;

    // Which note (if any) is currently in edit mode. Driven by ?editNote=3.
    public int? EditNoteId { get; private set; }

    [BindProperty] public NoteInput Input { get; set; } = new();

    public class NoteInput
    {
        [Required(ErrorMessage = "Note can't be empty.")]
        [StringLength(500, ErrorMessage = "Keep it under 500 characters.")]
        public string? Body { get; set; }
    }

    public IActionResult OnGet(int id, int? editNote)
    {
        var quote = store.Find(id);
        if (quote is null) return NotFound();

        Quote = quote;
        EditNoteId = editNote;
        return Page();
    }

    public IActionResult OnPostAddNote(int id)
    {
        var quote = store.Find(id);
        if (quote is null) return NotFound();
        Quote = quote;

        if (!ModelState.IsValid) return Page();

        store.AddNote(quote, Input.Body!);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostEditNote(int id, int noteId)
    {
        var quote = store.Find(id);
        if (quote is null) return NotFound();
        Quote = quote;

        if (!ModelState.IsValid)
        {
            EditNoteId = noteId; // stay in edit mode so the error shows in the edit form
            return Page();
        }

        store.EditNote(quote, noteId, Input.Body!);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostDeleteNote(int id, int noteId)
    {
        var quote = store.Find(id);
        if (quote is null) return NotFound();

        store.DeleteNote(quote, noteId);
        return RedirectToPage(new { id });
    }
}
