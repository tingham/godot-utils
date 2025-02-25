using System;
using URBANFORT.Utilities.Time;

namespace URBANFORT.Utilities.Interfaces
{
    public interface IClockDelegate
    {
        public void Started(Clock sender);
        public void Paused(Clock sender);
        public void Cancelled(Clock sender);
        public void Completed(Clock sender);
    }
}