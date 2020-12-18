using UnityEngine;

/// <summary>
/// Represents a single hexagonal grid cell
/// </summary>
public class HexCell : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private HexCell[] neighbors;

    public HexCoordinates coordinates;                          // stores the coordinate of a cell
    public Color color;                                         // stores the color of a cell
    public RectTransform uiRect;                               // stores the ui label of cell

    private int elevation;                                      // stores the elevationlevel of a cell

    #endregion

    #region Properties

    /// <summary>
    /// Gets and sets the elevation of a cell
    /// </summary>
    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.ELEVATIONSTEP;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -HexMetrics.ELEVATIONSTEP;
            uiRect.localPosition = uiPosition;
        }
    }

    #endregion

    #region Neighbor Cells

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

    #endregion
}
