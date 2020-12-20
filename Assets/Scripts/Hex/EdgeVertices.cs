using System;
using UnityEngine;

public struct EdgeVertices
{
    public Vector3 v1, v2, v3, v4;                  // the four vertices tht form an edge

    /// <summary>
    /// Constructs an edge
    /// </summary>
    /// <param name="corner1">first corner</param>
    /// <param name="corner2">second corner</param>
    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
        v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
        v4 = corner2;
    }

    /// <summary>
    /// Performs the terrace interpolation between
    /// al pairs of two edgevertices
    /// </summary>
    /// <param name="a">first edge</param>
    /// <param name="b">second edge</param>
    /// <param name="step">step along the slope</param>
    /// <returns></returns>
    public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
    {
        EdgeVertices result;

        result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
        result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
        result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
        result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);

        return result;
    }
}
