# Design decisions

## Purpose

Holds the reasoning behind system decisions that are already settled. The rules themselves live in `.claude/specs/`; what lives here is the why, so that nobody later "fixes" something that was done on purpose.

## Every capped attribute uses diminishing returns

`Cap x Points / (Points + Constant)`.

The rule for when to use the curve: an attribute representing a chance or a mitigation percentage is capped by nature and uses the curve. An attribute representing damage, health or speed is uncapped and stays linear, because a rate multiplier cannot saturate.

Since the cap is never reached, **there is no manual clamp anywhere**. If code of the form "if it goes above X, treat it as X" shows up, that is a bug.

## Reductions multiply, they never add

Evasion and mitigation are applied on top of each other. A character with 75% mitigation hit by a normal 40% evasion takes `100% x 60% x 25% = 15%` of the damage.

Adding them would be the wrong choice because it would allow reaching 100% and becoming immune, contradicting the diminishing returns rule. It is the same principle, applied to stacking rather than to a single attribute.

## There is no aggro attribute

Target selection is entirely positional. A taunt is a temporary effect coming from a skill, never a number that goes up and down.

The reason is the game's pitch: it is tiny and lives in a corner of the screen, so the player has to be able to predict the fight at a glance. A hidden number shifting during combat destroys that readability. Numeric aggro would also make a battle much harder to replay from a seed, which is how odd behaviour gets investigated.

## Armour scales with the attacker's level, and so does armour itself

The mitigation constant is `50 x attacker level`. Without it, a fixed amount of armour would grant the same percentage forever, and tanking would be a solved problem far too early in a game built on endless farming.

The half that was missing until 0.5.3.0 is that **defence has to grow too**. With the constant rising and the armour standing still, there was no difficulty curve at all — just decay. The villain fell from 56% mitigation at level 1 to 4% at level 50 with nobody touching it, which is the opposite of what the rising constant was for.

So a sheet declares a gain per level alongside the starting value. The calibration falls out of the algebra rather than being guessed: when the gain equals the base, armour becomes `base x level`, the level cancels against the constant, and mitigation holds still forever at `75b / (b + 50)`. Every current sheet uses that shape, which preserved the level 1 balance exactly.

A gain below the base means a character that slowly loses ground, and above it one that gains. Both are deliberate options, not mistakes.

For heroes this is a **stand-in for equipment**. When items arrive, the armour on a hero sheet should drop to zero and the gear should take over, otherwise a defensive item competes with a base that already solved the problem.

## Regeneration is multiplied by POW, never granted by it

The base is zero for everyone. POW's "0.5% of regeneration speed per point" multiplies whatever other sources provide, so a character with no source still regenerates nothing.

The alternative, a flat rate everyone gets, was rejected because it heals the party for free between waves early on and becomes irrelevant later, and attrition between waves is what `gameplay.md` leans on to make farming necessary.

Regeneration does run during the transition between waves. Nobody is topped up; each character recovers only what its own rate earns in those seconds. That is precisely where a regeneration item is supposed to pay for itself.

## Elemental resistance above 100% is a build goal

Common sources never go past the 75% of the curve. Only special sources are added after the curve and can push beyond it. This is the Divinity model: absorbing elemental damage is an achievement, not an accidental pile-up.

## The board is 6 columns by 8 rows

Chosen for three concrete reasons:

- At 30 pixels per unit it gives square 30x30 cells, and the board takes 75% of the screen height, leaving room for the interface.
- An even width allows centring a villain that occupies 2x2. An odd width would leave it lopsided.
- Crossing the board takes 3.5 seconds at base speed, against 5.5 for the 6x12 that was considered. In a game whose loop is engaging over and over, that is dead time.

Both areas are the same size on purpose. If it becomes clear that heroes need more or less room, that is the first thing to adjust.

## Damage carries across a stage

Heroes do not heal between waves, and a fallen hero stays down until the stage ends. Only a full defeat restarts the stage at full health.

This is what gives health regeneration and life steal a reason to exist as attributes: without attrition every fight would start with everyone intact and both would be decorative. It is also what creates the need to farm, since eventually a stage stops being winnable with the current team.

## Player facing content is written in English, and lives outside the assets

Decided during the 0.5.0.0 review, because the project had drifted into three answers at once: the sheets under `.claude/specs/` were in English, the `Display Name` of every asset was in Portuguese, and the stage files were in Portuguese without accents.

The rule is the one `CLAUDE.md` already stated and nobody was following: **everything is in English**, from variables to the names the player reads.

On top of that, the visible text does not belong in the asset. Names, stage titles and stage lore move to a strings file, keyed by the id the asset already has. Two reasons:

- A `Display Name` inside a `.asset` is the same file as the balance numbers, so a translation pass and a balance pass fight over the same diff.
- Localisation later becomes a new strings file rather than a second copy of every sheet.

Done in 0.5.5.0. `Assets/Strings/en.json` holds every line the player reads, keyed off the id the content already carries: `character.{id}.name`, `stage.{id}.name`, `stage.{id}.lore`. Deriving the key rather than adding a field is what makes the pair impossible to desynchronise — renaming an id is the same act as renaming its text.

Two things fell out of it that are worth keeping:

- **A missing key comes back as `#the.key#`, never as an empty string.** Empty text is simply not drawn, and a label that vanishes without a trace is far harder to notice than a visibly broken one.
- **The balance snapshot lost its translatable text**, because the tables now label rows by id. A translation pass can no longer dirty the diff of the numbers.

Nothing displays the table yet beyond the stage announcement in the Console, because there is no interface. Doing it at 4 sheets and 2 stages rather than at dozens was the whole point.

## Free movement, with no body blocking

Characters cross the whole board chasing their target. The areas only define where the battle starts.

Tanking still works because the second rule of the chain is "closest", and whoever stands in front is the closest. The assassin's blink keeps its identity because in real time its value is not *being able to reach*, it is *reaching now*, saving the seconds everyone else spends walking.

Giving melee characters body blocking would remove the value of free movement. Both are valid paths and we cannot have both.
