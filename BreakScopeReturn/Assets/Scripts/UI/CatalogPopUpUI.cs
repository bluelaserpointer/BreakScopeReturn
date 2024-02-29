using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CatalogPopUpUI : MonoBehaviour
{
    [SerializeField]
    Animator _catalogAnimator;
    [SerializeField]
    TranslatedTMP _nameTransTMP;
    [SerializeField]
    TranslatedTMP _descriptionTransTMP;
    [SerializeField]
    AudioSource _popUpSE;

    public void SetCatalog(Catalog catalog)
    {
        if (catalog == null)
            return;
        _nameTransTMP.sentence = catalog.NameTS;
        _nameTransTMP.UpdateText();
        _descriptionTransTMP.sentence = catalog.DescriptionTS;
        _descriptionTransTMP.UpdateText();
        _catalogAnimator.SetTrigger("PopUp");
        _popUpSE.Play();
    }
}
