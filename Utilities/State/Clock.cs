using System;
using System.Runtime.Serialization.Formatters;

namespace URBANFORT.Utilities.State
{
    public class Clock {
        public float TimerBase { get; set; }
        public float Timer { get; set; }
        public float Time { get; set; }
        public bool AutomaticallyReset { get; set; }

        public delegate void Expired (Clock clock);
        public event Expired OnExpiredEvent;
    
        public void Update (float delta) {
            Timer += delta;
            if (Timer >= TimerBase) {

                OnExpiredEvent?.Invoke(this);

                if (AutomaticallyReset)
                {
                    Reset();
                }
            }
        }

        public void Reset () {
            Timer = TimerBase;
        }

        public void Degrade (float newBase) {
            TimerBase = newBase;
            Timer = TimerBase;
        }
    }
}