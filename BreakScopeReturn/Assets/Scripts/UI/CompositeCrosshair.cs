using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class CompositeCrosshair : MonoBehaviour
{
    public float baseExpandDistance;
    [SerializeField]
    RectTransform _leaderPiece;
    [SerializeField]
    int _pieceAmount;
    public CanvasGroup CanvasGroup { get; private set; }
    [HideInInspector]
    public float expandDistance = 0;

    readonly List<RectTransform> _pieces = new List<RectTransform>();
    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        Regenerate();
    }
    private void Update()
    {
        UpdateUI();
    }
    public void Regenerate()
    {
        _pieces.Remove(_leaderPiece);
        _pieces.ForEach(piece => Destroy(piece.gameObject));
        _pieces.Clear();
        _pieces.Add(_leaderPiece);
        float angleSpan = 360F / _pieceAmount;
        for (int i = 1; i < _pieceAmount; i++)
        {
            var piece = Instantiate(_leaderPiece, _leaderPiece.parent);
            piece.localEulerAngles = Vector3.forward * angleSpan * i;
            _pieces.Add(piece);
        }
        UpdateUI();
    }
    public void UpdateUI()
    {
        float r = baseExpandDistance + expandDistance;
        foreach (var image in _pieces)
        {
            float zAngleRad = Mathf.Deg2Rad * image.transform.localEulerAngles.z;
            image.transform.localPosition = new Vector2(r * Mathf.Cos(zAngleRad), r * Mathf.Sin(zAngleRad));
        }
    }
    public void SetCanvasGroupAlpha(float value)
    {
        CanvasGroup.alpha = value;
    }
}
