using System.Collections.Generic;
using UnityEngine;

public class ProjectionRaycast : MonoBehaviour
{
    [Header("Ray Settings")]
    //blic float maxDistance = 1000f;
    public LayerMask hitLayers;

    [Header("Cone Resolution")]
    [Range(1, 50)] public int horizontalRays = 10;
    [Range(1, 50)] public int verticalRays = 6;

    [Header("Cone Angles")]
    [Range(1f, 90f)] public float horizontalFOV = 20f;
    [Range(1f, 90f)] public float verticalFOV = 12f;

    public List<Ray> Rays { get; private set; } = new();
    public List<RaycastHit> Hits { get; private set; } = new();

    public Camera cam;

    public void CreateRays(Vector2 xCoords, Vector2 yCoords, Vector2 zCoords)
    {
        Rays.Clear();
        Hits.Clear();

 

        for (int y = 0; y < verticalRays; y++)
        {
            float vT = (verticalRays == 1) ? 0.5f : (float)y / (verticalRays - 1);
            float vPos = Mathf.Lerp(yCoords.x, yCoords.y, vT);

            for (int x = 0; x < horizontalRays; x++)
            {
                float hT = (horizontalRays == 1) ? 0.5f : (float)x / (horizontalRays - 1);
                float hPos = Mathf.Lerp(xCoords.x, xCoords.y, hT);

                Vector3 startPos = cam.transform.TransformPoint(new Vector3(hPos,vPos,zCoords.y));
                Vector3 direction = cam.transform.forward*-1;

                Ray ray = new Ray(startPos, direction);
                Rays.Add(ray);

                if (Physics.Raycast(ray, out RaycastHit hit, zCoords.y, hitLayers))
                {
                    // Optional: ensure hit is actually inside camera frustum
                    Vector3 vp = cam.WorldToViewportPoint(hit.point);

                    bool inView =
                        vp.z > 0 &&
                        vp.x >= 0 && vp.x <= 1 &&
                        vp.y >= 0 && vp.y <= 1;

                    if (inView)
                        Hits.Add(hit);
                }
            }
        }
    }
}