using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelCamera : MonoBehaviour
{
    public Camera _camera;
    public List<Transform> all_cars;

    public static float CameraMove;

    public GameManager.GameState _gameState;

    public Vector3 RelativePos;
    public float RelativeSize;
    public Vector3 AimPos;
    public float AimSize;
    public Transform CameraTransform;

    public float removeSize;
    public float scalingSize;

    public Vector2 CameraViewLimit = new(50, 70);
    //是否是移动端
    public static bool IsMobilePlatform;
    public bool SetMobile;
    //视角旋转组件
    public bool RotateCamera;
    public bool ReallyMode;

    public float MouseScrollSensitivity = 50f;
    public float TouchScrollSensitivity = 2f;

    public AudioClip[] ac;
    public AudioSource asource;

    private void Awake()
    {
        AimPos = new(-2, -2, -2);
        if (Camera.main.name == "VideoMode")
        {
            GameManager.gameState = GameManager.GameState.Start;
        }
        else
        {
            GameManager.gameState = GameManager.GameState.Watch;
        }
        Application.targetFrameRate = 60;
    }
    void Start()
    {
        IsMobilePlatform = Application.isMobilePlatform;
    }
    public void TurnTo(string state)
    {
        switch (state)
        {
            case "Start":
                startbool = false;
                all_cars = new();
                GameManager.gameState = GameManager.GameState.Start;
                if (asource.clip != ac[1])
                {
                    asource.clip = ac[1];
                    asource.Play();
                }
                break;
            case "Ready":
                all_cars .Clear();
                all_cars.Add(
                GameObject.FindGameObjectWithTag("Player").transform);
                if (GameManager.gameState != GameManager.GameState.Ready)
                {
                    GameManager.gameState = GameManager.GameState.Ready;
                    AimPos = new Vector3(-2, -2, -2);
                }
                if (asource.clip != ac[0])
                {
                    asource.clip = ac[0];
                    asource.Play();
                }
                real = false;
                break;
            case "Watch":
                GameManager.gameState = GameManager.GameState.Watch;
                AimPos = new Vector3(-2, -2, -2);
                if (asource.clip != ac[0])
                {
                    asource.clip = ac[0];
                    asource.Play();
                }
                break;
            case "Win":
                GameManager.gameState = GameManager.GameState.Win;
                if (asource.clip != ac[2])
                {
                    asource.clip = ac[2];
                    asource.Play();
                    asource.loop = false;
                }
                break;

        }
    }
    float Y_angle = 0;
    float Z_angle = 0;

    Vector3 lastPosition;

    public static bool NonMove;

    Vector3 ReadyPos;
    float ReadyScale = 1.145f;

    private void OnValidate()
    {
        GameManager.gameState = _gameState;
    }

    void Update()
    {
        if (PutModeChange.Line)
        {
            Down = false;
            return;
        }
        if (NonMove)
        {
            Down = false;
            return;
        }
        CameraMove = (lastPosition - RelativePos).magnitude / (AimSize + RelativeSize) * 100f;
        lastPosition = RelativePos;
        _gameState = GameManager.gameState;
        switch (GameManager.gameState)
        {
            case GameManager.GameState.TurnWin:
                TurnTo("Win");
                break;
            case GameManager.GameState.Watch:
                if (AimPos == new Vector3(-2, -2, -2))
                {
                    all_cars.Clear();
                    GameObject check = GameObject.FindGameObjectWithTag("Player");
                    if (check != null)
                    {
                        all_cars.Add(check.transform);
                    }
                    check = GameObject.FindGameObjectWithTag("Enemy");
                    if (check != null)
                    {
                        all_cars.Add(check.transform);
                    }
                    check = GameObject.FindGameObjectWithTag("Flag");
                    if (check != null)
                    {
                        all_cars.Add(check.transform);
                    }
                    RelativeSize = 2f;
                    RelativePos = new();
                    if (all_cars != null)
                    {
                        float MaxX = -1000000, MaxY = -1000000, MinX = 1000000, MinY = 1000000;
                        for (int i = 0; i < all_cars.Count; i++)
                        {
                            Vector2 v2 = all_cars[i].position;
                            if (v2.x > MaxX) MaxX = v2.x;
                            if (v2.x < MinX) MinX = v2.x;
                            if (v2.y > MaxY) MaxY = v2.y;
                            if (v2.y < MinY) MinY = v2.y;
                        }
                        AimPos = new Vector3((MaxX + MinX), (MaxY + MinY)) / 2f;
                        AimSize = MaxY - MinY;
                        float f1 = (MaxX - MinX) / Screen.width * Screen.height;
                        if (f1 > AimSize)
                        {
                            AimSize = f1;
                        }
                        AimSize /= 2f;
                    }
                }
                else
                {
                    MovePart();
                    SmoothTo();
                }
                break;
            case GameManager.GameState.Ready:
                if (all_cars.Count == 0 || all_cars[0] == null)
                {
                    TurnTo("Ready");
                    return;
                }
                if (AimPos == new Vector3(-2, -2, -2))
                {
                    if (ReadyScale == 1.145)
                    {
                        RelativePos = new(0, -0.5f);
                        RelativeSize = 1.2f;
                    }
                    else
                    {
                        RelativePos = ReadyPos;
                        RelativeSize = ReadyScale;
                    }
                    Z_angle = 0;
                    AimPos = all_cars[0].position;
                    Vector2 Csize = all_cars[0].GetComponent<OwnCar>().ColliderArea;
                    AimSize = Csize.y / 1.75f;
                    float f1 = Csize.x / Screen.width * Screen.height;
                    if (f1 > AimSize) AimSize = f1;
                }
                else
                {
                    MovePart();

                    SmoothTo();
                }
                ReadyPos = RelativePos;
                ReadyScale = RelativeSize;
                break;
            case GameManager.GameState.Anim:
               
                break;
            case GameManager.GameState.Start:
                Vector3 v3;
                for (int i = 0; i < all_cars.Count; i++)
                {
                    if (all_cars[i] != null)
                    {
                        v3 = all_cars[i].transform.position;
                        AimPos = v3;
                        if (RotateCamera)
                        {
                            Z_angle = all_cars[i].transform.eulerAngles.z;
                        }
                        else
                        {
                            Z_angle = 0;
                        }
                        real = ReallyMode;
                        if (!startbool)
                        {
                            rr = RelativePos = transform.position - AimPos;
                            startbool = true;
                        }
                        break;
                    }
                    else
                    {
                        all_cars.RemoveAt(i);
                        i--;
                    }
                }
                MovePart();
                SmoothTo();
                break;
        }
       
    }
    bool startbool = false;
    private bool last;

    private bool OnUI;
    bool Down;
    Vector3 MX;
    public void MovePart()
    {
        if (Input.GetMouseButtonDown(2))
        {
            OnUI = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnUI = IsOnUIElement(Input.mousePosition);
        }

        //Remove
        if (Input.touchCount == 2)
        {
            if (Input.GetTouch(1).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Began)
            {
                oldDisBetweenTouch = (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;
                lastScale = AimSize + RelativeSize;
                MX = (Input.GetTouch(1).position + Input.GetTouch(0).position) / 2f;
                last = true;
            }
            if (oldDisBetweenTouch != 0)
            {
                float newDisBetweenTouch = (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;
                float new_scale = newDisBetweenTouch / oldDisBetweenTouch;
                if (new_scale != 0)
                    RelativeSize = Mathf.Clamp(lastScale / new_scale, CameraViewLimit[0] - AimSize, CameraViewLimit[1] - AimSize) - AimSize;
            }
            Vector3 center = (Input.GetTouch(1).position + Input.GetTouch(0).position) / 2f;
            RelativePos -= (center - MX) * ((RelativeSize + AimSize + 1) / Screen.height) * 2f;
            MX = center;
        }
        else
        {
            if (!OnUI)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
                {
                    Down = true;
                    MX = Input.mousePosition;
                }
                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2))
                {
                    Down = false;
                }

                if (last)
                {
                    last = false;
                    if (Input.touchCount == 1)
                    {
                        MX = Input.mousePosition;
                        Down = true;
                    }
                    else
                    {
                        Down = false;
                    }
                }

                if (Down)
                {
                    RelativePos -= (Input.mousePosition - MX) * ((AimSize + RelativeSize + 1) / Screen.height) * 2f;
                    MX = Input.mousePosition;
                }
            }
        }

        //PC
        float KeyBoradInput = 0;
        KeyBoradInput *= (scalingSize - 50) / 100 + 1;
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            KeyBoradInput += Input.GetAxis("Mouse ScrollWheel") * MouseScrollSensitivity * ((scalingSize - 50) / 100 + 1);
        }
        RelativeSize = Mathf.Clamp(RelativeSize - KeyBoradInput, CameraViewLimit[0] - AimSize, CameraViewLimit[1] - AimSize);

    }

    private float oldDisBetweenTouch;
    private float lastScale;

    //下面这段是抄的，能跑
    private EventSystem eventSystem;
    private PointerEventData eventData;
    private bool init = false;
    void Init()
    {
        if (!init)
        {
            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);
            init = true;
        }
    }
    public bool IsOnUIElement(Vector2 pos)
    {
        Init();
        eventData.pressPosition = pos;
        eventData.position = pos;
        List<RaycastResult> list = new();
        EventSystem.current.RaycastAll(eventData, list);


        foreach (var temp in list)
        {
            if (temp.gameObject.layer.Equals(5))
            {
                return true;//Equal(5)中的“5”是指图层第五层UI层
            }
        }
        return false;
    }
    //抄的到这里结束


    Vector3 rv;
    float rf;
    float yf;
    float sf;
    float sff;
    public float MoveTime;
    public float FollowTime;
    public float SizeTime;
    Vector3 rr;
    bool real;
    public void SmoothTo()
    {
        rr = Vector3.SmoothDamp(rr, RelativePos, ref rv, MoveTime);
        transform.position = AimPos + Quaternion.AngleAxis(Z_angle, Vector3.forward) * rr + new Vector3(0, 0, -40f);
        if (real)
        {
            _camera.orthographic = false;
            Vector2 speed = new();
            if (SpeedShow.center != null)
            {
                speed = SpeedShow.center.velocity;
            }
            Y_angle = Mathf.SmoothDamp(Y_angle, Mathf.Min(80f, Mathf.Atan(speed.x / 90f) * Mathf.Rad2Deg), ref yf, SizeTime * 5f);
            sf = Mathf.Min(40f, Mathf.SmoothDamp(sf, Mathf.Pow(speed.magnitude, 0.5f) * 2f, ref sff, SizeTime * 5f));
            transform.position -= new Vector3(Mathf.Tan(Y_angle * Mathf.Deg2Rad) *(57 - sf), 0, -sf);
            _camera.fieldOfView = Mathf.Max(Mathf.SmoothDamp(_camera.fieldOfView, (AimSize + RelativeSize) + Mathf.Min(sf*0.5f ,10f), ref rf, SizeTime), 0.5f);
        }
        else
        {
            _camera.orthographic = true;
            Y_angle = 0;
            _camera.orthographicSize = Mathf.Max(Mathf.SmoothDamp(_camera.orthographicSize, AimSize + RelativeSize, ref rf, SizeTime), 0.5f);
        }
        transform.eulerAngles = new(0, Y_angle, Z_angle);
    }
}
