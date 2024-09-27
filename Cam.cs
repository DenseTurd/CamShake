using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Cam : MonoBehaviour
{
    #region Instance
    public static Cam Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion Instance

    public Transform stashPos;

    Vector3 homePos;
    Quaternion homeRot;
    Vector3 currentPos;
    Quaternion currentRot;
    Vector3 startPos;
    Quaternion startRot;
    Vector3 endPos;
    Quaternion endRot;
    Vector3 shakePos;
    Quaternion shakeRot;
    Vector3 prevShakePos;
    Quaternion prevShakeRot;
    List<Shake> shakes = new();
    bool moving;
    bool shaking;
    bool goCurrentPos;
    float T;
    float shakeT;
    float easeDur = 0.5f;

    void Start()
    {   
        homePos = transform.position;
        currentPos = transform.position;
        shakePos = currentPos;
        prevShakePos = currentPos;

        homeRot = transform.rotation;
        currentRot = homeRot;
        shakeRot = homeRot;
        prevShakeRot = homeRot;
    }

    public void Shake(Shake shake)
    {
        shake.Init();
        shakes.Add(shake);
        goCurrentPos = false;
        shaking = true;
    }

    public void CutToHome() => CutTo(homePos, homeRot);

    public void CutToAspect(Aspect aspect)
    {
        Vector3 bodPos = aspect.Pos();
        float xOffset = Rand.Range(-1f, 1f);
        Vector3 pos = new(bodPos.x + xOffset, bodPos.y + 2, bodPos.z - 3);
        Vector3 relativePos = new Vector3(bodPos.x, bodPos.y + 0.75f, bodPos.z) - pos;
        Quaternion rot = Quaternion.LookRotation(relativePos);
        CutTo(pos, rot);
        GhostProps();
    }

    public void CutTo(Vector3 pos, Quaternion rot)
    {
        currentPos = pos;
        currentRot = rot;
        shakePos = pos;
        shakeRot = rot;

        transform.SetPositionAndRotation(currentPos, currentRot);
    }

    public async Task EaseToAspect(Aspect aspect, float dur=0.5f)
    {
        Vector3 bodPos = aspect.Pos();
        float xOffset = Rand.Range(-1f, 1f);
        Vector3 pos = new(bodPos.x + xOffset, bodPos.y + 2, bodPos.z - 3);
        Vector3 relativePos = new Vector3(bodPos.x, bodPos.y + 0.75f, bodPos.z) - pos;
        Quaternion rot = Quaternion.LookRotation(relativePos);
        await EaseTo(pos, rot, dur);
        GhostProps();
    }

    public async Task EaseToHome(float dur=0.5f) => await EaseTo(homePos, homeRot, dur);
    public async Task EaseTo(Vector3 pos, Quaternion rot, float dur)
    {
        startPos = currentPos;
        startRot = currentRot;

        endPos = pos;
        endRot = rot;

        if (startPos == endPos && startRot == endRot) return;

        moving = true;
        T = 0;
        easeDur = dur;
        await Task.Delay(Mathf.RoundToInt(dur * 1000));
    }

    public void GhostProps()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward);

        if (hit.transform == null) return;
        if (!hit.transform.TryGetComponent<Ghoster>(out var ghoster)) return;

        ghoster.Ghost(true);

        //RaycastHit[] hits = new RaycastHit[64];
        //Ray ray = new Ray(transform.position, transform.forward);
        //int numHits = Physics.RaycastNonAlloc(ray, hits);

        //for (int i = 0; i < numHits; i++)
        //{
        //    RaycastHit hit = hits[i];

        //    if (!hit.transform.TryGetComponent<Ghoster>(out var ghoster)) continue;

        //    ghoster.Ghost(true);
        //}
    }

    void Update()
    {
        Moving();
        Shaking();
    }

    void Moving()
    {
        if (!moving) return;
        T += Time.deltaTime * (1/easeDur);
        if (T >= 1) 
        { 
            moving = false;
            currentPos = endPos;
            currentRot = endRot;
            shakePos = endPos;
            shakeRot = endRot;
            transform.SetPositionAndRotation(currentPos, currentRot);
            return;
        }
        currentPos = Vector3.Lerp(startPos, endPos, Ease.InOutSine(T));
        currentRot = Quaternion.Lerp(startRot, endRot, Ease.InOutSine(T));
        transform.SetPositionAndRotation(currentPos, currentRot);
    }

    void Shaking()
    {
        float intensity = 0;
        if (shakes.Count > 0)
        {
            for (int i = 0; i < shakes.Count; i++)
            {
                Shake shake = shakes[i];
                float elapsed = Time.time - shake.startTime;
                shake.T = elapsed / shake.duration;

                if (shake.T > 1)
                {
                    shakes.RemoveAt(i);
                    continue;
                }

                intensity += shake.intensity * EaseIntensity(shake.T);
            }

            if (intensity > 1) intensity = 1;
        }
        else
        {
            shakePos = currentPos;
            shakeRot = currentRot;
            prevShakePos = currentPos;
            prevShakeRot = currentRot;
            goCurrentPos = true;
        }

        if (shaking) ShakeCam(intensity);
    }

    void ShakeCam(float intensity)
    {
        shakeT += Time.deltaTime * (30 + (intensity * 30));
        if (shakeT > 1)
        {
            if (goCurrentPos)
            {
                transform.SetPositionAndRotation(currentPos, currentRot);
                shaking = false;
                return;
            }

            prevShakePos = shakePos;
            shakePos = currentPos + ShakeOffset(intensity);
            prevShakeRot = shakeRot;
            shakeRot = ShakeRot(intensity);
            shakeT -= shakeT;
        }

        transform.position = Vector3.Lerp(prevShakePos, shakePos, Ease.InOutSine(shakeT));
        transform.rotation = Quaternion.Lerp(prevShakeRot, shakeRot, shakeT);
    }

    float EaseIntensity(float T)
    {
        return T <= 0.5 ? Ease.InOutSine(T) : Ease.InOutSine(1 - T);
    }

    Vector3 ShakeOffset(float intensity)
    {
        float randy = Rand.Range(0f, intensity / 5);

        float x, y, z;
        x = Rand.Range(-1f, 1f);
        y = Rand.Range(-1f, 1f);
        z = Rand.Range(-1f, 1f);

        var mag = Mathf.Sqrt(x * x + y * y + z * z);
        mag = mag == 0 ? 1 : mag; // Stops a NaN

        x /= mag; 
        y /= mag; 
        z /= mag;

        float radius = Mathf.Pow(randy, 1f / 3f); // this is cube root

        return new Vector3(x, y, z) * radius;
    }

    Quaternion ShakeRot(float intensity)
    {
        float x, y, z;
        x = Rand.Range(-33f, 33f);
        y = Rand.Range(-33f, 33f);
        z = Rand.Range(-33f, 33f);
        Vector3 currR = currentRot.eulerAngles;
        Vector3 randR = currR + new Vector3(x, y, z) * intensity;

        return Quaternion.Euler(randR);
    }
}
