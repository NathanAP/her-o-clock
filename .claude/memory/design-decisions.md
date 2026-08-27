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

## Regeneration is multiplied by CON, never granted by it

The base is zero for everyone. CON's "0.5% of regeneration speed per point" multiplies whatever other sources provide, so a character with no source still regenerates nothing.

It belonged to POW until 0.10.0.0, and moved when POW became offence only. Survival is CON's job now, and regeneration is survival.

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

## A save cannot be protected, so it is not pretended otherwise

The game runs on the player's machine and the process has to read the numbers to play. Any key the binary uses, the binary carries. The only design that would genuinely stop tampering is an authoritative server, and that was rejected: it demands internet and contradicts a light game sitting open in a corner of the screen all day.

What was chosen instead is a distance, not a defence. An HMAC over the payload text kills the person who opens the file in a text editor and changes `"money": 1500`, which costs ten seconds and no knowledge and is practically everyone who does this. It gives the person with a save editor some work. Against the person who opens the binary and recomputes the hash, nothing local works, and `save.md` says so out loud rather than hiding behind the word "encrypted".

It matters less than it sounds: there is no advertising, no external shop, no pay-to-win, no leaderboard and no play between players. Somebody who edits their own save spoils their own toy and takes nothing from anyone. The real risk to a real player is a corrupted file and a version migration, which is where the effort went.

### A save that fails its signature is loaded anyway, and marked

Refusing would punish the player whose disk dropped a byte, which is the case that actually happens; deleting and starting over would be worse, because it is irreversible. So it loads, the file is left untouched as evidence, and the save carries `integrity: broken` from then on — into every save written afterwards, since it is a fact about that save's history rather than a state it recovers from.

The mark limits nothing inside the game. It exists for a bug report, and as a door for anything that one day points outward, like an achievement or a leaderboard.

## Every save is a new file, and the old ones are the backup

Rather than rewriting one file and keeping a `.bak` beside it, each save is a new file named for the instant it was written. Two things fall out of it:

- **No good save is ever overwritten**, so a write interrupted halfway cannot destroy anything that already existed.
- The previous generations are the backup by construction, with no second mechanism to maintain.

What is left is a retention rule, and it has two rungs because there are two different accidents. The **5 newest** cover a disk that dropped a write moments ago. The **newest of each of the 3 most recent days with saves** cover the one that really costs a player their progress: a bug writing a save that is valid but wrong. A save lands every few seconds, so keeping only the newest would cover under a minute, and somebody who noticed the next day would have nothing to go back to.

Days that have saves, not days of the calendar, so a player who opens the game once a month still keeps three sessions.

## A save holds which stage, never where inside it

Loading starts the recorded stage from its first wave, with everybody at full health. No wave index, no health, no battle frame.

The first design saved the wave and the health and resumed exactly there, to protect the attrition rule of `gameplay.md`. That was more machinery than the rule needed. **Attrition was only ever a rule about the inside of a stage**: damage carries from wave to wave, and a stage that starts over starts whole. A defeat already restarts a stage at full health, and it is free, so a player who closes the game gains nothing they could not have by dying.

What it buys is that there is no halfway state to write down, to migrate, or to get subtly wrong — and `StageRunner` went back to having exactly one way in, used by the first attempt, the restart after a defeat, and the game being reopened alike.

### There is no periodic save either

The save points are the wave boundary and the beginning of a stage, and that is all.

A wave boundary is a save point because of what the wave **paid** — experience, money, the buckets of the last hour — and not because of where anyone is standing. Since no position is recorded, a write in the middle of a wave would hold nothing the boundary write does not already have.

The end of a stage is **not** a save point, and the beginning is. A victory and a defeat both restart the stage, so saving on the defeat itself would record the moment of losing rather than what the player carries forward.

The cost is stated rather than hidden: redistributing attribute points and having the process killed within the next few seconds loses the redistribution. The wave boundary comes round every few seconds, so the window is small.

## The offline experience ceiling keeps the fraction

"At most one level per absence" was ambiguous until the spec's own example pinned it down: somebody who left 95% of the way to level 10 comes back at level 10, 95% of the way to 11.

