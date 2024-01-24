using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class KeyHoldProgressRing : MonoBehaviour
{
    public KeyCode keyCode;
    public Cooldown holdTime = new Cooldown(1);
    public UnityEvent onKeyDown, onKeyUp, onHoldTimeReached;
    public Image Image {  get; private set; }

    private void Awake()
    {
        Image = GetComponent<Image>();
        Image.fillAmount = 0;
    }
    private void Update()
    {
        if (Input.GetKey(keyCode))
        {
            if (Input.GetKeyDown(keyCode))
            {
                onKeyDown.Invoke();
            }
            if (holdTime.AddDeltaTimeAndEat())
            {
                onHoldTimeReached.Invoke();
            }
        }
        else
        {
            if (Input.GetKeyUp(keyCode))
            {
                onKeyUp.Invoke();
            }
            holdTime.Add(-Time.deltaTime);
        }
        Image.fillAmount = holdTime.Ratio;
    }
}
