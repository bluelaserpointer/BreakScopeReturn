using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TranslatableSentence
{
    [Serializable]
    public struct LanguageAndSentence
    {
        public Language language;
        [TextArea]
        public string sentence;
        public LanguageAndSentence(Language language, string sentence)
        {
            this.language = language;
            this.sentence = sentence;
        }
    }
    public string defaultString;
    public LanguageAndSentence[] languageAndSentences;
    public override string ToString() {
        int pairID = Array.FindIndex(languageAndSentences, eachPair => eachPair.language.Equals(LanguageExtension.CurrentLanguage));
        return pairID == -1 ? defaultString : languageAndSentences[pairID].sentence;
    }
    public static implicit operator string(TranslatableSentence sentence)
    {
        return sentence.ToString();
    }
    public TranslatableSentence Clone()
    {
        return new TranslatableSentence
        {
            defaultString = defaultString,
            languageAndSentences = (LanguageAndSentence[])languageAndSentences.Clone()
        };
    }
    public void PutSentence(Language language, string str)
    {
        int pairID = Array.FindIndex(languageAndSentences, eachPair => eachPair.language.Equals(language));
        if (pairID != -1)
        {
            languageAndSentences[pairID].sentence = str;
            return;
        }
        Array.Resize(ref languageAndSentences, languageAndSentences.Length + 1);
        languageAndSentences[languageAndSentences.Length + 1] = new LanguageAndSentence(language, str);
    }
    public void PutSentence_EmptyStrMeansRemove(Language language, string str)
    {
        if (str.Length == 0) //remove
        {
            languageAndSentences.RemoveOne(pair => pair.language.Equals(language));
        }
        else //add
        {
            PutSentence(language, str);
        }
    }
}
