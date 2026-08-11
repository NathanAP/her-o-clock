using HerOClock.Combat;
using TMPro;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// A number that rises and fades above whoever took the blow.
    ///
    /// Besides giving the fight a rhythm, it is how we check on screen that the mitigation and
    /// evasion formulas are coming out as expected, without having to read the Console.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        private static readonly Color DamageColor = new Color(1f, 0.95f, 0.85f);
        private static readonly Color EvadedColor = new Color(0.7f, 0.78f, 0.9f);
        private static readonly Color PerfectEvasionColor = new Color(0.45f, 0.8f, 1f);
        private static readonly Color HealingColor = new Color(0.45f, 0.9f, 0.5f);

        private const float RiseDistance = 0.7f;
        private const float Lifetime = 0.7f;

        private TextMeshPro label;
        private Vector3 origin;
        private float elapsed;

        public static void Spawn(Transform parent, Vector3 worldPosition, DamageResult result, float cellSize)
        {
            string content;
            Color color;

            if (result.Healing > 0)
            {
                content = "+" + result.Healing;
                color = HealingColor;
            }
            else if (result.Damage > 0)
            {
                content = result.Damage.ToString();
                color = result.PerfectEvasion ? PerfectEvasionColor : result.Evaded ? EvadedColor : DamageColor;
            }
            else
            {
                // A fully absorbed blow still has to show up, otherwise it looks like the
                // attack never happened at all.
                content = "0";
                color = EvadedColor;
            }

            GameObject instance = new GameObject("DamageNumber");
            instance.transform.SetParent(parent, false);
            instance.transform.position = worldPosition;

            DamageNumber number = instance.AddComponent<DamageNumber>();
            number.Setup(content, color, cellSize);
        }

        private void Setup(string content, Color color, float cellSize)
        {
            label = gameObject.AddComponent<TextMeshPro>();
            label.text = content;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3f * cellSize;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.sortingLayerID = SortingLayer.NameToID("VFX");
            label.sortingOrder = 10;

            RectTransform rect = label.rectTransform;
            rect.sizeDelta = new Vector2(2f * cellSize, 0.5f * cellSize);

            origin = transform.position;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / Lifetime;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = origin + new Vector3(0f, RiseDistance * progress, 0f);

            Color color = label.color;
            // The number stays solid for the first half of its life and fades over the second.
            color.a = progress < 0.5f ? 1f : 1f - (progress - 0.5f) * 2f;
            label.color = color;
        }
    }
}
