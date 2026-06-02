# Demo guide — every change, in order, fully inline

Follow this top-to-bottom. **Everything you need is in this file** — you never
have to open `02-alpine-ajax/` (that's just the backup if a step goes wrong).
For bare copy/paste blocks with no prose, use **`SNIPPETS.md`** instead.

You edit **`01-razor-ssr/`** throughout. Steps are ordered most-essential →
most-droppable: if you run low on time, just stop after any step.

> **Reading the snippets.** The bit you actually add is shown
> <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">highlighted in plum</b>
> (same as the deck). The rest of the line is context so you can find it.

## Cheat sheet (at a glance)

| # | Beat | The change |
|---|------|-----------|
| 0 | Load Alpine | two `<script>` tags in `_Layout` (plugin before core) |
| 1 | Navigate | `x-target.push="page"` on quote links + back link — list↔detail, no reload, Back works |
| 2 | Add note | `x-target="notes"` on the form — note appears, form keeps text |
| 3 | + the form | `x-target="notes note-form"` — form resets, validation just works |
| 4 | Count | `x-sync` on `#count` — updates itself |
| 5 | Delete | `x-target="notes"` on the delete form |
| 6 | Inline edit | `x-target="note-@note.Id"` on edit link / form / cancel + `x-autofocus` |
| 7 | Confirm | `x-on:ajax:before="confirm('Delete this note?') \|\| $event.preventDefault()"` |
| 8 | Toasts | `TempData["Toast"]` in handlers + `#notifications` (`x-sync` + `x-merge="prepend"`) |
| 9 | Events | `#server_events` dispatcher + `#activity` listens `@@note:changed.window="$ajax(...)"` |
| 10 | Loading | `[aria-busy]` CSS spinner (Alpine sets it on targets) |
| 11 | Optimize | `Layout = null` (or `_LayoutAjax`) on the `X-Alpine-Request` header |

Full text + talking points for every step below.

---

## Before you start

```bash
cd 01-razor-ssr
dotnet watch
```

`dotnet watch` **hot-reloads** `.cshtml` edits (and applies the C# edits) as you
save — so changes appear in the browser without restarting. Open the printed
URL — it lands on **/Quotes** (the list). Keep that tab visible.

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

> **One sharp edge (this bit us).** On a `<form asp-*>` the ASP.NET *form tag
> helper* rebuilds the tag and **silently drops any attribute whose name starts
> with `@`** — so `@@ajax:before` on the delete form renders as *nothing* and the
> confirm never fires. Plain elements (`<section>`, `<button>`, `<a>`) are fine.
> Fix: on tag-helper forms use Alpine's longhand **`x-on:ajax:before`** (no `@`,
> identical behaviour). That's why Step 7 uses `x-on:` and Step 9's `@@note:changed`
> (on a `<section>`) stays as-is.

---

## Setup · Load Alpine

**File: `Pages/Shared/_Layout.cshtml`** — in `<head>`, right after the Tailwind
`<script>` line, add:

<pre>    @* Alpine-AJAX plugin must load BEFORE Alpine core. Both deferred. *@
    <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">&lt;script defer src="~/lib/alpine-ajax.js"&gt;&lt;/script&gt;</b>
    <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">&lt;script defer src="~/lib/alpine.js"&gt;&lt;/script&gt;</b></pre>

> *"Two script tags — no npm, no build step. Plugin first, then Alpine. That's
> the install."* (The files are already vendored in `wwwroot/lib`, so this runs
> offline.)

---

## Step 1 · Boosted navigation — list ↔ detail, no full reload  `[CORE]`

Both pages already wrap their content in `<div id="page">` — the shared swap
target. We just point the navigating links at it. `x-target.push` swaps `#page`
**and** pushes the URL to history (so Back/Forward work).

**(a)** `Pages/Quotes/Index.cshtml` — each row currently navigates with a
hand-written `onclick`. Replace that inline JS with declarative Alpine: forward
the row's click to its link, and give the link `x-target.push="page"`.

```html
<tr onclick="location.href='/Quotes/@quote.Id'" class="cursor-pointer hover:bg-slate-50">
    <td class="px-4 py-3">
        <a href="/Quotes/@quote.Id" class="font-medium text-[#6B3D5A] hover:underline">@quote.Reference</a>
```
→ becomes:

<pre>&lt;tr <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">@@click="$event.target.closest('a') || $el.querySelector('a').click()"</b> class="cursor-pointer hover:bg-slate-50"&gt;
    &lt;td class="px-4 py-3"&gt;
        &lt;a href="/Quotes/@quote.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target.push="page"</b> class="font-medium text-[#6B3D5A] hover:underline"&gt;@quote.Reference&lt;/a&gt;</pre>

> The row's `@@click` just clicks its own link (skipping if you clicked the link
> directly), so the **whole row** stays clickable — but now it's the link's
> `x-target` doing AJAX, not a page reload.

**(b)** `Pages/Quotes/Details.cshtml` — the **← All quotes** back link (top of page):

```html
<a href="/Quotes" class="mb-4 inline-block text-sm text-[#6B3D5A] hover:underline">&larr; All quotes</a>
```
→ add `x-target.push="page"`:

<pre>&lt;a href="/Quotes" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target.push="page"</b> class="mb-4 inline-block text-sm text-[#6B3D5A] hover:underline"&gt;&amp;larr; All quotes&lt;/a&gt;</pre>

**Do:** from the list, click a quote → the detail loads with **no full reload**,
the URL becomes `/Quotes/1`. Click **← All quotes**, then hit the browser **Back**
and **Forward** buttons — the content swaps each time and the URL keeps up.
**Say:** *"Same `x-target`, now for navigation. The link targets `#page` — a
container both the list and the detail render — so Alpine fetches the next page,
swaps just that container, and `.push` writes the URL into history. The Back
button works because those are real history entries. No router, no client state —
every page is still server-rendered; we only swap the part that changes."*

> `.push` writes the URL to history; alpine-ajax restores `#page` on Back/Forward
> (popstate). Without `.push` it'd swap content but leave the URL stale.

---

## Step 2 · `x-target` — add a note with no full reload  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the **add-note form** near the bottom:

```html
<form id="note-form" method="post" asp-page-handler="AddNote" asp-route-id="@Model.Quote.Id" class="mt-5 space-y-2 border-t border-slate-100 pt-5">
```

Add `x-target="notes"`:

<pre>&lt;form id="note-form" method="post" asp-page-handler="AddNote" asp-route-id="@Model.Quote.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target="notes"</b> class="mt-5 space-y-2 border-t border-slate-100 pt-5"&gt;</pre>

**Do:** type a note, click **Add note**.
**Say:** *"No reload — Alpine posted the form, followed the redirect, found
`#notes` in the response and swapped just that in. But look: my text is still
sitting in the box. We only targeted the list, not the form."*

---

## Step 3 · Target the form too — reset + validation for free  `[CORE]`

**Same form** — change `x-target="notes"` to:

<pre>x-target="notes <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">note-form</b>"</pre>

**Do:** add a note → form clears. Then click **Add note** with the box **empty**.
**Say:** *"Now the form's a target too, so it comes back empty. And free: submit
nothing and the server's ModelState validation renders straight back into the
form. We wrote zero JavaScript validation — the server is still the source of truth."*

---

## Step 4 · `x-sync` — the count updates itself  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the count in the right column:

```html
<div id="count" class="text-3xl font-semibold text-slate-900">@Model.Quote.Notes.Count</div>
```

Add `x-sync`:

<pre>&lt;div id="count" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-sync</b> class="text-3xl font-semibold text-slate-900"&gt;@Model.Quote.Notes.Count&lt;/div&gt;</pre>

**Do:** add a note — watch the count tick up.
**Say:** *"Nothing targets the count. `x-sync` makes it subscribe to any response
that contains a `#count` and refresh itself. The value already rides along in
the response — so no extra request."*

---

## Step 5 · Delete a note inline  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — find the **delete form** (in the note's view mode):

```html
<form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id">
```

Add `x-target="notes"`:

<pre>&lt;form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target="notes"</b>&gt;</pre>

**Do:** delete a note — gone, no reload, count drops.
**Say:** *"Row vanishes inline. And the count fixed itself again — it already has
`x-sync` from the last step."*

---

## Step 6 · Inline edit (view ↔ edit on one element)  `[CORE]`