That reading — one level with the fraction of progress preserved — is the only one that produces 95%, and it is also the only one worth exactly one level anywhere in the game. Granting "the cost of a level" instead would pay less than a level as the player advances, because the next level always costs more than the current one.

The money ceiling turned out to be simpler than it looked. Written as twelve hours of the **online** rate, it is really a ceiling on how many hours of open game an absence is worth. So the whole absence collapses to one number, `EffectiveHours`, and every statistic is that number times the same buckets. `progress.md` forbids the statistics on the welcome-back screen from being reached by a second path, and this makes a contradiction impossible to write rather than merely discouraged.

The ceilings are also the only defence against the player winding the system clock forward, and that is now written down. A month away pays the same as twelve hours, so winding the clock leads nowhere. Anyone who later tunes those ceilings is tuning two things at once.

## An area skill is centred on whoever used it

There is no search for the best spot on the board. An earlier version of `gameplay.md` described a scoring pass that placed an area wherever it would catch the most of the right people; it was removed in 0.6.0.3 along with the `bestPlacement` anchor that selected it.

The reason is the same one that rules out an aggro attribute: the game is tiny and lives in a corner of the screen, so the player has to be able to predict the fight at a glance. An area landing wherever an invisible sum decided destroys that. It also removes a scan of the board running per skill per step, and it makes where a hero stands part of the decision rather than something the game quietly optimises away.

## The same buff arriving twice refreshes, it never stacks

The strongest value wins and the duration restarts.

Stacking was rejected because it would need a ceiling **per buff**: with six heroes carrying the same slow, an enemy would otherwise sit at zero. That is one more constant to calibrate in every buff in the game, and not one of them would be arguable on its own. The price is that accumulation builds do not exist, which is cheap next to a system where the player's arithmetic depends on how many allies happen to carry the same piece.

Different effects on the same attribute still add up. The rule is about one source arriving again, not about the attribute.

### Flat first, and percentages add before they multiply

Two orderings the spec did not need to settle and the code did, both for the same reason: the same pair of buffs must not give two different answers depending on which arrived first.

- **Flat is applied before percent**, so a percentage always reads as a share of the whole rather than of whatever happened to land before it.
- **Percentages from different sources are added, then applied once.** Two 20% buffs give 40%, not 44%. Multiplying them would make the result depend on arrival order, which nobody looking at the screen could predict.

### An ability never fails whole because one effect did not fit

Effects resolve in the order they are written and are independent of each other. A reposition with nowhere to go leaves the character where it is and the damage beside it still happens.

The order is load-bearing rather than cosmetic, and the spec's own example is the proof: an ability that makes its user intangible has to apply that **before** dealing the damage that would otherwise come back at it.

## A skill never fails whole because one effect did not fit

`move_to` with `lastTargetAnySide` looks at the four cells around the last target and takes the first free one on the board, ordered by lowest row then lowest column. If none of them serves, the character simply stays where it is and the rest of the skill resolves normally.

Two things are load-bearing there. The order has to be **fixed** rather than "nearest" or "most convenient", or the same battle from the same seed can end differently. And effects are independent: they resolve in the order they are written, and one that cannot happen does not cancel the others.

## Free movement, with no body blocking

Characters cross the whole board chasing their target. The areas only define where the battle starts.

Tanking still works because the second rule of the chain is "closest", and whoever stands in front is the closest. The assassin's blink keeps its identity because in real time its value is not *being able to reach*, it is *reaching now*, saving the seconds everyone else spends walking.

Giving melee characters body blocking would remove the value of free movement. Both are valid paths and we cannot have both.

## Damage falloff multiplies, so it never reaches zero

`Damage = base x (1 - falloff) ^ (distance - 1)`, on the `deal_damage` effect, with distance counted in cells from the user.

It is the same reasoning as reductions multiplying instead of adding. A subtractive falloff would zero the damage past some distance, and then every ability using it would need a floor written by hand — one more constant to calibrate per ability, and none of them discussable on its own. Multiplying means being in the line always counts for something and being close always counts for more, with no extra number.

The `- 1` in the exponent is what makes the adjacent target take the full number written on the sheet. Without it nobody would ever receive the sheet's number, and the number would stop meaning anything.

It is refused on `self` and `single`, where every target sits at the same distance. There it would be read without error and change nothing, which is the same silent failure the priority rule guards against.

