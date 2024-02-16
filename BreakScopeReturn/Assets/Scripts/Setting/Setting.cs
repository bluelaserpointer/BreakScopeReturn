using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public static class Setting
{
    public const string LANGUAGE = "Language";
    public const string DIFFICULTY = "Difficulty";
    public const string MASTER_VOLUME = "MasterVolume";
    public const string BGM_VOLUME = "BGMVolume";
    public const string SE_VOLUME = "SEVolume";
    public const string MOUSE_SENSITIVITY = "MouseSensitivity";
    private enum ParamType { String, Int, Float }
    public static void Set(string playerPrefsKey, object value)
    {
        ParamType paramType;
        switch(playerPrefsKey)
        {
            case LANGUAGE:
                paramType = ParamType.Int;
                LanguageExtension.CurrentLanguage = (Language)value;
                break;
            case DIFFICULTY:
                paramType = ParamType.Int;
                break;
            case MASTER_VOLUME:
                paramType = ParamType.Float;
                AudioListener.volume = (float)value;
                break;
            case BGM_VOLUME:
                paramType = ParamType.Float;
                break;
            case SE_VOLUME:
                paramType = ParamType.Float;
                break;
            case MOUSE_SENSITIVITY:
                paramType = ParamType.Float;
                if (GameManager.Instance != null && GameManager.Instance.Player != null)
                {
                    GameManager.Instance.Player.MouseLook.baseMouseSensitivity = (float)value;
                }
                break;
            default:
                Debug.LogWarning("<!> Setting \"" + playerPrefsKey + "\" is not supported.");
                return;
        }
        switch (paramType)
        {
            case ParamType.String:
                PlayerPrefs.SetString(playerPrefsKey, (string)value);
                break;
            case ParamType.Int:
                PlayerPrefs.SetInt(playerPrefsKey, (int)value);
                break;
            case ParamType.Float:
                PlayerPrefs.SetFloat(playerPrefsKey, (float)value);
                break;
        }
    }
    public static void SetDefault()
    {
        Set(DIFFICULTY, 0);
        Set(MASTER_VOLUME, 1F);
        Set(BGM_VOLUME, 1F);
        Set(SE_VOLUME, 1F);
        Set(MOUSE_SENSITIVITY, 1F);
    }
}
