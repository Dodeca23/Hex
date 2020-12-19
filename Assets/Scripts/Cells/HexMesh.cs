using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Takes care o constructing the mesh of a hex cll
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    #region Fields

    private Mesh hexMesh;               // referene to te mesh component
    private List<Vector3> vertices;     // stores the vertices of the mesh
    private List<int> triangles;        // stores the triangles of the mesh
    private List<Color> colors;         // stores the possible colors of the mesh

    private MeshCollider collider;      // reference to the mesh collider component

    #endregion

    #region MonoBehaviors

    private void Awake()
    {
        hexMesh = GetComponent<MeshFilter>().mesh = new Mesh();
        collider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }

    #endregion

    #region Triangulation

    /// <summary>
    /// Triangulates each individual cell of the grid
    /// </summary>
    /// <param name="cells">cells to triangulate</param>
    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        collider.sharedMesh = hexMesh;
    }

    /// <summary>
    /// Triangulates an individual cell in each direction
    /// </summary>
    /// <param name="cell">cell to triangulate</param>
    private void Triangulate(HexCell cell)
    {
        for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }

    }

    /// <summary>
    /// Triangulates a cell in a given direction
    /// </summary>
    /// <param name="direction">direction to triangulate</param>
    /// <param name="cell">cell</param>
    private void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        Triangles.AddTriangle(
                center, v1, v2, vertices, triangles);
        Triangles.AddTriangleColor(cell.color, colors);

        if(direction <= HexDirection.SE)
            TriangulateConnection(direction, cell, v1, v2);

    }

    #endregion

    #region Connections

    /// <summary>
    /// Triangulates a connection by adding the bridge quad
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <param name="cell">current cell</param>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertex</param>
    private void TriangulateConnection(
        HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
            return;

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Elevation * HexMetrics.ELEVATIONSTEP;

        TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if(direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.ELEVATIONSTEP;

            Triangles.AddTriangle(
                v2, v4, v5, vertices, triangles);
            Triangles.AddTriangleColor(
                cell.color, neighbor.color, nextNeighbor.color, colors);
        }
    }

    #endregion

    #region Edges

    /// <summary>
    /// Triangulates the connections of terraces of an edge
    /// </summary>
    /// <param name="beginLeft">startpoint left</param>
    /// <param name="beginRight">startpoint right</param>
    /// <param name="beginCell">start cell</param>
    /// <param name="endLeft">endpoint left</param>
    /// <param name="endRight">endpoint right</param>
    /// <param name="endCell">end cell</param>
    private void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        Quads.AddQuad(beginLeft, beginRight, v3, v4, vertices, triangles);
        Quads.AddQuadColor(beginCell.color, c2, colors);

        for (int i = 2; i < HexMetrics.TERRACESTEPS; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;

            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);

            Quads.AddQuad(v1, v2, v3, v4, vertices, triangles);
            Quads.AddQuadColor(c1, c2, colors);
        }

        Quads.AddQuad(v3, v4, endLeft, endRight, vertices, triangles);
        Quads.AddQuadColor(c2, endCell.color, colors);
    }

    #endregion
}
