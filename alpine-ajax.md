# Alpine-AJAX Reference & Patterns

Source: https://alpine-ajax.js.org

## Core API

### x-target
Enables AJAX on forms/links. Value = space-separated target element IDs to replace.

```html
<form x-target="comments" method="post" action="/comment">
<a href="/page" x-target="main">Link</a>
```

**Modifiers:**
- `x-target.push` — push URL to browser history
- `x-target.replace` — replace URL without history entry
- `x-target.422="form_id"` — target on 422 responses
- `x-target.4xx="form_id"` — target on 400-class responses
- `x-target.error="form_id"` — target on 400/500-class responses
- `x-target.back="form_id"` — target on same-page redirects
- `x-target.away="form_id"` — target on cross-page redirects

**Special targets:** `_top` (full reload), `_none` (no action)

**Multiple targets:** `x-target="records pagination"` updates both elements.

**Target aliases:** `x-target="target_id:source_id"` when IDs differ between page and response.

### x-merge
Controls how incoming content merges with targets.

| Strategy | Behavior |
|----------|----------|
| `replace` | (default) Replace entire target element |
| `update` | Update target's inner content only |
| `before` | Insert before target |
| `after` | Insert after target |
| `prepend` | Add at start of target's children |
| `append` | Add at end of target's children |
| `morph` | Fine-grained DOM patching (needs Alpine Morph Plugin) |

`x-merge.transition` — animate with View Transitions API.

### x-sync
Auto-updates elements matching server response IDs, even if not explicitly targeted. Element must have unique `id`.

```html
<ul x-sync id="notifications"></ul>
```

### x-autofocus
Restores keyboard focus after AJAX content swap.

```html
<input name="email" x-autofocus />
<a href="/edit" x-target="contact" x-autofocus>Edit</a>
```

### x-headers
Adds custom request headers.

```html
<form x-headers="{'X-Custom': 'value'}" x-target="results">
```

Default headers sent: `X-Alpine-Request: true`, `X-Alpine-Target: [ids]`.

### formnoajax
Disables AJAX for a specific submit button within an x-target form.

```html
<button formnoajax>Full page submit</button>
```

## $ajax() Magic Helper

Programmatic AJAX requests from Alpine expressions.

```html
<div @change="$ajax('/validate', { method: 'post', body: { email } })">
```

Options: `method`, `target` (string), `targets` (array), `body` (object), `focus` (bool), `sync` (bool), `headers` (object).

## Events

| Event | When | Cancelable |
|-------|------|-----------|
| `ajax:before` | Before request | Yes |
| `ajax:send` | Request issued | No |
| `ajax:redirect` | 300-class response | No |
| `ajax:success` | 200/300 response | No |
| `ajax:error` | 400/500 response | No |
| `ajax:sent` | After response | No |
| `ajax:missing` | Target missing in response | Yes |
| `ajax:merge` | Content being merged | Yes |
| `ajax:merged` | After merge complete | No |
| `ajax:after` | All merging settled | No |

## Loading States

- Submit buttons auto-disabled during requests
- `aria-busy="true"` applied to all target elements during requests
- Use CSS `[aria-busy]` to style loading indicators

## Patterns (from official examples)

### 1. Toggle Button
Switch between states (like/unlike) by toggling HTTP method:

```html
<!-- Initial: POST to like -->
<form id="like" x-target method="post" action="/comments/1/like">
  <button name="user_id" value="1">Like</button>
</form>

<!-- Response: DELETE to unlike -->
<form id="like" x-target method="delete" action="/comments/1/like">
  <button name="user_id" value="1" x-autofocus>Unlike</button>
</form>
```

### 2. Loading Indicator
Style with CSS on the auto-applied `aria-busy` attribute:

```css
[aria-busy] { position: relative; opacity: .75; }
[aria-busy]:before {
  content: ''; position: absolute; top: 50%; left: 50%;
  width: 64px; height: 64px; margin: -32px 0 0 -32px;
  border: 6px solid rgba(0,0,0,0.15); border-radius: 50%;
  border-top-color: rgba(0,0,0,0.5);
  animation: rotate 1s linear infinite;
}
@keyframes rotate { 100% { transform: rotate(360deg); } }
```

### 3. Inline Edit
Toggle between view and edit mode by replacing the same container:

```html
<!-- View mode -->
<div id="contact_1">
  <p><strong>Name</strong>: Finn</p>
  <a href="/contacts/1/edit" x-target="contact_1">Edit</a>
</div>

<!-- Edit mode (server response) -->
<form id="contact_1" x-target method="put" action="/contacts/1">
  <input name="first_name" value="Finn" x-autofocus>
  <button>Update</button>
  <a href="/contacts/1" x-target="contact_1">Cancel</a>
</form>
```

### 4. Delete Row
Delete with confirmation, replace tbody with updated content:

```html
<tbody id="contacts" x-init @ajax:before="confirm('Are you sure?') || $event.preventDefault()">
  <tr>
    <td>Finn</td>
    <td>
      <form method="delete" action="/contacts/1" x-target="contacts">
        <button>Delete</button>
      </form>
    </td>
  </tr>
</tbody>
```

### 5. Edit Row
Inline table row editing — replace a single `<tr>`:

