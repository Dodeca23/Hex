using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Takes care o constructing the mesh of a hex cll
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    #region Fields

    public bool useCollider;
    public bool useColors;
    public bool useUVCoordinates;

    [NonSerialized] List<Vector3> vertices;     // stores the vertices of the mesh
    [NonSerialized] List<int> triangles;        // stores the triangles of the mesh
    [NonSerialized] List<Color> colors;         // stores the possible colors of the mesh
    [NonSerialized] List<Vector2> uvs;          // stores the uv coords of the mesh

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
        if(useCollider)
            collider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
    }

    #endregion
     
    #region Clear and Apply
    public void Clear()
    {
        hexMesh.Clear();
        vertices = ListPool<Vector3>.Get();
        if(useColors)
            colors = ListPool<Color>.Get();
        if (useUVCoordinates)
            uvs = ListPool<Vector2>.Get();
        triangles = ListPool<int>.Get();
    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);

        if(useColors)
        {
            hexMesh.SetColors(colors);
            ListPool<Color>.Add(colors);
        }

        if (useUVCoordinates)
        {
            hexMesh.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }

        hexMesh.SetTriangles(triangles, 0);
        ListPool<int>.Add(triangles);
        hexMesh.RecalculateNormals();
        if(useCollider)
            collider.sharedMesh = hexMesh;
    }

    #endregion
}
