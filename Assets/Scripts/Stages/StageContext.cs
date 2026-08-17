using System;
using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Progression;
using HerOClock.Text;
using HerOClock.View;
using UnityEngine;

namespace HerOClock.Stages
{
    /// <summary>
    /// Everything a <see cref="StageRunner"/> needs handed to it in one place.
    ///
    /// It replaced a Configure with nine positional parameters, which had reached the point where a
    /// caller could swap two of them and still compile. Naming them at the call site is worth more
    /// than the class costs.
    ///
    /// Two of the fields exist so that a stage can run with no scene around it, which is what lets
    /// the balance tests drive the real runner instead of a copy of its loop.
    /// </summary>
    public class StageContext
    {
        public BattleGrid Grid;
        public BattleDirector Director;
        public IReadOnlyDictionary<string, CharacterDefinition> CharactersById;
        public PlayerWallet Wallet;
        public StringTable Strings;
        public BattleRandom Random;
        public BoardScroller Scroller;

        /// <summary>Creates one enemy: sheet, side, cell, level and the stage's fine tuning multiplier.</summary>
        public Func<CharacterDefinition, Team, GridPosition, int, float, Character> Spawn;

        /// <summary>
        /// Throws an enemy away once its wave is over.
        ///
        /// Injected because Unity's deferred <c>Destroy</c> never runs outside Play Mode, so a test
        /// driving a whole stage would pile up every enemy it ever created and leave them holding
        /// their cells. A test passes <c>DestroyImmediate</c>; the game leaves this alone.
        /// </summary>
        public Action<GameObject> Destroy;
    }
}
