using UnityEngine;
public class Shake
{
    public float startTime;
    public float T;
    public float duration;
    public float intensity;

    public Shake(float duration=0.15f, float intensity=0.02f)
    {
        this.duration = duration;
        this.intensity = intensity;
    }

    public void Init()
    {
        T = 0;
        startTime = Time.time;
    }
}
