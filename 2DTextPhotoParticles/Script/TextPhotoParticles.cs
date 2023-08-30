using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPhotoParticles : MonoBehaviour
{
    public enum enumRenderType
    {
        RenderTexture2D, //图片渲染
        RenderText, //文字渲染
    }

    public enum enumTextureColorType
    {
        WhiteColor, //使用白色
        GrayColor, //将彩色图片转变为灰度
        OriginalColor, //使用图片原本采样的颜色
        CustomColor, //使用自定义颜色
        RandomColor, //随机颜色
    }

    public enum enumMouseInteractive
    {
        None, //不使用鼠标
        MouseZoom, //鼠标缩放粒子
        MouseBounce, //鼠标弹开粒子
    }

    public enum enumParticlesMode
    {
        ParticleConverge, //粒子聚拢
        ParticleDisperse, //粒子散开
    }


    [Header("Common")] public enumRenderType renderType = enumRenderType.RenderText;
    public enumParticlesMode particleMode = enumParticlesMode.ParticleConverge;
    public float disperseMinPosition = -30;
    public float disperseMaxPosition = 30;
    public float disperseSpeed = 0.1f;

    [Header("Image Render")] public Texture2D drawTexture;

    [Header("Text Render")] public Font fontName;
    public string text = "";

    public int textSpace = 2;
    //public int textFontSize = 28;
    //public string textFontName = "Arial";

    [Header("Color")] public enumTextureColorType fillColorType = enumTextureColorType.WhiteColor;
    public Color32 customColor = new Color32(255, 255, 255, 255);

    [Header("Drawing")] public float drawSpeed = 1f;
    public float drawScale = 10f;
    public int drawDensity = 1;


    [Header("Mouse Interactive")] public enumMouseInteractive mouseInteractive = enumMouseInteractive.None;
    public float mouseZoomDistance = 10f;
    public float mouseZoomScale = 2f;

    public float mouseBounceDistance = 10f;
    public float mouseBounceScale = 2f;

    [Header("Random")] public bool isRandomSize = true;
    public float normalSize = 2;
    public float randomMinSize = 1;
    public float randomMaxSize = 5;

    public bool isRandomAlpha = true;
    public float normalAlpha = 100;
    public float randomMinAlpha = 50;
    public float randomMaxAlpha = 200;

    [Header("Animation")] public bool isAnimation = true;
    public float animationSpeed = 0.8f;
    public float animationRange = 0.2f;

    private ParticleSystem PS;
    private ParticleSystem.MainModule _psmm;

    private List<ParticlePointInfo> calcPoints = new List<ParticlePointInfo>();

    // Use this for initialization
    void Start()
    {
        PS = GetComponent<ParticleSystem>();

        _psmm = PS.main;
        _psmm.simulationSpace = ParticleSystemSimulationSpace.Custom;
        _psmm.customSimulationSpace = transform;
        _psmm.startLifetime = 100000;

        transform.localRotation = Quaternion.identity;

        if (renderType == enumRenderType.RenderText)
            MakeTextParticle(text);
        else if (renderType == enumRenderType.RenderTexture2D)
            MakeTextureParticle(drawTexture);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateParticles();
        UpdateMouse();
    }

    public void MakeTextParticle(string newText)
    {
        TextConverter cov = new TextConverter();
        Texture2D tex = cov.TextToTexture(fontName, newText, textSpace, Color.white);
        MakeTextureParticle(tex);
    }

    public void MakeTextureParticle(Texture2D tex)
    {
        //ResetParticlePosition();
        calcPoints.Clear();

        int halfWidth = tex.width / 2;
        int halfHeight = tex.height / 2;

        for (int i = 0; i < tex.height; i += drawDensity)
        {
            for (int j = 0; j < tex.width; j += drawDensity)
            {
                Color32 c = tex.GetPixel(j, i);
                if (c.r != 0 || c.g != 0 || c.b != 0)
                {
                    Vector3 desPos = new Vector3((j - halfWidth) / drawScale, (i - halfHeight) / drawScale, 0);
                    ParticlePointInfo info = new ParticlePointInfo(isAnimation, animationSpeed, animationRange);
                    info.SetPoint(desPos, disperseMinPosition, disperseMaxPosition);

                    if (isRandomSize)
                        info.SetSize(Random.Range(randomMinSize, randomMaxSize));
                    else
                        info.SetSize(normalSize);

                    byte alpha = (byte)normalAlpha;
                    if (isRandomAlpha)
                        alpha = (byte)Random.Range(randomMinAlpha, randomMaxAlpha);

                    switch (fillColorType)
                    {
                        case enumTextureColorType.WhiteColor:
                            info.color = new Color32(255, 255, 255, alpha);
                            break;
                        case enumTextureColorType.CustomColor:
                            info.color = new Color32(customColor.r, customColor.g, customColor.b, alpha);
                            break;
                        case enumTextureColorType.OriginalColor:
                            info.color = new Color32(c.r, c.g, c.b, alpha);
                            break;
                        case enumTextureColorType.GrayColor:
                            float grayFloat = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                            byte gray = (byte)grayFloat;
                            //byte grayG = Convert.ToByte(grayFloat * 255.0f);
                            //byte grayB = Convert.ToByte(grayFloat * 255.0f );

                            info.color = new Color32(gray, gray, gray, alpha);

                            break;
                        case enumTextureColorType.RandomColor:
                            info.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255),
                                (byte)Random.Range(0, 255), alpha);
                            break;
                    }


                    calcPoints.Add(info);
                }
            }
        }
    }

    void UpdateParticles()
    {
        ParticleSystem.Particle[] ps = new ParticleSystem.Particle[PS.particleCount];
        int pCount = PS.GetParticles(ps);
        //ParticleSystem.Particle[] ps = new ParticleSystem.Particle[calcPoints.Count];
        //int pCount = calcPoints.Count;
        //Color32[] colors = tex.GetPixels32();

        //int pIndex = 0;

        for (int i = 0; i < calcPoints.Count; i++)
        {
            if (i < pCount)
            {
                Vector3 direction;

                if (particleMode == enumParticlesMode.ParticleConverge)
                {
                    direction = calcPoints[i].GetPoint() - ps[i].position;

                    if (mouseInteractive == enumMouseInteractive.MouseBounce)
                        calcPoints[i].SetParticlePoint(ps[i].position);


                    if (calcPoints[i].isBounce == false)
                    {
                        ps[i].velocity = direction * drawSpeed; // * (1.0f / 10f);
                        //else
                        //    ps[pIndex].velocity = Vector3.zero;
                    }
                    else
                    {
                        ps[i].position = calcPoints[i].GetPoint();
                    }
                }
                else
                {
                    Vector3 pos = calcPoints[i].GetDispersePoint();
                    direction = pos - ps[i].position;

                    float speed = disperseSpeed;
                    if (Vector3.Distance(pos, ps[i].position) < 1.0f)
                        speed = drawSpeed;

                    ps[i].velocity = direction * speed; // * (1.0f / 10f);
                }

                ps[i].startSize = calcPoints[i].GetSize();
                ps[i].startColor = calcPoints[i].color;

                //ps[i].startLifetime = 10000;
            }
        }

        //隐藏多余的粒子（如果有的话）
        for (int j = calcPoints.Count; j < pCount; j++)
        {
            //ps[j].startLifetime =0;
            //ps[j].position = new Vector3(-10000, -10000);
            ps[j].startColor = new Color32(255, 255, 255, 0);
            ps[j].startSize = 0;
        }

        PS.SetParticles(ps, pCount);
    }

    void UpdateMouse()
    {
        if (mouseInteractive == enumMouseInteractive.None)
            return;

        //计算鼠标碰撞点
        RaycastHit hitt = new RaycastHit();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out hitt);

        float x = hitt.point.x;
        float y = hitt.point.y;

        switch (mouseInteractive)
        {
            case enumMouseInteractive.MouseZoom:
                for (int i = 0; i < calcPoints.Count; i++)
                {
                    calcPoints[i].SetMouseZoomPoint(x, y, mouseZoomDistance, mouseZoomScale);
                }

                break;
            case enumMouseInteractive.MouseBounce:
                for (int i = 0; i < calcPoints.Count; i++)
                {
                    calcPoints[i].SetMouseBounce(x, y, mouseBounceDistance, mouseBounceScale);
                }

                break;
        }
    }
}