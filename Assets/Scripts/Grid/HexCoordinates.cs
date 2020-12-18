using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x;
    [SerializeField]
    private int z;

    // Returns the X coordinate
    public int X => x;
    // Returns the Y coordinates
    public int Y => -X - Z;

    // Returns the Z coordinate
    public int Z => z;

    // Constructs a single HexCoordinate based on a x and z coordinate
    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    /// <summary>
    /// Returns a set of coordinates using regular offset coordinates
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="z">z coordinate</param>
    /// <returns></returns>
    public static HexCoordinates FromOffsetCoordinates(int x, int z) =>
        new HexCoordinates(x - z / 2, z);

    /// <summary>
    /// Returns the HexCoordinate written on a single line
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";

    /// <summary>
    /// Returns  the hexcoordinate witten on seperate lines
    /// </summary>
    /// <returns></returns>
    public string ToStringOnSeperateLines() =>
        X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();

    /// <summary>
    /// Returns a coordinate that belong to a given position
    /// </summary>
    /// <param name="position">position of the coordinate</param>
    /// <returns></returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.INNERRADIUS * 2f);
        float y = -x;

        float offset = position.z / (HexMetrics.OUTERRADIUS * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x -y);

        if(iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
                iX = -iY - iZ;
            else if (dZ > dY)
                iZ = -iX - iY;
        }

        return new HexCoordinates(iX, iZ);
    }

}
