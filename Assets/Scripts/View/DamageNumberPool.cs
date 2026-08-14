using System.Collections.Generic;
using HerOClock.Combat;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// Keeps the floating damage numbers alive between uses instead of building one per blow.
    ///
    /// A number appears on every single hit, and a TextMeshPro object is not cheap to create. In a
    /// game meant to sit open all day, at a few blows a second, that is a steady stream of
    /// allocation and collection for objects that are identical apart from two words.
    ///
    /// The pool is an instance owned by the bootstrap, not a static. With Domain Reload disabled a
    /// static would survive leaving Play, holding references to objects the editor already
    /// destroyed, which is the trap architecture.md describes for ScriptableObjects.
    /// </summary>
    public class DamageNumberPool
    {
        private readonly Transform parent;
        private readonly float cellSize;
        private readonly Stack<DamageNumber> resting = new Stack<DamageNumber>();

        public DamageNumberPool(Transform parent, float cellSize)
        {
            this.parent = parent;
            this.cellSize = cellSize;
        }

        /// <summary>How many numbers exist, resting or rising. Used by the tests.</summary>
        public int Created { get; private set; }

        public void Show(Vector3 worldPosition, DamageResult result)
        {
            DamageNumber number;

            if (resting.Count > 0)
            {
                number = resting.Pop();
            }
            else
            {
                GameObject instance = new GameObject("DamageNumber");
                instance.transform.SetParent(parent, false);

                number = instance.AddComponent<DamageNumber>();
                number.Build(this, cellSize);

                Created++;
            }

            number.Show(worldPosition, result);
        }

        /// <summary>Called by a number once it has finished rising.</summary>
        public void Return(DamageNumber number)
        {
            number.gameObject.SetActive(false);
            resting.Push(number);
        }
    }
}
