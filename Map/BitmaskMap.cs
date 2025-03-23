using Godot;
using System;
using System.Collections.Generic;

// your namespace will vary
namespace URBANFORT.Map;

public enum Directions
{
    NorthWest = 1,
    North = 2,
    NorthEast = 4,
    East = 16,
    SouthEast = 128,
    South = 64,
    SouthWest = 32,
    West = 8
}

public enum BasisIndex 
{
    NoRotation = 0,
    HalfRotation = 10,
    Negative90 = 16,
    Positive90 = 22,
}

public struct Orientation
{
    public int OriginalMask { get; set; }
    public BasisIndex BasisIndex { get; set; }
}

/// <summary>
/// A grid map that uses a bitmask to determine the asset to place on the grid for each mask value.
/// </summary>
[Tool]
public partial class BitmaskMap : GridMap
{

    public static readonly List<Directions> Corners = [Directions.NorthWest, Directions.NorthEast, Directions.SouthEast, Directions.SouthWest];

    [ExportToolButton("Paint")]
    public Callable Cleanup => Callable.From(() => Repaint());

    // DEBUG: Annotate the grid with the bitmask value
    [Export]
    public bool Annotate { get; set; } = false;

    // The drawable grid map that will be painted
    [Export]
    public GridMap Drawable { get; set; }

    // I named my tiles simply by their bitmask value; but you might want to include SM_ or something
    [Export]
    public string MeshLibraryItemPrefix { get; set; } = "";

    // Same reasoning as the prefix, just wanted a little future-proofing
    [Export]
    public string MeshLibraryItemSuffix { get; set; } = "";

    public Dictionary<int, Orientation> Reorientations { get; set; } = new Dictionary<int, Orientation>() {
        { 8, new Orientation() { OriginalMask = 2, BasisIndex = BasisIndex.Negative90 } },
        { 16, new Orientation() { OriginalMask = 2, BasisIndex = BasisIndex.Positive90 } },
        { 64, new Orientation() { OriginalMask = 2, BasisIndex = BasisIndex.HalfRotation } },
        { 18, new Orientation() { OriginalMask = 10, BasisIndex = BasisIndex.Positive90 } },
        { 72, new Orientation() { OriginalMask = 10, BasisIndex = BasisIndex.Negative90 } },
        { 80, new Orientation() { OriginalMask = 10, BasisIndex = BasisIndex.HalfRotation } },
        { 22, new Orientation() { OriginalMask = 11, BasisIndex = BasisIndex.Positive90 } },
        { 104, new Orientation() { OriginalMask = 11, BasisIndex = BasisIndex.Negative90 } },
        { 208, new Orientation() { OriginalMask = 11, BasisIndex = BasisIndex.HalfRotation } },
        { 66, new Orientation() { OriginalMask = 24, BasisIndex = BasisIndex.Negative90 } },
        { 74, new Orientation() { OriginalMask = 26, BasisIndex = BasisIndex.Negative90 } },
        { 82, new Orientation() { OriginalMask = 26, BasisIndex = BasisIndex.Positive90 } },
        { 88, new Orientation() { OriginalMask = 26, BasisIndex = BasisIndex.HalfRotation } },
        { 120, new Orientation() { OriginalMask = 30, BasisIndex = BasisIndex.HalfRotation } },
        { 216, new Orientation() { OriginalMask = 27, BasisIndex = BasisIndex.HalfRotation } },
        { 248, new Orientation() { OriginalMask = 31, BasisIndex = BasisIndex.HalfRotation } },
        { 210, new Orientation() { OriginalMask = 75, BasisIndex = BasisIndex.HalfRotation } },
        { 106, new Orientation() { OriginalMask = 86, BasisIndex = BasisIndex.HalfRotation } },
        { 122, new Orientation() { OriginalMask = 91, BasisIndex = BasisIndex.Positive90 } },
        { 126, new Orientation() { OriginalMask = 91, BasisIndex = BasisIndex.Positive90 } },
        { 219, new Orientation() { OriginalMask = 91, BasisIndex = BasisIndex.NoRotation } },
        { 218, new Orientation() { OriginalMask = 94, BasisIndex = BasisIndex.Negative90 } },
        { 250, new Orientation() { OriginalMask = 95, BasisIndex = BasisIndex.HalfRotation } },
        { 214, new Orientation() { OriginalMask = 107, BasisIndex = BasisIndex.HalfRotation } },
        { 31, new Orientation() { OriginalMask = 107, BasisIndex = BasisIndex.Positive90 } },
        { 223, new Orientation() { OriginalMask = 127, BasisIndex = BasisIndex.Positive90 } },
        { 251, new Orientation() { OriginalMask = 127, BasisIndex = BasisIndex.Negative90 } },
        { 254, new Orientation() { OriginalMask = 127, BasisIndex = BasisIndex.HalfRotation } },
    };

