using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "BreakScope/Talker")]
public class DialogTalker : ScriptableObject
{
    [SerializeField]
    Color _textColor;
    [SerializeField]
    TranslatableSentence _talkerName;

    public Color TextColor => _textColor;
    public TranslatableSentence TalkerName => _talkerName;
    public string TalkerNameRichText => "<color=#" + TextColor.ToHexString() + ">" + TalkerName + ": </color>";
}
