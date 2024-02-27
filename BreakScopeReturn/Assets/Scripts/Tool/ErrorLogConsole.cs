using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ErrorLogConsole : MonoBehaviour
{
    public KeyCode showHideKeyCode = KeyCode.F3;

    readonly List<string> _logs = new List<string>();
    bool _visible;
    Rect _consoleRect = new Rect(0, 0, Screen.width, Screen.height);
    Vector2 _consoleScrollPosition;

    private void Awake()
    {
        Application.logMessageReceived += Application_logMessageReceived;
    }
    private void Update()
    {
        if (Input.GetKeyDown(showHideKeyCode))
        {
            _visible = !_visible;
        }
    }
    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (!_visible)
                _visible = true;
            _logs.Add(condition + "\n" + stackTrace);
        }
    }
    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(200), GUILayout.MaxHeight(40)))
        {
            _logs.Clear();
        }
        if (GUILayout.Button("Close", GUILayout.MaxWidth(200), GUILayout.MaxHeight(40)))
        {
            _visible = false;
        }
        GUILayout.EndHorizontal();
        _consoleScrollPosition = GUILayout.BeginScrollView(_consoleScrollPosition);
        Color originalColor = GUI.contentColor;
        GUI.contentColor = Color.red;
        foreach (var log in _logs)
        {
            GUILayout.TextArea(log);
        }
        GUI.contentColor = originalColor;
        GUILayout.EndScrollView();
    }
    private void OnGUI()
    {
        if (!_visible)
            return;
        _consoleRect = GUILayout.Window(0, _consoleRect, ConsoleWindow, "ErrorConsole ( Hit \"" + showHideKeyCode.ToString() + "\" to close )");
    }
}
