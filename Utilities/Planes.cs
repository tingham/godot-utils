using Godot;
using System;

namespace GodotUtils.Utilities
{
    public static class Planes
    {
        public static Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
        {
            return vector - planeNormal * vector.Dot(planeNormal);
        }

        public static Vector3 ReflectVector(Vector3 vector, Vector3 normal)
        {
            return vector - 2 * vector.Dot(normal) * normal;
        }

        public static Vector3 SameVerticalPlane(Vector3 source, Vector3 target)
        {
            return new Vector3(target.X, source.Y, target.Z);
        }
    }
}