using Godot;
using System;
using System.Diagnostics;

namespace URBANFORT.Utilities.Data
{
    public struct Damage
    {
        public float Value { get; set; }
        public Node Source { get; set; }

        public Damage(float value, Node source)
        {
            Value = value;
            Source = source;
        }

        public static Damage SimpleDamage(Node source, float value)
        {
            return new Damage(value, source);
        }
    }
}