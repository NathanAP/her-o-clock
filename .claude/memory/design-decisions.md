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

The reason is the game's pitch: it is tiny and lives in a corner of the screen, so the player has to be able to predict the fight at a glance. A hidden number shifting during combat destroys that readability. Numeric aggro would also be much harder to reproduce in the offline simulation.

## Armour scales with the attacker's level

The mitigation constant is `50 x attacker level`. Without it, a fixed amount of armour would grant the same percentage forever, and tanking would be a solved problem far too early in a game built on endless farming.

## Elemental resistance above 100% is a build goal

Common sources never go past the 75% of the curve. Only special sources are added after the curve and can push beyond it. This is the Divinity model: absorbing elemental damage is an achievement, not an accidental pile-up.

## The board is 6 columns by 8 rows

Chosen for three concrete reasons:

- At 30 pixels per unit it gives square 30x30 cells, and the board takes 75% of the screen height, leaving room for the interface.
- An even width allows centring a villain that occupies 2x2. An odd width would leave it lopsided.
- Crossing the board takes 3.5 seconds at base speed, against 5.5 for the 6x12 that was considered. In a game whose loop is engaging over and over, that is dead time.

Both areas are the same size on purpose. If it becomes clear that heroes need more or less room, that is the first thing to adjust.

## Free movement, with no body blocking

Characters cross the whole board chasing their target. The areas only define where the battle starts.

Tanking still works because the second rule of the chain is "closest", and whoever stands in front is the closest. The assassin's blink keeps its identity because in real time its value is not *being able to reach*, it is *reaching now*, saving the seconds everyone else spends walking.

Giving melee characters body blocking would remove the value of free movement. Both are valid paths and we cannot have both.
