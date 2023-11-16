using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
public class ImageAspectRatioFitter : MonoBehaviour
{
    public Image Image { get; private set; }
    public AspectRatioFitter AspectRatioFitter { get; private set; }
    private void Awake()
    {
        Image = GetComponent<Image>();
        AspectRatioFitter = GetComponent<AspectRatioFitter>();
    }
    private void Update()
    {
        UpdateAspect();
    }
    public void UpdateAspect()
    {
        if (Image.sprite == null)
            return;
        Rect spriteRect = Image.sprite.rect;
        AspectRatioFitter.aspectRatio = spriteRect.width / spriteRect.height;
    }
}
