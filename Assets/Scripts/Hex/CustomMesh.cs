using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh 
{
    public static Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexMetrics.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexMetrics.CELLPERTURBSTRENGTH;
        position.z += (sample.z * 2f - 1f) * HexMetrics.CELLPERTURBSTRENGTH;

        return position;
    }
}
