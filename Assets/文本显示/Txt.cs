using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Txt : MonoBehaviour
{

    public GameObject Word;
    public string context;
    string last;
    public List<string> con;
    private int length;
    public float size = 15f;
    public float offset;
    public float VerticalOffset;
    public Color color = Color.white;
    public float word_offset = 0.75f;
    void Update()
    {
        if (context.Length > length)
        {
            for (int i = length; i < context.Length; i++)
            {
                GameObject swarm = Instantiate(Word, transform);
                if (swarm.TryGetComponent(out ShowNum sn))
                {
                    sn.Set(i, size, offset, color, word_offset, VerticalOffset);
                }
                if (swarm.TryGetComponent(out ShowNum1 sn1))
                {
                    sn1.Set(i, size, offset, color, word_offset, VerticalOffset);
                }
                swarm.gameObject.layer = gameObject.layer;
            }
        }
        length = context.Length;
        if (last != context)
        {
            last = context;
            con.Clear();
            for(int i = 0;i<context.Length;i++)
            {
                con.Add(context[i].ToString());
            }
        }
    }
}
