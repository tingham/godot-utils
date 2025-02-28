using Godot;
using System;
using System.Collections.Generic;

namespace URBANFORT.Utilities.Queries
{
    public static class Queries
    {

        /// <summary>
        /// Perform a sphere query to find all targets within the detection radius
        /// </summary>
        /// <param name="origin">The origin of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="mask">The collision mask to use</param>
        /// <returns>An array of the objects within the sphere</returns>
        public static Godot.Collections.Array<Godot.Collections.Dictionary> MultiSphereCast(Node3D target, Vector3 origin, float radius, int mask = 1 << 0 , bool debug = false)
        {
            Rid shapeRid = PhysicsServer3D.SphereShapeCreate();
            PhysicsServer3D.ShapeSetData(shapeRid, radius);
            var queryParams = new PhysicsShapeQueryParameters3D
            {
                ShapeRid = shapeRid,
                Transform = new Transform3D(target.Basis, origin)
            };
            queryParams.Exclude.Add(shapeRid);

            if (debug)
            {
                // TODO: Figure out how to draw debug geometry efficiently
                DebugDraw3D.DrawSphere(origin, radius, Colors.Red, 0.05f);
            }

            var targets = target.GetWorld3D().DirectSpaceState.IntersectShape(queryParams);
            // Free the shape
            PhysicsServer3D.FreeRid(shapeRid);

            return targets;
        }
        /// <summary>
        /// Perform a sphere query to find all targets within the detection radius
        /// </summary>
        /// <param name="origin">The origin of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="mask">The collision mask to use</param>
        /// <returns>An array of the objects within the sphere</returns>
        public static Godot.Collections.Array<Godot.Collections.Dictionary> MultiCubeCast(Node3D target, Vector3 origin, Vector3 dimensions, int mask = 1 << 0 , bool debug = false)
        {
            Rid shapeRid = PhysicsServer3D.BoxShapeCreate();
            PhysicsServer3D.ShapeSetData(shapeRid, dimensions);
            var queryParams = new PhysicsShapeQueryParameters3D
            {
                ShapeRid = shapeRid,
                Transform = new Transform3D(target.Basis, origin)
            };
            queryParams.Exclude.Add(shapeRid);

            if (debug)
            {
                // TODO: Figure out how to draw debug geometry efficiently
                DebugDraw3D.DrawBox(origin, target.Basis.GetRotationQuaternion(), dimensions, Colors.Red, true, 0.05f);
            }

            var targets = target.GetWorld3D().DirectSpaceState.IntersectShape(queryParams);
            // Free the shape
            PhysicsServer3D.FreeRid(shapeRid);

            return targets;
        }

        /// <summary>
        /// Perform a raycast for a single target
        /// </summary>
        /// <param name="origin">The origin of the ray</param>
        /// <param name="direction">The direction of the ray</param>
        /// <param name="length">The length of the ray</param>
        /// <param name="mask">The collision mask to use</param>
        /// <param name="debug">Whether to draw debug geometry</param>
        /// <returns>The target of the raycast</returns>
        public static Node3D RayCast(Node3D target, Vector3 origin, Vector3 direction, float length, uint mask, bool debug)
        {
            var ray = PhysicsRayQueryParameters3D.Create(origin, origin + direction.Normalized() * length, mask, new Godot.Collections.Array<Rid>() { target.GetWorld3D().GetRid() });

            if (debug)
            {
                DebugDraw3D.DrawLine(origin, origin + direction.Normalized() * length, Colors.Red, 0.1f);
            }

            var result = target.GetWorld3D().DirectSpaceState.IntersectRay(ray);

            result.TryGetValue("collider", out var collider);

            return (Node3D)collider;
        }

        public static Node3D RayCast (Node3D target, Vector3 origin, Vector3 destination, uint mask, bool debug)
        {
            var direction = (destination - origin).Normalized();
            var length = (destination - origin).Length();
            return RayCast(target, origin, direction, length, mask, debug);
        }

        /// <summary>
        /// Given a list of targets, convert them to a list of node3ds
        /// </summary>
        /// <param name="targets">The targets to convert</param>
        /// <returns>A list of node3ds</returns>
        public static List<Node3D> ToNode3DList(Godot.Collections.Array<Godot.Collections.Dictionary> targets)
        {
            List<Node3D> nodes = [];
            foreach (Godot.Collections.Dictionary target in targets)
            {
                target.TryGetValue("collider", out var collider);
                var element = (Node3D)collider;
                if (element != null) {
                    nodes.Add(element);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Given a list of targets and a destination class (type), convert them to a list of the destination class
        /// </summary>
        /// <param name="targets">The targets to convert</param>
        /// <returns>A list of the destination class</returns>
        public static List<T> ToList<T>(Godot.Collections.Array<Godot.Collections.Dictionary> targets) where T : Node3D
        {
            List<T> nodes = [];
            foreach (Godot.Collections.Dictionary target in targets)
            {
                target.TryGetValue("collider", out var collider);
                var element = (Node3D)collider;
                if (element != null && element is T someType) {
                    nodes.Add(someType);
                }
                if (element != null && element.GetParent() is T someTypeParent) {
                    nodes.Add(someTypeParent);
                }
            }
            return nodes;
        }
    }
}
