using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RaycastVisuals : MonoBehaviour
{
    public Raycast raycastSource;
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    private readonly List<LineRenderer> lines = new();

    void Update()
    {
        if (raycastSource == null) return;

        EnsureLineCount(raycastSource.Hits.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            if (i < raycastSource.Hits.Count)
            {
                RaycastHit hit = raycastSource.Hits[i];
                Ray ray = raycastSource.Rays[i];

                lines[i].enabled = true;
                lines[i].SetPosition(0, ray.origin);
                lines[i].SetPosition(1, hit.point);
            }
            else
            {
                lines[i].enabled = false;
            }
        }
    }

    void EnsureLineCount(int count)
    {
        while (lines.Count < count)
        {
            GameObject go = new GameObject("ConeRayHit");
            go.transform.SetParent(transform);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = lineMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;

            lines.Add(lr);
        }
    }
}
