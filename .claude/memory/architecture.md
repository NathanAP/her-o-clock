# Architecture

## Purpose

Holds the code structure decisions that cannot be worked out by reading the files, and that must be respected in future versions.

## The scene holds a single GameObject

`SampleScene` contains only `Main Camera`, `Global Light 2D` and `Battle`. The board, the characters and the combat director are all created at runtime by `BattleBootstrap`.

The reason is git: a `.unity` file is among the worst to resolve conflicts on, and a lean scene practically removes the problem. When adding something new, prefer creating it in code over dragging it into the hierarchy.

## There is a single update loop, and it runs on a fixed step

No character has an `Update`. Everyone is updated by `BattleDirector.Tick`, always in the same order, and always by exactly `BattleDirector.FixedStep` — one sixtieth of a second, never the frame's own delta.

What this buys:

- **A battle can be replayed.** Same seed and same starting state give the same fight, blow by blow, on any machine and at any frame rate.
- **A battle can run headless.** A test drives a whole stage by calling `StageRunner.Advance` in a loop, with no scene and no rendering.
- **A battle can run at any speed.** `Time.timeScale` just makes the accumulator ask for more steps.

For the same reason, the target priority chain in `TargetSelector` compares integers only, with no floats involved.

### The remainder is carried, never discarded

This is the half that is easy to miss. A blow can only land on a step boundary, so if the overshoot is thrown away, every interval is rounded **up** to whole steps: a character at 4 attacks per second really makes 3.75 of them. It was measured before and after in 0.5.1.0, and the loss reached 6.25%, growing with speed — so it punished exactly the builds that pay for speed. Even values that divide evenly were affected, because one sixtieth is not exact in binary.

So `CharacterAttacker` does `cooldown += AttackInterval` rather than `=`, and `CharacterMover` keeps how far past the end of a step it went and applies it as the next step's starting progress.

Two rules survive on top of that, and both must be preserved:

- **Nobody banks blows.** The cooldown is held at zero while the character is walking or has no target. Without it, stepping in and out of range would charge up several blows at once.
- **A movement remainder only belongs to the step immediately after it.** It is dropped when the character stops to attack, runs out of targets, or gets cornered. Keeping it would hand out a free head start whenever it started walking again, possibly minutes later.

### `StageRunner` owns the only clock

It is the single place in the game that reads `Time.deltaTime`. Real time goes into its accumulator, and comes out as fixed steps that go either to the battle or to the transition between waves.

Two accumulators — one for the fight, one for the transition — would lose the leftover on every phase change. With one, a battle ending mid step hands the next step straight to the transition.

The accumulator is capped at `MaxStepsPerFrame`. Past it, time is dropped on purpose: a long hitch would otherwise ask for thousands of steps and freeze the game trying to catch up, making the next frame worse still.

### Abilities sit at the front of a character's turn

Inside one step, a character resolves in a fixed order, and that order **is** the spec's priority rules rather than an implementation of them:

1. The `AbilityCaster` advances, so "busy with an ability" is already true for this step.
2. While busy, the character neither moves nor attacks. Preparation, casting and recovery are one occupied block.
3. The attacker runs **before** a new ability may start. That is the whole of "the basic attack takes priority": no rule says so anywhere, the ordering does.

`CharacterAttacker` therefore takes `blocked` rather than `isMoving`. Its timer keeps running while the character is busy and is held at zero rather than banking, which is what makes a basic attack become available during a wind up and land in the first gap afterwards — one blow, not the four that a banking timer would release at once.

The cooldown of an ability starts when its **casting** ends, not when the recovery does. That is the one asymmetry in the model and it is deliberate: the recovery delays the next ability without delaying this one's recharge. Two sums, `BusySeconds` and `SecondsUntilCooldownStarts`, exist for exactly that reason, and they are the first thing to check when an ability's rhythm feels wrong.

### The resolution order alternates

Heroes used to be first in the list, so they won every exact tie: their blow landed and killed before the enemy whose own blow was due in the same step ever swung. Which side resolves first now flips every step, driven by the battle's own step counter so it stays reproducible.

`Begin` builds the list in two passes, heroes then enemies, instead of trusting the caller's ordering. That is what keeps each side in a contiguous half, so swapping who goes first is swapping two loops.

**This is not needed for offline progression.** Offline progression is arithmetic — the last hour's rate multiplied by the time away, then clamped. No fight is ever replayed. Earlier versions of this file claimed the loop existed to enable an offline simulator; that was wrong, and the reasons above are the real ones.

The reason a simulator is never needed is that `progress.md` forbids the three things that would demand one: no item drops, no stage advancement, and at most one level gained while away. Allowing any of them offline would mean writing a combat simulator.

## Nothing reads a base attribute directly

`CharacterStats` exposes `TotalOf(attribute)`, and everything derived from an attribute goes through it. The serialized fields are the sheet's **base** values and are not what the game plays with.

