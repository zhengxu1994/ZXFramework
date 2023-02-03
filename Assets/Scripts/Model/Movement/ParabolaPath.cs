using System;
using System.Collections;
using UnityEngine;

public class ParabolaPath : MonoBehaviour
{
    private Vector2 startPixel;

    private Vector2 controlPos;

    private Vector2 endPixel;

    private Action reachCb;

    private float time;

    private float ticker = 0.0f;

    private int offsetX;

    private int offsetY;

    void Update()
    {
        ticker += Time.deltaTime;
        if (ticker > time)
        {
            reachCb?.Invoke();
            reachCb = null;
        }
        else
        {
            var newPos =
                CalculateBezierPoint(ticker / time,
                startPixel,
                controlPos,
                endPixel);
            gameObject.transform.LookAt (newPos);
            gameObject.transform.Rotate(new Vector3(0, -90, 0));
            gameObject.transform.localPosition = newPos;
        }
    }

    Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    public ParabolaPath
    Init(
        Vector2 startPixel,
        Vector2 endPixel,
        float time,
        float heightFactor = 0
    )
    {
        gameObject.transform.localPosition = startPixel;
        ticker = 0;
        this.time = time;
        this.startPixel = startPixel;
        this.endPixel = endPixel;
        controlPos =
            new Vector2((startPixel.x + endPixel.x) / 2,
                (startPixel.y + endPixel.y) / 2 +
                Math.Abs(startPixel.x - endPixel.x) * heightFactor);
        return this;
    }
}
