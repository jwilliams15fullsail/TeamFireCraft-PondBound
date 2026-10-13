# PondBound

**Team FireCraft · Full Sail University · Project & Portfolio II**

PondBound is a 1–4 player cooperative physics adventure. Mutated lab frogs escape a research facility and work together to reach their home pond, using charged jumps, stretchy tongues and sticky climbing.

> **`main` = stable builds only.** Do all work on your own branch and merge it into `Development` first.

## Team
| Name | Role | Branch |
|---|---|---|
| Dev | Systems Test Branch | `Development` |
| JWilliams | Networking, integration, Core Gameplay Mechanics | `CoreGameplayMechanics` |
| <name> | <role> | `mackenzieBranch` |
| <name> | <role> | `moose-branch` |
| <name> | <role> | `vance` |

## Requirements
- **Unity 6000.6.0f1** exactly (install it through Unity Hub). Other versions will re-import or break the project.
- Universal Render Pipeline (URP) 17.7
- Netcode for GameObjects 2.13.3, Input System 1.20, Multiplayer Play Mode 3.0, Multiplayer Tools 2.2, Animation Rigging 6.6
- FishNet is in `Assets/FishNet` for evaluation only. The frog scripts use Netcode for GameObjects.(Subject to change)

## Getting started
1. Clone the repo and open the folder from **Unity Hub › Add › Add project from disk**.
2. Open `Assets/Scenes/SampleScene` (the frog test room).
3. **Window › Multiplayer › Multiplayer Play Mode** and tick **Player 2**. (Up to 4 if you like)
4. Save the scene (**Ctrl+S**). Player 2 loads it from disk, not from your open Editor.
5. Press Play. In the main window click **Host**, in Player 2 click **Join** (`127.0.0.1`).

To play over LAN, the host shares their IPv4 address and the other players enter it before clicking Join (port 7777; allow Unity through Windows Firewall).

## Controls (Current, subject to change)
| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / look | WASD / mouse | Left stick / right stick |
| Charged jump | Hold and release Space | Hold and release South |
| Tongue fire / let go | Left mouse | Right trigger |
| Reel tongue in | Hold right mouse | Hold left trigger |
| Stick to / release wall | E | West |
| Break free from a grab | Q | East |
| Free the mouse cursor | Tab | – |

## Project structure
| Folder | Contents |
|---|---|
| `Assets/Scripts/Frog` | Frog gameplay: movement, jump, tongue, climbing, camera, procedural animation |
| `Assets/Settings/Frog/FrogTuning.asset` | All frog feel values (speed, jump, grip, tongue). Edit here, not in code. |
| `Assets/Prefabs` | `Frog` (player), `Fly` (food) |
| `Assets/Models`, `Assets/Materials` | Frog models, materials and textures |
| `Assets/Scenes` | `SampleScene` (frog test room) |
| `Assets/FishNet` | FishNet networking (evaluation only, not used yet) |
| This is subject to change.

## How we work
- **Branches:** your branch → pull request into `Development` → two-player test → pull request into `main` only when stable.
- **Pull before you start**, and commit small changes often.
- **One person per scene or prefab at a time.** Unity scene merges usually break. Say in chat before you edit a shared scene.
- **Always commit `.meta` files** with their assets. Keep **Edit › Project Settings › Version Control › Mode** on **Visible Meta Files**.
- **Never commit** `Library/`, `Temp/`, `Logs/` or `UserSettings/`. `.gitignore` already blocks them.
- **Files over 50 MB** need Git LFS. GitHub rejects anything over 100 MB.

## Status
| Reference the Team's Trillo Link 
| https://trello.com/b/tZeAagF7/1-product-backlog


