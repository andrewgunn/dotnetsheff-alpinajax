# npm uninstall everything

Talk + live demo (dotnetsheff): building an SPA-feel CRUD app with **ASP.NET
Razor Pages + [Alpine AJAX](https://alpine-ajax.js.org)** — where the logic
never leaves the server.

## What's here

| Path | What it is |
|------|------------|
| `slides/index.html` | The ~8-min opener (reveal.js, offline). Open in a browser; press `S` for speaker notes. |
| `01-razor-ssr/` | **Pure server-rendered Razor Pages.** The starting point we live-code from. No JS framework — add/edit/delete are full POST + redirect. |
| `02-alpine-ajax/` | **The finished version** — *backup only.* Same app with Alpine AJAX applied; its git history has one commit per beat (`git log --oneline`). |
| **`DEMO_GUIDE.md`** | **The live walkthrough.** A one-glance cheat-sheet table up top, then every change in order with full text inline — follow it top-to-bottom during the talk. |

## No database, no setup

There is **no database and no external service**. Data is a plain in-memory
C# singleton (`Domain/QuoteStore.cs`) seeded with 5 quotes:

```csharp
builder.Services.AddSingleton<QuoteStore>();
```

It lives for the life of the process and **resets on restart** — handy for
getting back to a clean slate mid-demo. The only requirement is the .NET 10 SDK.

**Runs fully offline.** Alpine, alpine-ajax and Tailwind are vendored into each
app's `wwwroot/lib/`, and reveal.js + Mermaid into `slides/lib/` — nothing is
fetched from a CDN, so flaky venue wifi can't break the talk.

## Run it

```bash
# the starting point (live-code from here)
cd 01-razor-ssr && dotnet run

# the finished reference
cd 02-alpine-ajax && dotnet run
```

Then open the printed URL (e.g. `http://localhost:5xxx`) → it lands on `/Quotes`.

## The demo arc (10 beats, ordered to drop off the end if time runs short)

```
CORE       1  x-target="notes"            note appears, no full reload
           2  x-target="notes note-form"  form resets; validation just works
           3  #count x-sync               count updates itself
           4  delete (x-target="notes")   note removed inline
           5  edit (?editNote, note-{id}) inline view <-> edit
DROPPABLE  6  @ajax:before confirm()      "Delete this note?"
           7  #notifications x-sync prepend   self-dismissing toast
           8  #server_events + #activity  decoupled refetch via events
           9  [aria-busy] CSS             loading state
          10  Layout switch on X-Alpine-Request   lean fragments
```

Push/pull contrast in the right column: the **count** uses `x-sync` (server
*pushes* the value in the response); the **activity panel** uses a `note:changed`
**event** (server fires a signal, the panel *pulls* a fresh copy of itself).

## Tip for the talk

Do a dry-run in the browser beforehand. To make the **loading state** (beat 9)
visible, throttle the network in DevTools (Network → Slow 3G) — local requests
are otherwise instant.
