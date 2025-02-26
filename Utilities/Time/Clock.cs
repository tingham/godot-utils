using Godot;
using System;
using URBANFORT.Utilities.Interfaces;

namespace URBANFORT.Utilities.Time
{
    /// <summary>
    /// A clock that exposes a Progress method triggered every $interval and delegate invocations to respond to clock events
    /// </summary>
    public class Clock {

        public IClockDelegate ClockDelegate { get; set; }

        // The number of times that the clock can ever tick before it is completed
        public ulong Duration { get; set; }

        public ulong Intervals { get; set; }

        private ulong StartTime { get; set; } = 0;

        private ulong LastTime { get; set; } = 0;

        /// <summary>
        /// Clocks should only have access to their internal state so we use a lambda for what happens on clock ticks rather than an interface method
        /// </summary>
        public Action<double> Progress { get; set; }

        public Action Clear { get; set; }

        public bool IsRunning { get; set; } = false;
        public bool IsCancelled { get; set; } = false;

        // The update method must be called in some sort of main loop; we track our own delta time internally
        public void Update () {
            ulong deltaTime = LastTime == 0 ? Godot.Time.GetTicksMsec() - StartTime : Godot.Time.GetTicksMsec() - LastTime;
            float delta = deltaTime / 1000f;
            LastTime = Godot.Time.GetTicksMsec();
            if (IsRunning && !IsCancelled) {
                Progress?.Invoke(delta);
                Intervals++;
                if (Intervals >= Duration) {
                    Complete();
                }
            }

        }

        public void Reset () 
        {
            Intervals = 0;
            IsRunning = false;
            IsCancelled = false;
            Clear?.Invoke();
        }

        // <region> Delegate Methods

        public void Start () {
            ClockDelegate?.Started(this);
            StartTime = Godot.Time.GetTicksMsec();
            LastTime = 0;
            IsRunning = true;
        }

        public void Pause () {
            ClockDelegate?.Paused(this);
            IsRunning = false;
        }

        public void Cancel () {
            ClockDelegate?.Cancelled(this);
            IsRunning = false;
            IsCancelled = true;
        }

        public void Complete () {
            IsRunning = false;
            ClockDelegate?.Completed(this);
        }

        // </region>

    }
}