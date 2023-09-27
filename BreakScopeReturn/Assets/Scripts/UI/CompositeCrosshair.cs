using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class CompositeCrosshair : MonoBehaviour
{
    public float expandDistance = 100;
    [SerializeField]
    Transform _sliceParent;
    [SerializeField]
    Image _slicePrefab;
    [SerializeField]
    Vector3 _slicePrefabAxis = Vector3.right;
    [SerializeField]
    int _sliceAmount;
    public CanvasGroup CanvasGroup { get; private set; }

    readonly List<Image> _sliceImages = new List<Image>();
    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        if (_sliceParent == null)
        {
            _sliceParent = transform;
        }
    }
    private void Start()
    {
        Regenerate();
    }
    private void Update()
    {
        UpdateUI();
    }
    public void Regenerate()
    {
        _sliceParent.DestroyAllChildren();
        _sliceImages.Clear();
        _sliceImages.Add(_slicePrefab);
        float angleSpan = 360F / _sliceAmount;
        for (int i = 0; i < _sliceAmount; i++)
        {
            GameObject sliceShaft = new GameObject("sliceShaft");
            sliceShaft.transform.SetParent(transform, false);
            sliceShaft.transform.localEulerAngles = Vector3.forward * angleSpan * i;
            Image sliceImage = Instantiate(_slicePrefab, _sliceParent);
            sliceImage.transform.SetParent(sliceShaft.transform, false);
            _sliceImages.Add(sliceImage);
        }
    }
    public void UpdateUI()
    {
        foreach (var image in _sliceImages)
        {
            image.transform.localPosition = _slicePrefabAxis * expandDistance;
        }
    }
    public void SetCanvasGroupAlpha(float value)
    {
        CanvasGroup.alpha = value;
    }
}
