using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    #region Fields

    [Tooltip("Collection of colors to choose from.")]
    [SerializeField]
    private Color[] colors = default;
    [Tooltip("Stores the hex grid object.")]
    [SerializeField]
    private HexGrid hexGrid = default;
    [Tooltip("Textfields used for showing the active elevationlevel.")]
    [SerializeField]
    private Text elevationLevelText = default;
    [SerializeField]
    private Text brushSizeText = default;

    private enum OptionalToggle
    {
        Ignore, Yes, No
    }

    private OptionalToggle riverMode;               // checks the different modes for applying a river
    private Color activeColor;                      // currently applied color
    private HexDirection dragDirection;             // direction of a drag
    private HexCell previousCell;                   // previous cell during a drag

    private int activeElevation;                    // currently applied elevation 
    private int brushSize;                          // size of the brush to edit cells

    private bool applyColor;                        // should terrain.Colors be applied?
    private bool applyElevation = true;             // should elevation be used?
    private bool isDrag;                            // is there a valid drag?

    #endregion

    #region MonoBehaviors
    private void Awake()
    {
        SelectColor(0);
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
            HandleInput();
        else
            previousCell = null;
    }

    #endregion

    #region Input
    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit) && !EventSystem.current.IsPointerOverGameObject())
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
                ValidateDrag(currentCell);
            else
                isDrag = false;
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else
            previousCell = null;
    }

    /// <summary>
    /// Verify that the current cell is a neighbor of the previous cell,
    /// to determine the validation of a drag
    /// </summary>
    /// <param name="currentCell">current cell</param>
    private void ValidateDrag(HexCell currentCell)
    {
        for(dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            if(previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }

        isDrag = false;
    }

    #endregion

    #region Editing

    /// <summary>
    /// Edit a single cell
    /// </summary>
    /// <param name="cell">cell to edit</param>
    private void EditCell(HexCell cell)
    {
        if(cell)
        {
            if(applyColor)
                cell.Color = activeColor;
            if(applyElevation)
                cell.Elevation = activeElevation;

            if (riverMode == OptionalToggle.No)
                cell.RemoveRiver();
            else if (isDrag && riverMode == OptionalToggle.Yes)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell)
                    otherCell.SetOutgoingRiver(dragDirection);
            }
        }
    }

    /// <summary>
    /// Edits multiple cells at one
    /// </summary>
    /// <param name="center">center of the brush</param>
    private void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for(int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for(int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    /// <summary>
    /// Selects a color based on its index in the list of available terrain.Colors
    /// </summary>
    /// <param name="index">color index</param>
    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if(applyColor)
            activeColor = colors[index];
    }

    /// <summary>
    /// Sets the current elevation to the cell's elevation
    /// </summary>
    /// <param name="elevation"></param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
        elevationLevelText.text = activeElevation.ToString();
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
        brushSizeText.text = brushSize.ToString();
    }

    #endregion

    #region Toggle On/Off

    /// <summary>
    /// Toggles whether elevation should b applied
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /// <summary>
    /// Toggle whether the cell labels should be visible
    /// </summary>
    /// <param name="visible">on or off</param>
    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    /// <summary>
    /// Applies the correct rivermode based on the toggle setting
    /// </summary>
    /// <param name="mode">toggle mode</param>
    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    #endregion
}
