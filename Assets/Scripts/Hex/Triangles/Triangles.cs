using System.Collections.Generic;
using UnityEngine;

public class Triangles : Shape
{
    /// <summary>
    /// Adds a distorted triangle given three vertex positions
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertex</param>
    /// <param name="v3">third vertex </param>
    /// <param name="vertices">collection of vertices</param>
    /// <param name="triangles">collection of triangles</param>
    public static void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// Adds a triangle given three vertex positions
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertex</param>
    /// <param name="v3">third vertex </param>
    /// <param name="vertices">collection of vertices</param>
    /// <param name="triangles">collection of triangles</param>
    public static void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// Adds the color data to a single triangle
    /// </summary>
    /// <param name="color">color to add</param>
    /// <param name="colors">colection of possible colors</param>
    public static void AddTriangleColor(Color color, List<Color> colors)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    /// <summary>
    /// Adds a color to each seperate vertex of a triangle
    /// </summary>
    /// <param name="c1">first corner</param>
    /// <param name="c2">second corner</param>
    /// <param name="c3">thirs corner</param>
    /// <param name="colors">available colors</param>
    public static void AddTriangleColor(Color c1, Color c2, Color c3, List<Color> colors)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }
}

