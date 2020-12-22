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
    public RectTransform uiRect;                                // stores the ui label of cell
    public HexGridChunk chunk;

    private int elevation = int.MinValue;                       // stores the elevationlevel of a cell

    private bool hasIncomingRiver;                              // does the cell contain an incoming river?
    private bool hasOutgoingRiver;                              // does the cell contain an outgoing river?
    private HexDirection incomingRiver;                         // direction of incoming river
    private HexDirection outgoingRiver;                         // direction of outgoing river

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

            if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
                RemoveOutgoingRiver();
            if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
                RemoveIncomingRiver();

            Refresh();
        }
    }

    /// <summary>
    /// Gets and sets the color of a cell
    /// </summary>
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

    #region Rivers

    /// <summary>
    /// Returns whether a cell has an outgoing river
    /// </summary>
    public bool HasOutgoingRiver => hasOutgoingRiver;

    /// <summary>
    /// Returns whether a cell has an incoming river
    /// </summary>
    public bool HasIncomingRiver => hasIncomingRiver;

    /// <summary>
    /// Returns the direction of an incoming river
    /// </summary>
    public HexDirection IncomingRiver => incomingRiver;

    /// <summary>
    /// Returns the direction of an outgoing river
    /// </summary>
    public HexDirection OutgoingRiver => outgoingRiver;

    /// <summary>
    /// Returns whether a cell contains a river
    /// </summary>
    public bool HasRiver => hasIncomingRiver || hasOutgoingRiver;

    /// <summary>
    /// Returns whether the cell has a begin or endpoint for a river
    /// </summary>
    public bool HasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;
    
    #endregion

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

    #region Rivers

    /// <summary>
    /// Returns whether a river flows through a given edge
    /// </summary>
    /// <param name="direction">edge to check</param>
    /// <returns></returns>
    public bool HasRiverThroughEdge(HexDirection direction) =>
        hasIncomingRiver && incomingRiver == direction ||
        hasOutgoingRiver && outgoingRiver == direction;
    
    /// <summary>
    /// Sets the outgoing river of a cell through an edge in a given direction
    /// </summary>
    /// <param name="direction">direction of edge</param>
    public void SetOutgoingRiver(HexDirection direction)
    {
        // If there already is a river, do nothing
        if (hasOutgoingRiver && outgoingRiver == direction) return;

        // If there doesn't exist a neighbor or
        // when the neighbor has a higher elevation, do nothing
        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation) return;

        // Clear the previous outgoing river and the incomingriver when it
        // overlaps with the newly created outgoing river
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
            RemoveIncomingRiver();

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }

    /// <summary>
    /// Removes the outgoing part of a river from a cell
    /// </summary>
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver) return;

        hasOutgoingRiver = false;
        RefreshSelfOnly();

        // Also remove the incoming part of the river from the neighbor
        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    /// <summary>
    /// Removes the incoming part of a river from a cell
    /// </summary>
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver) return;

        hasIncomingRiver = false;
        RefreshSelfOnly();

        // Also remove the outgoing part of the river from the neighbor
        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    /// <summary>
    /// Removes the entire river from a cell
    /// </summary>
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    #endregion

    #region Updating

    void Refresh()
    {
        if(chunk)
        {
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

    public void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    #endregion
}

