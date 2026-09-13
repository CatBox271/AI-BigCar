using UnityEngine;

public class ShowNum : MonoBehaviour
{
    public int num;
    public Sprite[] texture2D;
    private Txt FatherObject;
    private SpriteRenderer im;
    public float size;
    public float offset;
    public float vertical_offset;
    private Color color = Color.white;
    public float word_offset = 0.75f;
    void Start()
    {
        im = GetComponent<SpriteRenderer>();
        im.color = color;
        Vector3 v3 = transform.parent.lossyScale;
        v3.x = 1f / v3.x;
        v3.y = 1f / v3.y;
        transform.localScale = v3 * size;
        FatherObject = transform.parent.GetComponent<Txt>();
        CorrectPos();
    }

    public void Set(int n, float s, float o,Color c,float w,float vs)
    {
        num = n;
        size = s;
        offset = o;
        color = c;
        word_offset = w;
        vertical_offset = vs;
    }

    private void CorrectPos()
    {
        transform.localPosition = new Vector3(
            FatherObject.context.Length / (-2 / word_offset) / transform.parent.lossyScale.x +
            word_offset * num / transform.parent.lossyScale.x +
            0.5f / transform.parent.lossyScale.x +
            offset
            , vertical_offset, 0f);
    }
    void Update()
    {
        if (lasL != FatherObject.context.Length)
        {
            if (FatherObject.context.Length < num + 1)
            {
                Destroy(gameObject);
                return;
            }
            CorrectPos();
            lasL = FatherObject.context.Length;
        }
        if (FatherObject.con[num] != last)
        {
            last = FatherObject.con[num];
            switch (FatherObject.con[num])
            {

                case "0":
                    im.sprite = texture2D[0];
                    break;
                case "1":
                    im.sprite = texture2D[1];
                    break;
                case "2":
                    im.sprite = texture2D[2];
                    break;
                case "3":
                    im.sprite = texture2D[3];
                    break;
                case "4":
                    im.sprite = texture2D[4];
                    break;
                case "5":
                    im.sprite = texture2D[5];
                    break;
                case "6":
                    im.sprite = texture2D[6];
                    break;
                case "7":
                    im.sprite = texture2D[7];
                    break;
                case "8":
                    im.sprite = texture2D[8];
                    break;
                case "9":
                    im.sprite = texture2D[9];
                    break;
                case "a":
                    im.sprite = texture2D[10];
                    break;
                case "b":
                    im.sprite = texture2D[11];
                    break;
                case "c":
                    im.sprite = texture2D[12];
                    break;
                case "d":
                    im.sprite = texture2D[13];
                    break;
                case "e":
                    im.sprite = texture2D[14];
                    break;
                case "f":
                    im.sprite = texture2D[15];
                    break;
                case "g":
                    im.sprite = texture2D[16];
                    break;
                case "h":
                    im.sprite = texture2D[17];
                    break;
                case "i":
                    im.sprite = texture2D[18];
                    break;
                case "j":
                    im.sprite = texture2D[19];
                    break;
                case "k":
                    im.sprite = texture2D[20];
                    break;
                case "l":
                    im.sprite = texture2D[21];
                    break;
                case "m":
                    im.sprite = texture2D[22];
                    break;
                case "n":
                    im.sprite = texture2D[23];
                    break;
                case "o":
                    im.sprite = texture2D[24];
                    break;
                case "p":
                    im.sprite = texture2D[25];
                    break;
                case "q":
                    im.sprite = texture2D[26];
                    break;
                case "r":
                    im.sprite = texture2D[27];
                    break;
                case "s":
                    im.sprite = texture2D[28];
                    break;
                case "t":
                    im.sprite = texture2D[29];
                    break;
                case "u":
                    im.sprite = texture2D[30];
                    break;
                case "v":
                    im.sprite = texture2D[31];
                    break;
                case "w":
                    im.sprite = texture2D[32];
                    break;
                case "x":
                    im.sprite = texture2D[33];
                    break;
                case "y":
                    im.sprite = texture2D[34];
                    break;
                case "z":
                    im.sprite = texture2D[35];
                    break;
                case "A":
                    im.sprite = texture2D[10];
                    break;
                case "B":
                    im.sprite = texture2D[11];
                    break;
                case "C":
                    im.sprite = texture2D[12];
                    break;
                case "D":
                    im.sprite = texture2D[13];
                    break;
                case "E":
                    im.sprite = texture2D[14];
                    break;
                case "F":
                    im.sprite = texture2D[15];
                    break;
                case "G":
                    im.sprite = texture2D[16];
                    break;
                case "H":
                    im.sprite = texture2D[17];
                    break;
                case "I":
                    im.sprite = texture2D[18];
                    break;
                case "J":
                    im.sprite = texture2D[19];
                    break;
                case "K":
                    im.sprite = texture2D[20];
                    break;
                case "L":
                    im.sprite = texture2D[21];
                    break;
                case "M":
                    im.sprite = texture2D[22];
                    break;
                case "N":
                    im.sprite = texture2D[23];
                    break;
                case "O":
                    im.sprite = texture2D[24];
                    break;
                case "P":
                    im.sprite = texture2D[25];
                    break;
                case "Q":
                    im.sprite = texture2D[26];
                    break;
                case "R":
                    im.sprite = texture2D[27];
                    break;
                case "S":
                    im.sprite = texture2D[28];
                    break;
                case "T":
                    im.sprite = texture2D[29];
                    break;
                case "U":
                    im.sprite = texture2D[30];
                    break;
                case "V":
                    im.sprite = texture2D[31];
                    break;
                case "W":
                    im.sprite = texture2D[32];
                    break;
                case "X":
                    im.sprite = texture2D[33];
                    break;
                case "Y":
                    im.sprite = texture2D[34];
                    break;
                case "Z":
                    im.sprite = texture2D[35];
                    break;
                case "+":
                    im.sprite = texture2D[36];
                    break;
                case "-":
                    im.sprite = null;
                    break;
                case "*":
                    im.sprite = texture2D[38];
                    break;
                case "/":
                    im.sprite = texture2D[39];
                    break;
            }
        }
    }

    int lasL;
    string last;
}