**File: `Pages/Quotes/Details.cshtml`** — four small additions on each note.
(The Edit control is a pencil icon now — find it by its `editNote=` href.)

**(a)** The **Edit link** (view mode):

```html
<a href="/Quotes/@Model.Quote.Id?editNote=@note.Id" aria-label="Edit" title="Edit" class="text-[#6B3D5A] hover:text-[#44243A]">
```
→ add `x-target="note-@note.Id"`:

<pre>&lt;a href="/Quotes/@Model.Quote.Id?editNote=@note.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target="note-@note.Id"</b> aria-label="Edit" title="Edit" class="text-[#6B3D5A] hover:text-[#44243A]"&gt;</pre>

**(b)** The **edit form** (edit mode):
```html
<form method="post" asp-page-handler="EditNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" class="space-y-2">
```
→ add `x-target="note-@note.Id"`:

<pre>&lt;form method="post" asp-page-handler="EditNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target="note-@note.Id"</b> class="space-y-2"&gt;</pre>

**(c)** The **edit textarea** — add `x-autofocus`:

<pre>&lt;textarea name="Input.Body" rows="2" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-autofocus</b> class="w-full rounded-md border border-slate-300 p-2 text-sm"&gt;@note.Body&lt;/textarea&gt;</pre>

**(d)** The **Cancel link** — add `x-target="note-@note.Id"`:

<pre>&lt;a href="/Quotes/@Model.Quote.Id" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-target="note-@note.Id"</b> class="rounded-md px-3 py-1.5 text-sm text-slate-600 hover:bg-slate-100"&gt;Cancel&lt;/a&gt;</pre>

**Do:** click **Edit** → the note becomes a form in place (cursor lands in it);
change the text, **Save** → swaps back to view; try **Cancel** too.
**Say:** *"Edit GETs that note rendered as a form and swaps just that `<li>`.
Save swaps it back to view. One element, toggling between two server-rendered
states — the edit-row pattern."*

---

> ### Everything below is the droppable tail — stop here if time is short.

---

## Step 7 · Confirm before deleting  `[tail]`

**File: `Pages/Quotes/Details.cshtml`** — on the **delete form** (from Step 5),
add the lifecycle hook. **Use `x-on:ajax:before` — not `@@ajax:before`** (the form
tag helper would drop a leading `@`; see the gotcha box above):

<pre>&lt;form method="post" asp-page-handler="DeleteNote" asp-route-id="@Model.Quote.Id" asp-route-noteId="@note.Id" x-target="notes" <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-on:ajax:before="confirm('Delete this note?') || $event.preventDefault()"</b>&gt;</pre>

**Do:** click Delete → confirm dialog; cancel it (nothing happens), then accept.
**Say:** *"`ajax:before` fires before the request goes out. If `confirm` returns
false we `preventDefault()` and nothing happens. Scoped to the delete form, so
Edit and Save aren't affected. `x-on:` is just Alpine's longhand for `@` — we use
it here because the .NET form helper strips `@`-prefixed attributes."*

---

## Step 8 · Toasts (`x-sync` + prepend)  `[tail]`

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

Then **just before `</body>`**, add the notifications list — all new (note **`@@click`**):
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

## Step 9 · Server events — the activity panel, decoupled  `[tail]`

**(a) Layout — `Pages/Shared/_Layout.cshtml`.** Just **after** the `</ul>` you
added in Step 8 (still before `</body>`), add the dispatcher slot — all new:
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
Add the listener to the `<section>` (this is a plain element, so **`@@note:changed`**
is correct here):

<pre>        &lt;section <b style="background:#ECDBE5;color:#44243A;border-radius:4px;padding:0 .2em">x-init @@note:changed.window="$ajax('/Quotes/@Model.Quote.Id', { target: 'activity' })"</b>
                 class="rounded-lg border border-slate-200 bg-white p-6"&gt;
            &lt;h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500"&gt;Recent activity&lt;/h2&gt;</pre>

> Put it on the **Recent-activity** `<section>` (the one wrapping `#activity`) —
> not the quote-header card. The `.window` modifier means it technically fires
> from anywhere, but keeping it on the panel that refetches itself is what makes
> the story read.

**Do:** add a note — the activity panel updates a beat after the rest.
**Say:** *"The note form has no idea this panel exists. The server fires a signal
into `#server_events`; the panel hears it and refetches just itself. That's the
contrast: the count **pushes** via x-sync; the activity panel **pulls** via an
event. Both ship in our real app."*

> ### How the activity sync actually works (the part that's confusing)
>
> Trace one **Add note**, end to end:
> 1. The form posts (`x-target="notes note-form"`). The handler saves, sets
>    `TempData["Toast"]`, and 302-redirects back to the page.
> 2. Alpine follows the redirect and gets a fresh render. Because a toast is
>    pending, **`#server_events` comes back containing
>    `<div x-init="$dispatch('note:changed')">`**.
> 3. `#server_events` has `x-sync`, so Alpine swaps it in *even though no form
>    targeted it*. The new `<div>` mounts → its `x-init` runs →
>    `$dispatch('note:changed')` fires a normal DOM event on `window`.
> 4. The Recent-activity `<section>` is listening with
>    `@note:changed.window`. It catches the event and runs
>    `$ajax('/Quotes/1', { target: 'activity' })` — a **second** request that
>    re-renders just `#activity`.
>
> So it's a relay, not magic: **server → `#server_events` (x-sync) → `window`
> event → listener → `$ajax` refetch of `#activity`.** The point of the
> indirection is decoupling — the add-note form never names the activity panel.
> Anything that sets a toast (add, edit, delete, or some future action) makes the
> panel refresh, and you never wire them together. That's why the count (which
> just rides along in the same response via `x-sync`) is the "push" example and
> the activity panel is the "pull" example.