## An ability never kills the character that used it

Self targeted damage is how the cost of a powerful ability is written, and it stops at 1 health.

The alternative considered was refusing to cast when the arithmetic would kill. It was rejected for two reasons. Abilities have preparation time, so the check would look at a number that can be outdated by the time the ability actually fires — the promise would not hold. And it would disarm the character exactly at low health, which is when the effect is most needed; an ability that disappears when the battle gets hard is an ability that does not exist.

The limit covers **only the ability's own damage against its own user**. Thorns, an ally's area and anybody's basic attack still kill normally. Widening it would turn a cost into a defence and make a character with a costly ability immortal by accident.

## A hero's abilities live in trees, an enemy's live in a flat list

Hero sheets use `abilityTrees`, each with `id`, `name` and its own `abilities`. Minion and villain sheets use a flat `abilities`.

This is not an inconsistency to be tidied up later. A tree is progression, and nobody progresses a minion: the player spends points, picks a path and unlocks ranks on a hero, while a minion is born with what its sheet says. Wrapping an enemy's abilities in a tree would create a node nobody ever buys, and somebody would eventually try to give it meaning.

A hero can have more than one tree, and that is what the class and subclass system rests on.

## The swing is a separate event from the blow

`CharacterAttacker.Struck`, re-raised by the director as `BasicAttackLanded`, exists alongside `Attacked` and must not be merged back into it.

`Attacked` cannot answer "was this a basic attack". It fires for the blow, fires **again** for the thorns coming back at the attacker, and the director raises it once more for every blow an ability lands. Anything that draws the swing itself needs that question answered: hung off `Attacked`, a projectile flies backwards out of whoever was hit, and a ranged character shoots arrows while casting a fire ability.

It is announced **before** the damage is resolved, so whatever draws the swing sees the board as it was when the blow was thrown, with the target still standing. A killing shot would otherwise be drawn out of a fight the target had already left.

Abilities will need their own signal when they get visuals. This is the shape to copy, not a thing to generalise into one event with a flag.

## The view layer never touches the simulation

Projectiles are drawn after the damage has already been applied, and they fly to a position captured at launch rather than following the target.

Both halves are load-bearing. Damage staying instantaneous is what keeps the whole question of flight time — determinism, when evasion is rolled, targets dying mid-flight — out of the engine entirely; the reasoning for deferring that is in the roadmap under "No radar". Holding a position instead of a reference is what survives the target dying, since a dead minion is deactivated while a bolt aimed at it is still travelling.

The practical rule: anything under `Assets/Scripts/View/` may read the battle, and may never change it. A view that can change the fight makes the fight depend on the frame rate, which 0.5.1.0 spent a whole version removing.

## An id names a character, never a copy of one

Ids are lowercase and hyphenated, and they never carry an instance number. Several identical enemies in one wave are the **same id placed at different cells**, which is what the stage format already does.

Numbering them (`discarded-prototype-1`, `-2`) would create one sheet per copy holding the same block of numbers, which is the duplication `progress.md` exists to prevent, plus one strings key per copy all spelling the same name. The number would also never mean anything: a wave needing a seventh enemy would force somebody to invent a seventh character to place the same enemy once more.

Two enemies that genuinely differ are two characters, and each gets a descriptive id such as `discarded-prototype-armored`. The same holds for heroes: a copy of one would be a character with an identity of its own, not `gadrat-2`.

## An NPC fights on the hero side but never counts as one

`CharacterKind.Npc` is the fourth type. It gains no experience and, crucially, **it does not count towards defeat**.

That second half is the one that touches code. `BattleDirector` ends a battle when one side has nobody standing, and an NPC sits in the hero list. Without the exception a lost stage would keep running until the story character happened to die, and an NPC could win a fight the player had already lost.

An NPC is placed by the stage, in `allies`, and stays across every wave like a hero rather than being spawned per wave. It belongs to nobody: the player never picks, equips or positions one.

## Only heroes carry experience

`Character.Progress` is null on minions, villains and NPCs, and `Level` is a field of its own.

Before 0.9.0.0 every character was given a `LevelProgress` at spawn and only heroes were ever awarded any, so each enemy carried a bar nothing read — allocation per spawn in a game meant to sit open all day.

