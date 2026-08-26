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
        private CharacterSprites sprites;
        private bool usesSprites;
        private Facing shownFacing = (Facing)(-1);
        private float swingRemaining;
        private bool showingSwing;
        private Vector3 lastPosition;
        private float cellsTravelled;
        private bool moving;
        private int shownRunFrame = -1;

        public void Build(Character character, CharacterViewSettings settings, Color color, float cellSize)
        {
            this.character = character;
            this.settings = settings;
            this.cellSize = cellSize;

            sprites = character.Definition.Sprites;
            usesSprites = sprites != null && sprites.HasAny;

            // A character with art is drawn in its own colours, so the placeholder tint has to
            // become white or it would be multiplied over the sprite and muddy every pixel.
            bodyColor = usesSprites ? Color.white : color;

            body = usesSprites
                ? CreateBodySprite()
                : CreateSprite("Body", settings.BodySize, settings.BodyOffsetY, color, 0);
            healthBackground = CreateSprite("HealthBarBackground", settings.HealthBarSize, settings.HealthBarOffsetY, settings.HealthBarBackground, 1);
            healthFill = CreateSprite("HealthBarFill", settings.HealthBarSize, settings.HealthBarOffsetY, settings.HealthBarFull, 2);
            levelLabel = CreateLevelLabel();

            lastPosition = transform.position;

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

        /// <summary>
        /// Puts the swinging drawing up. Called once for each basic attack thrown.
        ///
        /// It restarts the pose rather than queueing behind one already up, and that is the
        /// whole design: with attack speed and an eightfold game speed on top, blows can arrive
        /// faster than the screen refreshes. A queue would run behind the fight and eventually
        /// play swings for blows that landed seconds ago.
        ///
        /// Holding instead degrades the right way. Past a certain speed the pose simply never
        /// comes down, which is what somebody attacking without pause should look like.
        /// </summary>
        public void Swing()
        {
            if (!usesSprites)
            {
                return;
            }

            swingRemaining = settings.AttackPoseDuration;
        }

        private void Update()
        {
            if (body == null)
            {
                return;
            }

            // Walking does not raise Changed, and raising it on every step would rebuild the
            // stats and the health bar for a character that only turned. So the direction is
            // read here instead, which costs one enum comparison per frame and nothing else.
            TrackMovement();

            // Both timers run in real time on purpose. The game reaches eight times speed, and
            // Time.deltaTime shrinks with it: a pose measured in game time would be gone before
            // a single frame had drawn it.
            if (swingRemaining > 0f)
            {
                swingRemaining -= Time.unscaledDeltaTime;
            }

            UpdateFacing(character.IsAlive);

            if (flashRemaining <= 0f)
            {
                return;
            }

            flashRemaining -= Time.unscaledDeltaTime;

            float amount = Mathf.Clamp01(flashRemaining / settings.FlashDuration);
            Color target = usesSprites ? settings.SpriteFlashColor : settings.FlashColor;
            body.color = Color.Lerp(bodyColor, target, amount);
        }

        /// <summary>
        /// Measures ground covered since the last frame, which is what drives the running cycle.
        ///
        /// Read from the transform rather than asked of the mover, so the view stays a spectator
        /// and the simulation keeps knowing nothing about drawings.
        ///
        /// A jump of more than half a cell in a single frame is not walking, it is being put
        /// somewhere: restarting a battle, entering a stage, or the board sliding between waves.
        /// Counting those would spin the legs for a journey nobody made.
        /// </summary>
        private void TrackMovement()
        {
            Vector3 now = transform.position;
            float moved = Vector3.Distance(now, lastPosition);
            lastPosition = now;

            float cells = cellSize > 0f ? moved / cellSize : 0f;

            if (cells > 0.5f)
            {
                moving = false;
                return;
            }

            moving = cells > 0.0001f;

            if (moving)
            {
                cellsTravelled += cells;
            }
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
            UpdateFacing(alive);

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
                // A fallen hero stays on the field so it can be revived in place. With art it
                // lies down; without it, the rectangle just dims.
                bool fallen = usesSprites && sprites.Dead != null;
                body.color = fallen ? bodyColor : bodyColor * 0.3f;
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

        /// <summary>
        /// The body of a character that has art.
        ///
        /// The scale stays at one, unlike the placeholder rectangle. A sprite already knows how
        /// big it is through the pixels per unit of its texture, so scaling it here would fight
        /// the pixel grid and make the art shimmer as the character walks.
        /// </summary>
        private SpriteRenderer CreateBodySprite()
        {
            GameObject child = new GameObject("Body");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = new Vector3(0f, settings.SpriteOffsetY * cellSize, 0f);

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.color = Color.white;
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = 0;

            return renderer;
        }

        /// <summary>
        /// Points the body the way the character is turned, and lays it down when it falls.
        ///
        /// Only touches the renderer when the direction actually changed, because a character
        /// standing still would otherwise be assigned the same sprite on every event it raises.
        /// </summary>
        private void UpdateFacing(bool alive)
        {
            if (!usesSprites)
            {
                return;
            }

            if (!alive && sprites.Dead != null)
            {
                if (body.sprite != sprites.Dead)
                {
                    body.sprite = sprites.Dead;
                    body.flipX = false;

                    // Cleared so that reviving in place is seen as a change and puts the
                    // character back on its feet, facing wherever it was facing.
                    shownFacing = (Facing)(-1);
                    showingSwing = false;
                    swingRemaining = 0f;
                    shownRunFrame = -1;
                }

                return;
            }

            Facing facing = character.Facing;
            Sprite swing = swingRemaining > 0f ? sprites.Attack(character.AutoAttack) : null;
            bool wantsSwing = swing != null;

            // Swinging beats running, which beats standing. A character that stopped to strike
            // should be seen striking, not caught mid stride.
            bool wantsRun = !wantsSwing && moving && sprites.HasRun;
            int runFrame = wantsRun
                ? RunCycle.FrameAt(cellsTravelled, settings.RunCycleCells, sprites.Run.Length)
                : -1;

            if (facing == shownFacing && wantsSwing == showingSwing && runFrame == shownRunFrame)
            {
                return;
            }

            shownFacing = facing;
            showingSwing = wantsSwing;
            shownRunFrame = runFrame;

            // None of the three replaces the direction: a character turned left swings to the
            // left and runs to the left.
            if (wantsSwing)
            {
                body.sprite = swing;
            }
            else if (wantsRun)
            {
                body.sprite = sprites.Run[runFrame];
            }
            else
            {
                body.sprite = sprites.For(facing);
            }

            body.flipX = CharacterSprites.IsMirrored(facing);
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
