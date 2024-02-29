using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BreakScope/Catalog", fileName = "new" + nameof(Catalog))]
public class Catalog : ScriptableObject
{
    [SerializeField]
    TranslatableSentence nameTS;
    [SerializeField]
    TranslatableSentence descriptionTS;

    public TranslatableSentence NameTS => nameTS;
    public TranslatableSentence DescriptionTS => descriptionTS;
}

public interface IHasCatalog : IComponentInterface
{
    public Catalog Catalog { get; }
}