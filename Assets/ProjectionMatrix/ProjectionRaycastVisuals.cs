using UnityEngine;
using System.Collections.Generic;

public class ProjectionRaycastVisuals : MonoBehaviour
{

    [Header("Visuals")]
    public bool LineRendering = true;
    public void ToggleLines()
    {
        LineRendering = !LineRendering;
    }

    public ProjectionRaycast raycastSource;
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    public ProjectionMatrix projectionMatrix;

    private readonly List<LineRenderer> lines = new();

    void Update()
    {
        if (raycastSource == null) return;
        if (!LineRendering)
        {
            DisableAllLines();
            return;
        }

        raycastSource.CreateRays(projectionMatrix.rearXRange, projectionMatrix.rearYRange, projectionMatrix.zCoords);
        EnsureLineCount(raycastSource.Hits.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            if (i < raycastSource.Hits.Count)
            {
                RaycastHit hit = raycastSource.Hits[i];

                Vector3 screenPosition = projectionMatrix.WorldToScreen(hit.point, projectionMatrix.appliedCamera.projectionMatrix);

                RaycastHit newHit = projectionMatrix.NewRaycast(screenPosition, hit.point);
                screenPosition = projectionMatrix.WorldToScreen(newHit.point, projectionMatrix.appliedCamera.projectionMatrix);

                lines[i].enabled = true;
                lines[i].SetPosition(0, screenPosition);
                lines[i].SetPosition(1, newHit.point);
                lines[i].endWidth = lineWidth;
                lines[i].startWidth = lineWidth;    
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
            GameObject go = new GameObject("RayHit");
            go.transform.SetParent(transform);
            go.layer = gameObject.layer;
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
