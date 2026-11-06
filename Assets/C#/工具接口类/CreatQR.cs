using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class VehicleSave
{
    public string[] carsave;
}
public class CreatQR : MonoBehaviour,IPointerClickHandler
{

    //需要生产二维码的字符串数组
    public string QrCodeStr;
    //在屏幕上显示二维码
    public RawImage image;
    //存放二维码
    Texture2D encoded;
    int Nmuber = 0;
    // Use this for initialization
    void Start()
    {

        encoded = new Texture2D(256, 256);
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Btn_CreatQr();
            if (!Application.isMobilePlatform)
            {
                SaveRenderTextureToPNG("QR");
            }
            else
            {
                GetTexture2d("QR");
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            byte[] j =  Encoding.UTF8.GetBytes(QrCodeStr);//中文编码
            string g = Convert.ToBase64String(j);//编码转Base
            byte[] a = Convert.FromBase64String(g);//Base转byte
            string d = Encoding.UTF8.GetString(a);//byte转string
            print(BitConverter.ToString(j));
            print(g);
            print(d);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            var vs = JsonUtility.FromJson<VehicleSave>(File.ReadAllText(FileUtils.GetFilesInPersistentFolder("VehicleDesign")[1]));
            List<string> sl = new();
            sl.AddRange(vs.carsave);
            QrCodeStr = CompressSave(sl);
        }
    }
#endif

    //摄像头实时显示的画面
    private WebCamTexture m_webCameraTexture;
    //申请一个读取二维码的变量
    private BarcodeReader m_barcodeRender = new BarcodeReader();

    //多久检索一次二维码
    private float m_delayTime = 0.5f;


    public void Camera()
    {
        //调用摄像头并将画面显示在屏幕RawImage上
        WebCamDevice[] tDevices = WebCamTexture.devices; //获取所有摄像头
        string tDeviceName = tDevices[0].name; //获取第一个摄像头，用第一个摄像头的画面生成图片信息
        m_webCameraTexture = new WebCamTexture(tDeviceName,512,512); //名字
        image.texture = m_webCameraTexture; //赋值图片信息
        m_webCameraTexture.Play(); //开始实时显示
        InvokeRepeating("CheckQRCode", 0, m_delayTime);
    }

    /// <summary>
    /// 检索二维码方法
    /// </summary>
    void CheckQRCode()
    {
        //存储摄像头画面信息贴图转换的颜色数组
        Color32[] m_colorData = m_webCameraTexture.GetPixels32();

        //将画面中的二维码信息检索出来
        var tResult = m_barcodeRender.Decode(m_colorData, m_webCameraTexture.width, m_webCameraTexture.height);

        if (tResult != null)
        {
            CancelInvoke("CheckQRCode");
            m_webCameraTexture.Stop();

            List<string> ss = DepressSave(tResult.ToString());
            foreach (string s in ss)
            {
                print(s);
            }
        }
    }

    public string CompressSave(List<string> save)
    {
        byte[] b = Encoding.UTF8.GetBytes(save[0]);
        string BaseName = Convert.ToBase64String(b);
        string FinalSave = BaseName + "<n";
        for (int i = 1; i < save.Count; i++)
        {
            FinalSave += save[i] + ",";
        }
        print(FinalSave);
        FinalSave = GZipCompressString(FinalSave);
        print(FinalSave);
        return FinalSave;
    }
    public string CompressSave(string Name, int width, List<float> BlockItem)
    {
        byte[] b = Encoding.UTF8.GetBytes(Name);
        string BaseName = Convert.ToBase64String(b);
        string FinalSave = BaseName + "<n";
        FinalSave += width + ",";
        for (int i = 0; i < BlockItem.Count; i++)
        {
            FinalSave += BlockItem[i] + ",";
        }
        FinalSave = GZipCompressString(FinalSave);
        return FinalSave;
    }

    public List<string> DepressSave(string RawSave)
    {
        string Save = GZipDecompressString(RawSave);
        int NameEnd = Save.LastIndexOf("<n");
        string Name = Save.Substring(0, NameEnd);
        byte[] b = Convert.FromBase64String(Name);
        Name = Encoding.UTF8.GetString(b);
        Save = Save.Substring(NameEnd + 6);
        List<string> FinalSave = new() { Name };
        do
        {
            int k = Save.IndexOf(",");
            FinalSave.Add(Save.Substring(0, k));
            Save = Save.Substring(k + 1);
        } while (Save.Contains(","));
        return FinalSave;
    }

    /// <summary>
    /// 定义方法生成二维码
    /// </summary>
    /// <param name="textForEncoding">需要生产二维码的字符串</param>
    /// <param name="width">宽</param>
    /// <param name="height">高</param>
    /// <returns></returns>
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }


    /// <summary>
    /// 生成二维码
    /// </summary>
    public Texture2D Btn_CreatQr()
    {
        //二维码写入图片
        var color32 = Encode(QrCodeStr, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        //生成的二维码图片附给RawImage
        image.texture = encoded;
        return encoded;
    }

    #region  压缩和解压字符串
        /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }
        }
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        static byte[] Compress(byte[] rawData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// 解压Sring 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public string GetStringByString(string Value)
        {
            //DataSet ds = new DataSet();
            string CC = GZipDecompressString(Value);
            //System.IO.StringReader Sr = new System.IO.StringReader(CC);
            //ds.ReadXml(Sr);
            return CC;
        }

        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
            }
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public byte[] Decompress(byte[] zippedData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(zippedData);
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
            System.IO.MemoryStream outBuffer = new System.IO.MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();

        }
    #endregion

#if UNITY_EDITOR
    private void SaveRenderTextureToPNG(string textureName, Action<TextureImporter> importAction = null)
    {
        string path = EditorUtility.SaveFilePanel("Save to png", Application.dataPath, textureName + "_painted.png", "png");
        if (path.Length != 0)
        {
            byte[] pngData = encoded.EncodeToPNG();
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
                AssetDatabase.Refresh();
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importAction != null)
                    importAction(importer);
            }

            Debug.Log(path);
        }
    }
#endif
    /// <summary>
    /// 图片存储至本地相册
    /// </summary>
    /// <param name="target">需要存储的指定图片</param>
    private void GetTexture2d(string Name)
    {
        // 编码纹理为PNG格式
        byte[] bytes = encoded.EncodeToPNG();
        //存储路径
        string path = "";
        //应用平台判断，路径选择
        if (Application.platform == RuntimePlatform.Android)
        {
            //手机文件管理中的存储位置：我的手机/Android/data/come.XX.XX(PackageName)/files/XXX.png
            path = Application.persistentDataPath + "/" + Name + ".png";
        }
        //保存文件
        File.WriteAllBytes(path, bytes);
        //调用安卓方法，刷新相册
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                //为安卓方法提供图片存储的路径参数，方便刷新至相册
                jo.Call("file", path);
            }
        }
    }

    bool OpenCamera;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!OpenCamera)
        {
            Camera();
            OpenCamera = true;
        }
    }
}
