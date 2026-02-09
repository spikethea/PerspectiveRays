using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RaycastVisuals : MonoBehaviour
{
    [Header("Visuals")]
    public bool LineRendering = true;
    public void ToggleLines()
    {
        LineRendering = !LineRendering;
    }

    public Raycast raycastSource;
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    private readonly List<LineRenderer> lines = new();

    void Update()
    {
        if (raycastSource == null) return;
        if (!LineRendering)
        {
            DisableAllLines();
            return;
        }

        EnsureLineCount(raycastSource.Hits.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            if (i < raycastSource.Hits.Count)
            {
                RaycastHit hit = raycastSource.Hits[i];
                Ray ray = raycastSource.Rays[i];

                lines[i].enabled = true;
                // Creates offset to display line renderers.
                float startOffset = 0.1f; 
                Vector3 start = ray.origin + ray.direction * startOffset;

                lines[i].SetPosition(0, start);
                lines[i].SetPosition(1, hit.point);
            }
            else
            {
                lines[i].enabled = false;
            }
        }
    }

    void DisableAllLines()
    {
        foreach (var line in lines)
            line.enabled = false;
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
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;

            lines.Add(lr);
        }
    }

}
