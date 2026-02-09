using UnityEngine;
using System.Collections.Generic;

public class Raycast : MonoBehaviour
{
    [Header("Ray Settings")]
    public float maxDistance = 10f;
    public LayerMask hitLayers;

    [Header("Cone Resolution")]
    [Range(1, 50)] public int horizontalRays = 10;
    [Range(1, 50)] public int verticalRays = 10;

    [Header("Cone Angles")]
    [Range(1f, 120f)] public float horizontalFOV = 10f;
    [Range(1f, 120f)] public float verticalFOV = 30f;

    public List<Ray> Rays { get; private set; } = new();
    public List<RaycastHit> Hits { get; private set; } = new();

    void Update()
    {
        Rays.Clear();
        Hits.Clear();

        Camera cam = Camera.main;

        float hHalf = horizontalFOV * 0.2f;
        float vHalf = verticalFOV * 0.2f;

        for (int y = 0; y < verticalRays; y++)
        {
            float vT = (verticalRays == 1) ? 0.5f : (float)y / (verticalRays - 1);
            float vAngle = Mathf.Lerp(-vHalf, vHalf, vT);

            for (int x = 0; x < horizontalRays; x++)
            {
                float hT = (horizontalRays == 1) ? 0.5f : (float)x / (horizontalRays - 1);
                float hAngle = Mathf.Lerp(-hHalf, hHalf, hT);

                // defines local space for camera's raycast
                Vector3 direction =
                    Quaternion.AngleAxis(hAngle, cam.transform.up) *
                    Quaternion.AngleAxis(vAngle, cam.transform.right) *
                    cam.transform.forward;

                Ray ray = new Ray(cam.transform.position, direction);
                Rays.Add(ray);

                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitLayers))
                {
                    // Ensure hit detection is within camera's range.
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