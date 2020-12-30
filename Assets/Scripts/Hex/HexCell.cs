using UnityEngine;

/// <summary>
/// Represents a single hexagonal grid cell
/// </summary>
public class HexCell : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private HexCell[] neighbors = default;                      // stores the neighbors of a cell
    [SerializeField]
    private bool[] roads = default;                             // keeps track of the roads of a cell

    public HexCoordinates coordinates;                          // stores the coordinate of a cell
    public Color color;                                         // stores the color of a cell
    public RectTransform uiRect;                                // stores the ui label of cell
    public HexGridChunk chunk;

    private int elevation = int.MinValue;                       // stores the elevationlevel of a cell
    private int waterLevel;                                     // stores the elevation of the water surface
    private int urbanLevel;                                     // stores the level of urban buildings
    private int ruralLevel;                                     // stores the level of rural buildings
    private int woodLevel;                                      // stores the level of trees

    private bool hasIncomingRiver;                              // does the cell contain an incoming river?
    private bool hasOutgoingRiver;                              // does the cell contain an outgoing river?
    private HexDirection incomingRiver;                         // direction of incoming river
    private HexDirection outgoingRiver;                         // direction of outgoing river

    #endregion

    #region Properties

    #region Cell Data
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

            ValidateRivers();

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                    SetRoad(i, false);
            }

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

    #endregion

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

    /// <summary>
    /// Retrieves the vertical position of the cell's streambed
    /// </summary>
    public float StreamBedY =>
        (elevation + HexMetrics.STREAMBEDELEVATIONOFFSET) * HexMetrics.ELEVATIONSTEP;

    /// <summary>
    /// Returns the direction of the in- or outgoing river
    /// </summary>
    public HexDirection RiverBeginOrEndDirection =>
        hasIncomingRiver ? incomingRiver : outgoingRiver;

    /// <summary>
    /// Retrieves the vertical position of the river surface
    /// </summary>
    public float RiverSurfaceY =>
        (elevation + HexMetrics.WATERSURFACEELEVATIONOFFSET) * HexMetrics.ELEVATIONSTEP;

    #endregion

    #region Roads

    /// <summary>
    /// Returns true when a cell contains at least one road
    /// </summary>
    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                    return true;
            }

            return false;
        }
    }

    #endregion

    #region Water

    /// <summary>
    /// Gets and sets the elevation of the water surface
    /// </summary>
    public int WaterLevel
    {
        get => waterLevel;
        set
        {
            if (waterLevel == value)
                return;

            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    /// <summary>
    /// Returns true when a cell's elevation is less than a cell waterlevel
    /// </summary>
    public bool IsUnderwater =>
        waterLevel > elevation;

    /// <summary>
    /// Returns the watersurface of a submerged cell
    /// </summary>
    public float WaterSurfaceY =>
        (waterLevel + HexMetrics.WATERSURFACEELEVATIONOFFSET) * HexMetrics.ELEVATIONSTEP;

    #endregion

    #region Features

    /// <summary>
    /// Gets and sets the cell's level of urban buildings
    /// </summary>
    public int UrbanLevel
    {
        get => urbanLevel;
        set
        {
            if (urbanLevel != value)
                urbanLevel = value;

            RefreshSelfOnly();
        }
    }

    /// <summary>
    ///  Gets and sets the cell's level of rural buildings
    /// </summary>
    public int RuralLevel
    {
        get => ruralLevel;
        set
        {
            if (ruralLevel != value)
                ruralLevel = value;

            RefreshSelfOnly();
        }
    }

    /// <summary>
    /// Gets and sets the cell's level of wood
    /// </summary>
    public int WoodLevel
    {
        get => woodLevel;
        set
        {
            if (woodLevel != value)
                woodLevel = value;

            RefreshSelfOnly();
        }
    }

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

    /// <summary>
    /// Returns the absolute elevationdifference between the cell
    /// and a neighbor in a given direction
    /// </summary>
    /// <param name="direction">direction of neighbor</param>
    /// <returns></returns>
    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }

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
        if (!IsValidRiverDestination(neighbor)) return;

        // Clear the previous outgoing river and the incomingriver when it
        // overlaps with the newly created outgoing river
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
            RemoveIncomingRiver();

        hasOutgoingRiver = true;
        outgoingRiver = direction;

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();

        SetRoad((int)direction, false);
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

    /// <summary>
    /// Checks whether a cell is a valid destination for a river
    /// </summary>
    /// <param name="neighbor">neighbor cell</param>
    /// <returns></returns>
    private bool IsValidRiverDestination(HexCell neighbor) =>
        neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);

    /// <summary>
    /// Validates the rivers when changing either the elevation or water level.
    /// </summary>
    private void ValidateRivers()
    {
        if (hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(outgoingRiver)))
            RemoveOutgoingRiver();
        if (hasIncomingRiver && GetNeighbor(incomingRiver).IsValidRiverDestination(this))
            RemoveIncomingRiver();
    }

    #endregion

    #region Roads

    /// <summary>
    /// Returns whether a cell has a road through an edge in a given direction
    /// </summary>
    /// <param name="direction">direction of edge</param>
    /// <returns></returns>
    public bool HasRoadThroughEdge(HexDirection direction) =>
        roads[(int)direction];

    /// <summary>
    /// Remove all roads from a cell
    /// </summary>
    public void RemoveRoads()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    /// <summary>
    /// Adds a road to a cell in a given direction
    /// </summary>
    /// <param name="direction">direction</param>
    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            GetElevationDifference(direction) <= 1)
        {
            SetRoad((int)direction, true);
        }
    }

    /// <summary>
    /// Removes or adds a road depending of the state
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="state">state (add or remove)</param>
    private void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
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

