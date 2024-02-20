using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FPSCounter : MonoBehaviour
{
    public KeyCode activationKey;
    public bool show;
    [Min(0)]
    public int fontSize;
    public Color textColor = Color.white;

    public float FPS { get; private set; }
    public float MSec { get; private set; }

    int _frameCount;
    float _prevTime;

    private void Awake()
    {
        _frameCount = 0;
        _prevTime = 0.0f;
    }

    private void Update()
    {
        ++_frameCount;
        float time = Time.realtimeSinceStartup - _prevTime;
        if (time >= 0.5f)
        {
            FPS = _frameCount / time;
            MSec = Time.deltaTime * 1000;
            _frameCount = 0;
            _prevTime = Time.realtimeSinceStartup;
        }
        if (Input.GetKeyDown(activationKey))
        {
            show = !show;
        }
    }
    private void OnGUI()
    {
        if (!show || Time.timeScale != 1.0F)
            return;
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height),
            string.Format("{0:0.0} ms ({1:0.} fps)", MSec, FPS),
            style);
    }
}
