using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsRigAdvice : MonoBehaviour
{
    //
    public Avatar recommendedAvater;
    [Header("LeftHand")]
    public Transform leftHand;
    public Transform leftHandHint;
    public Transform leftThumb, leftIndex, leftMiddle, leftRing, leftPinky;
    [Header("RightHand")]
    public Transform rightHand;
    public Transform rightHandHint;
    public Transform rightThumb, rightIndex, rightMiddle, rightRing, rightPinky;
}
