using System.Collections;

using UnityEngine;


public class Zoom : MonoBehaviour
{
    public Camera _cam;
    public bool ON;
    public SpriteRenderer tabel;
    // Update is called once per frame
    void LateUpdate()
    {
        ON = Input.GetKey(KeyCode.Z);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StopCoroutine("FadeOut");
            StartCoroutine("FadeOut");
        }
        Vector3 Pos = transform.position;
        Pos.x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        if (ON)
        {
            _cam.enabled = true;
            transform.position = Vector3.Lerp(transform.position, Pos, 0.25f);
            Rect rect =_cam.rect;
            //0 ~0.627 ; -16.33,16.33
            rect.x = transform.position.x / 32.66f * 0.627f + 0.5f * 0.627f;
            _cam.rect = rect;
        }
        else
        {
            _cam.enabled = false;
            transform.position = Pos;
        }
    }
    IEnumerator FadeOut()
    {
        tabel.color = new Color(1, 1, 1, 1f);
        yield return new WaitForSeconds(1f);
        do
        {
            tabel.color -= new Color(0, 0, 0, 5f) * Time.deltaTime;
            
            
            ;
        } while (tabel.color.a > 0);
    }
}
