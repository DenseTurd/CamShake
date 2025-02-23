using System.Collections.Generic;
using UnityEngine;

public class FluxCurve
{
    public List<FluxNode> nodes = new();

    public float Get(float T)
    {
        FluxNode node;
        for (int i = 0; i < nodes.Count; i++)
        {
            node = nodes[i];

            if (T > node.T) continue;

            float prevT = 0;
            float prevVal = 0;
            if (i > 0)
            {
                FluxNode prev = nodes[i - 1];
                prevT = prev.T;
                prevVal = prev.val; 
            }

            return CurveLerp(prevT, prevVal, node.T, node.val, T);
        }

        node = nodes[^1];
        return CurveLerp(node.T, node.val, 1, 0, T);
    }

    float CurveLerp(float prevT, float prevVal, float currT, float currVal, float T)
    {
        float gap = currT - prevT;
        float norm = 1 / gap;
        float relativeT = (T - prevT) * norm;
        float retVal = Mathf.Lerp(prevVal, currVal, relativeT);
        return retVal;
    }
}