The distinction that matters, and that is easy to get backwards: **an enemy is the source of experience and never its destination.** Killing minions and villains still pays the heroes; that is the whole progression.

## A wounded arrival is not a smaller character

`startingHealthPercent` on a stage placement lowers the health a character walks in with, and never its maximum. The character is still exactly who its sheet says; it has just been hit already.

Reducing maximum health instead would change mitigation, the experience granted and everything else derived from the sheet, which is a different statement from "somebody was fighting them before you arrived".

## Slots, owned heroes and the team are three different things

`Roster` keeps them apart because they look alike and are not:

- **Slots** are how many positions the team has. One at the start, four at most, opened by clearing stages.
- **Owned** is who the player has met, also grown by clearing stages.
- **Team** is the ordered few who walk into a stage; whoever is owned and not fielded is benched.

A hero arriving and a position opening are separate events that usually land together, and collapsing them would make it impossible to have a hero waiting for room, or room waiting for a hero.

The stage's `heroLimit` is a fourth number and belongs to the stage, not here: it is a rule of that stage and still applies when a later run replays it with a full team. The party is the front of the team, so the order the player chose is what decides who goes.

While there is no menu, an unlocked hero takes a free position by itself and a new slot pulls whoever has been benched longest. Otherwise an unlock would look like it had failed.

## The early difficulty is a cliff, not a slope

Measured in 0.9.1.0: with one hero and no healing between waves, the level a stage demands jumps between "1" and "many" over tiny changes to the enemies. A 0.8 multiplier on stage 4 asked for level 7; 0.6 asked for level 1.

The cause is structural rather than a tuning mistake. Damage carries across a whole stage, and a solo hero has no redundancy: either the party survives the attrition of every wave, in which case level 1 is enough, or it does not, in which case it needs many levels. There is almost no middle.

Anything that widens that middle is a design lever and not a number: regeneration between waves, fewer waves early, or a partial heal on clearing one. Until one of them exists, the early stages can only be tuned to "flows" or "wall", and nothing in between.

## Rank steps are what make a weak rank 1 possible

Every ability declares `rankAvailability`, one character level per rank, and the caster picks the rank from the character's level.

Without it, an ability's damage curve has to be gentle end to end, because nothing stops a player reaching the top rank early. Strong enough to matter at rank 1 becomes absurd a few levels later; weak enough to be safe never stops being weak. With the step, the sheet decides when the jump enters the game rather than the rate the player accumulates points.

It is **per ability and never per tree**: two abilities in one tree can open at completely different paces, which is what lets one be the early worker and the other the late reward.

Attribute scaling is the other half and not a substitute. The step controls when the jump happens; the fraction of an attribute is what makes the ability answer to the build between one step and the next.

## The party is composed when a stage begins, never before

Who walks in is read at `StartStage` and only there — first attempt, advance after a win, restart after a defeat. The hero limit of that stage, the fielded team and its order, the formation, and later the equipment are all read together at that moment.

The player-facing rule falls out of it: changing gear, team or formation only takes effect on the next stage.

**Heroes are not rebuilt between stages.** One instance per owned hero lives for the whole session, so the level and experience earned in one stage carry into the next; what changes per stage is who stands on the board and where. Destroying and recreating them would have thrown away exactly what the player just earned, and would have needed the progress carried somewhere else and put back.

## Where a stage's difficulty peak lives is a design decision, written per stage

Measured in 0.9.2.0: removing two minions from the final wave of stage 4 moved the level it demands by eight. The five other waves together do not come close.

So without saying it, every stage ends up calibrated at the same place — the last wave. `stages.md` now declares whether each stage peaks at the villain, at the attrition, or is balanced across both, and the three tune very differently. A villain peak is graded and adjusts in small steps through who accompanies it; an attrition peak is far coarser and needs many enemy instances before the multiplier stops jumping.

## A balance test measures a distribution, never one party

`StageSweep` plays a stage many times — different parties, different builds, different seeds — and reports how many cleared. It replaced a single number that came from one party with the sheet's own build on one seed, and was presented as a property of the stage.

The number was not merely imprecise, it was the **best case wearing the clothes of the requirement**: the old test said stage 4 needed level 5, and the sweep found that at level 5 fewer than a third of sampled builds get through.

