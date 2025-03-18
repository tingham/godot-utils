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
                    int meshItemIndex = Drawable.MeshLibrary.FindItemByName($"{MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");

                    if (meshItemIndex != -1)
                    {
                        Drawable.SetCellItem(cell, meshItemIndex);
                    }
                    else
                    {
                        GD.Print($"No item found for cell {cell} {MeshLibraryItemPrefix}{filledTotal}{MeshLibraryItemSuffix}");
                    }
                    if (Annotate)
                    {
                        var label = new Label3D
                        {
                            Text = filledTotal.ToString(),
                            FontSize = 48
                        };
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