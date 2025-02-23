using UnityEngine;

public class Flux
{
    public float startTime;
    public float T;
    public float duration;
    public float intensity;
    public FluxCurve curve;

    public Flux(float duration = 0.15f, float intensity = 0.02f, FluxCurve curve = null)
    {
        this.duration = duration;
        this.intensity = intensity;
        this.curve = curve;
    }

    public void Init()
    {
        T = 0;
        startTime = Time.time;
    }

    public float Intensity()
    {
        float elapsed = Time.time - startTime;
        T = elapsed / duration;

        if (curve != null)
        {
            return intensity * curve.Get(T);
        }
        return intensity * EaseIntensity(T);
    }

    float EaseIntensity(float T)
    {
        return T <= 0.5 ? Ease.InOutSine(T) : Ease.InOutSine(1 - T);
    }
}