    public void Repaint ()
    {
        // Unset all cells in drawable
        Drawable.Clear();

        // This has a bug that makes labels appear in all open scenes that I have not worked on - Reloading Godot clears them and they are debug annotations in any case
        Node3D annotationsContainer = GetTree().Root.GetNode<Node3D>("Annotations");
        if (annotationsContainer == null)
        {
            annotationsContainer = new Node3D();
            annotationsContainer.Name = "Annotations";
            GetTree().Root.AddChild(annotationsContainer);
        } else {
            foreach (Node child in annotationsContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        var cells = GetUsedCells();

        foreach (Vector3I cell in cells)
        {
            int filledTotal = 0;

            foreach (Directions dir in Directions.GetValues(typeof(Directions)))
            {
                var neighbor = GetNeighborCoordinate(cell, dir);
                if (GetCellItem(neighbor) == InvalidCellItem)
                {
                    continue;
                }

                if (Corners.Contains(dir))
                {
                    var northNeighbor = GetNeighborCoordinate(cell, Directions.North);
                    var eastNeighbor = GetNeighborCoordinate(cell, Directions.East);
                    var southNeighbor = GetNeighborCoordinate(cell, Directions.South);
                    var westNeighbor = GetNeighborCoordinate(cell, Directions.West);

                    // If dir is northwest and either north or west of that are empty skip
                    if (dir == Directions.NorthWest && (GetCellItem(northNeighbor) == InvalidCellItem || GetCellItem(westNeighbor) == InvalidCellItem))
                    {
                        continue;
                    }

                    // If dir is northeast and either north or east of that are empty skip
                    if (dir == Directions.NorthEast && (GetCellItem(northNeighbor) == InvalidCellItem || GetCellItem(eastNeighbor) == InvalidCellItem))
                    {
                        continue;
                    }

                    // If dir is southeast and either south or east of that are empty skip
                    if (dir == Directions.SouthEast && (GetCellItem(southNeighbor) == InvalidCellItem || GetCellItem(eastNeighbor) == InvalidCellItem))
                    {
                        continue;
                    }

                    // If dir is southwest and either south or west of that are empty skip
                    if (dir == Directions.SouthWest && (GetCellItem(southNeighbor) == InvalidCellItem || GetCellItem(westNeighbor) == InvalidCellItem))
                    {
                        continue;
                    }
                }

                filledTotal += (int)dir;
            }

            if (Drawable != null) {
                try
                {
                    int orientation = 0;
                    if (Reorientations.TryGetValue(filledTotal, out Orientation reorientation))
                    {
                        filledTotal = reorientation.OriginalMask;
                        orientation = (int)reorientation.BasisIndex;
                    }
                    int meshItemIndex = Drawable.MeshLibrary.FindItemByName($"{MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");

                    if (meshItemIndex != -1)
                    {
                        Drawable.SetCellItem(cell, meshItemIndex, orientation);
                    }
                    else
                    {
                        GD.Print($"No item found for cell {cell} {MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error setting cell at {cell}, filledTotal({filledTotal}): {e.Message}");
                    foreach (int index in Drawable.MeshLibrary.GetItemList())
                    {
                        GD.PrintErr($"Item: {index} - {Drawable.MeshLibrary.GetItemName(index)}");
                    }
                }
            }
        }
    }

    // In Godot, Z forward (down) is negative
    public static Vector3I GetNeighborCoordinate (Vector3I coordinate, Directions direction)
    {
        var neighbor = coordinate;
        switch (direction)
        {
            case Directions.NorthWest:
                neighbor.X -= 1;
                neighbor.Z -= 1;
                break;
            case Directions.North:
                neighbor.Z -= 1;
                break;
            case Directions.NorthEast:
                neighbor.X += 1;
                neighbor.Z -= 1;
                break;
            case Directions.East:
                neighbor.X += 1;
                break;
            case Directions.SouthEast:
                neighbor.X += 1;
                neighbor.Z += 1;
                break;
            case Directions.South:
                neighbor.Z += 1;
                break;
            case Directions.SouthWest:
                neighbor.X -= 1;
                neighbor.Z += 1;
                break;
            case Directions.West:
                neighbor.X -= 1;
                break;
        }
        return neighbor;
    }



}