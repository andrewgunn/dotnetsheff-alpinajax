namespace QuotesApp.Domain;

public class Quote
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string PropertyAddress { get; set; } = "";
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

public enum ActivityKind { Added, Edited, Deleted }

public class Activity
{
    public string Description { get; set; } = "";
    public ActivityKind Kind { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>Formats a timestamp as a short relative time, e.g. "just now", "2m ago".</summary>
public static class TimeAgo
{
    public static string From(DateTime t)
    {
        var seconds = (DateTime.Now - t).TotalSeconds;
        if (seconds < 5) return "just now";
        if (seconds < 60) return $"{(int)seconds}s ago";
        var minutes = seconds / 60;
        if (minutes < 60) return $"{(int)minutes}m ago";
        var hours = minutes / 60;
        if (hours < 24) return $"{(int)hours}h ago";
        return t.ToString("d MMM");
    }
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
        Log(quote, "Note added", ActivityKind.Added);
        return note;
    }

    public void EditNote(Quote quote, int noteId, string body)
    {
        var note = quote.Notes.FirstOrDefault(n => n.Id == noteId);
        if (note is null) return;
        note.Body = body.Trim();
        Log(quote, "Note edited", ActivityKind.Edited);
    }

    public void DeleteNote(Quote quote, int noteId)
    {
        var note = quote.Notes.FirstOrDefault(n => n.Id == noteId);
        if (note is null) return;
        quote.Notes.Remove(note);
        Log(quote, "Note deleted", ActivityKind.Deleted);
    }

    private void Log(Quote quote, string description, ActivityKind kind) =>
        quote.Activities.Add(new Activity { Description = description, Kind = kind, Timestamp = DateTime.Now });

    private void Seed()
    {
        _quotes.AddRange(new[]
        {
            new Quote { Id = 1, Reference = "Q-1001", ClientName = "Acme Property Ltd", PropertyAddress = "14 Ecclesall Road, Sheffield, S11 8PA", Amount = 1850m, Status = "Sent" },
            new Quote { Id = 2, Reference = "Q-1002", ClientName = "Bramble & Co",        PropertyAddress = "27 Abbeydale Road, Sheffield, S7 1FD",  Amount = 950m,  Status = "Draft" },
            new Quote { Id = 3, Reference = "Q-1003", ClientName = "Carter Holdings",     PropertyAddress = "3 Kelham Island, Sheffield, S3 8RY",    Amount = 4200m, Status = "Accepted" },
            new Quote { Id = 4, Reference = "Q-1004", ClientName = "Delphine Estates",    PropertyAddress = "88 Crookes Valley Road, Sheffield, S10 1BP", Amount = 2750m, Status = "Sent" },
            new Quote { Id = 5, Reference = "Q-1005", ClientName = "Everett Legal",       PropertyAddress = "12 Division Street, Sheffield, S1 4GF", Amount = 1325m, Status = "Rejected" },
        });

        var first = _quotes[0];
        AddNote(first, "Client asked for a breakdown of the fixed fees.");
        AddNote(first, "Left a voicemail to confirm the completion date.");
    }
}
