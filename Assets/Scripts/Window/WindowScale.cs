using UnityEngine;
using UnityEngine.InputSystem;

namespace HerOClock.Window
{
    /// <summary>
    /// Keeps the game window on one of the sizes <see cref="WindowScaleLadder"/> allows, and lets
    /// the player step between them.
    ///
    /// The window is not resizable by dragging and cannot go fullscreen. Both are switched off in
    /// the player settings, because either one lets the window take a shape the game was never
    /// framed for: the board is exactly as wide as the reference resolution, so widening the
    /// window by a single pixel starts showing the ground run out at the sides.
    ///
    /// The size is remembered in PlayerPrefs rather than in the save file. It describes the
    /// player's screen, not their progress, so it should survive starting a new game and should
    /// not travel to another machine.
    /// </summary>
    public class WindowScale : MonoBehaviour
    {
        private const string PreferenceKey = "HerOClock.WindowScale";

        private int referenceWidth;
        private int referenceHeight;
        private int maximum;

        /// <summary>The multiplier currently applied. 1 means the window is the reference size.</summary>
        public int Scale { get; private set; }

        /// <summary>
        /// Picks a starting size and applies it.
        ///
        /// The first size is chosen from the screen rather than fixed, because the same number of
        /// pixels is a small window on one monitor and a postage stamp on another. A remembered
        /// choice wins, as long as it still fits the screen the game is on now.
        /// </summary>
        public void Configure(int referenceWidth, int referenceHeight)
        {
            this.referenceWidth = Mathf.Max(1, referenceWidth);
            this.referenceHeight = Mathf.Max(1, referenceHeight);

            int screenWidth = Display.main.systemWidth;
            int screenHeight = Display.main.systemHeight;

            maximum = WindowScaleLadder.Largest(
                this.referenceWidth, this.referenceHeight, screenWidth, screenHeight,
                WindowScaleLadder.MaximumShare);

            int chosen = PlayerPrefs.HasKey(PreferenceKey)
                ? WindowScaleLadder.Snap(PlayerPrefs.GetInt(PreferenceKey), maximum)
                : WindowScaleLadder.Largest(
                    this.referenceWidth, this.referenceHeight, screenWidth, screenHeight,
                    WindowScaleLadder.DefaultShare);

            Apply(chosen);

            Debug.Log("WindowScale: " + Scale + "x (" + this.referenceWidth * Scale + "x"
                + this.referenceHeight * Scale + "), up to " + maximum + "x on this screen. "
                + "Minus and equals change it.", this);
        }

        private void Update()
        {
            HoldTheSize();

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.equalsKey.wasPressedThisFrame)
            {
                Apply(WindowScaleLadder.Next(Scale, maximum));
            }

            if (keyboard.minusKey.wasPressedThisFrame)
            {
                Apply(WindowScaleLadder.Previous(Scale));
            }
        }

        private void Apply(int scale)
        {
            if (scale == Scale)
            {
                return;
            }

            Scale = scale;

            PlayerPrefs.SetInt(PreferenceKey, scale);

            // Only does anything in a built player. In the editor the Game view is sized by the
            // editor itself, so this is one of the few things that has to be seen in a build.
            Screen.SetResolution(referenceWidth * scale, referenceHeight * scale, FullScreenMode.Windowed);
        }

        /// <summary>
        /// Keeps the window at exactly the size it is supposed to be, and keeps the frame off.
        ///
        /// Both are checked every frame rather than set once, because both come undone on their
        /// own. Unity applies a resolution change at the end of a frame rather than when asked,
        /// and changing the size can bring the frame back with it.
        ///
        /// The size is then forced through Windows instead of being left to Unity.
        /// <c>Screen.SetResolution</c> asks for a client size and lets Windows size the frame
        /// around it, so once the frame is gone the two disagree by a handful of pixels. That is
        /// enough to show the ground run out at the sides, because the board is exactly as wide
        /// as the reference resolution and has no margin at all to spare.
        /// </summary>
        private void HoldTheSize()
        {
            if (!DesktopWindow.IsSupported)
            {
                return;
            }

            int wanted = referenceWidth * Scale;
            int wantedHeight = referenceHeight * Scale;

            DesktopWindow.Rect window = DesktopWindow.Bounds();

            if (window.Width == wanted && window.Height == wantedHeight)
            {
                return;
            }

            DesktopWindow.RemoveFrame();
            DesktopWindow.Resize(wanted, wantedHeight);
        }
    }
}
