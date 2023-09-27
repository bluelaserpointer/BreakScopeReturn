using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class HitDiractionIndicatorPiece : MonoBehaviour
{
    [SerializeField]
    float _displayTime;

    CanvasGroup _canvasGroup;
    float _spawnTime;
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        _spawnTime = Time.time;
    }
    private void Update()
    {
        _canvasGroup.alpha = 1 - Mathf.Clamp01((Time.time - _spawnTime) / _displayTime);
        if (_canvasGroup.alpha == 0)
        {
            Destroy(gameObject);
        }
    }
}
