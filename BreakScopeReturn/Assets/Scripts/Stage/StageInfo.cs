using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BreakScope/StageInfo", fileName = "new" + nameof(StageInfo))]
public class StageInfo : ScriptableObject
{
    [SerializeField]
    string _sceneName;
    [SerializeField]
    TranslatableSentence _nameTS;
    [SerializeField]
    TranslatableSentence _descriptionTS;

    public string SceneName => _sceneName;
    public TranslatableSentence NameTS => _nameTS;
    public TranslatableSentence DescriptionTS => _descriptionTS;
}
