using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera[] cameras;
    private int currentIndex = 0;

    void Start()
    {
        ActivateCamera(currentIndex);
    }

    public void ActivateCamera(int index)
    {
        if (cameras == null || cameras.Length == 0) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
    }

    public void NextCamera()
    {
        currentIndex++;
        if (currentIndex >= cameras.Length)
            currentIndex = 0;

        ActivateCamera(currentIndex);
    }
}
