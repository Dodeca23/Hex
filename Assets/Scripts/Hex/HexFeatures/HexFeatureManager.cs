using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexFeatureManager : MonoBehaviour
{
    [SerializeField]
    private HexFeatureCollection[] urbanCollections = default;
    [SerializeField]
    private HexFeatureCollection[] ruralCollections = default;
    [SerializeField]
    private HexFeatureCollection[] woodCollection = default;

    private Transform container;

    public void Clear()
    {
        if (container)
            Destroy(container.gameObject);

        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }

    public void Apply() { }

    /// <summary>
    /// Adds a feature to a cell
    /// </summary>
    /// <param name="cell">cell to add feature to</param>
    /// <param name="position">position of feature</param>
    public void AddFeature(HexCell cell, Vector3 position)
    {
        HexHash hash = HexMetrics.SampleHashGrid(position);
        Transform prefab = PickPrefab(urbanCollections , cell.UrbanLevel, hash.a, hash.d);
        Transform otherPrefab = PickPrefab(ruralCollections, cell.RuralLevel, hash.b, hash.d);

        float usedMesh = hash.a;
        if (prefab)
        {
            if (otherPrefab && hash.b < hash.a)
            {
                prefab = otherPrefab;
                usedMesh = hash.b;
            }
        }
        else if (otherPrefab)
        {
            prefab = otherPrefab;
            usedMesh = hash.b;
        }

        otherPrefab = PickPrefab(woodCollection, cell.WoodLevel, hash.c, hash.d);
        if (prefab)
        {
            if (otherPrefab && hash.c < usedMesh)
                prefab = otherPrefab;
        }
        else if (otherPrefab)
            prefab = otherPrefab;
        else
            return;

        Transform instance = Instantiate(prefab);
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = CustomMesh.Perturb(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * hash.c, 0f);
        instance.SetParent(container, false);
    }

    /// <summary>
    /// Returns a prefab with the corresponding feature level
    /// </summary>
    /// <param name="collection">collection to pick from</param>
    /// <param name="level">feature level</param>
    /// <param name="hash">hash table for randomness</param>
    /// <param name="choice">prefab of choice</param>
    /// <returns></returns>
    private Transform PickPrefab(HexFeatureCollection[] collection, int level, float hash, float choice)
    {
        if(level > 0)
        {
            float[] tresholds = HexMetrics.GetFeatureTresholds(level - 1);
            for (int i = 0; i < tresholds.Length; i++)
            {
                if (hash < tresholds[i])
                    return collection[i].Pick(choice);
            }
        }

        return null;
    }
}
