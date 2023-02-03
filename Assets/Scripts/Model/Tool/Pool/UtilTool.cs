using System;
using UnityEngine;
using TrueSync;
public static class UtilTool
{
    public static readonly string SessionSecrect = "pemelo_session_secret_winddy";

    public static TResult SafeExecute<TResult>(Func<TResult> rFunc)
    {
        if (rFunc == null) return default(TResult);
        return rFunc();
    }


    public static float GetAngle2D(Vector2 v1,Vector2 t)
    {
        Vector2 vector = t - v1;
        var angle = Mathf.Atan2(vector.y, vector.x) * 180 / Mathf.PI;
        if (angle < 0 || angle > 180)
            angle += 360;
        return angle;
    }
}