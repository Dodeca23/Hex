﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMetrics : MonoBehaviour
{
    #region Constants

    // Stores the outer radius, at which the corners of the hexagon will be
    public const float OUTERRADIUS = 10f; 
    // Stores the inner radius at which the centers of each edge will be
    public const float INNERRADIUS = OUTERRADIUS * 0.866025404f;
    // Part of te cell that is not blended
    public const float SOLIDFACTOR = 0.75f;
    // Part of the cell that is blended
    public const float BLENDFACTOR = 1f - SOLIDFACTOR;
    // The height of each elevationstep
    public const float ELEVATIONSTEP = 5f;
    // Amount of terraces on a slope
    public const int TERRACESPERSLOPE = 2;
    // Amount of steps on a slope (terraced and steep parts)
    public const int TERRACESTEPS = TERRACESPERSLOPE * 2 + 1;
    // Width of a terracestep
    public const float HORIZONTALTERRACESTEPSIZE = 1f / TERRACESTEPS;
    // Height of a terracestep
    public const float VERTICALTERRACESTEPSIZE = 1f / (TERRACESPERSLOPE + 1);

    #endregion

    #region Static Fields

    // Defines the six corners relative to the cell's center
    private static Vector3[] corners =
    {
        new Vector3(0f, 0f, OUTERRADIUS),
        new Vector3(INNERRADIUS, 0f, 0.5f * OUTERRADIUS),
        new Vector3(INNERRADIUS, 0f, -0.5f * OUTERRADIUS),
        new Vector3(0f, 0f, -OUTERRADIUS),
        new Vector3(-INNERRADIUS, 0f, -0.5f * OUTERRADIUS),
        new Vector3(-INNERRADIUS, 0f, 0.5f * OUTERRADIUS),
        new Vector3(0f, 0f, OUTERRADIUS)
    };

    #endregion

    #region Static Methods

    /// <summary>
    /// Returns the first corner in a given direction
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static Vector3 GetFirstCorner(HexDirection direction) =>
        corners[(int)direction];

    /// <summary>
    /// Returns the second corner in a given direction
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static Vector3 GetSecondCorner(HexDirection direction) =>
        corners[(int)direction + 1];

    /// <summary>
    /// Returns the first corner of the solid inner hexagon
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static Vector3 GetFirstSolidCorner(HexDirection direction) =>
        corners[(int)direction] * SOLIDFACTOR;

    /// <summary>
    /// Returns the second corner of the solid inner hexagon
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static Vector3 GetSecondSolidCorner(HexDirection direction) =>
        corners[(int)direction + 1] * SOLIDFACTOR;

    /// <summary>
    /// Retrusn the part of an edge where the corners are cut; the bridge
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static Vector3 GetBridge(HexDirection direction) =>
        (corners[(int)direction] + corners[(int)direction + 1]) * BLENDFACTOR;

    /// <summary>
    /// Interpolates each step along a slope
    /// The Y-coordinate only changes on odd steps
    /// </summary>
    /// <param name="a">first position</param>
    /// <param name="b">secong position</param>
    /// <param name="step">step along the slope</param>
    /// <returns></returns>
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HORIZONTALTERRACESTEPSIZE;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        float v = ((step + 1) / 2) * HexMetrics.VERTICALTERRACESTEPSIZE;
        a.y += (b.y - a.y) * v;

        return a;
    }

    /// <summary>
    /// Interpolates the colors along a slope
    /// </summary>
    /// <param name="a">first color</param>
    /// <param name="b">second corner</param>
    /// <param name="step">step along the slope</param>
    /// <returns></returns>
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * HexMetrics.HORIZONTALTERRACESTEPSIZE;

        return Color.Lerp(a, b, h);
    }

    #endregion

}
