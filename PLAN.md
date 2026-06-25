# TUSK — 12-Week Plan to Steam Early Access

## Philosophy

Avoid the Daily Heist failure mode: **don't iterate on broken foundations.** Each week ends with a playtest checkpoint. If the foundation isn't fun by that week's deadline, we kill or pivot — no "just one more iteration" loops.

**Required from Peter:**
- **30-min daily playtest** during build phases. Without this, we're flying blind.
- **Weekly direction review** (15 min) where Peter sees the build progress + approves next week's scope.
- **Hard kill commitment** if a phase fails its checkpoint.

---

## Week-by-Week

### Week 1 — Foundation (alpha controller + island)

**Goal:** Walk a stylized 3D character around a procedural island. No combat yet, no creatures, no UI. Just movement + camera.

| Day | Deliverable |
|-----|-------------|
| 1 | Unity 6 LTS URP project + Synty Nature pack imported + 100x100 floor plane with terrain |
| 2 | Third-person character controller (WASD movement, mouse-look camera, sprint, jump) |
| 3 | Procedural island generator (random tree/rock placement on grid, Synty props) |
| 4 | Camera polish (Cinemachine third-person rig, smooth follow, mouse-controlled orbit) |
| 5 | Basic stats (HP, stamina) + minimal HUD (HP bar, stamina bar) |
| 6 | **PLAYTEST CHECKPOINT**: 5-min walk-around. Does the movement FEEL good? |
| 7 | Buffer day — fix controller feel issues OR cut Week 1 to kill if controller is bad |

**Kill criterion:** If at end of week 1 the player controller doesn't feel responsive and fun to move around in, we kill the project.

**Output**: builds a Windows .exe Peter can run.

---

### Week 2 — Storm + Combat

**Goal:** Add the storm zone (shrinking circle of death) and basic combat against placeholder enemies.

| Day | Deliverable |
|-----|-------------|
| 1 | Shrinking storm zone visualizer (transparent dome, shrinks over 20 min, damage outside) |
| 2 | Enemy spawn system + 3 placeholder enemies (cubes that chase you) |
| 3 | Melee combat (left-click attack, hit detection, enemy HP) |
| 4 | Death/respawn flow (you die → run ends → return to start menu) |
| 5 | Replace 3 placeholder enemies with Meshy-generated creatures (wolf, boar, lizard) |
| 6 | **PLAYTEST CHECKPOINT**: 15-min run, combat against 3 creatures, escape the zone |
| 7 | Buffer / kill day |

**Kill criterion:** If combat feels janky or storm zone tension doesn't create urgency, we kill.

---

### Week 3 — Taming + Crafting + Resources

**Goal:** Add the Pokémon hook — tame defeated creatures to fight alongside you.

| Day | Deliverable |
|-----|-------------|
| 1 | Resource gathering (chop trees → wood, mine rocks → stone, kill creatures → meat) |
| 2 | Crafting recipe system (open menu, combine resources → items) — 8 basic items |
| 3 | Inventory UI (grid of slots, drag-drop, hotbar) |
| 4 | Taming mechanic: enemy below 30% HP, hold E for 3s → tame chance based on HP% and item used |
| 5 | Tamed creature AI (follows player, attacks nearby enemies, can be commanded with Q) |
| 6 | **PLAYTEST CHECKPOINT**: 25-min run — gather, craft, fight, tame 1 creature, survive |
| 7 | Buffer / kill day |

**Kill criterion:** If taming a creature doesn't feel like a victory moment, we kill the Pokémon hook.

---

### Week 4 — Extraction + Meta-Progression + Daily Seed

**Goal:** Complete the core loop. You can WIN a run by extracting, and progress persists between runs.

| Day | Deliverable |
|-----|-------------|
| 1 | Extraction portal (spawns at random location, you must reach it before zone closes) |
| 2 | Run summary screen (loot, creatures tamed, survival time, score) |
| 3 | Meta-progression: save profile across runs, unlock new starter loadouts (3 starters) |
| 4 | Daily seed system: same procedural island for every player on a given UTC date |
| 5 | Steam Leaderboards integration (post score after extraction or death) |
| 6 | **PLAYTEST CHECKPOINT**: 1 full run — drop → survive → extract → see leaderboard |
| 7 | Buffer day |

**Kill criterion:** If a full run doesn't feel like an engaging 20-30 min experience, we kill.

**END OF MONTH 1 — go/no-go gate.** If the core loop isn't fun by end of week 4, abort the project. Sunk cost so far: ~$50 in Meshy + Synty already owned.

---

### Week 5-6 — Content Pass

**Goal:** Variety. 30 creatures, 20 craftable items, 5 biome patches within the island.

- Meshy-generate 27 more creatures (week 5)
- Author 12 more craftable items + recipes (week 5)
- Add biome variation: forest / meadow / rocky / cave / shore patches within the island (week 6)
- Permanent unlocks: rare creature trophies, blueprint discoveries, skill points (week 6)

**PLAYTEST CHECKPOINT** end of week 6: 3 full runs in a row. Did each feel different?

---

### Week 7-8 — Polish & Alpha Test

**Goal:** Make it ship-quality. 10 alpha testers playing builds.

- Sound design (CC0 ambient + creature audio + UI clicks + storm howl)
- Particle FX (combat hits, taming success, extraction effect)
- Music (paid asset pack)
- UI polish (proper menus, settings screen, controls remapping)
- Balance pass based on alpha tester feedback
- Build distribution via itch.io for closed alpha

**PLAYTEST CHECKPOINT**: 10 alpha testers play 30 min each. Net Promoter Score from them.

---

### Week 9-10 — Steam Page + Next Fest Demo

**Goal:** Steam discoverability prep.

- Capsule art (1-min trailer, screenshots, GIFs)
- Steam page copy + tags + categories
- Demo build for Steam Next Fest (if timing works)
- Press kit + early Reddit/YouTube reaches
- Wishlists target: 5,000 before launch

---

### Week 11-12 — Early Access Launch

**Goal:** Launch on Steam, $9.99 Early Access pricing.

- Final balance pass
- Steam achievements (~20 launch achievements)
- Steam Cloud save integration
- Day-1 patch buffer
- Marketing push (Reddit, YouTube creators, X/Twitter)

**Realistic launch revenue projections:**
- Floor: $5K (no marketing traction)
- Median solo Steam roguelike launch: $30-100K month 1
- Top decile: $1M+ month 1 (Vampire Survivors, Brotato territory — unlikely but possible)

---

## What You (Peter) Need to Do Today

See [DAY1.md](DAY1.md).
