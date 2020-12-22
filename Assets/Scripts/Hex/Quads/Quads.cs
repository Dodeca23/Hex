using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quads : CustomMesh
{
    /// <summary>
    /// Adds a distorted quad given four verrtex position
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertexparam>
    /// <param name="v3">third vertex</param>
    /// <param name="v4">fourth vertex</param>
    /// <param name="vertices">list of vertices</param>
    /// <param name="triangles">list of triangles</param>
    public static void AddQuad(
        Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
        List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    /// <summary>
    /// Adds a quad given four verrtex position
    /// </summary>
    /// <param name="v1">first vertex</param>
    /// <param name="v2">second vertexparam>
    /// <param name="v3">third vertex</param>
    /// <param name="v4">fourth vertex</param>
    /// <param name="vertices">list of vertices</param>
    /// <param name="triangles">list of triangles</param>
    public static void AddQuadUnperturbed(
        Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
        List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    /// <summary>
    /// Adds the color data to a single quad
    /// </summary>
    /// <param name="c1">first color</param>
    /// <param name="c2">second color</param>
    /// <param name="c3">third color</param>
    /// <param name="c4">fourth color</param>
    /// <param name="colors">list of possible colors</param>
    public static void AddQuadColor(
        Color c1, Color c2, Color c3, Color c4, List<Color> colors)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    /// <summary>
    /// Adds the color data to a single quad, using two colors
    /// </summary>
    /// <param name="c1">first color</param>
    /// <param name="c2">second color</param>
    /// /// <param name="colors">list of possible colors</param>
    public static void AddQuadColor(Color c1, Color c2, List<Color> colors)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    /// <summary>
    /// Adds a single color to a single quad
    /// </summary>
    /// <param name="color">color</param>
    /// /// <param name="colors">list of possible colors</param>
    public static void AddQuadColor(Color color, List<Color> colors)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    /// <summary>
    /// Adds uv coord to each vertex of a quad
    /// </summary>
    /// <param name="uv1">first uv</param>
    /// <param name="uv2">second uv</param>
    /// <param name="uv3">third uv</param>
    /// <param name="uv4">fourth uv</param>
    /// <param name="uvs">list of uvs</param>
    public static void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, List<Vector2> uvs)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }

    /// <summary>
    /// Adds a rectangular uv area to a quad
    /// </summary>
    /// <param name="uMin">first corner</param>
    /// <param name="uMax">second corner</param>
    /// <param name="vMin">third corner</param>
    /// <param name="vMax">fourth corner</param>
    /// <param name="uvs">list of uvs</param>
    public static void AddQuadUV(float uMin, float uMax, float vMin, float vMax, List<Vector2> uvs)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }
}


