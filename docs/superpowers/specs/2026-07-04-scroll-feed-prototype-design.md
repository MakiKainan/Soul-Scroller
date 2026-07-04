# Phase 1 Prototype: Scrollable Feed + Money Design

## Goal

Prove the core doomscroll loop before any real game systems exist: player scrolls a
portrait, Instagram/TikTok-style feed of solid-color cards, each completed swipe
mints money, and a reset button zeroes the money counter. Colors only — no art,
no upgrades, no persistence.

## Scope

In scope:
- Portrait-locked screen (imitates a phone feed).
- Vertically scrolling feed of full-screen solid-color cards.
- Snap-to-card scrolling: a swipe settles on exactly one card, not a free-scroll position.
- Every completed swipe (settling on a new card, either direction) adds a fixed
  amount of money.
- On-screen money display.
- Reset button that zeroes money only.

Out of scope (explicitly deferred to later phases):
- Upgrades, shop, prestige.
- BigDouble/large-number currency type — plain `double` is used here and is
  expected to be swapped out once upgrades/exponential growth are added
  (see `ponytail:` comment in `GameManager`).
- Save/load and offline progress.
- Infinite or pooled feed generation — a fixed set of pre-generated cards is enough
  to prove the loop.
- Any real content/art in the cards (solid colors only).

## Project settings

- Player Settings → Resolution and Presentation → Default Orientation: **Portrait**.
- Default resolution 1080×1920 (9:16); editor Game view aspect set to match.

## Components

### Feed
- Canvas (Screen Space – Overlay) → `ScrollRect` (vertical movement only, horizontal
  disabled) → Viewport → Content.
- Content has a `Vertical Layout Group` (no spacing, child alignment top) and a
  `Content Size Fitter` (vertical: preferred size) so the layout group sizes it
  automatically as cards are added.
- At scene start, a spawner instantiates ~50 full-screen `Image` cards under
  Content, each assigned a random solid color (`Random.ColorHSV` or similar).
  Fixed count, generated once — no runtime pooling/recycling in this phase.

### Snap + swipe detection — `FeedScroller`
- One `MonoBehaviour` on the ScrollRect GameObject.
- On `OnEndDrag` (from `IEndDragHandler`), computes the nearest card index from
  the Content's current anchored position divided by card height, then smoothly
  moves (lerp/coroutine) Content to the exact snapped position for that index.
- Tracks the last *settled* card index. When a snap completes on an index
  different from the last settled index, that is one completed swipe — calls
  `GameManager.Instance.AddMoney(fixedSwipeAmount)` once per swipe, regardless of
  scroll direction.
- `fixedSwipeAmount` is a serialized field on `FeedScroller` for easy tuning in
  the Inspector.

### Currency + reset — `GameManager`
- Single `MonoBehaviour`, one instance in the scene (simple scene singleton via
  `GameManager.Instance`, no DI framework).
- Holds `double money` (starts at 0).
- `AddMoney(double amount)`: adds to `money`, raises `OnMoneyChanged(double)`.
- `ResetMoney()`: sets `money` to 0, raises `OnMoneyChanged(double)`. Does not
  touch the feed or scroll position.
- `ponytail:` comment on the `money` field noting it must become a BigDouble type
  once upgrades/exponential growth are introduced — a plain `double` is
  sufficient for this loop-proving phase only.

### UI
- TextMeshPro label subscribed to `GameManager.OnMoneyChanged`, displaying the
  current money value (plain number formatting is fine at this phase — no
  K/M/B abbreviation needed yet).
- A UI `Button` wired (via Inspector `OnClick`) to `GameManager.Instance.ResetMoney()`.

## Data flow

```
Player drag → ScrollRect drag → FeedScroller.OnEndDrag
  → snap to nearest card → index changed?
    → GameManager.AddMoney(fixedSwipeAmount)
      → OnMoneyChanged event → money label updates

Reset button click → GameManager.ResetMoney()
  → OnMoneyChanged event → money label updates to 0
```

## Testing

This phase is almost entirely Unity wiring (ScrollRect, layout, UI bindings),
which is best verified by running the game, not unit tests. The one piece of
non-trivial logic is the nearest-card-index snap calculation in `FeedScroller`
— worth a small edit-mode test asserting the index math against a few known
content-position/card-height inputs.

## Notes on tooling

This repo uses Plastic SCM (`.plastic/`), not git. This spec is saved to disk
under `docs/superpowers/specs/` per convention but is not committed to git.
