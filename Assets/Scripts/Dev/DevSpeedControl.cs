using UnityEngine;
using UnityEngine.InputSystem;

namespace HerOClock.Dev
{
    /// <summary>
    /// Runs the game faster or slower while developing.
    ///
    /// Calibrating an experience curve or a stage at normal speed means waiting real minutes
    /// for every change, so this is less a convenience than a working condition.
    ///
    /// It is only attached in the editor and in development builds. A speed slider in a
    /// released build would defeat a game whose whole point is that time passes.
    /// </summary>
    public class DevSpeedControl : MonoBehaviour
    {
        /// <summary>
        /// Highest speed allowed.
        ///
        /// The limit is not arbitrary: the frame rate is raised by the same factor, so the
        /// simulation keeps receiving time steps of exactly the same size as at normal speed.
        /// Going past this would mean a frame rate that is expensive for no good reason.
        /// </summary>
        public const float MaxSpeed = 8f;

        [Tooltip("Multiplier applied to the passage of time. Can be dragged while playing.")]
        [Range(0.25f, MaxSpeed)]
        [SerializeField] private float speed = 1f;

        private int baseFrameRate;
        private float appliedSpeed = -1f;

        public float Speed
        {
            get { return speed; }
            set { speed = Mathf.Clamp(value, 0.25f, MaxSpeed); }
        }

        private void Awake()
        {
            baseFrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 30;
        }

        private void OnDestroy()
        {
            // Time scale is engine state and would otherwise stay sped up for whatever runs next.
            Time.timeScale = 1f;
            Application.targetFrameRate = baseFrameRate;
        }

        private void Update()
        {
            ReadShortcuts();

            if (Mathf.Approximately(appliedSpeed, speed))
            {
                return;
            }

            appliedSpeed = speed;
            Time.timeScale = speed;

            // Raising the frame rate along with the speed is what keeps the simulation honest.
            // The code lands at most one attack per frame, so speeding up time without adding
            // frames would silently make fast characters attack less often than they should.
            // Matching the two leaves the per frame time step identical to normal speed.
            Application.targetFrameRate = Mathf.Clamp(
                Mathf.RoundToInt(baseFrameRate * speed), baseFrameRate, 240);

            Debug.Log("DevSpeedControl: running at " + speed + "x (" + Application.targetFrameRate + " fps).", this);
        }

        private void ReadShortcuts()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit0Key.wasPressedThisFrame) Speed = 0.5f;
            if (keyboard.digit1Key.wasPressedThisFrame) Speed = 1f;
            if (keyboard.digit2Key.wasPressedThisFrame) Speed = 2f;
            if (keyboard.digit3Key.wasPressedThisFrame) Speed = 4f;
            if (keyboard.digit4Key.wasPressedThisFrame) Speed = 8f;
        }
    }
}
