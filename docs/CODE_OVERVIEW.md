# SoulScroll Phase 1 — Code Overview

What each script does, how they connect, and what to touch for common changes.

## The scripts

### `Assets/Scripts/GameManager.cs`
The single source of truth for money.

- `GameManager.Instance` — scene singleton, set in `Awake()`.
- `Money` (double) — current balance, read-only from outside.
- `OnMoneyChanged` (event) — fires with the new balance whenever it changes. Anything that displays money subscribes to this instead of polling every frame.
- `AddMoney(amount)` — adds to the balance and fires the event. Called by `FeedScroller` once per completed swipe.
- `ResetMoney()` — zeroes the balance and fires the event. Called by the Reset button.

**Touch this if:** you want money to behave differently in general — e.g. multipliers, a different currency type (the `double` → BigDouble swap noted in the code comment belongs here), saving/loading money to disk.

### `Assets/Scripts/FeedScroller.cs`
All the scroll-feel logic. This is the file you'll touch most.

- `cardHeight` (Inspector field, default `1920`) — how tall one card is, in canvas units. Must match the actual card height built into the scene.
- `moneyPerSwipe` (Inspector field, default `1`) — money awarded per completed swipe.
- `snapSpeed` (Inspector field, default `10`) — how fast the feed glides into place after you let go. Higher = snappier.
- `swipeThreshold` (Inspector field, default `960`, half of `cardHeight`) — how far you need to drag before it commits to the next/previous card instead of snapping back.
- `OnBeginDrag`/`OnEndDrag` — Unity calls these automatically while the player drags. `OnEndDrag` compares how far the content actually moved against `swipeThreshold` to decide: next card, previous card, or snap back to the current one.
- `Update()` — while a snap is in progress, glides the content toward the target card position every frame. When it arrives, if the card actually changed, calls `GameManager.Instance.AddMoney(...)`.
- `CalculateNearestIndex(...)` (static) — pure math, no Unity dependencies: given the feed's current position, works out which card is closest. Covered by the unit tests.

**Touch this if:** you want the scroll to feel different — faster/slower snap, easier/harder swipe commit, different money-per-swipe, or you're changing how many cards are visible at once.

**Don't touch the sign convention** (`-targetIndex * cardHeight`, `-contentAnchoredY / cardHeight`) without also checking `Assets/Editor/FeedSceneSetup.cs`'s `Content` anchor setup — they have to agree, and that mismatch is exactly the bug that got fixed in this file.

### `Assets/Scripts/FeedSpawner.cs`
Fills the feed with cards. Runs once, in `Start()`.

- `cardPrefab` (Inspector field) — the prefab to stamp out (`Assets/Prefabs/ColorCard.prefab`).
- `content` (Inspector field) — where to put the spawned cards.
- `cardCount` (Inspector field, default `50`) — how many cards to spawn.
- Each spawned card gets a random color via `Random.ColorHSV`.

**Touch this if:** you want actual content instead of random colors (e.g. images, text), or a different spawn count/behavior (infinite/pooled scrolling is a deliberate later-phase item, not built yet — see `CLAUDE.md`).

### `Assets/Scripts/MoneyLabel.cs`
Pure UI glue — no logic of its own.

- Subscribes to `GameManager.OnMoneyChanged` in `Start()`, unsubscribes in `OnDestroy()`.
- Formats the balance as `$<number>` and writes it to its `TMP_Text`.

**Touch this if:** you want different money formatting (e.g. `1.2K`/`3.4M` once numbers get big — the "centralize number formatting" convention from `CLAUDE.md` would live here or in a shared utility this script calls).

### `Assets/Editor/FeedSceneSetup.cs` (Editor-only, not in builds)
A one-shot scene builder, not gameplay code. It exists because the scene hierarchy (Canvas, ScrollRect, Viewport, Content, the `ColorCard` prefab, the Reset button's click wiring) was built headlessly rather than by hand in the Editor.

Run via **Tools > Setup Feed Scene** in the Unity menu. It's idempotent-ish — re-running it rebuilds everything from scratch into `Assets/Scenes/SampleScene.unity`.

**Touch this if:** you want to change the scene *structure* itself — layout, anchors, which GameObjects exist, or how components get wired together. If you just want to retune a *value* (money per swipe, snap speed, card count), it's faster to change it directly in the Inspector on the already-built scene, or change the field default in the relevant script — you don't need to re-run this.

### `Assets/Tests/EditMode/FeedScrollerTests.cs`
Unit tests for `FeedScroller.CalculateNearestIndex` only — the one piece of math worth testing per `CLAUDE.md`'s testing guidance. Run via **Window > General > Test Runner > EditMode > Run All**.

## How it fits together

```
Player drags the feed
  → ScrollRect (built-in Unity component) moves Content in real time
  → FeedScroller.OnEndDrag decides: next card / previous card / snap back
  → FeedScroller.Update() glides Content to that card's position
  → if the card actually changed: GameManager.AddMoney(moneyPerSwipe)
      → OnMoneyChanged event fires
      → MoneyLabel updates the on-screen text

Reset button click
  → GameManager.ResetMoney()
      → OnMoneyChanged event fires
      → MoneyLabel updates to $0 (feed position untouched)
```

## Quick reference: "I want to change X"

| Change | File / place to touch |
|---|---|
| How much money per swipe | `FeedScroller.moneyPerSwipe` (Inspector or default in code) |
| How far you must drag to commit | `FeedScroller.swipeThreshold` |
| How fast the snap animation is | `FeedScroller.snapSpeed` |
| How many cards / their colors | `FeedSpawner.cardCount`, or `FeedSpawner.Start()`'s color logic |
| What a "card" looks like | `Assets/Prefabs/ColorCard.prefab` |
| Money display formatting | `MoneyLabel.UpdateLabel` |
| What Reset actually resets | `GameManager.ResetMoney()` |
| Scene structure/wiring itself | `FeedSceneSetup.cs`, then re-run **Tools > Setup Feed Scene** |
| Adding a new upgrade/currency system | New, per `CLAUDE.md` conventions (ScriptableObject-defined upgrades, not hardcoded) — not built yet |

For most day-to-day tuning (speeds, thresholds, amounts), you don't need to touch `FeedSceneSetup.cs` at all — just edit the values directly on the components in the already-built scene, or change the field defaults in the relevant script.
