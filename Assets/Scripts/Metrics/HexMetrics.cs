using System.Collections;
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

    #endregion

}
