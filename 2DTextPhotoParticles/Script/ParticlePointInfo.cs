using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePointInfo //: MonoBehaviour {
{ 
    //目标坐标
    Vector3 point;
    //当前粒子的坐标
    Vector3 currentPoint;
    //沿着圆心旋转的坐标
    Vector3 circlePoint;
    //大小尺寸
    float size;
    //经过鼠标处理过的鼠标
    public float circleSize;
    public Color32 color;

    Vector3 dispersePoint;

    float tick = 0;

    bool isAnim = true;
    float animSpeed = 0.8f;
    float animRange = 0.2f;

    public bool isBounce = false;

    public ParticlePointInfo(bool isAnimation, float animationSpeed, float animationRange)
    {
        tick = Random.Range(0, 100);
        isAnim = isAnimation;
        animSpeed = animationSpeed;
        animRange = animationRange;
    }

    public void SetPoint(Vector3 newPoint,float disperseMinPosition,float disperseMaxPosition)
    {
        point = newPoint;
        circlePoint = newPoint;

        dispersePoint.x = Random.Range(newPoint.x + disperseMinPosition, newPoint.x + disperseMaxPosition);
        dispersePoint.y = Random.Range(newPoint.y + disperseMinPosition, newPoint.y + disperseMaxPosition);

    }

    public Vector3 GetDispersePoint()
    {
        if (isAnim)
        {
            tick += Time.deltaTime;

            circlePoint.x = dispersePoint.x + animRange * Mathf.Sin(tick * animSpeed);
            circlePoint.y = dispersePoint.y + animRange * Mathf.Cos(tick * animSpeed);

            return circlePoint;
        }
        else
            return dispersePoint;

    }

    public Vector3 GetPoint()
    {
        if (isBounce)
            return circlePoint;


        if (isAnim)
        {
            tick += Time.deltaTime;

            circlePoint.x = point.x + animRange * Mathf.Sin(tick * animSpeed);
            circlePoint.y = point.y + animRange * Mathf.Cos(tick * animSpeed);

            return circlePoint;
        }
        else
            return point;


    }

    public void SetSize(float newSize)
    {
        size = newSize;
        circleSize = newSize;
    }

    public float GetSize()
    {
        return circleSize;
    }

    public void SetMouseZoomPoint(float rayX, float rayY,float zoomDistance,float zoomScale)
    {
        if (rayX == 0 && rayY == 0)
        {
            circleSize = size;
            return;
        }

        float distance = Vector3.Distance(point, new Vector3(rayX, rayY, 0));


        float scale = Mathf.Max(Mathf.Min(zoomScale - distance / zoomDistance, zoomDistance), 1f);


        //if (distance > 8)
        //    circleSize = size;
        //else
        //    circleSize =size + distance / 3 * size;

        circleSize = scale * size;
    }

    public void SetParticlePoint(Vector3 pos)
    {
        currentPoint = pos;

    }

    public void SetMouseBounce(float rayX,float rayY, float bounceDistance,float bounceScale)
    {
        if (rayX == 0 && rayY == 0)
        {
            circleSize = size;
            isBounce = false;
            return;
        }

        float dx = currentPoint.x - rayX;
        float dy = currentPoint.y - rayY;

        float d = Mathf.Sqrt(dx * dx + dy * dy);

        float orgD = Mathf.Sqrt(( point.x - rayX) * (point.x - rayX) + (point.y - rayY) * (point.y - rayY));

        float f = 0;

        if (orgD > bounceDistance)
        {
            //f = 0;
            isBounce = false;
        }
        else
        {
            isBounce = true;
            f = bounceScale / d;
        }

        float a = Mathf.Atan2(dy, dx);

        //if (d > 0)
        //    f = 0.5f / d ;
        //else
        //    f = 0;
        //float f = 0;

        Vector3 pos = currentPoint;

        pos.x += (point.x - pos.x) * 0.05f;
        pos.y += (point.y - pos.y) * 0.05f;

        pos.x += Mathf.Cos(a) * f;
        pos.y += Mathf.Sin(a) * f;
        //pos.x = pp.oldPos.x + Mathf.Cos(a) * f;
        //pos.y = pp.oldPos.y + Mathf.Sin(a) * f;
        pos.z = 0;

        circlePoint = pos;
    }
}
