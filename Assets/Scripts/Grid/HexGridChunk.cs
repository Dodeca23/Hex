using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private HexMesh terrain = default;
    [SerializeField]
    private HexMesh rivers = default;

    private HexCell[] cells;
    private Canvas gridCanvas;

    #endregion

    #region MonoBehaviors

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        // hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.CHUNKSIZEX * HexMetrics.CHUNKSIZEZ];
        ShowUI(false);
    }

    private void LateUpdate()
    {
        Triangulate();
        enabled = true;
    }

    #endregion

    #region Chunks

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
        Triangulate();
        enabled = false;
    }

    public void ShowUI(bool visible)
    {
        gridCanvas.gameObject.SetActive(visible);
    }

    #endregion

    #region Triangulation

    /// <summary>
    /// Triangulates each individual cell of the grid
    /// </summary>
    public void Triangulate()
    {
        terrain.Clear();
        rivers.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        terrain.Apply();
        rivers.Apply();
    }

    /// <summary>
    /// Triangulates an individual cell in each direction
    /// </summary>
    /// <param name="cell">cell to triangulate</param>
    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
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
        Vector3 center = cell.Position;
        EdgeVertices e = new EdgeVertices(
        center + HexMetrics.GetFirstSolidCorner(direction),
        center + HexMetrics.GetSecondSolidCorner(direction));

        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction))
            {
                e.v3.y = cell.StreamBedY;

                if (cell.HasRiverBeginOrEnd)
                    TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                else
                    TriangulateWithRiver(direction, cell, center, e);
            }
            else
                TriangulateAdjacentToRiver(direction, cell, center, e);
        }
        else
            TriangulateEdgeFan(center, e, cell.Color);

        if (direction <= HexDirection.SE)
            TriangulateConnection(direction, cell, e);
    }

    #endregion

    #region Connections

    /// <summary>
    /// Triangulates a connection by adding the bridge quad
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <param name="cell">current cell</param>
    /// <param name="e1">edge to triangulate</param>
    private void TriangulateConnection(
        HexDirection direction, HexCell cell,
        EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
            return;

        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.Position.y - cell.Position.y;
        EdgeVertices e2 = new EdgeVertices(
            e1.v1 + bridge, e1.v5 + bridge);

        if (cell.HasRiverThroughEdge(direction))
        {
            e2.v3.y = neighbor.StreamBedY;
            TriangulateRiverQuad(
                e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                cell.HasIncomingRiver &&
                cell.IncomingRiver == direction);
        }

        // Only create terraces for sloped edges
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        else
            TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            // First, determine what the bottom cell is
            //  check whether the cell being triangulated is lower than its neighbors, or tied for lowest. 
            // If this is the case, we can use it as the bottom cell.
            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                // If the innermost check fails, it means that the next neighbor is the lowest cell. 
                // Rotate the triangle counterclockwise to keep it correctly oriented.
                else
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
            // If the edge neighbor is the lowest, then we have to rotate clockwise...
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            ///...otherwise, rotate counterclockwise
            else
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
        }
    }

    /// <summary>
    /// Triangulates the connection between terraces and a cliff
    /// </summary>
    /// <param name="begin">begin of the triangle</param>
    /// <param name="beginCell">begin cell</param>
    /// <param name="left">left of the triangle</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="boundary">boundary point along the cliff</param>
    /// <param name="boundaryColor">color of boundary</param>
    private void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = CustomMesh.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        Triangles.AddTriangleUnperturbed(CustomMesh.Perturb(begin), v2, boundary, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(beginCell.Color, c2, boundaryColor, terrain.Colors);

        for (int i = 2; i < HexMetrics.TERRACESTEPS; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = CustomMesh.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);

            Triangles.AddTriangleUnperturbed(v1, v2, boundary, terrain.Vertices, terrain.Triangles);
            Triangles.AddTriangleColor(c1, c2, boundaryColor, terrain.Colors);
        }

        Triangles.AddTriangleUnperturbed(v2, CustomMesh.Perturb(left), boundary, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(c2, leftCell.Color, boundaryColor, terrain.Colors);
    }

    #endregion

    #region Edges

    /// <summary>
    /// Triangulates the connections of terraces of an edge
    /// </summary>
    /// <param name="begin"> starting edge</param>
    /// <param name="beginCell">start cell</param>
    /// <param name="end">target edge</param>
    /// <param name="endCell">end cell</param>
    private void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        for (int i = 2; i < HexMetrics.TERRACESTEPS; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;

            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);

            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        TriangulateEdgeStrip(e2, c2, end, endCell.Color);
    }

    /// <summary>
    /// Triangulates a fan between a cells center and one of its edges
    /// </summary>
    /// <param name="center">center of the cell</param>
    /// <param name="edge">edge</param>
    /// <param name="color">color</param>
    private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        Triangles.AddTriangle(center, edge.v1, edge.v2, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(color, terrain.Colors);
        Triangles.AddTriangle(center, edge.v2, edge.v3, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(color, terrain.Colors);
        Triangles.AddTriangle(center, edge.v3, edge.v4, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(color, terrain.Colors);
        Triangles.AddTriangle(center, edge.v4, edge.v5, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(color, terrain.Colors);
    }

    /// <summary>
    /// Tringulates a strip of quads between two edges
    /// </summary>
    /// <param name="e1">first edge</param>
    /// <param name="c1">first color</param>
    /// <param name="e2">second edge</param>
    /// <param name="c2">second color</param>
    private void TriangulateEdgeStrip(
        EdgeVertices e1, Color c1,
        EdgeVertices e2, Color c2)
    {
        Quads.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(c1, c2, terrain.Colors);
        Quads.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(c1, c2, terrain.Colors);
        Quads.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(c1, c2, terrain.Colors);
        Quads.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(c1, c2, terrain.Colors);
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
            if (rightEdgeType == HexEdgeType.Slope)
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            // If the right edge is flat, then we have to begin terracing from the left instead of the bottom
            else if (rightEdgeType == HexEdgeType.Flat)
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            // If the right isn't a slope or a flat, it's a cliff
            else
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
        }
        // If the left edge is flat, then we have to begin from the right.
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            // Otherwise do it the other way around
            else
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
        }
        // Triangulate the cliff-cliff-slope versions
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            else
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
        }
        else
        {
            Triangles.AddTriangle(bottom, left, right, terrain.Vertices, terrain.Triangles);
            Triangles.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color, terrain.Colors);
        }
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

        Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

        Triangles.AddTriangle(begin, v3, v4, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(beginCell.Color, c3, c4, terrain.Colors);

        for (int i = 2; i < HexMetrics.TERRACESTEPS; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);

            Quads.AddQuad(v1, v2, v3, v4, terrain.Vertices, terrain.Triangles);
            Quads.AddQuadColor(c1, c2, c3, c4, terrain.Colors);
        }

        Quads.AddQuad(v3, v4, left, right, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color, terrain.Colors);
    }

    /// <summary>
    /// Triangulates a slope-cliff edge, with the cliff on the right
    /// </summary>
    /// <param name="begin">begin of the triangle</param>
    /// <param name="beginCell">begin cell</param>
    /// <param name="left">left of the triangle</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="right">right of the triangle</param>
    /// <param name="rightCell">right cell</param>
    private void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        // The bottom part
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
            b = -b;
        Vector3 boundary = Vector3.Lerp(CustomMesh.Perturb(begin), CustomMesh.Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        // The top part
        // If we have a slope, add a rotated boundary triangle.
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        // Otherwise, add a triangle
        else
        {
            Triangles.AddTriangleUnperturbed(CustomMesh.Perturb(left), CustomMesh.Perturb(right), boundary, terrain.Vertices, terrain.Triangles);
            Triangles.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor, terrain.Colors);
        }
    }

    /// <summary>
    /// Triangulates a slope-cliff edge, with the cliff on the left
    /// </summary>
    /// <param name="begin">begin of the triangle</param>
    /// <param name="beginCell">begin cell</param>
    /// <param name="left">left of the triangle</param>
    /// <param name="leftCell">left cell</param>
    /// <param name="right">right of the triangle</param>
    /// <param name="rightCell">right cell</param>
    private void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        // The bottom part
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
            b = -b;
        Vector3 boundary = Vector3.Lerp(CustomMesh.Perturb(begin), CustomMesh.Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

        // The top part
        // If we have a slope, add a rotated boundary triangle.
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        // Otherwise, add a triangle
        else
        {
            Triangles.AddTriangleUnperturbed(CustomMesh.Perturb(left), CustomMesh.Perturb(right), boundary, terrain.Vertices, terrain.Triangles);
            Triangles.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor, terrain.Colors);
        }
    }

    #endregion

    #region River

    /// <summary>
    /// Triangulates a cell with a river
    /// </summary>
    /// <param name="direction">direction</param>
    /// <param name="cell">cell</param>
    /// <param name="center">center of cell</param>
    /// <param name="e">edge</param>
    private void TriangulateWithRiver(
        HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        Vector3 centerL, centerR;

        if (cell.HasRiverThroughEdge(direction.Opposite()))
        {
            centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
            centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
        }
        else if (cell.HasRiverThroughEdge(direction.Next()))
        {
            centerL = center;
            centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
            centerR = center;
        }
        else if (cell.HasRiverThroughEdge(direction.Next2()))
        {
            centerL = center;
            centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) *
                (0.5f * HexMetrics.INNERTOOUTER);
        }
        else
        {
            centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) *
                (0.5f * HexMetrics.INNERTOOUTER);
            centerR = center;
        }

        center = Vector3.Lerp(centerL, centerR, 0.5f);

        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(centerL, e.v1, 0.5f),
            Vector3.Lerp(centerR, e.v5, 0.5f),
            1f / 6f);

        m.v3.y = center.y = e.v3.y;

        TriangulateEdgeStrip(m, cell.color, e, cell.color);

        Triangles.AddTriangle(centerL, m.v1, m.v2, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(cell.color, terrain.Colors);
        Quads.AddQuad(centerL, center, m.v2, m.v3, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(cell.color, terrain.Colors);
        Quads.AddQuad(center, centerR, m.v3, m.v4, terrain.Vertices, terrain.Triangles);
        Quads.AddQuadColor(cell.color, terrain.Colors);
        Triangles.AddTriangle(centerR, m.v4, m.v5, terrain.Vertices, terrain.Triangles);
        Triangles.AddTriangleColor(cell.color, terrain.Colors);

        bool reversed = cell.IncomingRiver == direction;
        TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
        TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
    }

    /// <summary>
    /// Triangulates a cell that only has the start or end of a river
    /// </summary>
    /// <param name="direction">direction</param>
    /// <param name="cell">cell</param>
    /// <param name="center">center of cell</param>
    /// <param name="e">edge</param>
    private void TriangulateWithRiverBeginOrEnd(
        HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f));

        m.v3.y = e.v3.y;

        TriangulateEdgeStrip(m, cell.color, e, cell.color);
        TriangulateEdgeFan(center, m, cell.color);

        bool reversed = cell.HasIncomingRiver;
        TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);

        center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
        Triangles.AddTriangle(center, m.v2, m.v4, rivers.Vertices, rivers.Triangles);
        if (reversed)
            Triangles.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f), rivers.Uvs);
        else
            Triangles.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.6f), new Vector2(1f, 0.6f), rivers.Uvs);
    }

    /// <summary>
    /// Triangulate te cell part directly next to a river
    /// </summary>
    /// <param name="direction">direction</param>
    /// <param name="cell">cell</param>
    /// <param name="center">center of cell</param>
    /// <param name="e">edge</param>
    private void TriangulateAdjacentToRiver(
        HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        // Check whether we're on the inside of a curve
        // If so, move the center towards the edge.
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
                center += HexMetrics.GetSolidEdgeMiddle(direction) * (HexMetrics.INNERTOOUTER * 0.5f);
            // check whether it's a straight river. If so, move the center towards our first solid corner.
            else if (cell.HasRiverThroughEdge(direction.Previous2()))
                center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
        }
        // When we have a river in the previous direction, and it is a straight one, 
        // it required moving the center towards the next solid corner.
        else if (
            cell.HasRiverThroughEdge(direction.Previous()) &&
            cell.HasRiverThroughEdge(direction.Next2()))
        {
            center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
        }

        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f));

        TriangulateEdgeStrip(m, cell.color, e, cell.color);
        TriangulateEdgeFan(center, m, cell.color);
    }

    /// <summary>
    /// Triangulates a single river quad
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertex</param>
    /// <param name="v3">third vertex</param>
    /// <param name="v4">fourth vertex</param>
    /// <param name="y1">first vertical position</param>
    /// <param name="y2">second vertical position</param>
    /// <param name="v">V coordinate</param>
    /// <param name="reversed">is the riverflow reversed?</param>
    private void TriangulateRiverQuad(
        Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed)
    {
        // Elevation of each vertex is the same 
        v1.y = v2.y = y1;
        v3.y = v4.y = y2;
        Quads.AddQuad(v1, v2, v3, v4, rivers.Vertices, rivers.Triangles);
        if (reversed)
            Quads.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v, rivers.Uvs);
        else
            Quads.AddQuadUV(0f, 1f, v, v + 0.2f, rivers.Uvs);
    }

    /// <summary>
    /// Triangulates a single river quad with no elevationdifference
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertex</param>
    /// <param name="v3">third vertex</param>
    /// <param name="v4">fourth vertex</param>
    /// <param name="y">vertical position</param>
    /// <param name="v">V coordinate</param>
    /// <param name="reversed">is the riverflow reversed?</param>
    private void TriangulateRiverQuad(
        Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed)
    {
        TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed);
    }

    #endregion
}