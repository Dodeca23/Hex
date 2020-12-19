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

        // Only create terraces for sloped edges
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        else
        {
            Quads.AddQuad(v1, v2, v3, v4, vertices, triangles);
            Quads.AddQuadColor(cell.color, neighbor.color, colors);
        }


        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if(direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.ELEVATIONSTEP;

            // First, determine what the bottom cell is
            //  check whether the cell being triangulated is lower than its neighbors, or tied for lowest. 
            // If this is the case, we can use it as the bottom cell.
            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                // If the innermost check fails, it means that the next neighbor is the lowest cell. 
                // Rotate the triangle counterclockwise to keep it correctly oriented.
                else
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }
            // If the edge neighbor is the lowest, then we have to rotate clockwise...
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            ///...otherwise, rotate counterclockwise
            else
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
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

    #region Corners

    /// <summary>
    /// Triangulates the corners at the side of edges
    /// </summary>
    /// <param name="bottom">bottom of the triangle</param>
    /// <param name="bottomCell">cell at the bottom</param>
    /// <param name="left">left of the triangle</param>
    /// <param name="leftCell">cell to the left</param>
    /// <param name="right">right of the triangle</param>
    /// <param name="rightCell">cell to the right</param>
    private void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        // If both edges are slopes, then we have terraces on both the left and the right side.
        // Also, because the bottom cell is the lowest, we know that those slopes go up. 
        // Furthermore, this means that the left and right cell have the same elevation, 
        // so the top edge connection is flat. We can identify this case as slope-slope-flat.
        if (leftEdgeType == HexEdgeType.Slope)
        {
            if(rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }
            // If the right edge is flat, then we have to begin terracing from the left instead of the bottom
            if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                return;
            }
        }
        // If the left edge is flat, then we have to begin from the right.
        if (rightEdgeType == HexEdgeType.Slope)
        {
            if(leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
        }

        Triangles.AddTriangle(bottom, left, right, vertices, triangles);
        Triangles.AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color, colors);
    }

    /// <summary>
    /// Triangulates a slope-slope-flat,
    /// a terrace between two sloped cells
    /// </summary>
    /// <param name="begin">begin of the triangle</param>
    /// <param name="beginCell">begin cell</param>
    /// <param name="left">left of the triangle</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="right">right of the triangle</param>
    /// <param name="rightCell">right cell</param>
    private void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

        Triangles.AddTriangle(begin, v3, v4, vertices, triangles);
        Triangles.AddTriangleColor(beginCell.color, c3, c4, colors);

        for(int i = 2; i < HexMetrics.TERRACESTEPS; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);

            Quads.AddQuad(v1, v2, v3, v4, vertices, triangles);
            Quads.AddQuadColor(c1, c2, c3, c4, colors);
        }

        Quads.AddQuad(v3, v4, left, right, vertices, triangles);
        Quads.AddQuadColor(c3, c4, leftCell.color, rightCell.color, colors);
    }

    #endregion
}