Two rules fell out of building it, and both are easy to get wrong again:

- **The sample size grows with the space, per stage and never per row.** Sizing it per level looks reasonable and is a trap: the builds that enter at higher levels are the strange corners, so the share of clears falls as the level rises and the rows stop being comparable.
- **It is a sample and never a proof**, and the snapshot says so in as many words. Past the opening stages the space of parties, builds, orders, items and trees cannot be covered.

The assertions ask about things the project actually promises — the sheet's own build gets through at the recommended level, more than one build gets through at all, and the level matters below the range — rather than a percentage. A share of a sample that deliberately includes terrible builds has a ceiling that has nothing to do with the stage.

## POW is the only attribute that is offence and defence at once

Measured by the sweep in 0.9.3.0: no AGI or SPE build clears stage 3 onwards at any level, and on stage 4 pure POW clears every attempt while the sheet's own distribution clears half.

The cause is structural rather than a number. POW gives physical damage **and** maximum health; CON gives health; AGI gives evasion and speed; SPE gives elemental damage and cooldown. Only one of the four pays on both sides of a fight.

Deliberately not fixed. Items do not exist yet and will move everything, and a rebalance could just as well come from the other direction — raising enemy physical armour hurts POW while leaving elemental damage alone, and no enemy today has a single point of elemental resistance. It is on the watch list, not on the fix list.

## An attribute multiplies the base, it never adds to it

Ten points of POW or SPE raise damage by one percent. The base comes from content: the weapon for a basic attack, the rank for an ability, and the character's own punch until a weapon exists.

A point that added flat damage hands a character at the 500 cap five hundred damage for free, and no weapon is worth five hundred of anything — **the item would stop mattering before it was built**. Multiplying makes the two grow together: a better weapon is always better, and more POW makes every weapon better.

The cost is real and was accepted knowingly: five points per level become half a percent of damage, so levelling is barely felt on offence. Items and the skill tree carry that feeling, never the level. It is also why this landed beside items rather than earlier.

An ability's `scaling` block is a **weight on that rate**, and not damage per point, for the same reason one level up: flat damage per point would make the rank stop mattering once a character had enough points.

## POW is offence only, CON is survival only

POW gives damage and nothing else. CON gives maximum health and multiplies regeneration. AGI gives evasion and speed. SPE gives ability damage and cooldown reduction.

Before 0.10.0.0 POW gave damage **and** health, and the sweep measured what that did: no AGI or SPE build cleared stage 3 onwards, and pure POW cleared stage 4 outright while the sheet's own distribution cleared half of it.

After the change the dominance inverted — pure CON clears everything, pure POW clears nothing. That is not the old problem returning: CON wins because survival decides a fight nobody can win on damage yet, and a punch is all anybody has. The weapon is what puts POW back in the conversation, which is what the next version is for.

## The damage base grows per level, like armour

With the attribute multiplying instead of adding, **the level stops raising damage on its own**. A hero is fine, since equipment raises theirs. A minion has no equipment, so one from a late act would punch exactly like one from the first.

So the damage base declares a gain per level, the same shape defence already used, and for the same reason: a number that never moves is worth less every level, because what it is measured against keeps rising. Heroes leave the gain at zero; minions and villains use it.


## A hero and the thing that fights are two objects

What the player builds up — level, experience, skill points, attribute allocation, later equipment — belongs to a **record**. What stands on the board belongs to a **combatant**, built from the record when a stage begins and destroyed when it ends. Experience flows from the combatant to the record and nothing flows back.

This is the answer to a question that kept being asked one system at a time: *what happens if this changes mid stage?* Attributes needed a mechanism for it in 0.10.3.0, and equipment would have needed its own. Now nothing does, because a combatant is a photograph and the record can be edited freely while a fight is running.

Four things stopped existing rather than being handled:

- **The level in two places**, which could disagree with itself. It lives on the record only.
- **A rebuild reaching a fight in progress.** The fight is not reading the record.
- **Maximum health moving under a wounded character.** It comes only from CON, which only changes between stages.
- **A hand written list of state to clear between stages.** There is no list, because there is nothing to clear. That list was already incomplete: the regeneration carry crossed stages unnoticed for a long time.

