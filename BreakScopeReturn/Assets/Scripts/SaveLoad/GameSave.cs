using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSave
{
    public List<Catalog> catalogedList;
    public void AddCatalog(Catalog catalog)
    {
        if (catalogedList.Contains(catalog))
            return;
        catalogedList.Add(catalog);
        GameManager.Instance.CatalogPopUpUI.SetCatalog(catalog);
    }
}
