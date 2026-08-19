using System;
using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Progression;
using HerOClock.Stages;
using UnityEngine;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Decides when the game writes a save.
    ///
    /// The moments are chosen in save.md, and there are only two of them plus closing the window:
    ///
    /// - **A wave was cleared**, which is when the experience and money it paid are worth keeping.
    /// - **A stage began**, including the restart that follows a victory or a defeat.
    ///
    /// There is no periodic save, and that is not laziness. A save holds no position inside a
    /// stage — loading starts the recorded stage from its beginning — so a write taken mid-wave
    /// would record nothing a write at the wave boundary does not already hold.
    /// </summary>
    public class SaveService : MonoBehaviour
    {
        private SaveStore store;
        private StageRunner runner;
        private IReadOnlyList<Character> heroes;
        private PlayerWallet wallet;
        private ActivityLog activity;

        /// <summary>
        /// Carried from the save that was loaded and never cleared. Once a file has failed its
        /// signature, every save written after it says so: it is a fact about this save's history,
        /// not a state it can recover from.
        /// </summary>
        private string integrity = SavePayload.IntegrityOk;

        private Roster roster;
        private IReadOnlyList<string> clearedStages;

        public void Configure(
            SaveStore store,
            StageRunner runner,
            IReadOnlyList<Character> heroes,
            PlayerWallet wallet,
            ActivityLog activity,
            string integrity,
            Roster roster,
            IReadOnlyList<string> clearedStages)
        {
            this.roster = roster;
            this.clearedStages = clearedStages;

            this.store = store;
            this.runner = runner;
            this.heroes = heroes;
            this.wallet = wallet;
            this.activity = activity;
            this.integrity = string.IsNullOrEmpty(integrity) ? SavePayload.IntegrityOk : integrity;

            runner.WaveCleared += Save;
            runner.StageStarted += Save;
        }

        private void OnDestroy()
        {
            if (runner != null)
            {
                runner.WaveCleared -= Save;
                runner.StageStarted -= Save;
            }
        }

        /// <summary>
        /// Closing the window is a courtesy, never the guarantee.
        ///
        /// Alt+F4 gets here; a killed process does not. That is why the wave boundary carries the
        /// weight, and why nothing important is ever left waiting for this.
        /// </summary>
        private void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            if (store == null || runner == null || runner.Stage == null)
            {
                return;
            }

            SavePayload payload = SaveMapper.Capture(
                heroes,
                wallet,
                activity,
                runner.Stage.id,
                integrity);

            payload.roster = SaveMapper.ToSave(roster, clearedStages);

            try
            {
                store.Write(payload, DateTime.UtcNow);
                store.Prune();
            }
            catch (Exception exception)
            {
                // A save that cannot be written must not take the game down with it. The next wave
                // boundary is seconds away and will try again.
                Debug.LogWarning("Save: the progress could not be written. " + exception.Message, this);
            }
        }
    }
}
