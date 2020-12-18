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
    private int width = default;
    [Tooltip("Height of the grid.")]
    [SerializeField]
    private int height = default;
    [Tooltip("Prefab of a single hex cell.")]
    [SerializeField]
    private HexCell cellPrefab = default;
    [Tooltip("Prefab of a cell label.")]
    [SerializeField]
    private Text cellLabelPrefab = default;
    [Tooltip("Default color of a cell.")]
    [SerializeField]
    private Color defaultColor = default;

    private Canvas gridCanvas;                          // reference to the child's canvas component
    private HexMesh hexMesh;                            // reference to the hexmesh component
    private HexCell[] cells;                            // collection of all cells that form a grid

    #endregion

    #region MonoBehaviors

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        // Create a cell on each grid position
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }

    }
    private void Start()
    {
        hexMesh.Triangulate(cells);
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
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        // Make the east-west connection between each cell
        if (x > 0)
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        // Connect the cells in even rows, with the SE neighbor
        if(z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                // Connect the SW neighbors
                if (x > 0)
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
            }
            // And the same for the odd rows
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
            }
        }

        Text label = Instantiate(cellLabelPrefab) as Text;
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeperateLines();
    }   

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        return cells[index];
    }

    public void Refresh()
    {
        hexMesh.Triangulate(cells);
    }

    #endregion

}