Today the total is the base plus the points earned by levelling, scaled by the stage multiplier. Items, skill trees and buffs will become further sources **inside that method**, and no other file will have to change. That is the entire point of the seam.

The defensive values go through the same seam as of 0.5.3.0. `BasePhysicalArmor` and the three resistances are sheet values with a gain per level beside them, and everything else reads the computed `PhysicalArmor`, `FireResistance` and so on. `CharacterAttacker` and the fifth rule of `TargetSelector` used to read the fields raw; if that pattern reappears, the seam has been broken again.

The level contribution is computed from the level, never accumulated level by level. That keeps it free of rounding drift and lets a level 40 minion be created without walking through 39 level ups.

`AttributeAllocation` had to preserve that while also letting a player place points by hand, and the trick is what it stores: the automatic share is kept as a **count of points**, not as a distributed result, and redistributed whole every time it is read. Handing out five points thirty-nine times does not give the same split as handing out 195 once, because the percentages are resolved by largest remainder. Store the result and a minion created at level 40 stops matching one that climbed there, which quietly breaks replaying a battle from a seed.

If you find yourself reading `BasePower` outside this class, something is being calculated in the wrong place.

**As of 0.7.0.0 the seam reaches the derived values too**, and that was not the original plan. `TotalOf` only ever covered the four primaries, but the first real content buffs attack speed, movement speed and cooldown reduction, none of which is an attribute. So `AttacksPerSecond`, `CellsPerSecond`, `CooldownReduction` and `PhysicalArmor` each run their computed value through `StatModifiers` at the end. Anything that gains a buff from now on joins them there rather than growing a second calculation beside them.

Rebuilding the stats also clamps current health down to the new maximum. Points only arrive while levelling, so the maximum only ever rises and the clamp does nothing — until a player takes their points back to rebuild, which drops the maximum with the current health still above it. `attributes.md` has always said current health can never exceed maximum; nothing enforced it until 0.6.0.0, and it was the save round trip that noticed, because restoring a hero clamped it and the hero it was restored from never had been.

## Sheet and instance are separate

`CharacterDefinition` is a shared asset: the immutable mould, holding story, design, colour and later skills. `Character` holds its own copy of the stats, created from the sheet on initialisation, plus its own level.

Never read stats straight from the definition again. Doing so makes every character of the same type share one set of numbers, and in the editor it edits the asset on disk, so the change survives leaving Play.

The player's six heroes are fixed designs; what varies is the instance the player builds. Levels, attributes and equipment belong to the instance.

## ScriptableObjects must hold no runtime state

This project runs with Domain Reload disabled, which makes entering Play Mode almost instant. The price is that nothing is cleared between sessions: static fields keep their values, and so do the fields of any ScriptableObject, because the asset stays loaded.

A lazily built cache inside a ScriptableObject is therefore permanent. `CharacterDatabase` had one, built the first time it was asked for an id. It was built during a session where the sheets still had no ids, came out empty, and stayed empty across every later Play. Every stage then failed to resolve every character, and no amount of restarting Play fixed it.

Treat a ScriptableObject as read-only data. Anything derived from it is built at startup and passed around, never stored back on the asset. If some cache really has to live there, it needs an explicit rebuild call, not a null check.

## Content lives in JSON, assets live in ScriptableObjects

Stages are `.json` files under `Assets/Stages/`. Character sheets stay ScriptableObjects.

The split follows what each one needs. Sheets have to reference sprites, animations and skills, which only exist as Unity assets. Stages are bulk content: dozens of files that need to be authored quickly, diffed in git and mass-edited for balance.

Because JSON cannot hold an asset reference, characters are named by a text `Id` and resolved through `CharacterDatabase`. Save games will need the same mechanism, since a save cannot store an object reference either.

The cost of text references is that a typo would only surface at runtime. `StageValidator` is what pays that cost: it has no Unity dependency, so it can be tested outside the editor, and it reports every problem in a file at once instead of stopping at the first.

An earlier version of this file said the validator **was** covered by tests. It was not: no test file has ever existed in this repository. The coverage arrives in 0.5.2.0.

## The save signs text, never an object

`SaveEnvelope` splits a file into a signature and the payload text it covers, and the payload is taken out **verbatim** — the stretch between `"payload":` and the envelope's closing brace. Nothing is parsed and rebuilt on the way.

That is not a style choice, it is the only shape that survives a version bump. A save written by a later build holds fields this one has never heard of; reserialising to verify would drop them, and the signature would never match again. Signing the text means an unknown field travels through untouched and is covered like everything else.

The same reasoning runs through `SaveMigration`: a missing section becomes what a game that never had it would hold, never an invented value. Between the two, adding a field to the format costs no conversion step at all.

A file from a **newer** format is refused outright, which is a stronger reaction than a failed signature. A bad signature says the numbers may have been edited; a version from the future says we cannot tell which number is which.

### Only `SaveStore` touches a disk

