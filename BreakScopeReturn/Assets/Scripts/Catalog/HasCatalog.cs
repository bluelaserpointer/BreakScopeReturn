using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasCatalog : MonoBehaviour, IHasCatalog
{
    [SerializeField]
    Catalog _catalog;

    public Catalog Catalog => _catalog;
}
