using UnityEngine;
using UnityEngine.InputSystem;

namespace HerOClock.Window
{
    /// <summary>
    /// Makes the window a desktop widget: no frame, above everything, moved by dragging the game
    /// itself, and magnetic to the edges of the screen.
    ///
    /// Taking the frame off is what forces the rest. Without a title bar the operating system
    /// offers no way to move the window, so dragging stops being a convenience and becomes the
    /// only way. That is why all of this arrived at once rather than in pieces.
    ///
    /// The frame and the size belong to <see cref="WindowScale"/>, which is the piece that knows
    /// how big the window is supposed to be. This one owns where it sits and that it stays on top.
    ///
    /// Windows is asked to do the moving, not Unity. The drag is tracked in screen coordinates
    /// through <see cref="DesktopWindow"/>, because Unity reports the mouse relative to the
    /// window, and while dragging the window that frame of reference is moving too.
    /// </summary>
    public class WindowDrag : MonoBehaviour
    {
        /// <summary>
        /// How often the window reasserts that it belongs on top, in seconds.
        ///
        /// Being topmost is not permanent. Another program going topmost, or the shell
        /// reshuffling, can drop this window back into the pile without telling anybody.
        /// </summary>
        private const float ReassertInterval = 2f;

        private bool dragging;
        private int grabOffsetX;
        private int grabOffsetY;
        private float sinceReassert;

        private void Start()
        {
            if (!DesktopWindow.IsSupported)
            {
                enabled = false;
                return;
            }

            DesktopWindow.KeepAbove();
        }

        private void Update()
        {
            sinceReassert += Time.unscaledDeltaTime;

            if (sinceReassert >= ReassertInterval)
            {
                sinceReassert = 0f;
                DesktopWindow.KeepAbove();
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginDrag();
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
            }

            if (dragging && mouse.leftButton.isPressed)
            {
                ContinueDrag();
            }
        }

        private void BeginDrag()
        {
            int cursorX;
            int cursorY;
            DesktopWindow.CursorPosition(out cursorX, out cursorY);

            DesktopWindow.Rect window = DesktopWindow.Bounds();

            // Kept as the distance from the corner to the cursor, so the window does not jump to
            // centre itself under the pointer when the drag starts.
            grabOffsetX = cursorX - window.Left;
            grabOffsetY = cursorY - window.Top;

            dragging = true;
        }

        private void ContinueDrag()
        {
            int cursorX;
            int cursorY;
            DesktopWindow.CursorPosition(out cursorX, out cursorY);

            DesktopWindow.Rect window = DesktopWindow.Bounds();
            DesktopWindow.Rect monitor = DesktopWindow.Monitor();

            WindowSnap.Position target = WindowSnap.Apply(
                cursorX - grabOffsetX,
                cursorY - grabOffsetY,
                window.Width,
                window.Height,
                monitor.Left,
                monitor.Top,
                monitor.Right,
                monitor.Bottom);

            DesktopWindow.MoveTo(target.X, target.Y);
        }
    }
}