```html
<!-- View -->
<tr id="contact_1">
  <td>Finn</td>
  <td><a href="/contacts/1/edit" x-target="contact_1" x-autofocus>Edit</a></td>
</tr>

<!-- Edit (response) -->
<tr id="contact_1">
  <td><input form="contact_1_form" name="name" value="Finn" x-autofocus></td>
  <td>
    <a href="/contacts" x-target="contact_1">Cancel</a>
    <form id="contact_1_form" x-target="contact_1" method="put" action="/contacts/1">
      <button>Save</button>
    </form>
  </td>
</tr>
```

### 6. Bulk Update
Checkboxes associated with an external form via `form` attribute:

```html
<table id="contacts">
  <tbody>
    <tr>
      <td><input type="checkbox" form="contacts_form" name="ids" value="1"></td>
      <td>Finn</td>
      <td>Active</td>
    </tr>
  </tbody>
</table>
<form x-target="contacts" id="contacts_form" method="put" action="/contacts">
  <button name="status" value="Active">Activate</button>
  <button name="status" value="Inactive">Deactivate</button>
</form>
```

### 7. Instant Search
Debounced search with `requestSubmit()`:

```html
<form x-target="contacts" action="/contacts" role="search" autocomplete="off">
  <input type="search" name="search"
         @input.debounce="$el.form.requestSubmit()"
         @search="$el.form.requestSubmit()">
  <button x-show="false">Search</button>
</form>
<tbody id="contacts">...</tbody>
```

Key: hidden submit button for progressive enhancement. `@search` fires when user clears the input.

### 8. Filterable Content
Filter buttons with morph for focus preservation:

```html
<div id="contacts" x-merge="morph">
  <form action="/contacts" x-target="contacts">
    <button name="status" value="Active" aria-pressed="false">Active</button>
    <button name="status" value="Inactive" aria-pressed="false">Inactive</button>
  </form>
  <table>...</table>
</div>
```

Requires Alpine Morph Plugin. Server toggles `aria-pressed` and adds Reset button.

### 9. Inline Validation
Validate fields on change via `$ajax()`:

```html
<form x-data="{ email: '' }">
  <div id="email_field" @change="$ajax('/validate-email', {
    method: 'post', body: { email }
  })">
    <input type="email" x-model="email">
  </div>
</form>
```

Server returns the same `#email_field` div with error message and `aria-describedby`.

### 10. Lazy Loading
Load content on init:

```html
<article id="post" x-init="$ajax('/posts/1')">
  <div class="loading-skeleton">...</div>
</article>
```

### 11. Dialog (Modal)
Open modal on AJAX trigger, load content into it:

```html
<ul x-init @ajax:before="$dispatch('dialog:open')">
  <li><a href="/contacts/1" x-target="contact">Finn</a></li>
</ul>

<dialog x-init @dialog:open.window="$el.showModal()">
  <div id="contact"></div>
  <form method="dialog" novalidate><button>Close</button></form>
</dialog>
```

### 12. Dialog Form (Modal with Form)
Edit in modal, refresh table on save via server events:

```html
<tbody id="contacts" x-init
       @ajax:before="$dispatch('dialog:open')"
       @contact:updated.window="$ajax('/contacts')">
  <tr>
    <td>Finn</td>
    <td><a href="/contacts/1/edit" x-target="contact">Edit</a></td>
  </tr>
</tbody>

<dialog x-init
        @dialog:open.window="$el.showModal()"
        @contact:updated.window="$nextTick(() => $el.close())">
  <div id="contact"></div>
</dialog>
```

Server response after save includes: `<div x-init="$dispatch('contact:updated')"></div>` to trigger table refresh and modal close.

### 13. Progress Bar
Poll server with `x-init` + `setTimeout`:

```html
<div id="jobs" x-init="setTimeout(() => $ajax('/jobs/1'), 600)">
  <div role="progressbar" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100">
    <svg style="width:25%; transition: width .3s">
      <rect width="100%" height="100%" fill="blue"></rect>
    </svg>
  </div>
</div>
```

Each response includes another `x-init="setTimeout(..."` until complete.

### 14. Infinite Scroll
Combine Alpine Intersect Plugin with append merge:

```html
<tbody id="records" x-merge="append">...</tbody>
<div id="pagination" x-init x-intersect="$ajax('/contacts?page=2', { target: 'records pagination' })"></div>
```

### 15. Notifications (Toasts)
Use `x-sync` + `x-merge="prepend"` for server-pushed notifications:

```html
<ul x-sync id="notification_list" x-merge="prepend" role="status"></ul>

<!-- Server includes in any response: -->
<ul x-sync id="notification_list">
  <li x-data="{ show: false, init() { this.$nextTick(() => this.show = true) } }"
      x-show="show" x-transition.duration.500ms>
    <span>Success!</span>
    <button @click="show = false; setTimeout(() => $root.remove(), 500)">x</button>
  </li>
</ul>
```

### 16. Server Events
Decouple components with custom window events:

```html
<!-- List refreshes when event fires -->
<ul x-init @comment:created.window="$ajax('/comments')" id="comments">...</ul>

<!-- Server includes event dispatcher in response -->
<div x-sync id="server_events">
  <div x-init="$dispatch('comment:created')"></div>
</div>
```
