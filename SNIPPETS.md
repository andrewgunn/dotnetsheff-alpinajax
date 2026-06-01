# Live-coding cheat-sheet

You live-code from **`01-razor-ssr/`** (pure server-rendered). The finished
reference is **`02-alpine-ajax/`**, whose git history has one commit per beat —
if a step goes sideways, `git show` / diff the matching commit or copy from the
folder.

Beats are ordered **most-essential → most-droppable**. If you run low on time,
just stop — every beat stands on its own.

> All the swap targets already have `id`s in the SSR baseline (`#notes`,
> `#note-form`, `#count`, `#activity`, `#note-{id}`), so every beat below is
> *only adding attributes / small bits* — never restructuring markup.

---

## Setup — load Alpine (do this first)

In `Pages/Shared/_Layout.cshtml`, in `<head>` (plugin **before** core, both `defer`):

```html
<script defer src="https://cdn.jsdelivr.net/npm/@imacrayon/alpine-ajax@0.12.x/dist/cdn.min.js"></script>
<script defer src="https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js"></script>
```

> *"Two script tags. No npm, no build step. That's the install."*

---

## CORE BEATS (type these live)

### 1 · x-target — add a note without a full reload
`Pages/Quotes/Details.cshtml`, the **add-note form** → add `x-target="notes"`:

```html
<form id="note-form" method="post" asp-page-handler="AddNote" asp-route-id="@Model.Quote.Id"
      x-target="notes" ...>
```

> *"Submit, follow the redirect, find `#notes` in the response, swap just that
> in. Watch — the note appears, no reload. But look: my typed text is still in
> the box. We only targeted the list, not the form."*

### 2 · target the form too — reset + validation for free
Same form → `x-target="notes note-form"`:

```html
<form id="note-form" ... x-target="notes note-form" ...>
```

> *"Now the form's a target too, so it comes back empty. And the freebie:
> submit an empty note —"* (do it) *"— there's the validation error, rendered
> by the server's ModelState. We wrote zero JavaScript validation."*

### 3 · x-sync — the count updates itself
Right column, the count → add `x-sync`:

```html
<div id="count" x-sync ...>@Model.Quote.Notes.Count</div>
```

> *"Nothing targets the count. `x-sync` makes it subscribe to any response that
> contains a `#count`. It just rides along."*

### 4 · delete inline
The per-note **delete form** → add `x-target="notes"`:

```html
<form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id"
      asp-route-noteId="@note.Id" x-target="notes">
```

> *"Row vanishes, no reload. And the count fixes itself — it already has x-sync."*

### 5 · inline edit (edit-row pattern)
On each note: the **Edit link**, the **edit form**, and **Cancel** → `x-target="note-@note.Id"`;
add `x-autofocus` to the edit textarea:

```html
<a href="...?editNote=@note.Id" x-target="note-@note.Id">Edit</a>
<form ... asp-page-handler="EditNote" ... x-target="note-@note.Id">
  <textarea name="Input.Body" x-autofocus ...>@note.Body</textarea>
<a href="/Quotes/@Model.Quote.Id" x-target="note-@note.Id">Cancel</a>
```

> *"Edit GETs the note as a form and swaps just that `<li>`. Save swaps it back
> to view. One element toggles between two server-rendered states."*

---

## DROPPABLE TAIL (stop here if short on time)

### 6 · confirm before delete
On the **delete form** → add the lifecycle hook:

```html
@ajax:before="confirm('Delete this note?') || $event.preventDefault()"
```

> *"`ajax:before` fires before the request. Cancel the confirm → preventDefault
> → no request."*

### 7 · toasts (x-sync + prepend)
**Server:** in each handler, before the redirect — `TempData["Toast"] = "Note added";`
(`"Note edited"`, `"Note deleted"`).
**Layout:** add the notifications list + a self-dismissing toast `<li>` (copy from
`02-alpine-ajax/Pages/Shared/_Layout.cshtml` → moved to `_LiveRegions.cshtml` in beat 10):

```html
<ul id="notifications" x-sync x-merge="prepend" role="status" class="fixed ...">
  @if (TempData["Toast"] is string toast) { <li x-data="{...self-dismiss after 6s...}">@toast</li> }
</ul>
```

> *"`prepend`, so new toasts stack instead of replacing. The server just drops a
> `<li>` in; x-sync slides it in. Works without JS too — it shows on the reload."*

### 8 · server events — the activity panel, decoupled
**Layout:** add the dispatcher slot:

```html
<div id="server_events" x-sync>
  @if (toast is not null) { <div x-init="$dispatch('note:changed')"></div> }
</div>
```

**Details.cshtml:** the activity `<section>` listens and refetches *itself*:

```html
<section x-init @note:changed.window="$ajax('/Quotes/@Model.Quote.Id', { target: 'activity' })">
```

> *"The note form has no idea this panel exists. Server fires a signal, the panel
> pulls a fresh copy of itself. Compare the count: x-sync **pushes**, events
> **pull**. Both ship in our real app."*

### 9 · loading state (pure CSS)
`<head>` → a `[aria-busy]` rule (dim + spinner). Alpine sets `aria-busy` on
targets during a request and disables submit buttons automatically.

```css
[aria-busy] { opacity:.6; position:relative; }
[aria-busy]::after { /* small spinner */ }
```

> *"No JS. Alpine flags the busy element; CSS does the rest. Local requests are
> instant — I'll throttle the network so you can see it."* (DevTools → Network → Slow 3G)

### 10 · optimize — lean fragments
Extract the live regions to `_LiveRegions.cshtml`, add `_LayoutAjax.cshtml`
(`@RenderBody()` + the partial, no `<head>`/nav), and switch in `_ViewStart.cshtml`:

```csharp
Layout = Context.Request.Headers.ContainsKey("X-Alpine-Request") ? "_LayoutAjax" : "_Layout";
```

> *"Alpine sends an `X-Alpine-Request` header. When we see it, skip the chrome
> and send just the fragment. Full navigations still get the whole page — so it
> all still works with no JS. The server decides how much to send."*
