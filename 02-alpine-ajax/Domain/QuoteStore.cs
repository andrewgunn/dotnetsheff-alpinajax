namespace QuotesApp.Domain;

public class Quote
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string ClientName { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Draft";
    public List<Note> Notes { get; } = new();
    public List<Activity> Activities { get; } = new();
}

public class Note
{
    public int Id { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class Activity
{
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Dead-simple in-memory store. Registered as a singleton, so it survives for
/// the life of the process and resets on restart — handy for a live demo.
/// </summary>
public class QuoteStore
{
    private readonly List<Quote> _quotes = new();
    private int _nextNoteId = 1;

    public QuoteStore()
    {
        Seed();
    }

    public IReadOnlyList<Quote> All() => _quotes;

    public Quote? Find(int id) => _quotes.FirstOrDefault(q => q.Id == id);

    public Note AddNote(Quote quote, string body)
    {
        var note = new Note { Id = _nextNoteId++, Body = body.Trim(), CreatedAt = DateTime.Now };
        quote.Notes.Add(note);
        Log(quote, "Note added");
        return note;
    }

    public void EditNote(Quote quote, int noteId, string body)
    {
        var note = quote.Notes.FirstOrDefault(n => n.Id == noteId);
        if (note is null) return;
        note.Body = body.Trim();
        Log(quote, "Note edited");
    }

    public void DeleteNote(Quote quote, int noteId)
    {
        var note = quote.Notes.FirstOrDefault(n => n.Id == noteId);
        if (note is null) return;
        quote.Notes.Remove(note);
        Log(quote, "Note deleted");
    }

    private void Log(Quote quote, string description) =>
        quote.Activities.Add(new Activity { Description = description, Timestamp = DateTime.Now });

    private void Seed()
    {
        _quotes.AddRange(new[]
        {
            new Quote { Id = 1, Reference = "Q-1001", ClientName = "Acme Property Ltd", Amount = 1850m, Status = "Sent" },
            new Quote { Id = 2, Reference = "Q-1002", ClientName = "Bramble & Co",        Amount = 950m,  Status = "Draft" },
            new Quote { Id = 3, Reference = "Q-1003", ClientName = "Carter Holdings",     Amount = 4200m, Status = "Accepted" },
            new Quote { Id = 4, Reference = "Q-1004", ClientName = "Delphine Estates",    Amount = 2750m, Status = "Sent" },
            new Quote { Id = 5, Reference = "Q-1005", ClientName = "Everett Legal",       Amount = 1325m, Status = "Rejected" },
        });

        var first = _quotes[0];
        AddNote(first, "Client asked for a breakdown of the fixed fees.");
        AddNote(first, "Left a voicemail to confirm the completion date.");
    }
}
