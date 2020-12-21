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

    private Color activeColor;                      // currently applied color
    private int activeElevation;                    // currently applied elevation 

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
    }

    #endregion

    #region Input
    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit) && !EventSystem.current.IsPointerOverGameObject())
            EditCell(hexGrid.GetCell(hit.point));
    }

    #endregion

    #region Editing

    private void EditCell(HexCell cell)
    {
        cell.Color = activeColor;
        cell.Elevation = activeElevation;
    }

    /// <summary>
    /// Selects a color based on its index in the list of available colors
    /// </summary>
    /// <param name="index">color index</param>
    public void SelectColor(int index)
    {
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

    #endregion
}
