using HerOClock.Characters;
using HerOClock.Common;
using TMPro;
using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// How a character looks: the body, the health bar and the flash when it takes damage.
    ///
    /// It is a MonoBehaviour because it is pure presentation. The combat simulation stays
    /// entirely inside the BattleDirector, so nothing here affects reproducibility.
    /// </summary>
    public class CharacterView : MonoBehaviour
    {
        private Character character;
        private CharacterViewSettings settings;
        private float cellSize;

        private SpriteRenderer body;
        private SpriteRenderer healthBackground;
        private SpriteRenderer healthFill;
        private TextMeshPro levelLabel;
        private Color bodyColor;
        private float flashRemaining;
        private int shownLevel = -1;

        public void Build(Character character, CharacterViewSettings settings, Color color, float cellSize)
        {
            this.character = character;
            this.settings = settings;
            this.cellSize = cellSize;

            bodyColor = color;

            body = CreateSprite("Body", settings.BodySize, settings.BodyOffsetY, color, 0);
            healthBackground = CreateSprite("HealthBarBackground", settings.HealthBarSize, settings.HealthBarOffsetY, settings.HealthBarBackground, 1);
            healthFill = CreateSprite("HealthBarFill", settings.HealthBarSize, settings.HealthBarOffsetY, settings.HealthBarFull, 2);
            levelLabel = CreateLevelLabel();

            character.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (character != null)
            {
                character.Changed -= Refresh;
            }
        }

        /// <summary>Makes the body flash. Called when the character takes a blow.</summary>
        public void Flash()
        {
            flashRemaining = settings.FlashDuration;
        }

        private void Update()
        {
            if (flashRemaining <= 0f)
            {
                return;
            }

            flashRemaining -= Time.deltaTime;

            float amount = Mathf.Clamp01(flashRemaining / settings.FlashDuration);
            body.color = Color.Lerp(bodyColor, settings.FlashColor, amount);
        }

        private void Refresh()
        {
            bool alive = character.IsAlive;

            // A dead minion disappears but comes back when the battle restarts, so the object
            // has to be reactivated before anything else happens.
            if (alive && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            UpdateHealthBar();
            UpdateLevelLabel();

            healthBackground.enabled = alive;
            healthFill.enabled = alive;

            if (alive)
            {
                body.color = bodyColor;
                flashRemaining = 0f;
                return;
            }

            if (character.Kind == CharacterKind.Hero)
            {
                // A fallen hero stays on the field, dimmed, so it can be revived in place.
                body.color = bodyColor * 0.3f;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateHealthBar()
        {
            float fraction = character.HealthFraction;
            float fullWidth = settings.HealthBarSize.x * cellSize;

            Vector3 scale = healthFill.transform.localScale;
            healthFill.transform.localScale = new Vector3(fullWidth * fraction, scale.y, 1f);

            // The bar shrinks with its left edge pinned, so it has to slide left by half of
            // whatever width it lost.
            Vector3 position = healthFill.transform.localPosition;
            healthFill.transform.localPosition = new Vector3(-fullWidth * (1f - fraction) * 0.5f, position.y, position.z);

            healthFill.color = Color.Lerp(settings.HealthBarEmpty, settings.HealthBarFull, fraction);
        }

        /// <summary>
        /// The level shown under the character.
        ///
        /// Provisional: it exists so progression is visible while there is no interface, and it
        /// is meant to come out later. A tiny screen sitting in the corner cannot afford to
        /// carry a number that the player only cares about once in a while.
        /// </summary>
        private TextMeshPro CreateLevelLabel()
        {
            GameObject child = new GameObject("LevelLabel");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = new Vector3(0f, -0.5f * cellSize, 0f);

            TextMeshPro label = child.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 1.6f * cellSize;
            label.color = new Color(0.85f, 0.85f, 0.9f, 0.8f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.sortingLayerID = SortingLayer.NameToID("Characters");
            label.sortingOrder = 3;
            label.rectTransform.sizeDelta = new Vector2(2f * cellSize, 0.4f * cellSize);

            return label;
        }

        private void UpdateLevelLabel()
        {
            if (levelLabel == null || shownLevel == character.Level)
            {
                return;
            }

            shownLevel = character.Level;
            levelLabel.text = "Lv " + shownLevel;
        }

        private SpriteRenderer CreateSprite(string name, Vector2 size, float offsetY, Color color, int order)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = new Vector3(0f, offsetY * cellSize, 0f);
            child.transform.localScale = new Vector3(size.x * cellSize, size.y * cellSize, 1f);

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite.Get();
            renderer.color = color;
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = order;

            return renderer;
        }
    }
}