---

## Step 10 · Loading state (pure CSS)  `[tail]`

**File: `Pages/Shared/_Layout.cshtml`** — in `<head>`, after the Alpine scripts,
add (note **`@@keyframes`**):
```html
    @* Alpine-AJAX auto-adds aria-busy to targets during a request and disables
       submit buttons. CSS is the only hook we need. *@
    <style>
        /* Skip the invisible live-region plumbing (#notifications, #server_events)
           so they don't render a stray spinner — only real content shows loading. */
        [aria-busy]:not(#notifications):not(#server_events) { position: relative; opacity: .6; transition: opacity .15s; }
        [aria-busy]:not(#notifications):not(#server_events)::after {
            content: ''; position: absolute; top: 50%; left: 50%;
            width: 20px; height: 20px; margin: -10px 0 0 -10px;
            border: 2px solid rgba(0,0,0,.15); border-top-color: rgba(0,0,0,.55);
            border-radius: 50%; animation: spin .6s linear infinite;
        }
        @@keyframes spin { to { transform: rotate(360deg); } }
    </style>
```

> **Why the `:not(...)`?** Alpine marks *every* synced element `aria-busy`,
> including the invisible plumbing (`#notifications`, `#server_events`). Without
> the exclusion `#server_events` — an empty full-width div at the bottom of the
> page — paints a stray spinner floating under the card (the mystery third
> spinner). Excluding the two live regions leaves spinners only on real content.

**Do:** in DevTools → **Network → Slow 3G**, then add a note — the target dims
with a spinner; the button disables.
**Say:** *"No JS. Alpine flags the busy element with `aria-busy` and disables the
submit button; a few lines of CSS do the rest. Local requests are instant, so
I've throttled the network so you can see it."*

---

## Step 11 · Optimize — lean fragments for Alpine requests  `[tail]`

> ### ⚠️ Restart `dotnet watch` after this step
> `_ViewStart.cshtml` and **new** `.cshtml` files (`_LayoutAjax`, `_LiveRegions`)
> do **not** reliably hot-reload — the layout switch won't take and every
> response stays the full page (the "ajax is returning full HTML" symptom). Stop
> `dotnet watch` and start it again once these four edits are in.

Three edits (well, four files).

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
and the two blocks you added in Steps 8 & 9 (`<ul id="notifications">…</ul>` and
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

**Now restart `dotnet watch`.** Then: open DevTools → Network, add a note or click
between list and detail, click the request → **Response** is just the fragment
(your `#page`/`#notes`, no `<html>`/`<head>`/nav). Compare a full navigation.
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
