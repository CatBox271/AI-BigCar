using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCombine : MonoBehaviour
{
    List<Collider2D> call = new();
    private void FixedUpdate()
    {
        call.Clear();
        call.AddRange(Physics2D.OverlapPointAll(transform.position));
        var crash = call.Find(s => s.gameObject != gameObject && s.CompareTag("Finish"));
            if(crash!=null)
        {
            Destroy(gameObject);
            Destroy(crash.gameObject);
        }
    }
}
