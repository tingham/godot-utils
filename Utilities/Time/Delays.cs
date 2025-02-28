using Godot;
using System;

namespace URBANFORT.Utilities.Time
{
    // Static class providing a variety of methods for shifting time around
    public static class Delays {

        public static void DelayAction (Node caller, Action fn, float delay) {
            caller.GetTree().CreateTimer(delay).Connect("timeout", Callable.From(fn));
        }
    }

}