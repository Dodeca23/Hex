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
    public HexGridChunk chunk;

    private int elevation = int.MinValue;                     // stores the elevationlevel of a cell

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
            if (elevation == value)
                return;

            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.ELEVATIONSTEP;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.ELEVATIONPERTURBSTRENGTH;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            Refresh();
        }
    }

    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    /// <summary>
    /// Retrieves the position of a cell
    /// </summary>
    public Vector3 Position => transform.localPosition;

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

    /// <summary>
    /// Returns the edgetype between two neighboring cells
    /// </summary>
    /// <param name="direction">direction of neighbor</param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexDirection direction) =>
        HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);

    /// <summary>
    /// Returns the edgetype between any two cells
    /// </summary>
    /// <param name="othercell">other cell</param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexCell othercell) =>
        HexMetrics.GetEdgeType(elevation, othercell.elevation);

    #endregion

    void Refresh()
    {
        if(chunk)
            chunk.Refresh();

        for (int i = 0; i < neighbors.Length; i++)
        {
            HexCell neighbor = neighbors[i];
            if (neighbor != null && neighbor.chunk != chunk)
            {
                neighbor.chunk.Refresh();
            }
        }
    }
}

