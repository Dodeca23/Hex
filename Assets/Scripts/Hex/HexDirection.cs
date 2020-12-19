using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions
{
    /// <summary>
    /// Returns the opposite of a given direction
    /// </summary>
    /// <param name="direction">initial direction</param>
    /// <returns></returns>
    public static HexDirection Opposite(this HexDirection direction) =>
        (int)direction < 3 ? (direction + 3) : (direction - 3);


    /// <summary>
    /// Returns the previous direction
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static HexDirection Previous(this HexDirection direction) =>
        direction == HexDirection.NE ? HexDirection.NW : (direction - 1);

    /// <summary>
    /// Returns the next direction
    /// </summary>
    /// <param name="direction">current direction</param>
    /// <returns></returns>
    public static HexDirection Next(this HexDirection direction) =>
        direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
}
