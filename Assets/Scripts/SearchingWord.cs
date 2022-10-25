using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingWord : MonoBehaviour
{
    public Text displayedText;
    public Image crossLine;

    private string _word;

    void Start()
    {
    }

    private void OnEnable()
    {
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDestroy()
    {
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    public void SetWord(string word)
    {
        _word = word;
        displayedText.text = word;
    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (word == _word || Reverse(_word) == word)
        {
            crossLine.gameObject.SetActive(true);
        }
    }
    
    private String Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        Debug.Log(new string(charArray));
        return new string(charArray);
    }
}