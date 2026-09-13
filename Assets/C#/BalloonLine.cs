using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonLine : MonoBehaviour
{
    int Tie = 3;
    SpringJoint2D sj;
    public LineRenderer lr;
    private void FixedUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start && Tie >= 0)
        {
            Tie--;
            if (Tie == 0)
            {
                Tie--;
                if (transform.parent.GetChild(0).name.Contains("Balloon"))
                {
                    foreach (RaycastHit2D rh in Physics2D.LinecastAll(transform.position, transform.position - new Vector3(0, 2f)))
                    {
                        if (rh.collider.gameObject == gameObject) continue;
                        if (rh.collider.gameObject.TryGetComponent(out Heath h))
                        {
                            SpringJoint2D dj = gameObject.AddComponent<SpringJoint2D>();
                            dj.enableCollision = true;
                            dj.autoConfigureDistance = false;
                            dj.connectedBody = h.GetComponent<Rigidbody2D>();
                            dj.dampingRatio = 0.75f;
                            dj.frequency = 0f;
                            dj.anchor = new(0, 0.14f);
                            dj.connectedAnchor = new(0, 0.5f);
                            dj.distance = 2f;
                            sj = dj;
                            break;
                        }
                    }
                }
            }
        }
    }
    private void LateUpdate()
    {
        if (sj != null)
        {
            if (sj.connectedBody != null)
            {
                Transform tf = sj.connectedBody.transform;
                lr.gameObject.SetActive(true);
                lr.SetPosition(0, transform.position + transform.up * sj.anchor.y * transform.lossyScale.y);

                lr.SetPosition(1, tf.position + tf.up * sj.connectedAnchor.y * tf.lossyScale.y);
            }
            else
            {
                lr.gameObject.SetActive(false);
            }
        }
        else
        {
            lr.gameObject.SetActive(false);
        }
    }
}