The format, the signature, the naming and the retention rule are pure code with no `UnityEngine` in them, and each is tested by being handed strings. `SaveRetention` in particular is a pure function from file names to file names, so the rule that decides whether a player can go back to yesterday is checked without a single file existing.

`SaveStore` takes its folder as a parameter rather than looking it up, which is what lets a test point it at a throwaway directory and exercise the real reading and writing.

## The tests drive the real stage runner

`StageSimulation` used to repeat the `StageRunner` loop instead of driving it, because the runner destroys its enemies between waves and Unity's deferred `Destroy` never runs outside Play Mode. The balance numbers were therefore measured against the copy, so a change to the real loop that nobody mirrored would have been reported as "nothing moved".

The fix was one field: `StageContext.Destroy`. The game leaves it alone and gets Unity's deferred `Destroy`; a test passes `DestroyImmediate`. The copy is gone.

`StageContext` itself replaced a `Configure` with nine positional parameters, which had reached the point where two of them could be swapped and still compile.

**The snapshot did not move by a single digit when this landed**, which is the evidence the refactor preserved behaviour — including the parts that are now genuinely exercised for the first time, since the simulation runs the real transitions between waves.

## Assemblies

The game code lives in `Assets/Scripts/HerOClock.asmdef`, and the tests in
`Assets/Tests/EditMode` and `Assets/Tests/PlayMode`, each with its own.

The game needs one because of a rule that is easy to trip over: a `.asmdef` assembly **cannot
reference `Assembly-CSharp`**, only the other way round. The Test Framework requires every test
assembly to be a `.asmdef`, so without one on the game itself no test can see any of it.

Almost everything lives in Edit Mode, because it needs no scene and runs in milliseconds. Play
Mode holds only the smoke test, which is the one thing that genuinely needs the engine running.

## The random source belongs to the battle

`UnityEngine.Random` is not used anywhere in combat, and must not be. It is static and global, so any other system drawing a number would change the outcome of the fight by accident.

`BattleRandom` keeps its own sequence and depends only on the seed. It uses a 32 bit xorshift because that produces the same sequence on every platform, which `System.Random` does not guarantee across runtime versions.

The seed in use is written to the Console on start. Putting that number into the `Random Seed` field of `BattleBootstrap` replays the entire battle, blow by blow.

**The seed is scrambled before it becomes the state, and that is not optional.** Xorshift diffuses
slowly, so feeding the raw seed in made the first draw almost exactly `0.0161 x seed`: the first
sixteen seeds produced sixteen rising values, every one of them below 0.30. Since 30% of evasion
becomes perfect evasion, every hand typed seed rolled a perfect evasion on its first try — and a
hand typed seed is precisely what a developer uses to investigate something. The one case that had
to be trustworthy was the broken one.

Found by `BattleRandomTests`, which is the clearest argument for the suite that exists: no amount
of reading the code would have shown it.

## The damage calculation knows nothing about Unity

`DamageCalculator` takes a `DamageInput` made only of numbers and returns a `DamageResult` made only of numbers. It never touches `Character`, `MonoBehaviour` or `Transform`.

That is what allows the whole calculation to be tested outside the editor against the examples written in `attributes.md`. If it ever becomes necessary to pass a character into it, that is a sign the separation is being broken.

For the same reason, it is `CharacterAttacker` that builds the `DamageInput` from the characters, not the calculator.

**The arithmetic is done in double, and must stay that way.** The inputs are floats because that is what the sheets hold, but C# lets a runtime evaluate float operations at a higher precision and decide for itself whether to round intermediates back. That is enough to move a result across a rounding boundary: the same attack of exactly 262.5 came out 262 outside Unity and 263 inside it, because `40f / 100f` landed either side of 0.4. `DiminishingReturns` therefore computes in double as well, and the percentage properties only narrow at the very end.

It matters more than the size of the error suggests. `BattleRandom` goes out of its way to produce the same sequence on every platform; a damage calculation that does not undoes the whole guarantee, and the editor and a build could disagree.

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

Four layers are referenced by name, and renaming any of them breaks rendering with no error in the Console:

- `Background` — the board cells, in `BoardRenderer`.
- `Ground` — the area divider, in `BoardRenderer`.
- `Characters` — the body, the health bar and the level label, in `CharacterView`.
- `VFX` — the floating damage numbers, in `DamageNumber`.

The list matters because of the trap described in the next section: a layer that the global light does not target renders black, silently.

## The sorting layer trap with 2D lighting

This already cost a debugging session in 0.1.0.0, so it is worth recording.

URP 2D renders sprites with `Sprite-Lit-Default`. A sorting layer that is not listed in `Target Sorting Layers` of the `Global Light 2D` renders **black**, with no error, no warning, nothing in the Console. The object exists in the Hierarchy, has the right colour in the Inspector, and vanishes on screen.

The global light is born pointing only at the sorting layers that existed when it was created. Since our layers were added after the scene, it was still pointing only at `Default` and the entire board disappeared.

Whenever a new sorting layer is created, check the global light. If something draws black and the Console is clean, suspect this first.
