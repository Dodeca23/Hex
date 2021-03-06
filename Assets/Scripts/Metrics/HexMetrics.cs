﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexMetrics 
{
    #region Constants

    // Size of a single chunk
    public const int CHUNKSIZEX = 5;
    public const int CHUNKSIZEZ = 5;

    // Converts from he outer to inner radius
    public const float OUTERTOINNER = 0.866025404f;
    // Converts from the inner to outer radius
    public const float INNERTOOUTER = 1f / OUTERTOINNER;
    // Stores the outer radius, at which the corners of the hexagon will be
    public const float OUTERRADIUS = 10f; 
    // Stores the inner radius at which the centers of each edge will be
    public const float INNERRADIUS = OUTERRADIUS * OUTERTOINNER;
    // Part of te cell that is not blended
    public const float SOLIDFACTOR = 0.8f;
    // Part of the cell that is blended
    public const float BLENDFACTOR = 1f - SOLIDFACTOR;
    // The height of each elevationstep
    public const float ELEVATIONSTEP = 3f;
    // Amount of terraces on a slope
    public const int TERRACESPERSLOPE = 2;
    // Amount of steps on a slope (terraced and steep parts)
    public const int TERRACESTEPS = TERRACESPERSLOPE * 2 + 1;
    // Width of a terracestep
    public const float HORIZONTALTERRACESTEPSIZE = 1f / TERRACESTEPS;
    // Height of a terracestep
    public const float VERTICALTERRACESTEPSIZE = 1f / (TERRACESPERSLOPE + 1);
    // Strength of the noise that is applied
    public const float CELLPERTURBSTRENGTH = 4f;
    // Strength of the noise that is applied vertically
    public const float ELEVATIONPERTURBSTRENGTH = 1.5F;
    // Scales the noisesample so it covers a larger area
    public const float NOISESCALE = 0.003f;
    // Defines the vertical elevation of the streambed
    public const float STREAMBEDELEVATIONOFFSET = -1.75f;
    // Defines the vertical elevation of a water surface
    public const float WATERSURFACEELEVATIONOFFSET = -0.5f;
    // radius of the water hexagons
    public const float WATERFACTOR = 0.6f;
    // size of the water bridge
    public const float WATERBLENDFACTOR = 1f - WATERFACTOR;
    // size of the hash table
    public const int HASHGRIDSIZE = 256;
    // scale at which the grid is stretched
    public const float HASHGRIDSCALE = 0.25f;

    #endregion

    #region Static Fields

    // stores the noise texture that is used to distort the shape of hexcells
    public static Texture2D noiseSource;

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

    // stores the hashgrid as an array of floats
    private static HexHash[] hashGrid;

    // Tresholds for randomizing features
    private static float[][] featureTresholds =
    {
        new float[]{0.0f, 0.0f, 0.4f },
        new float[]{0.0f, 0.4f, 0.6f },
        new float[]{0.4f, 0.6f, 0.8f }
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
    /// Interpolates the terrain.Colors along a slope
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

    /// <summary>
    /// Compares two elevationlevels, and determines the edgetype
    /// </summary>
    /// <param name="elevation1">first elevation</param>
    /// <param name="elevation2">second elevation</param>
    /// <returns>edgetype</returns>
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        // If there's no difference, it's a flat edge
        if (elevation1 == elevation2)
            return HexEdgeType.Flat;

        // If the elevationdifference is 1, it's a sloped edge
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
            return HexEdgeType.Slope;

        // If the difference is 2 or more, it's a cliff edge
        return HexEdgeType.Cliff;
    }

    /// <summary>
    /// Takes a worldposition and produces a 4d vector containing the noise samples
    /// </summary>
    /// <param name="position">world position</param>
    /// <returns></returns>
    public static Vector4 SampleNoise(Vector3 position) =>
        noiseSource.GetPixelBilinear(position.x * NOISESCALE, position.z * NOISESCALE);

    /// <summary>
    /// Returns the average of two adjacent corner vectors
    /// </summary>
    /// <param name="direction">direction</param>
    /// <returns></returns>
    public static Vector3 GetSolidEdgeMiddle(HexDirection direction) =>
        (corners[(int)direction] + corners[(int)direction + 1]) *
        (0.5f * SOLIDFACTOR);

    /// <summary>
    /// Gets the first water corner in a given direction
    /// </summary>
    /// <param name="direction">direction</param>
    /// <returns></returns>
    public static Vector3 GetFirstWaterCorner(HexDirection direction) =>
        corners[(int)direction] * WATERFACTOR;

    /// <summary>
    /// Gets the second water corner in a given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetSecondWaterCorner(HexDirection direction) =>
        corners[(int)direction + 1] * WATERFACTOR;

    //Returns the bridge between two water corners
    public static Vector3 GetWaterBridge(HexDirection direction) =>
        (corners[(int)direction] + corners[(int)direction + 1]) * WATERBLENDFACTOR;

    /// <summary>
    /// Fills the hashgrid with a random value on each position
    /// </summary>
    /// <param name="seed">random seed</param>
    public static void InitializeHashGrid(int seed)
    {
        hashGrid = new HexHash[HASHGRIDSIZE * HASHGRIDSIZE];
        Random.State currentState = Random.state;
        Random.InitState(seed);
        for (int i = 0; i < hashGrid.Length; i++)
        {
            hashGrid[i] = HexHash.Create();
        }
        Random.state = currentState;
    }

    /// <summary>
    /// Retrieves a value using the x and z coordinates
    /// </summary>
    /// <param name="position">position</param>
    /// <returns></returns>
    public static HexHash SampleHashGrid(Vector3 position)
    {
        int x = (int)(position.x * HASHGRIDSCALE) % HASHGRIDSIZE;
        if (x < 0)
            x += HASHGRIDSIZE;
        int z = (int)(position.z * HASHGRIDSCALE) % HASHGRIDSIZE;
        if (z < 0)
            z += HASHGRIDSIZE;

        return hashGrid[x + z * HASHGRIDSIZE];
    }

    /// <summary>
    /// Retruns the tresholds for a specific feature level
    /// </summary>
    /// <param name="level>"feature level</param>
    /// <returns></returns>
    public static float[] GetFeatureTresholds(int level) =>
        featureTresholds[level];

    #endregion

}
