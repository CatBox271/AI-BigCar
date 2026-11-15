using UnityEngine;

public class StuckBoom : MonoBehaviour
{
    public float Depth = 0.3f;//陷进地面的深度超过这个值算卡住
    public float StuckTime = 1f;//连续卡住多久自爆

    static Bounds Limit;//场景里 tag=LegalArea 的物体范围，size 为 0 表示没有合法范围
    static bool limitSet;
    static bool HasLimit
    {
        get
        {
            EnsureLimit();
            return Limit.size != Vector3.zero;
        }
    }
    static void EnsureLimit()
    {
        if (limitSet) return;
        limitSet = true;
        GameObject go = GameObject.FindWithTag("LegalArea");
        if (go == null) return;
        if (go.TryGetComponent(out SpriteRenderer sr)) Limit = sr.bounds;
    }

    Rigidbody2D rb;
    Heath heath;
    ContactPoint2D[] Contacts = new ContactPoint2D[8];
    float stuck;
    int tick;
    void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out heath);
    }
    void FixedUpdate()
    {
        if (rb == null || heath == null) return;
        if (heath.camp != 0 && HasLimit && !Limit.Contains(transform.position))//camp 为 0 说明还没发车（在设计区拼装），不参与出界判定
        {
            heath.Die();
            return;
        }
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;
        if (rb.velocity.sqrMagnitude > 0.01f || Mathf.Abs(rb.angularVelocity) > 1f)
        {
            stuck = 0;
            return;
        }
        tick++;
        if (tick < 10) return;
        tick = 0;
        if (!Stuck())
        {
            stuck = 0;
            return;
        }
        stuck += Time.fixedDeltaTime * 10;
        if (stuck < StuckTime) return;
        heath.Die();
    }
    bool Stuck()
    {
        int count = rb.GetContacts(Contacts);
        for (int i = 0; i < count; i++)
        {
            if (Contacts[i].separation > -Depth) continue;
            return true;
        }
        return false;
    }
}
