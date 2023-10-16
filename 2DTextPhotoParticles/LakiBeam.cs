using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Rect = OpenCVForUnity.CoreModule.Rect;

/// <summary>
/// LakiBeam1 型号雷达接收数据
/// </summary>
public class LakiBeam : MonoBehaviour
{
    #region 数据包

    [Serializable]
    public struct DataBlock
    {
        /// <summary> 标志位 有效为0xFFEE </summary>
        public bool flag;

        /// <summary> 水平旋转角度信息 </summary>
        public float azimuth;

        /// <summary> 测量结果 </summary>
        public MeasuringResult[] results;

        /// <summary>  </summary>
        /// <param name="dataBlock">长度应为100 2flag ; 2azimuth ; (6 x 16)Measuring Result </param>
        public DataBlock(byte[] dataBlock)
        {
            byte flag1 = dataBlock[0];
            byte flag2 = dataBlock[1];
            flag = flag1 == 0xFF && flag2 == 0xEE;

            byte[] azimuthArr = { dataBlock[2], dataBlock[3] };
            azimuth = BitConverter.ToUInt16(azimuthArr, 0) * 0.01f;

            int measuringLen = 16;
            int resultLen = 6;
            results = new MeasuringResult[measuringLen];
            for (int i = 0; i < measuringLen; i++)
            {
                //单个Measuring Result长度为6 resultLen
                byte[] measuringResult = new byte[resultLen];

                Array.Copy(dataBlock, (2 + 2) + (resultLen * i), measuringResult, 0, resultLen);

                byte[] dis1Arr = new[] { measuringResult[0], measuringResult[1] };
                byte rssi_1st = measuringResult[2];

                byte[] dis2Arr = new[] { measuringResult[3], measuringResult[4] };
                byte rssi_2st = measuringResult[5];

                var mr = new MeasuringResult();
                mr.SetDisRssi(dis1Arr, rssi_1st, dis2Arr, rssi_2st);
                results[i] = mr;
                // Debug.Log($"DB:{BitConverter.ToString(dataBlock)}\n  MR{i}:{BitConverter.ToString(measuringResult)}\n 结果:{results[i].ToString()}");
            }
        }
    }

    [Serializable]
    public struct MeasuringResult
    {
        /// <summary> 距离 单位mm </summary>
        public int distance;

        /// <summary> 反射强度 </summary>
        public int rssi;

        /// <summary> 水平角度 单位角度°</summary>
        public float angle;

        public float x;

        public float y;

        public void SetDisRssi(
            byte[] dis1Arr, byte rssi_1st,
            byte[] dis2Arr, byte rssi_2st)
        {
            int dis1 = BitConverter.ToUInt16(dis1Arr, 0);
            int rssi1 = Convert.ToUInt16(rssi_1st);

            int dis2 = BitConverter.ToUInt16(dis2Arr, 0);
            int rssi2 = Convert.ToUInt16(rssi_2st);

            if (dis1 != 0 && dis2 == 0)
            {
                distance = dis1;
                rssi = rssi1;
            }
            else if (dis1 == 0 && dis2 != 0)
            {
                distance = dis2;
                rssi = rssi2;
            }
            else if (dis1 != 0 && dis2 != 0)
            {
                if (rssi1 > rssi2)
                {
                    distance = dis1;
                    rssi = rssi1;
                }
                else
                {
                    distance = dis2;
                    rssi = rssi2;
                }
            }
            else
            {
                distance = 0;
                rssi = 0;
            }
        }

        public override string ToString()
        {
            return $"[距离:{distance},反射:{rssi},角度:{angle},点:({x},{y})]";
        }
    }

    /// <summary> 单个数据包 </summary>
    [Serializable]
    public struct DataPacket
    {
        public DataBlock[] blocks; //12个数据块
        // public uint timeStamp; //时间戳 无用
        // public ushort factory; //工厂信息 无用

        public void SetData(byte[] dataSource)
        {
            int blockLength = 100; //2 + 2 + (16 * 6) 单个Data Block长度
            int dbCount = 12; //12个Data Block
            blocks = new DataBlock[dbCount];

            float delta = 0.25f; //Mathf.Abs(azimuth0 - azimuth1) / 16f; //角度递增值 每不同型号的雷达设备不一样 LaKiBeam1L为0.25
            int mrCount = 16; //单个DataBlock测量结果数量

            for (int i = 0; i < dbCount; i++)
            {
                byte[] db = new byte[blockLength]; //单条Data Block 长度为100
                Array.Copy(dataSource, i * blockLength, db, 0, blockLength);

                blocks[i] = new DataBlock(db);
                var block = blocks[i];
                var flag = block.flag;
                float azimuth = block.azimuth;
                for (int j = 0; j < mrCount; j++)
                {
                    var result = block.results[j];
                    float distance = result.distance;
                    if (flag && distance > 0)
                    {
                        float angle = azimuth + (delta * j);
                        float radians = (angle + LakiBeam.rotate) * Mathf.Deg2Rad;
                        float x = distance * Mathf.Sin(radians) * LakiBeam.scale * (LakiBeam.XFlip ? -1 : 1) +
                                  LakiBeam.XOffset;
                        float y = distance * Mathf.Cos(radians) * LakiBeam.scale * (LakiBeam.YFlip ? -1 : 1) +
                                  LakiBeam.YOffset;
                        result.angle = angle;
                        result.x = x;
                        result.y = y;

                        // LakiBeam.instance.SpawnTest(x, y);
                        // LakiBeam.instance.AddRectangle(x, y);
                        // Debug.Log($"DataBlock{i} 有效：{flag} 方位:{azimuth} id:{j} 角度:{angle} 距离:{result.distance}");
                    }
                }
            } 
        }
    }

