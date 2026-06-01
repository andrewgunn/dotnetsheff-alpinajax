# Demo guide — every change, in order, fully inline

Follow this top-to-bottom. **Everything you need is in this file** — you never
have to open `02-alpine-ajax/` (that's just the backup if a step goes wrong).

You edit **`01-razor-ssr/`** throughout. Steps are ordered most-essential →
most-droppable: if you run low on time, just stop after any step.

---

## Before you start

```bash
cd 01-razor-ssr
dotnet watch run
```

`dotnet watch` **hot-reloads** `.cshtml` edits (and applies the C# edits) as you
save — so changes appear in the browser without restarting. Open the printed
URL, click into **Q-1001** (`/Quotes/1`). Keep that tab visible.

> Optional opener: in DevTools, disable JavaScript and add/delete a note — it
> still works (full reload). That's the no-JS baseline. Re-enable JS before Step 1.

### ⚠️ Razor `@@` gotcha (read this once)

This is a `.cshtml` file, so a bare `@` starts C# code. Anywhere Alpine uses
`@` you must type **`@@`** (double), which renders as a single `@`:

| You want (Alpine) | You type (Razor) |
|---|---|
| `@ajax:before` | `@@ajax:before` |
| `@note:changed.window` | `@@note:changed.window` |
| `@click` | `@@click` |
| `@keyframes` | `@@keyframes` |

`$ajax`, `$event`, `$dispatch`, `x-target`, `x-sync` have no `@` — type them as-is.
(For a .NET crowd this is a nice aside: "yep, the Razor double-at.")

---

## Setup · Load Alpine

**File: `Pages/Shared/_Layout.cshtml`** — in `<head>`, right after the Tailwind
`<script>` line, add:

```html
    @* Alpine-AJAX plugin must load BEFORE Alpine core. Both deferred. *@
    <script defer src="~/lib/alpine-ajax.js"></script>
    <script defer src="~/lib/alpine.js"></script>
```

> *"Two script tags — no npm, no build step. Plugin first, then Alpine. That's
> the install."* (The files are already vendored in `wwwroot/lib`, so this runs
> offline.)

---

## Step 1 · `x-target` — add a note with no full reload  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the **add-note form** near the bottom:

```html
<form id="note-form" method="post" asp-page-handler="AddNote" asp-route-id="@Model.Quote.Id" class="mt-5 space-y-2 border-t border-slate-100 pt-5">
```

Add `x-target="notes"`:

```html
<form id="note-form" method="post" asp-page-handler="AddNote" asp-route-id="@Model.Quote.Id" x-target="notes" class="mt-5 space-y-2 border-t border-slate-100 pt-5">
```

**Do:** type a note, click **Add note**.
**Say:** *"No reload — Alpine posted the form, followed the redirect, found
`#notes` in the response and swapped just that in. But look: my text is still
sitting in the box. We only targeted the list, not the form."*

---

## Step 2 · Target the form too — reset + validation for free  `[CORE]`

**Same form** — change `x-target="notes"` to:

```html
x-target="notes note-form"
```

**Do:** add a note → form clears. Then click **Add note** with the box **empty**.
**Say:** *"Now the form's a target too, so it comes back empty. And free: submit
nothing and the server's ModelState validation renders straight back into the
form. We wrote zero JavaScript validation — the server is still the source of truth."*

---

## Step 3 · `x-sync` — the count updates itself  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the count in the right column:

```html
<div id="count" class="text-3xl font-semibold text-slate-900">@Model.Quote.Notes.Count</div>
```

Add `x-sync`:

```html
<div id="count" x-sync class="text-3xl font-semibold text-slate-900">@Model.Quote.Notes.Count</div>
```

**Do:** add a note — watch the count tick up.
**Say:** *"Nothing targets the count. `x-sync` makes it subscribe to any response
that contains a `#count` and refresh itself. The value already rides along in
the response — so no extra request."*

---

## Step 4 · Delete a note inline  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the **delete form** (in the note's view mode):

```html
<form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id">
```

Add `x-target="notes"`:

```html
<form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" x-target="notes">
```

**Do:** delete a note — gone, no reload, count drops.
**Say:** *"Row vanishes inline. And the count fixed itself again — it already has
`x-sync` from the last step."*

---

## Step 5 · Inline edit (view ↔ edit on one element)  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — four small additions on each note.

**(a)** The **Edit link** (view mode):

```html
<a href="/Quotes/@Model.Quote.Id?editNote=@note.Id" class="text-blue-600 hover:underline">Edit</a>
```
→ add `x-target="note-@note.Id"`:
```html
<a href="/Quotes/@Model.Quote.Id?editNote=@note.Id" x-target="note-@note.Id" class="text-blue-600 hover:underline">Edit</a>
```

**(b)** The **edit form** (edit mode):
```html
<form method="post" asp-page-handler="EditNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" class="space-y-2">
```
→ add `x-target="note-@note.Id"`:
```html
<form method="post" asp-page-handler="EditNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" x-target="note-@note.Id" class="space-y-2">
```

**(c)** The **edit textarea** — add `x-autofocus`:
```html
<textarea name="Input.Body" rows="2" x-autofocus class="w-full rounded-md border border-slate-300 p-2 text-sm">@note.Body</textarea>
```

**(d)** The **Cancel link** — add `x-target="note-@note.Id"`:
```html
<a href="/Quotes/@Model.Quote.Id" x-target="note-@note.Id" class="rounded-md px-3 py-1.5 text-sm text-slate-600 hover:bg-slate-100">Cancel</a>
```

**Do:** click **Edit** → the note becomes a form in place (cursor lands in it);
change the text, **Save** → swaps back to view; try **Cancel** too.
**Say:** *"Edit GETs that note rendered as a form and swaps just that `<li>`.
Save swaps it back to view. One element, toggling between two server-rendered
states — the edit-row pattern."*

---

> ### Everything below is the droppable tail — stop here if time is short.

---

## Step 6 · Confirm before deleting  `[tail]`

**File: `Pages/Quotes/Details.cshtml`** — on the **delete form** (from Step 4),
add the lifecycle hook (note the **`@@`**):

```html
<form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" x-target="notes" @@ajax:before="confirm('Delete this note?') || $event.preventDefault()">
```

**Do:** click Delete → confirm dialog; cancel it (nothing happens), then accept.
**Say:** *"`ajax:before` fires before the request goes out. If `confirm` returns
false we `preventDefault()` and nothing happens. Scoped to the delete form, so
Edit and Save aren't affected."*

---

## Step 7 · Toasts (`x-sync` + prepend)  `[tail]`

Two parts.

**(a) Backend — `Pages/Quotes/Details.cshtml.cs`.** Add one line before each
`return RedirectToPage(...)`:

In `OnPostAddNote`:
```csharp
        store.AddNote(quote, Input.Body!);
        TempData["Toast"] = "Note added";
        return RedirectToPage(new { id });
```
In `OnPostEditNote`:
```csharp
        store.EditNote(quote, noteId, Input.Body!);
        TempData["Toast"] = "Note edited";
        return RedirectToPage(new { id });
```
In `OnPostDeleteNote`:
```csharp
        store.DeleteNote(quote, noteId);
        TempData["Toast"] = "Note deleted";
        return RedirectToPage(new { id });
```

**(b) Layout — `Pages/Shared/_Layout.cshtml`.**

At the **very top of the file** (before `<!DOCTYPE html>`), add:
```html
@{ var toast = TempData["Toast"] as string; }
```

Then **just before `</body>`**, add the notifications list (note **`@@click`**):
```html
    @* Toasts. x-sync = updates from ANY Alpine response; prepend so new toasts
       stack on top. The server drops a <li> in here after each mutation. *@
    <ul id="notifications" x-sync x-merge="prepend" role="status"
        class="pointer-events-none fixed bottom-4 right-4 z-50 flex flex-col gap-2">
        @if (toast is not null)
        {
            <li x-data="{ show: false,
                          init() { this.$nextTick(() => this.show = true); setTimeout(() => this.dismiss(), 6000) },
                          dismiss() { this.show = false; setTimeout(() => this.$root.remove(), 500) } }"
                x-show="show" x-transition.duration.500ms
                class="pointer-events-auto flex items-center gap-3 rounded-md bg-slate-900 px-4 py-2 text-sm text-white shadow-lg">
                <span>@toast</span>
                <button @@click="dismiss" type="button" aria-label="Dismiss" class="text-slate-400 hover:text-white">&times;</button>
            </li>
        }
    </ul>
```

**Do:** add / edit / delete a note — a toast slides in bottom-right and
self-dismisses after 6s.
**Say:** *"`x-merge=prepend` so new toasts stack instead of replacing. The server
just drops a `<li>` in; x-sync slides it in. No form targets it. And it works
with no JS too — it just shows on the full reload."*

---

## Step 8 · Server events — the activity panel, decoupled  `[tail]`

**(a) Layout — `Pages/Shared/_Layout.cshtml`.** Just **after** the `</ul>` you
added in Step 7 (still before `</body>`), add the dispatcher slot:
```html
    @* Server-dispatched events. Empty on a normal load; after a mutation the
       server fills it with an element whose x-init fires 'note:changed'. *@
    <div id="server_events" x-sync>
        @if (toast is not null)
        {
            <div x-init="$dispatch('note:changed')"></div>
        }
    </div>
```

**(b) Page — `Pages/Quotes/Details.cshtml`.** Find the **Recent activity** section:
```html
        <section class="rounded-lg border border-slate-200 bg-white p-6">
            <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">Recent activity</h2>
```
Add the listener to the `<section>` (note **`@@note:changed`**):
```html
        <section x-init @@note:changed.window="$ajax('/Quotes/@Model.Quote.Id', { target: 'activity' })"
                 class="rounded-lg border border-slate-200 bg-white p-6">
            <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">Recent activity</h2>
```

**Do:** add a note — the activity panel updates a beat after the rest.
**Say:** *"The note form has no idea this panel exists. The server fires a signal
into `#server_events`; the panel hears it and refetches just itself. That's the
contrast: the count **pushes** via x-sync; the activity panel **pulls** via an
event. Both ship in our real app."*

---

## Step 9 · Loading state (pure CSS)  `[tail]`

**File: `Pages/Shared/_Layout.cshtml`** — in `<head>`, after the Alpine scripts,
add (note **`@@keyframes`**):
```html
    @* Alpine-AJAX auto-adds aria-busy to targets during a request and disables
       submit buttons. CSS is the only hook we need. *@
    <style>
        [aria-busy] { position: relative; opacity: .6; transition: opacity .15s; }
        [aria-busy]::after {
            content: ''; position: absolute; top: 50%; left: 50%;
            width: 20px; height: 20px; margin: -10px 0 0 -10px;
            border: 2px solid rgba(0,0,0,.15); border-top-color: rgba(0,0,0,.55);
            border-radius: 50%; animation: spin .6s linear infinite;
        }
        @@keyframes spin { to { transform: rotate(360deg); } }
    </style>
```

**Do:** in DevTools → **Network → Slow 3G**, then add a note — the target dims
with a spinner; the button disables.
**Say:** *"No JS. Alpine flags the busy element with `aria-busy` and disables the
submit button; a few lines of CSS do the rest. Local requests are instant, so
I've throttled the network so you can see it."*

---

## Step 10 · Optimize — lean fragments for Alpine requests  `[tail]`

Three edits.

**(a) New file — `Pages/Shared/_LiveRegions.cshtml`.** Move the toast + events
markup here so both layouts can share it. Full contents:
```html
@{ var toast = TempData["Toast"] as string; }

<ul id="notifications" x-sync x-merge="prepend" role="status"
    class="pointer-events-none fixed bottom-4 right-4 z-50 flex flex-col gap-2">
    @if (toast is not null)
    {
        <li x-data="{ show: false,
                      init() { this.$nextTick(() => this.show = true); setTimeout(() => this.dismiss(), 6000) },
                      dismiss() { this.show = false; setTimeout(() => this.$root.remove(), 500) } }"
            x-show="show" x-transition.duration.500ms
            class="pointer-events-auto flex items-center gap-3 rounded-md bg-slate-900 px-4 py-2 text-sm text-white shadow-lg">
            <span>@toast</span>
            <button @@click="dismiss" type="button" aria-label="Dismiss" class="text-slate-400 hover:text-white">&times;</button>
        </li>
    }
</ul>

<div id="server_events" x-sync>
    @if (toast is not null)
    {
        <div x-init="$dispatch('note:changed')"></div>
    }
</div>
```

**(b) `Pages/Shared/_Layout.cshtml`.** Delete the top `@{ var toast = ... }` line
and the two blocks you added in Steps 7 & 8 (`<ul id="notifications">…</ul>` and
`<div id="server_events">…</div>`), and replace them — just before `</body>` —
with:
```html
    <partial name="_LiveRegions" />
```

**(c) New file — `Pages/Shared/_LayoutAjax.cshtml`.** The chrome-free layout:
```html
@* Lean layout for Alpine-AJAX requests: no <html>/<head>, no nav — just the
   page body plus the live regions Alpine needs to find by id. *@
@RenderBody()
<partial name="_LiveRegions" />
```

**(d) `Pages/_ViewStart.cshtml`.** Switch layout on the request header:
```csharp
@{
    Layout = Context.Request.Headers.ContainsKey("X-Alpine-Request")
        ? "_LayoutAjax"
        : "_Layout";
}
```

**Do:** open DevTools → Network, add a note, click the request → **Response** is
just the fragment (no `<html>`/`<head>`/nav). Compare a full navigation (whole page).
**Say:** *"Alpine sends an `X-Alpine-Request` header on every request. When we see
it, we skip the chrome and send just the fragment. Full navigations still get the
whole page — so it all still works with no JS. The server decides how much to
send based on who's asking. That's the whole idea: logic and rendering stay on
the server; Alpine just swaps in the bits that changed."*

---

## If a step breaks

The finished, working version of every file is in `02-alpine-ajax/`. Open the
matching file there, copy the relevant bit across, and carry on. Or
`git -C .. checkout 02-alpine-ajax -- <path>` if you're comfortable.
