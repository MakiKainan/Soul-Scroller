# SoulScroll

2D incremental/idle game parodying doomscrolling. Core loop: **scroll → currency → upgrades → more/faster scrolling**. Same genre as Cookie Clicker / Adventure Capitalist — the player's main action generates a resource, resources buy generators/multipliers, generators run passively (including offline), numbers grow large.

## Stack

- Unity **6000.4.5f1**, Universal Render Pipeline (2D Renderer), new Input System.
- C# scripts under `Assets/Scripts`.
- Version control is **Plastic SCM** (`.plastic/`), not git. Don't run git commands here.
- No code exists yet — this file sets conventions for what gets built, not a description of what's there.

## Core loop design

- **Scroll action**: player input (tap/click/swipe) → mints "Attention" (or whatever the currency's called) directly. This is the one action that must always feel good — never gate it behind a menu.
- **Currency**: a `double` is not enough once numbers exceed ~1e15 (idle games blow past this in hours). Use a **BigDouble/BigNumber** type (mantissa+exponent) for all currency and production values from day one — retrofitting this later touches every system. Don't write your own; port a known-correct one (e.g. Antimatter Dimensions' `break_infinity.cs`) rather than hand-rolling float exponent math.
- **Upgrades/generators**: define as data, not code. One `ScriptableObject` type (`UpgradeDefinition`: id, cost curve, effect) with instances as assets. Adding upgrade #47 should mean adding an asset, not a new class.
- **Cost scaling**: standard idle-game curve is `cost = baseCost * rate^owned` (rate ~1.07–1.15). Keep this in one place (the SO or a shared cost-curve utility), not duplicated per upgrade.
- **Offline/passive progress**: store last-seen timestamp on save, compute elapsed production on load (`production/sec * secondsElapsed`, clamp to a max offline cap if desired). This is a core idle-game expectation, not optional polish.
- **Prestige/reset layer** (if added later): a second currency earned by resetting progress for permanent multipliers. Don't build this until the base loop is fun — it's a common idle-game addition but pure speculative scope right now.

## Architecture conventions

- **Event-driven currency**: currency changes fire a C# event/`UnityEvent`; UI subscribes and redraws. Don't have UI poll a singleton every frame.
- **One save system**: JSON serialization to `Application.persistentDataPath`, versioned with a schema/save-version int so old saves can be migrated instead of wiped when fields change.
- **Avoid MonoBehaviour singletons for game state** where a plain C# class + one composition root will do — but don't fight Unity's grain either; a single `GameManager` MonoBehaviour holding the save/currency/tick systems is fine for a game this size. Don't split into a DI framework for one project.
- **Numbers formatting**: centralize "1.23M / 4.5B / 1.2e15" style formatting in one utility — every UI label needs it.

## What NOT to build speculatively

- No multiplayer/leaderboard scaffolding unless requested.
- No analytics/ads SDK integration until the loop is proven fun.
- No settings/options menu beyond mute + save-reset until asked.
- No localization system for a solo prototype.

## Testing

Idle-game math (cost curves, offline-progress calc, BigDouble formatting) is the part that silently breaks and is hard to eyeball — write small edit-mode tests for those, not for MonoBehaviour wiring or UI layout.