Minions, villains and NPCs were always built per stage and thrown away. Heroes were the exception, and the exception was the bug.

## A point placed only counts from the next stage

Placing an attribute point and the point taking effect are two different moments. It applies to every way of placing one, and **the automatic distribution is not an exception** — it decides *where* a point goes, never *when* it counts.

Without it there is a whole exploit: taking points back is free, instant and unlimited by design, so a player could rebuild mid fight — CON while being hit, POW for the killing blow — and meet every enemy with the build made for that enemy. A choice that can be undone at any moment stops being a choice.

It is the same rule item, team, order and formation already followed under "the party is composed when a stage begins". Attributes were the only thing escaping it.

**How it is enforced changed in 0.10.4.0 and the rule did not.** 0.10.3.0 implemented it with a second allocation held back until a commit. Separating the record from the combatant gives the same rule for free — a fight reads a copy — so the mechanism was deleted. If a future change ever reunites the two objects, this rule needs its mechanism back.

The clamp in `RebuildStats` stays, and still earns its place: a debuff on CON lowers the maximum mid fight, where no stage boundary is coming to tidy up. That is the only case left that can move a maximum during a fight.

**What does not wait: the level itself.** Armour, elemental resistance and base damage per level arrive immediately. Those growths exist so a character does not rot against stronger enemies — the diminishing returns constant is `50 x attacker level` — so holding them back would work against the reason they exist. `TheArmourALevelBuysArrivesImmediately` guards that boundary, because it looks like an inconsistency and somebody will eventually try to "fix" it.

## Why the rank step exists

Moved out of `abilities.md` in 0.10.4.2: the spec states that ranks unlock at levels the sheet declares, and this is the reasoning behind it.

It is what allows **low damage at rank 1 without condemning the ability to be weak forever**.

Without the step, an ability's damage curve has to be smooth from end to end, because nothing stops a player from reaching the top rank early. One that starts strong enough to be useful becomes absurd a few levels later; one that starts weak enough not to break anything never stops being weak.

With it, the sheet decides when the jump enters the game, not the pace at which the player accumulates points. That is what makes a curve like `15, 50, 90, 120, 150` calibratable at all.

**It does not replace attribute scaling.** The step controls when the jump happens; the attribute fraction is what makes the ability answer to the build between one step and the next.

## Why an attribute multiplies instead of adding

Moved out of `attributes.md` in 0.10.4.2. The rule itself is stated there; this is why it was chosen over the alternative.

With the 500 point cap, an attribute giving one point of damage per point would hand out five hundred damage for free. No weapon is worth five hundred of anything, so **the item would stop mattering** — and items are half the game.

Multiplying makes the two grow together: a better weapon is always better, and more POW makes every weapon better. That is what keeps hunting for items worthwhile from beginning to end.

The percentage is deliberately small, so even an extreme POW leaves the weapon deciding the damage. **The cost was accepted knowingly:** five points per level is a fraction of a percent, so levelling is barely felt on offence. Items and the skill tree carry that feeling, never the level. The current numbers are in `.claude/balance/snapshot.md`.

## Why the experience exponent is what it is

Moved out of `progress.md` in 0.10.4.2. The formula lives there; this is the calibration behind the number.

What controls the pace of the whole game is not the constants, it is the **difference between the two exponents**. It decides how many same-level enemies one level costs, and that count grows as the player advances.

It was calibrated against Task Bar Hero on a free progression, the closest reference in the genre, aiming to land at a comparable share of the maximum level after a long time played. The intent is that the last stretch of levels costs more than everything before it: level 100 exists, but nobody needs to reach it.

The hours this actually produces are in `.claude/balance/snapshot.md` and never in prose, because they move whenever the exponent or the enemy rate moves.

## Why saving happens only at those four moments

Moved out of `save.md` in 0.10.4.2.

**No periodic save**, because the save holds no position inside a stage — one taken mid-wave would record nothing the wave boundary does not already hold. Wave boundaries are frequent, so a save is never far away.

**No save when attributes are touched**, because the game never stops: while the player is placing points the party keeps clearing waves, and the next boundary arrives in seconds. The price is a small window, and since 0.10.4.0 it is smaller still — points only take effect on the next stage anyway.
