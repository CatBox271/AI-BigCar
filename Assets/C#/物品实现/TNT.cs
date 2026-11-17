using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour,ION,IStart
{
    private Heath heath;
    public GameObject BoomWave;

    public float Radius = 5.5f;
    public float Hit = 10f;
    bool Video;
    private void Awake()
    {
        TryGetComponent(out heath);
    }
    private void Start()
    {
        SceneProp sceneProp = SceneProp.Instance;
        Video = sceneProp != null && (sceneProp.gameMode == SceneProp.GameMode.video || sceneProp.gameMode == SceneProp.GameMode.card);
    }


    bool Find;
    public bool Fly;

    public bool OnSet { get; set; }
    public float NumSet { get; set; }

    public bool Started;//被载具 SetStart 激活过；没激活的拖拽预览松手销毁时不炸
    public void SetStart(float t)
    {
        Started = true;
    }

    private void Update()
    {
        if (OnSet)
        {
            Boom();
            return;
        }
        if (!Video) return;
        if (transform.position.y > 10f)
        {
            Fly = true;
        }
        else
        {
            Fly = false;
        }
        if (Fly && !Find)
        {
            RaycastHit2D[] line = Physics2D.LinecastAll(transform.position, transform.position - new Vector3(0, 13));
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].transform.TryGetComponent(out Heath h))
                {
                    if (h.camp != heath.camp && h.HP > 0)
                    {
                        Find = true;
                        if (TryGetComponent(out RelativeJoint2D rj))
                        {
                            Destroy(rj);
                        }
                        heath.KeepRecord[0] += 5;
                        if (TryGetComponent(out Rigidbody2D rb)) rb.velocity = new(0, -20f);
                    }
                }
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Find && !collision.gameObject.name.Contains("Bullet"))
        {
            Boom();
        }
    }

    private static bool quitting;

    // 编辑器设置里关掉了 Domain Reload（EnterPlayModeOptions=3），静态字段不会在进 Play 时重置。
    // 一旦 OnApplicationQuit 把它写成 true，同一个域里之后每一局 TNT 死亡都会被 OnDestroy 里的
    // 第一道守卫挡掉（表现为“一局炸一局不炸”）。SubsystemRegistration 每次进 Play 都会执行，
    // 即使关着 Domain Reload 也一样，用来把这个闸门复位。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        quitting = false;
    }

    private void OnApplicationQuit() { quitting = true; }

    private void OnDestroy()
    {
        // 场景卸载 / 切场景 / 退出播放也会走这里，此时再爆炸只会往正在销毁的
        // 场景里塞 TNTWave。正常玩法里“被摧毁就爆炸”不经过这三道判断。
        if (quitting) return;                    // 应用退出
        if (!Application.isPlaying) return;      // 退出播放模式
        if (!gameObject.scene.isLoaded) return;  // 场景正在卸载/切换
        if (!Started) return;                    // 卡片拖拽预览/未激活的块，销毁不炸
        Boom();
    }

    bool boomed;
    public void Boom()
    {
        if (boomed) return;//Destroy 触发的 OnDestroy 会再进这里，只炸一次
        boomed = true;
        Color col = Color.white;
        int lay = 0;
        if (heath.camp == 1)
        {
            col = new Color(0.75f, 0.75f, 1f);
        }
        if (heath.camp == 2)
        {
            col = new Color(1f, 0.75f, 0.75f);
        }
        TNTWave.Spawn(BoomWave, transform.position, transform.rotation).Setting(col, lay, Radius, 10f, Hit, heath.camp);
        Destroy(gameObject);
    }
}
