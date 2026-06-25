# TUSK

> Single-player drop-in roguelike survival. Tame wild creatures, outlast the storm, extract before the zone closes.

## Vision

**Fortnite's drop-and-extract loop + Pokémon's creature taming + roguelike permadeath. Single-player. Steam-first.**

You drop onto a stylized 3D island. A storm zone shrinks around you over 20 minutes. You forage, craft, fight wildlife, **tame creatures to fight alongside you**, and reach the extraction portal before the zone closes. Daily seed = everyone gets the same island layout today → global leaderboard ranks survival + loot.

Roguelike: each run is permadeath. Meta-progression unlocks new starter loadouts, creature species, and run modifiers.

## Why this is different

| Reference game | What we share | What we DON'T copy |
|---|---|---|
| **Fortnite** | Shrinking zone, drop-in, scavenge, extract | No multiplayer, no building, no shooter mechanics |
| **Palworld** | Tame creatures, fight alongside | Single-biome, no farming, no breeding |
| **Vampire Survivors** | Roguelike, daily challenge, meta-unlocks | Third-person 3D, not auto-attacker |
| **Pokémon** | Creature collection + taming | No story, no turn-based, no IP |
| **Stardew Valley** | Cozy crafting | We're action-survival, not cozy |

## Platform

- **Primary**: Steam (PC + Mac via Mac App Store + Steam Deck)
- **Controls**: WASD + mouse keyboard, full controller support (Xbox/PS/Steam Deck)
- **Pricing**: $9.99-14.99 premium (Early Access), no IAP, no live service
- **Networking**: ZERO multiplayer. Steam leaderboards only.

## Infrastructure (solo+AI feasible)

- **No game servers** — fully single-player
- **No matchmaking** — solo runs
- **No anti-cheat** — no PvP
- **No backend** — Steam handles leaderboards, cloud saves, achievements
- **Optional ghost replays** — Firebase free tier covers thousands of users

This is *exactly* why we're targeting Steam: the operational complexity that kills solo mobile projects doesn't exist here.

## Stack

- **Engine**: Unity 6 LTS, URP
- **3D environment**: Synty POLYGON Nature pack (owned)
- **3D creatures**: Meshy AI (generates ~30 unique creatures over 1-2 weeks)
- **Player character**: Synty Heist character + Mixamo animations (humanoid retargeting)
- **Audio**: CC0 + paid asset packs (~$30-50)
- **Build target**: Windows + macOS Steam binaries

## Roadmap (12 weeks to Steam Early Access)

See [PLAN.md](PLAN.md) for week-by-week detail.

| Phase | Weeks | Goal |
|-------|-------|------|
| Alpha prototype | 1-2 | Drop, walk around island, basic combat |
| Core loop | 3-4 | Storm zone, taming, extraction |
| Content pass | 5-6 | 30 creatures, crafting, meta-progression |
| Polish & alpha test | 7-8 | Sound, UI, balance, 10 testers |
| Steam page + demo | 9-10 | Steam Next Fest demo |
| Early Access launch | 11-12 | $9.99 launch on Steam |

## Status

**2026-06-25** — Day 0. Repo scaffolded. Waiting on user to create Unity 6 project.

See [DAY1.md](DAY1.md) for the user's setup steps and the autonomous build plan once Unity is initialized.
