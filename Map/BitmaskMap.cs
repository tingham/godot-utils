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

    public static readonly List<Directions> Corners = [Directions.NorthWest, Directions.NorthEast, Directions.SouthEast, Directions.SouthWest];

    public static readonly List<Directions> Cardinals = [Directions.North, Directions.East, Directions.South, Directions.West];

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
    // 215 should alias to 214
    // 152 shoud alias to 24
    // 56 shoud alias to 24
    // 112 should alias to 80

    public static readonly Dictionary<int, int> Alias = new()
    {
        { 232, 104 }, { 215, 214 }, { 152, 24 }, { 56, 24 }, { 112, 80 }, { 110, 106 }, { 252, 240 },
        { 14, 10 }, { 28, 24 }, { 25, 24 }, { 60, 24 }, { 235, 107 }, { 246, 214 }, { 57, 24 }, { 40, 8 },
        { 15, 11 }, { 116, 80 }, { 62, 30 }, { 195, 66 }, { 46, 10 }, { 84, 80 }, { 102, 66 }, { 20, 16 }
    };


    // 1*0 + 2*1 + 4*1 + 8*0 + 16*1 + 32*1 + 64*0 + 128*1 = 182
    // 1*0 + 2*1 + 4*1 + 8*0 + 16*1 + 32*0 + 64*0 + 128*0 = 22


    [ExportToolButton("Paint")]
    public Callable Cleanup => Callable.From(() => Repaint());

    [Export]
    public bool Annotate { get; set; } = false;

    [ExportToolButton("Produce Possible Values")]
    public Callable ProducePossibleValuesButton => Callable.From(() => ProducePossibleValues());

    public void ProducePossibleValues ()
    {
    }

    public void Repaint ()
    {
        // Unset all cells in drawable
        Drawable.Clear();

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

        // Get a list of cells that have something assigned

        var cells = GetUsedCells();
        foreach (Vector3I cell in cells)
        {
            int filledTotal = 0;

            foreach (var aliasKey in Alias.Keys)
            {
                GD.Print($"{aliasKey} - {Alias[aliasKey]} = {aliasKey - Alias[aliasKey]}");
            }

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
                    // int meshItemIndex = Drawable.MeshLibrary.FindItemByName($"{MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");
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
                    if (Annotate)
                    {
                        // Create a Label3D for the cell at its world position with the text value of filledTotal
                        var label = new Label3D();
                        label.Text = filledTotal.ToString();
                        // Make the font size 22px
                        label.FontSize = 48;
                        Vector3 labelPosition = Drawable.ToGlobal(Drawable.ToLocal(cell)) * Drawable.CellSize;
                        labelPosition.X += 0.25f;
                        labelPosition.Z += 0.25f;
                        labelPosition.Y += 3;
                        label.Transform = new Transform3D(Basis.Identity.Rotated(Vector3.Right, -90f), labelPosition);
                        if (meshItemIndex == -1) {
                            label.Modulate = new Color(1, 0, 0);
                        } else {
                            label.Modulate = new Color(0, 0.5f, 0);
                        }
                        annotationsContainer.AddChild(label);
                        label.SetOwner(annotationsContainer);
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