    #endregion

    private int listenPort = 2346;
    [ShowInInspector, Range(0f, 360f)] public static float rotate = 0f;
    [ShowInInspector, Range(0.001f, 3f)] public static float scale = 0.04f;
    [ShowInInspector, Range(-500f, 500f)] public static float XOffset = 0f;
    [ShowInInspector, Range(-500f, 500f)] public static float YOffset = 0f;
    [ShowInInspector] public static bool XFlip;
    [ShowInInspector] public static bool YFlip;
    [SerializeField] private Transform parentLDItem = null;
    [SerializeField] private RectTransform prefabLDItem = null;
    [SerializeField] private RawImage rawImageOrigin = null;
    public bool HasInit { get; private set; } = false;
    private Texture2D originTex = null;
    private Mat originMat = null;
    private Mat drawMat = null;

    public Mat OriginMat
    {
        get { return originMat; }
    }

    private List<RectTransform> listLDItem = new List<RectTransform>();
    private int listMax = 2960; //12960
    private int tempCurIdx = 0;

    private float width = 640;
    private float height = 480;
    private float whRatio;

    public static LakiBeam instance { get; private set; } = null;

    private SingleSocket socket = null;


    public void StartLidar(int port = 2346)
    {
        if (HasInit == false)
        {
            this.listenPort = port;
            Init();
        }
    }

    public void StopLidar()
    {
        TimerManager.Instance.RemoveTimer(nameof(ClearMat));

        if (socket != null)
        {
            socket.Dispose();
        }

        if (originMat != null)
        {
            originMat.Dispose();
            originMat.release();
        }

        HasInit = false;
    }

    public void SetFlip(bool flipX, bool flipY)
    {
        XFlip = flipX;
        YFlip = flipY;
    }

    public void SetTransform(float offsetX, float offsetY, float rotate, float scale, int thickness)
    {
        XOffset = offsetX;
        YOffset = offsetY;
        LakiBeam.rotate = rotate;
        LakiBeam.scale = scale;
        this.thickness = thickness;
    }

    private void Init()
    {
        if (HasInit == false)
        {
            instance = this;

            whRatio = height / width;

            originMat = new Mat((int)height, (int)width, CvType.CV_8UC4);

            if (rawImageOrigin != null)
            {
                drawMat = new Mat((int)height, (int)width, CvType.CV_8UC4);
                originTex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                rawImageOrigin.texture = originTex;
            }

            // SocketManager.Instance.Init(listenPort, 1206); //1206
            socket = new SingleSocket();
            socket.Init(listenPort, 1206);

            TimerManager.Instance.AddTimer(nameof(ClearMat), clearInterval, ClearMat);

            HasInit = true;
        }
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Dispose();
        }
    }


    public void LdUpdate()
    {
        var DataQueue = socket.DataQueue;
        lock (DataQueue)
        {
            for (int i = 0, length = DataQueue.Count; i < length; i++)
            {
                var data = DataQueue.Dequeue();
                Deserialize(data);
            }
        }
    }

    private Rect rect = new Rect();
    [Title("Opencv")] [SerializeField] private int rectWidth = 10;
    [SerializeField] private int thickness = -1;
    private Scalar pointColor = new Scalar(51, 247, 51, 255);
    [SerializeField] private bool flip = false;
    [SerializeField, Range(-1, 1)] private int flipCode = 0;
    [SerializeField] private float cvXOffset = 320;
    [SerializeField] private float cvYOffset = 240;
    private const float CvScale = 0.00208f;
    [SerializeField] private float clearInterval = 0.05f;

    private void Deserialize(byte[] data)
    {
        DataPacket dp = new DataPacket();
        dp.SetData(data);
        for (int blocksIdx = 0, blocksL = dp.blocks.Length; blocksIdx < blocksL; blocksIdx++)
        {
            var block = dp.blocks[blocksIdx];
            for (int rIdx = 0, rL = block.results.Length; rIdx < rL; rIdx++)
            {
                var mResult = block.results[rIdx];
                // Debug.Log($"Block:{blocksIdx} result:{rIdx} ({mResult.x},{mResult.y})");
                AddRectangle(mResult.x, mResult.y);
            }
        }

        if (rawImageOrigin != null)
        {
            originMat.copyTo(drawMat);
            Utils.matToTexture2D(drawMat, originTex, flip, flipCode);
        }
    }

    public void AddRectangle(float x, float y)
    {
        int mX = (int)(width * whRatio * ((x + cvXOffset) * CvScale));
        int mY = (int)(height * ((y + cvYOffset) * CvScale));
        rect.set(mX, mY, rectWidth, rectWidth);
        Imgproc.rectangle(originMat, rect, pointColor, thickness);
    }

    private void ClearMat()
    {
        originMat.setTo(new Scalar(0, 0, 0, 0));
    }

    /// <summary> 编辑器测试显示真实点位 </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SpawnTest(float x, float y)
    {
        if (listLDItem.Count < listMax)
        {
            for (int i = 0; i < listMax; i++)
            {
                var newGo = Instantiate(prefabLDItem, parentLDItem);
                newGo.anchoredPosition = Vector2.zero;
                listLDItem.Add(newGo);
            }
        }

        var item = listLDItem[tempCurIdx];
        item.anchoredPosition = new Vector2(x, y);

        tempCurIdx++;
        if (tempCurIdx >= listMax)
        {
            tempCurIdx = 0;
        }
    }
}