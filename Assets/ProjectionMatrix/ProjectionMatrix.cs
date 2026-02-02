using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class ProjectionMatrix : MonoBehaviour
{
    public Camera appliedCamera;
    [Header("Screen Coordinates")]
    public Vector2 xCoordsInput = new Vector2(-1, 1); //(l,r)
    public Vector2 yCoordsInput = new Vector2(-1, 1); //(b,t)
    [Tooltip("Near and Far Clipping Planes")]
    public Vector2 zCoordsInput = new Vector2(-.3f, -50); //(n,f)

    [Header("FOV Settings")]
    [Tooltip("Overrides x and y coordinate inputs")]
    public bool useFOV = false;
    public float horizontalFOV = 85;
    public float verticalFOV = 60;

    [Header("FOV Settings")]
    public bool useAspectRatio = false;
    [Tooltip("Overrides other coordinate/FOV value")]
    public bool useHorizontal = false;
    public Vector2 aspectRatio = new Vector2(16, 10);

    [Range(0,1f)]
    public float orthography = 0;
    public float orthographicSize = 5;

    private Vector2 xCoords;
    private Vector2 yCoords;
    private Vector2 zCoords;

    private Vector2 prevX = new Vector2(0, 0);
    private Vector2 prevY = new Vector2(0, 0);
    private Vector2 prevZ = new Vector2(0, 0);


    public GameObject panel;

    // Start is called once before the first execution of Update after the MonoBehaviour is create

    Matrix4x4 ProcessInput()
    {
        xCoords = xCoordsInput;
        yCoords = yCoordsInput;
        zCoords = zCoordsInput;

        if (useFOV)
        {
            float t = Mathf.Tan(horizontalFOV * Mathf.Deg2Rad / 2) * zCoords.x;
            float r = Mathf.Tan(verticalFOV * Mathf.Deg2Rad / 2) * zCoords.x;

            xCoords = new Vector2(-1 * t, t);
            yCoords = new Vector2(-1 * r, r);
        }

        if (useAspectRatio)
        {
            if (useHorizontal)
            {
                yCoords = aspectRatio.y / aspectRatio.x * xCoords;
            }
            else
            {
                xCoords = aspectRatio.x / aspectRatio.y * yCoords;
            }
        }

        xCoords *= 1+orthographicSize * orthography;
        yCoords *= 1+orthographicSize * orthography;

        return (ConstructMatrix(xCoords, yCoords, zCoords, orthography));
    }

    Matrix4x4 ConstructMatrix(Vector2 x, Vector2 y, Vector2 z, float o)
    {
        Matrix4x4 M = Matrix4x4.zero;

        M[0, 0] = 2*((1 - o) * z.x + o) / (x.y - x.x); // perspective: 2n/(r-l), ortho: 2/(r-l)

        float rl = (x.x + x.y) / (x.y - x.x); // (r+l)/(r-l)
        M[0, 2] = (1 - o) * rl; // perspective: (r+l)/(r-l), ortho: 0
        M[0, 3] = -1 * o * rl; // perspective: 0, ortho: -(r+l)/(r-l)

        M[1, 1] = 2 * ((1 - o) * z.x + o) / (y.y - y.x); // perspective:  2n/(t-b), ortho: 2/(t-b)
        float tb = (y.x + y.y) / (y.y - y.x); //(t+b)/(t-b)
        M[1, 2] = (1 - o) * tb; // perspective: (t+b)/(t-b), ortho: 0
        M[1, 3] = -1 * o * tb; // perspective: 0, ortho: -(t+b)/(t-b)

        M[2, 2] = ((o - 1) * (z.y + z.x) - 2 * o) / (z.y - z.x); // perspective: -(f+n)/(f-n), ortho: -2/(f-n)
        M[2, 3] = ((2 * o - 2) * z.x * z.y - o * (z.y + z.x)) / (z.y - z.x); // perspective: -2fn/(f-n), ortho: -(f+n)/(f-n)
        M[3, 2] = -1 + o; // perspective: -1, ortho: 0
        M[3, 3] = o; // perspective: 0, ortho: 1

        return M;
    }


    // Update is called once per frame
    void Update()
    {
        Matrix4x4 input = ProcessInput();
        appliedCamera.projectionMatrix = input;
        appliedCamera.nearClipPlane = zCoords.x;
        appliedCamera.farClipPlane = zCoords.y;

        float aspectWidth = Mathf.Abs(xCoords.x - xCoords.y);
        float aspectHeight = Mathf.Abs(yCoords.y - yCoords.x);

        appliedCamera.aspect = aspectWidth / aspectHeight;

        panel.transform.localScale = new Vector3(aspectWidth/aspectHeight, 1, 1);

    }
}
