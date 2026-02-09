using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;


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

    [Range(0, 1f)]
    public float orthography = 0;
    [Range(1, 100)]
    public float orthographicSize = 5;

    public void SetOrthography(float value)
    {
        orthography = value;
        Debug.Log("Orthography = " + orthography);
    }

    private Vector2 xCoords;
    private Vector2 yCoords;
    public Vector2 zCoords;


    public bool write = false;
    public LineRenderer screen;
    public LineRenderer rear;
    public LineRenderer right;
    public LineRenderer left;


    public Mesh areaMesh;
    public MeshFilter meshFilter;
    public CameraArea cameraArea;
    public GameObject panel;
    // Start is called once before the first execution of Update after the MonoBehaviour is create

    private Vector3 topLeftCoords = new Vector3(-1,1,1);
    private Vector3 topRightCoords = new Vector3(1,1,1);
    private Vector3 bottomRightCoords = new Vector3(1, -1, 1);
    private Vector3 bottomLeftCoords = new Vector3(-1,-1,1);

    public List<GameObject> targetObjects;
    public ProjectionRaycast raycast;

    private bool first = true;

    public Vector2 rearXRange;
    public Vector2 rearYRange;

    public Matrix4x4 ProcessInput()
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
        float orthographicMultiplier;

        if (useHorizontal)
        {
            orthographicMultiplier = Mathf.Lerp((xCoords.y - xCoords.x) / 2, orthographicSize, Mathf.Pow(orthography, 2)) / ((xCoords.y - xCoords.x) / 2);
        }
        else
        {
            orthographicMultiplier = Mathf.Lerp((yCoords.y - yCoords.x) / 2, orthographicSize, Mathf.Pow(orthography, 2)) / ((yCoords.y - yCoords.x) / 2);
        }

        xCoords *= orthographicMultiplier;
        yCoords *= orthographicMultiplier;




        Matrix4x4 output = (ConstructMatrix(xCoords, yCoords, zCoords, orthography));
        return output;
    }



     public void createVolume(Matrix4x4 m)
    {
        Matrix4x4 invOutput = m.inverse;

        //rear.loop = true;

        rear.positionCount = 4;
        screen.positionCount = 4;
        right.positionCount = 4;
        left.positionCount = 4;
        rear.loop = true;
        screen.loop = true;
        right.loop = true;
        left.loop = true;

        Vector3 ntl = new Vector3(xCoords.x, yCoords.y, zCoords.x);
        Vector3 ntr = new Vector3(xCoords.y, yCoords.y, zCoords.x);
        Vector3 nbr = new Vector3(xCoords.y, yCoords.x, zCoords.x);
        Vector3 nbl = new Vector3(xCoords.x, yCoords.x, zCoords.x);

        Vector3 ftl = new();
        Vector3 ftr = new();
        Vector3 fbr = new();
        Vector3 fbl = new();

        Mesh newMesh = new Mesh();
        Vector3[] newVertices = new Vector3[areaMesh.vertices.Length];
        int idx = 0;
        foreach (Vector3 v in areaMesh.vertices)
        {
            Vector3 transPoint = invOutput.MultiplyPoint(v * 2);
            transPoint.Scale(new Vector3(1, 1, -1));
            newVertices[idx] = (transPoint);

            if (v * 2 == topLeftCoords)
            {
                ftl = transPoint;
            }
            else if (v * 2 == topRightCoords)
            {
                ftr = transPoint;
            }
            else if (v * 2 == bottomRightCoords)
            {
                fbr = transPoint;
            }
            else if (v * 2 == bottomLeftCoords)
            {
                fbl = transPoint;
            }



            idx++;
        }

        rear.SetPositions(new Vector3[]
        {
            ftl,
            ftr,
            fbr,
            fbl,
        });

        screen.SetPositions(new Vector3[]
        {
            ntl,
            ntr,
            nbr,
            nbl
        });

        right.SetPositions(new Vector3[]
        {
            ntr,
            ftr,
            fbr,
            nbr
        });

        left.SetPositions(new Vector3[]
        {
            ntl,
            ftl,
            fbl,
            nbl
        });


        newMesh.SetVertices(newVertices);
        newMesh.SetTriangles(areaMesh.triangles, 0);
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        meshFilter.sharedMesh = newMesh;
    }   
        

    Matrix4x4 ConstructMatrix(Vector2 x, Vector2 y, Vector2 z, float o)
    {
        Matrix4x4 M = Matrix4x4.zero;

        M[0, 0] = 2 * ((1 - o) * z.x + o) / (x.y - x.x); // perspective: 2n/(r-l), ortho: 2/(r-l)

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

        if (write)
        {


            Debug.Log(input.inverse);
            Debug.Log(input);
            Debug.Log(Camera.main.projectionMatrix);
            write = false;
        }

        if (input != appliedCamera.projectionMatrix || first)
        {
            createVolume(input);
            if (first)
            {
                first = false;
            }
            appliedCamera.projectionMatrix = input;
            
            appliedCamera.nearClipPlane = zCoords.x;
            appliedCamera.farClipPlane = zCoords.y + 100;

            float aspectWidth = Mathf.Abs(xCoords.x - xCoords.y);
            float aspectHeight = Mathf.Abs(yCoords.y - yCoords.x);

            appliedCamera.aspect = aspectWidth / aspectHeight;

            Vector3 tl = rear.GetPosition(0);
            Vector3 br = rear.GetPosition(2);

            rearXRange = new Vector2(tl.x, br.x);
            rearYRange = new Vector2(br.y, tl.y);

            panel.transform.localScale = new Vector3(aspectWidth / aspectHeight, 1, 1);
        }

        

    }

    public Vector3 WorldToScreen(Vector3 worldPoint, Matrix4x4 m)
    {
        Vector3 eyePoint = transform.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
        eyePoint.Scale(new Vector3(1, 1, -1));
        Vector3 projectedPoint = m.MultiplyPoint(eyePoint);
        projectedPoint.x = Remap(projectedPoint.x, -1, 1, xCoords.x, xCoords.y);
        projectedPoint.y = Remap(projectedPoint.y, -1, 1, yCoords.x, yCoords.y);
        projectedPoint.z = zCoords.x;



        Vector3 screenPoint = transform.TransformPoint(projectedPoint);
        return screenPoint;
    }

    public float Remap(float value, float min, float max, float newMin, float newMax)
    {
        if (Mathf.Approximately(max, min))
        {
            return value;
        }

        return newMin + (value-min)*(newMax-newMin)/(max-min);
    }

    public RaycastHit NewRaycast(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 direction = (endPoint-startPoint).normalized;
        RaycastHit hit = raycast.CastRay(startPoint, direction, zCoords.y);
        return hit;
    }
}
