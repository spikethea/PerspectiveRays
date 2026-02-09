using UnityEngine;

public class SliderAdjuster : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float coneWidth = 30f;

    public void SetConeWidth(float value)
    {
        coneWidth = value;
    }

}
