using UnityEngine;

/// <summary>
/// Represents a single hexagonal grid cell
/// </summary>
public class HexCell : MonoBehaviour
{
    [SerializeField]
    private HexCell[] neighbors;

    public HexCoordinates coordinates;                          // stores the coordinate of a cell
    public Color color;                                         // stores the color of a cell
    public int elevation;                                       // stores the elevationlevel of a cell

    /// <summary>
    /// Retrieves a cell's neighbor in one direction
    /// </summary>
    /// <param name="direction">direction of neighbor</param>
    /// <returns></returns>
    public HexCell GetNeighbor(HexDirection direction) =>
        neighbors[(int)direction];

    /// <summary>
    /// Sets the cell's neighbor in one direction
    /// </summary>
    /// <param name="direction">direction of neighbor</param>
    /// <param name="cell">cell</param>
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
}
