# Day 1 — Bootstrap

## What YOU do (15 minutes, one-time)

This is the only manual setup. Once Unity is initialized, I take over autonomously.

### Step 1: Install Unity 6 LTS (if not already)

1. Open **Unity Hub** on your Mac
2. Go to **Installs** → **Install Editor**
3. Pick the **latest Unity 6 LTS** (something like 6.1.x or 6.2.x LTS — pick the most recent LTS marked stable)
4. **Modules to include**: Mac Build Support (already on), **Windows Build Support (Mono)**, **Linux Build Support (optional but nice for Steam Deck)**
5. Click Install — takes ~10 min

### Step 2: Create the Unity project

1. In Unity Hub, click **New Project**
2. Template: **Universal 3D** (URP)
3. Project name: **wildfall**
4. Location: **~/wildfall** (this is the path the GitHub repo was cloned to — Unity will populate it)
5. Click **Create Project** — takes 3-5 min to scaffold

### Step 3: Push the Unity scaffold

In Mac Terminal:

```bash
cd ~/wildfall && \
git pull origin main && \
git add . && \
git commit -m "Unity 6 LTS URP project scaffold" && \
git push origin main
```

If you get an LFS warning, run `git lfs install` first.

### Step 4: Import Synty Nature pack

You said you own Synty packs. Find your **POLYGON Nature** pack on Asset Store, import it into the wildfall project. (If you DON'T own Nature pack, we'll buy it — $30, essential for the biome.)

In Terminal again:

```bash
cd ~/wildfall && \
git add . && \
git commit -m "Import Synty Nature pack" && \
git push origin main
```

### Step 5: Tell me "Unity ready"

Once you've pushed the Unity scaffold + Synty pack, ping me. I take over from there.

---

## What I do (autonomously, starting from your Day 1 push)

Once you push the Unity scaffold, here's what I do over the next 24-48 hours without needing more input from you:

1. **Write the third-person character controller** (CharacterController-based, sprint/jump/crouch, Mixamo animation hooks)
2. **Write the procedural island generator** (terrain mesh from noise, Synty prop placement)
3. **Write the camera rig** (Cinemachine third-person, mouse-look orbit)
4. **Write the editor scene-builder** that constructs the playable test scene from menu (Daily Heist learning: no manual scene wiring)
5. **Write Day 6 playtest deliverable**: a `.exe` you can run, walk around the island, feel the controls
6. **Commit + push daily** with progress notes
7. **Push a release-tagged Windows build** on Day 6 so you can download + playtest

You don't need to touch Unity again until Day 6 when I tag the first playtest build.

---

## What I need from you on Day 6 (playtest checkpoint)

30 minutes:

1. Download the Day 6 release build (Windows or Mac exe)
2. Play for 15-30 minutes — walk around, jump, sprint, look around
3. Answer 3 questions:
   - Does the movement feel responsive? (yes/no)
   - Does the island feel like a real environment? (yes/no)
   - Would you want to keep playing if there was something to do? (yes/no)
4. Reply to me with answers + any visual screenshots of issues

If all 3 are YES → Week 2 starts (storm zone + combat).
If any is NO → I iterate on Week 1 or we kill.

---

## What I don't need from you in Week 1

- ❌ Don't need you to write any code
- ❌ Don't need you to manually wire anything in Unity Inspector
- ❌ Don't need you to import assets manually (after Synty Nature is in)
- ❌ Don't need you to think about animation pipeline yet — that's Week 2

**Your one job in Week 1: scaffold Unity on Day 1, playtest on Day 6.**

---

## Costs incurred so far

- $0 (using owned Synty packs + existing Meshy subscription)
