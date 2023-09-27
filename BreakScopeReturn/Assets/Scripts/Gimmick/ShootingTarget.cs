using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTarget : MonoBehaviour
{
    [SerializeField]
    Transform rotateJoint;
    [SerializeField]
    Cooldown wakeupCD = new Cooldown(3);

}
