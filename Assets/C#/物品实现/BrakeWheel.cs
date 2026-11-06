using UnityEngine;

public class BrakeWheel : MonoBehaviour,ION
{
    public HingeJoint2D hj;
    public bool OnSet { get; set; } = false;
    public float NumSet { get; set; }

    private void FixedUpdate()
    {
        hj.useMotor = OnSet;
    }
}
