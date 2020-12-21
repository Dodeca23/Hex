using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a grid of hexcells
/// </summary>
public class HexGrid : MonoBehaviour
{
    #region Fields

    [Tooltip("Width of the grid.")]
    [SerializeField]
    private int chunkCountX = 4;
    [Tooltip("Height of the grid.")]
    [SerializeField]
    private int chunkCountZ = 3;
    [Tooltip("Prefab of a single hex cell.")]
    [SerializeField]
    private HexCell cellPrefab = default;
    [Tooltip("Prefab of a chunk.")]
    [SerializeField]
    private HexGridChunk chunkPrefab = default;
    [Tooltip("Prefab of a cell label.")]
    [SerializeField]
    private Text cellLabelPrefab = default;
    [Tooltip("Default color of a cell.")]
    [SerializeField]
    private Color defaultColor = default;
    [Tooltip("The noise texture used for adding distortion to the grid of cells.")]
    [SerializeField]
    private Texture2D noiseSource = default;

    private HexGridChunk[] chunks;                      // collection of grid chunks
    private HexCell[] cells;                            // collection of all cells that form a grid
    private int cellCountX;                             // amount of cells along x-axis
    private int cellCountZ;                             // amount of cells along z-axis

    #endregion

    #region MonoBehaviors

    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        cellCountX = chunkCountX * HexMetrics.CHUNKSIZEX;
        cellCountZ = chunkCountZ * HexMetrics.CHUNKSIZEZ;

        CreateChunks();
        CreateCells();

    }

    private void OnEnable()
    {
        // Reassign the noise texture on enabling
        HexMetrics.noiseSource = noiseSource;
    }


    #endregion

    #region Grid Creation

    /// <summary>
    /// Creates a single hex cell
    /// Offset it horizontal and vertical
    /// Create a label for each cell that shows its x and z coordinates
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="z">z position</param>
    /// <param name="i">index</param>
    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.INNERRADIUS * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.OUTERRADIUS * 1.5f);

        HexCell cell = cells[i] = Instantiate(cellPrefab) as HexCell;
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;

        // Make the east-west connection between each cell
        if (x > 0)
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        // Connect the cells in even rows, with the SE neighbor
        if(z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                // Connect the SW neighbors
                if (x > 0)
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
            }
            // And the same for the odd rows
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
            }
        }

        Text label = Instantiate(cellLabelPrefab) as Text;
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeperateLines();
        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }   

    /// <summary>
    /// Adds a single cell to a chunk
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="z">z position</param>
    /// <param name="cell">cell to add</param>
    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.CHUNKSIZEX;
        int chunkZ = z / HexMetrics.CHUNKSIZEZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.CHUNKSIZEX;
        int localZ = z - chunkZ * HexMetrics.CHUNKSIZEZ;

        chunk.AddCell(localX + localZ * HexMetrics.CHUNKSIZEX, cell);
    }

    /// <summary>
    /// Creates all the chunks that form the grid
    /// </summary>
    private void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for(int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for(int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab) as HexGridChunk;
                chunk.transform.SetParent(transform);
            }
        }
    }

    /// <summary>
    /// Create all the cells that form a grid
    /// </summary>
    private void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

    #endregion

}
