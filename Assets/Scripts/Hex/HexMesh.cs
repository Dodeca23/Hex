using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Takes care o constructing the mesh of a hex cll
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    #region Fields

    private static List<Vector3> vertices =
        new List<Vector3>();                    // stores the vertices of the mesh
    private static List<int> triangles =
        new List<int>();                        // stores the terrain.Triangles of the mesh
    private static List<Color> colors =
        new List<Color>();                      // stores the possible terrain.Colors of the mesh

    private Mesh hexMesh;                       // referene to te mesh component

    private new MeshCollider collider;          // reference to the mesh collider component

    #endregion

    #region Properties

    public List<Vector3> Vertices => vertices;

    public List<int> Triangles => triangles;
    public List<Color> Colors => colors;

    #endregion

    #region MonoBehaviors

    private void Awake()
    {
        hexMesh = GetComponent<MeshFilter>().mesh = new Mesh();
        collider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
    }

    #endregion
     
    #region Clear and Apply
    public void Clear()
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        hexMesh.SetColors(colors);
        hexMesh.SetTriangles(triangles, 0);
        hexMesh.RecalculateNormals();
        collider.sharedMesh = hexMesh;
    }

    #endregion

}
