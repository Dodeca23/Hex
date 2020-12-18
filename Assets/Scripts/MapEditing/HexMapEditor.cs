using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    [Tooltip("Collection of colors to choose from.")]
    [SerializeField]
    private Color[] colors = default;
    [Tooltip("Stores the hex grid object.")]
    [SerializeField]
    private HexGrid hexGrid = default;

    private Color activeColor;

    private void Awake()
    {
        SelectColor(0);
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
            HandleInput();
    }

    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit) && !EventSystem.current.IsPointerOverGameObject())
           hexGrid.ColorCell(hit.point, activeColor);
    }

    /// <summary>
    /// Selects a color based on its index in the list of available colors
    /// </summary>
    /// <param name="index">color index</param>
    public void SelectColor(int index)
    {
        activeColor = colors[index];
    }
}
