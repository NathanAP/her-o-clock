using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Which stage the player is on.
    ///
    /// Deliberately nothing finer than that. No wave, no cooldowns, no positions, no random state:
    /// loading starts the recorded stage **from its beginning, with everybody at full health**.
    ///
    /// That is the same thing a defeat already does, so closing the game opens nothing a player
    /// could not already have by losing, and losing is free. What it buys is that there is no
    /// mid-stage state to write down, to migrate, or to get subtly wrong.
    ///
    /// The attrition of gameplay.md is untouched, because it was only ever a rule about the inside
    /// of a stage: damage carries from wave to wave, and a stage that starts over starts whole.
    /// </summary>
    [Serializable]
    public class StageSave
    {
        public string id;
    }
}
