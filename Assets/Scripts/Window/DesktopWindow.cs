using System;
using System.Runtime.InteropServices;

namespace HerOClock.Window
{
    /// <summary>
    /// The few things about the game's own window that Unity cannot do, done through Windows.
    ///
    /// Unity can size a window and it can go fullscreen, and that is the end of it. Sitting above
    /// everything else, losing the title bar and being moved by the game itself are all things
    /// only the operating system can be asked for.
    ///
    /// **None of this ever runs in the editor.** The handle these calls would find is the Unity
    /// editor's own window, so a stray call here would strip the editor of its title bar or pin
    /// it above every other program. Everything is behind a compile time guard for that reason,
    /// and the whole class quietly does nothing when the guard is off.
    /// </summary>
    public static class DesktopWindow
    {
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }
        }

        /// <summary>True when the calls below actually do something.</summary>
        public static bool IsSupported
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            get { return true; }
#else
            get { return false; }
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private const int GwlStyle = -16;

        private const uint WsPopup = 0x80000000;
        private const uint WsVisible = 0x10000000;

        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpFrameChanged = 0x0020;
        private const uint SwpShowWindow = 0x0040;

        private const int MonitorDefaultToNearest = 2;

        private static readonly IntPtr Topmost = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor;
            public NativeRect Work;
            public uint Flags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativePoint point);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr window, out NativeRect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr window, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        private static IntPtr handle;

        private static IntPtr Handle
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    handle = GetActiveWindow();
                }

                return handle;
            }
        }
#endif

        /// <summary>
        /// Takes the frame off the window: no title bar, no border, no close button.
        ///
        /// It has to be re-applied after anything that resizes the window, because changing the
        /// resolution can bring the frame back.
        ///
        /// Losing the title bar means losing the only way Windows gives the player to move or
        /// close the window. Moving is handled by <see cref="WindowDrag"/>; closing is Alt+F4
        /// until there is a menu offering it.
        /// </summary>
        public static void RemoveFrame()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowLongPtr(Handle, GwlStyle, new IntPtr(WsPopup | WsVisible));

            // Without SwpFrameChanged, Windows keeps drawing the old frame until something else
            // forces a repaint.
            SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoMove | SwpNoSize | SwpFrameChanged | SwpShowWindow);
#endif
        }

        /// <summary>
        /// Puts the window above every other window, the taskbar included.
        ///
        /// Worth re-asserting from time to time. Another program going topmost, or the shell
        /// reshuffling the order, can quietly drop this window back down.
        ///
        /// A game running in exclusive fullscreen is the one thing this cannot get above, since
        /// that mode takes over the display outright. Borderless fullscreen, which is what most
        /// games use now, is a normal window and behaves.
        /// </summary>
        public static void KeepAbove()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowPos(Handle, Topmost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate);
#endif
        }

        public static void MoveTo(int x, int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowPos(Handle, Topmost, x, y, 0, 0, SwpNoSize | SwpNoActivate);
#endif
        }

        /// <summary>
        /// Sets the window to an exact size, in pixels.
        ///
        /// This exists because <c>Screen.SetResolution</c> asks for a *client* size and lets
        /// Windows work out the window size around it, using the frame Unity believes it has.
        /// Once the frame has been taken off, those two disagree, and the client ends up a little
        /// larger than asked for. A few stray pixels are enough to show the ground run out at the
        /// sides, since the board is exactly as wide as the reference resolution.
        ///
        /// With no frame there is no non-client area, so window size and client size are the same
        /// thing and this is exact by construction.
        /// </summary>
        public static void Resize(int width, int height)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowPos(Handle, Topmost, 0, 0, width, height, SwpNoMove | SwpNoActivate);
#endif
        }

        /// <summary>Where the window is, in screen coordinates.</summary>
        public static Rect Bounds()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            NativeRect native;

            if (GetWindowRect(Handle, out native))
            {
                return new Rect { Left = native.Left, Top = native.Top, Right = native.Right, Bottom = native.Bottom };
            }
#endif
            return new Rect();
        }

        /// <summary>
        /// The bounds of the monitor the window is currently on.
        ///
        /// Asked per window rather than assuming the primary display, because a second monitor is
        /// common and a game that always snapped to the wrong screen would be worse than one that
        /// did not snap at all.
        /// </summary>
        public static Rect Monitor()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr monitor = MonitorFromWindow(Handle, MonitorDefaultToNearest);

            MonitorInfo info = new MonitorInfo();
            info.Size = Marshal.SizeOf(typeof(MonitorInfo));

            if (GetMonitorInfo(monitor, ref info))
            {
                return new Rect
                {
                    Left = info.Monitor.Left,
                    Top = info.Monitor.Top,
                    Right = info.Monitor.Right,
                    Bottom = info.Monitor.Bottom
                };
            }
#endif
            return new Rect();
        }

        /// <summary>
        /// The mouse position in screen coordinates.
        ///
        /// Unity reports the mouse relative to the window, which is useless while dragging the
        /// window itself: moving the window moves the frame of reference along with it, and the
        /// two chase each other.
        /// </summary>
        public static void CursorPosition(out int x, out int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            NativePoint point;

            if (GetCursorPos(out point))
            {
                x = point.X;
                y = point.Y;
                return;
            }
#endif
            x = 0;
            y = 0;
        }
    }
}
