using System.Collections.Generic;

namespace HerOClock.Progression
{
    /// <summary>
    /// Who the player owns, who is fielded, and who is sitting out.
    ///
    /// Three things that look alike and are not, and keeping them apart is the whole point of this
    /// class:
    ///
    /// - **Slots** are how many positions the team has. They are unlocked by clearing stages, and
    ///   the cap is four.
    /// - **Owned** is every hero the player has met. It grows by clearing stages too, but a hero
    ///   arriving and a position opening are separate events that happen to often land together.
    /// - **Team** is who actually walks into a stage, in order. Whoever is owned and not on the
    ///   team is on the bench.
    ///
    /// A stage then applies its own limit on top, taking only the first few of the team. That is a
    /// fourth number, and it belongs to the stage rather than here.
    ///
    /// There is no interface for any of this yet. It exists now so the menus of 0.11.0.0 have
    /// something to draw instead of inventing the model at the same time as the screen.
    /// </summary>
    public class Roster
    {
        /// <summary>Most positions a team can ever have, from characters.md.</summary>
        public const int MaxSlots = 4;

        private readonly List<string> owned = new List<string>();
        private readonly List<string> team = new List<string>();

        public Roster(int slots = 1)
        {
            Slots = slots < 1 ? 1 : slots > MaxSlots ? MaxSlots : slots;
        }

        /// <summary>How many positions the team has. Never below 1, never above <see cref="MaxSlots"/>.</summary>
        public int Slots { get; private set; }

        /// <summary>Every hero the player owns, in the order they arrived.</summary>
        public IReadOnlyList<string> Owned
        {
            get { return owned; }
        }

        /// <summary>Who is fielded, in the order the player put them. Never longer than the slots.</summary>
        public IReadOnlyList<string> Team
        {
            get { return team; }
        }

        /// <summary>Owned but not fielded.</summary>
        public List<string> Benched()
        {
            List<string> bench = new List<string>();

            for (int i = 0; i < owned.Count; i++)
            {
                if (!team.Contains(owned[i]))
                {
                    bench.Add(owned[i]);
                }
            }

            return bench;
        }

        /// <summary>
        /// Adds a hero the player just met.
        ///
        /// It walks straight into a free position when there is one. With no interface to move
        /// anybody yet, a hero that arrived and sat invisibly on the bench would look like the
        /// unlock simply failed.
        /// </summary>
        public bool Unlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || owned.Contains(id))
            {
                return false;
            }

            owned.Add(id);

            if (team.Count < Slots)
            {
                team.Add(id);
            }

            return true;
        }

        /// <summary>
        /// Opens one more position on the team, up to the cap.
        ///
        /// Whoever has been waiting on the bench longest takes it, for the same reason a new hero
        /// joins straight away: nothing can move them there by hand yet.
        /// </summary>
        public bool GrantSlot()
        {
            if (Slots >= MaxSlots)
            {
                return false;
            }

            Slots++;

            List<string> bench = Benched();

            if (bench.Count > 0)
            {
                team.Add(bench[0]);
            }

            return true;
        }

        /// <summary>Puts a fielded hero on the bench.</summary>
        public bool SendToBench(string id)
        {
            return team.Remove(id);
        }

        /// <summary>Fields a benched hero, if there is a free position.</summary>
        public bool Field(string id)
        {
            if (!owned.Contains(id) || team.Contains(id) || team.Count >= Slots)
            {
                return false;
            }

            team.Add(id);
            return true;
        }

        /// <summary>
        /// Who walks into a stage that accepts at most <paramref name="heroLimit"/> heroes.
        ///
        /// The first few of the team, so the order the player chose is what decides. A limit of 0
        /// or less means the stage did not declare one, and the whole team goes.
        /// </summary>
        public List<string> PartyFor(int heroLimit)
        {
            int take = heroLimit > 0 && heroLimit < team.Count ? heroLimit : team.Count;

            List<string> party = new List<string>();

            for (int i = 0; i < take; i++)
            {
                party.Add(team[i]);
            }

            return party;
        }

        /// <summary>Replaces the whole state, used when a save is read.</summary>
        public void Restore(int slots, IReadOnlyList<string> ownedIds, IReadOnlyList<string> teamIds)
        {
            Slots = slots < 1 ? 1 : slots > MaxSlots ? MaxSlots : slots;

            owned.Clear();
            team.Clear();

            if (ownedIds != null)
            {
                for (int i = 0; i < ownedIds.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(ownedIds[i]) && !owned.Contains(ownedIds[i]))
                    {
                        owned.Add(ownedIds[i]);
                    }
                }
            }

            if (teamIds != null)
            {
                for (int i = 0; i < teamIds.Count && team.Count < Slots; i++)
                {
                    if (owned.Contains(teamIds[i]) && !team.Contains(teamIds[i]))
                    {
                        team.Add(teamIds[i]);
                    }
                }
            }
        }
    }
}
