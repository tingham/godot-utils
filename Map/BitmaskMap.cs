using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace URBANFORT.Map;

/// <summary>
/// A grid map that uses a bitmask to determine the asset to place on the grid for each mask value.
/// </summary>
[Tool]
public partial class BitmaskMap : GridMap
{
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

    public static readonly Dictionary<int, int> Remap = new()
    {
        { 2, 1 }, { 8, 2 }, { 10, 3 }, { 11, 4 }, { 16, 5 }, { 18, 6 }, { 22, 7 },
        { 24, 8 }, { 26, 9 }, { 27, 10 }, { 30, 11 }, { 31, 12 }, { 64, 13 },
        { 66, 14 }, { 72, 15 }, { 74, 16 }, { 75, 17 }, { 80, 18 }, { 82, 19 },
        { 86, 20 }, { 88, 21 }, { 90, 22 }, { 91, 23 }, { 94, 24 }, { 95, 25 },
        { 104, 26 }, { 106, 27 }, { 107, 28 }, { 120, 29 }, { 122, 30 }, { 123, 31 },
        { 126, 32 }, { 127, 33 }, { 208, 34 }, { 210, 35 }, { 214, 36 }, { 216, 37 },
        { 218, 38 }, { 219, 39 }, { 222, 40 }, { 223, 41 }, { 248, 42 }, { 250, 43 },
        { 251, 44 }, { 254, 45 }, { 255, 46 }, { 0, 47 }
    };

    public static readonly List<int> RemapKeys = new () {
        2, 8, 10, 11, 16, 18, 22, 24, 26, 27, 30, 31, 64, 66, 72, 74, 75, 80, 82, 86, 88, 90, 91, 94, 95, 104, 106, 107, 120, 122, 123, 126, 127, 208, 210, 214, 216, 218, 219, 222, 223, 248, 250, 251, 254, 255, 0
    };

    // 232 should alias to 104

    // 1*0 + 2*1 + 4*1 + 8*0 + 16*1 + 32*1 + 64*0 + 128*1 = 182
    // 1*0 + 2*1 + 4*1 + 8*0 + 16*1 + 32*0 + 64*0 + 128*0 = 22


    [ExportToolButton("Cleanup")]
    public Callable Cleanup => Callable.From(() => Repaint());

    [ExportToolButton("Produce Possible Values")]
    public Callable ProducePossibleValuesButton => Callable.From(() => ProducePossibleValues());

    public void ProducePossibleValues ()
    {
    }

    public void Repaint ()
    {
        // Unset all cells in drawable
        Drawable.Clear();

        // Get a list of cells that have something assigned

        var cells = GetUsedCells();
        foreach (Vector3I cell in cells)
        {
            int filledTotal = 0;

            bool hasFourCorners = false;
            int cornerCount = 0;
            foreach (var corner in new List<Directions>() { Directions.NorthWest, Directions.NorthEast, Directions.SouthWest, Directions.SouthEast })
            {
                var neighbor = GetNeighborCoordinate(cell, corner);
                if (GetCellItem(neighbor) != GridMap.InvalidCellItem)
                {
                    cornerCount++;
                }
            }
            hasFourCorners = cornerCount == 4;

            if (hasFourCorners)
            {
                // For each direction in Directions get the filled status
                foreach (var dir in Directions.GetValues(typeof(Directions)))
                {
                    var neighbor = GetNeighborCoordinate(cell, (Directions)dir);
                    if (GetCellItem(neighbor) != GridMap.InvalidCellItem)
                    {
                        filledTotal += (int)dir;
                    }
                }
            } else {
                foreach (var dir in new List<Directions>() { Directions.North, Directions.East, Directions.South, Directions.West })
                {
                    var neighbor = GetNeighborCoordinate(cell, dir);
                    if (GetCellItem(neighbor) != GridMap.InvalidCellItem)
                    {
                        filledTotal += (int)dir;
                    }
                }
            }

            if (Drawable != null) {
                try
                {
                    int meshItemIndex = Drawable.MeshLibrary.FindItemByName($"{MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");
                    if (meshItemIndex != -1)
                    {
                        GD.Print($"Setting cell at {cell} to {filledTotal}");
                        Drawable.SetCellItem(cell, meshItemIndex);
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

    [Export]
    public GridMap Drawable { get; set; }

    [Export]
    public string MeshLibraryItemPrefix { get; set; } = "";

    [Export]
    public string MeshLibraryItemSuffix { get; set; } = "";

    // In Godot, Z forward (down) is negative
    public Vector3I GetNeighborCoordinate (Vector3I coordinate, Directions direction)
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