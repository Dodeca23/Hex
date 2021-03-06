﻿using UnityEngine;
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
    [SerializeField]
    private Text waterLevelText = default;
    [SerializeField]
    private Text urbanLevelText = default;
    [SerializeField]
    private Text ruralLevelText = default;
    [SerializeField]
    private Text woodLevelText = default;

    private enum OptionalToggle
    {
        Ignore, Yes, No
    }

    private OptionalToggle riverMode;               // checks the different modes for applying a river
    private OptionalToggle roadMode;                // checks the different modes for applying a road

    private Color activeColor;                      // currently applied color
    private HexDirection dragDirection;             // direction of a drag
    private HexCell previousCell;                   // previous cell during a drag

    private int activeElevation;                    // currently applied elevation 
    private int brushSize;                          // size of the brush to edit cells
    private int activeWaterLevel;                   // currently applied waterlevel
    private int activeUrbanLevel;                   // currently applied urbanlevel
    private int activeRuralLevel;                   // currently applied rurallevel
    private int activeWoodLevel;                    // currently applied woodlevel

    private bool applyColor;                        // should terrain.Colors be applied?
    private bool applyElevation = true;             // should elevation be used?
    private bool applyWaterLevel = false;           // should submerging cells be applied?
    private bool applyUrbanLevel = false;           // should the cell contain urban buildings?
    private bool applyRuralLevel = false;           // should the cell contain rural buildings?
    private bool applyWoodLevel = false;            // should the cell contain trees?

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
            if (applyWaterLevel)
                cell.WaterLevel = activeWaterLevel;
            if (applyUrbanLevel)
                cell.UrbanLevel = activeUrbanLevel;
            if (applyRuralLevel)
                cell.RuralLevel = activeRuralLevel;
            if (applyWoodLevel)
                cell.WoodLevel = activeWoodLevel;

            if (riverMode == OptionalToggle.No)
                cell.RemoveRiver();
            if (roadMode == OptionalToggle.No)
                cell.RemoveRoads();
            if(isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell)
                {
                    if(riverMode == OptionalToggle.Yes)
                        otherCell.SetOutgoingRiver(dragDirection);
                    if (roadMode == OptionalToggle.Yes)
                        otherCell.AddRoad(dragDirection);
                }
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

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
        waterLevelText.text = activeWaterLevel.ToString();
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
        urbanLevelText.text = activeUrbanLevel.ToString();
    }

    public void SetRuralLevel(float level)
    {
        activeRuralLevel = (int)level;
        ruralLevelText.text = activeRuralLevel.ToString();
    }

    public void SetWoodLevel(float level)
    {
        activeWoodLevel = (int)level;
        woodLevelText.text = activeWoodLevel.ToString();
    }

    #endregion

    #region Toggle On/Off

    /// <summary>
    /// Toggles whether elevation should be applied
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /// <summary>
    /// Toggles whether cells should be covered with water
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    /// <summary>
    /// Toggles whether cells should contain urban buildings
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    /// <summary>
    /// Toggles whether cells should contain rural buildings
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyRuralLevel(bool toggle)
    {
        applyRuralLevel = toggle;
    }

    /// <summary>
    /// Toggles whether cells should contain trees
    /// </summary>
    /// <param name="toggle">on or off</param>
    public void SetApplyWoodLevel(bool toggle)
    {
        applyWoodLevel = toggle;
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

    /// <summary>
    /// Applies the correct roadmode based on the toggle setting
    /// </summary>
    /// <param name="mode">toggle mode</param>
    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    #endregion
}
