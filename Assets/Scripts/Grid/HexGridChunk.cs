using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
    private HexCell[] cells;
    private HexMesh hexMesh;
    private Canvas gridCanvas;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.CHUNKSIZEX * HexMetrics.CHUNKSIZEZ];
    }

    private void LateUpdate()
    {
        hexMesh.Triangulate(cells);
        enabled = true;
    }

    /// <summary>
    /// Adds the cell to itself to keep track of it
    /// </summary>
    /// <param name="index">index of the cell</param>
    /// <param name="cell">cell to add</param>
    public void AddCell(int index, HexCell cell)
    {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    public void Refresh()
    {
        hexMesh.Triangulate(cells);
        enabled = false;
    }
}