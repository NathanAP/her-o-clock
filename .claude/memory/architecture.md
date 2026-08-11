# Architecture

## Purpose

Holds the code structure decisions that cannot be worked out by reading the files, and that must be respected in future versions.

## The scene holds a single GameObject

`SampleScene` contains only `Main Camera`, `Global Light 2D` and `Battle`. The board, the characters and the combat director are all created at runtime by `BattleBootstrap`.

The reason is git: a `.unity` file is among the worst to resolve conflicts on, and a lean scene practically removes the problem. When adding something new, prefer creating it in code over dragging it into the hierarchy.

## There is a single update loop

No character has an `Update`. Everyone is updated by `BattleDirector`, always in the same order.

This is not fussiness: offline progression, which is the heart of the genre according to `references.md`, needs to simulate a whole fight without rendering anything. With one loop in a fixed order, that is just calling the same code with a different delta time. With `Update` scattered around, it would mean rewriting everything.

For the same reason, the target priority chain in `TargetSelector` compares integers only, with no floats involved.

## The random source belongs to the battle

`UnityEngine.Random` is not used anywhere in combat, and must not be. It is static and global, so any other system drawing a number would change the outcome of the fight by accident.

`BattleRandom` keeps its own sequence and depends only on the seed. It uses a 32 bit xorshift because that produces the same sequence on every platform, which `System.Random` does not guarantee across runtime versions.

The seed in use is written to the Console on start. Putting that number into the `Random Seed` field of `BattleBootstrap` replays the entire battle, blow by blow.

## The damage calculation knows nothing about Unity

`DamageCalculator` takes a `DamageInput` made only of numbers and returns a `DamageResult` made only of numbers. It never touches `Character`, `MonoBehaviour` or `Transform`.

That is what allows the whole calculation to be tested outside the editor against the examples written in `attributes.md`. If it ever becomes necessary to pass a character into it, that is a sign the separation is being broken.

For the same reason, it is `CharacterAttacker` that builds the `DamageInput` from the characters, not the calculator.

## Rounding

Damage, healing, life steal and thorns use the language's default rounding, where an exact half goes to the nearest even integer. The spec was written to describe that behaviour rather than fight it.

Besides being the native behaviour, it accumulates no bias: always rounding halves up would push totals upward over thousands of blows.

## The target chain serves two purposes

`TargetSelector.Select` takes a `respectRange` parameter:

- `true` decides who to attack, considering only those within range.
- `false` decides who to walk towards, ignoring range entirely.

It is the same chain in both cases, and that is what keeps a taunt working even when the taunter is too far away to be attacked.

## Numbers that are tied to each other

The camera resolution and the board size have to change together:

- `30` pixels per unit on the `Pixel Perfect Camera`, with `CellSize = 1`, makes a cell measure 30x30 pixels.
- Reference resolution `180x320`: the width divided by 30 gives exactly the 6 columns of the board.
- The orthographic size `5.33` comes from `320 / 30 / 2`.

Changing one without recalculating the others misaligns the pixel art silently.

## Sorting layers used by code

`BoardRenderer` uses `Background` and `BattleBootstrap` uses `Characters`, both by name. Renaming either one breaks rendering with no error in the Console.

## The sorting layer trap with 2D lighting

This already cost a debugging session in 0.1.0.0, so it is worth recording.

URP 2D renders sprites with `Sprite-Lit-Default`. A sorting layer that is not listed in `Target Sorting Layers` of the `Global Light 2D` renders **black**, with no error, no warning, nothing in the Console. The object exists in the Hierarchy, has the right colour in the Inspector, and vanishes on screen.

The global light is born pointing only at the sorting layers that existed when it was created. Since our layers were added after the scene, it was still pointing only at `Default` and the entire board disappeared.

Whenever a new sorting layer is created, check the global light. If something draws black and the Console is clean, suspect this first.
