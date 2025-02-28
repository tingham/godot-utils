using Godot;
using System;

namespace URBANFORT.Utilities.Interfaces
{
    public interface IKillable
    {
        public delegate void Killed(Node sender);
        public event Killed OnKilled;

        void Kill();
    }
}