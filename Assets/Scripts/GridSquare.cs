using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }
    private AlphabetData.LetterData _normalLetterData;
    private AlphabetData.LetterData _selectedLetterData;
    private AlphabetData.LetterData _correctLetterData;

    private SpriteRenderer _displayedImage;

    private bool _correct;

    private int _column;
    private int _row;
    private int[,] _index;

    private AudioSource _source;
    // get , set column

    public void SetColumn(int column)
    {
        _column = column;
    }

    public int GetColumn()
    {
        return _column;
    }

    public void SetRow(int row)
    {
        _row = row;
    }

    public int GetRow()
    {
        return _row;
    }

    public String GetLetter(int index)
    {
        // Debug.Log(index);
        if (_index[_column, _row] == index)
            return _normalLetterData.letter;
        return null;
    }

    void Start()
    {
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
        _source = GetComponent<AudioSource>();
    }

    public void CreateArray(int col, int row)
    {
        _index = new int[col, row];
    }

    public void SetIndex(int index)
    {
        _index[_column, _row] = index;
        //Debug.Log($"x,y position [{_column},{_row}] <> {index}");
    }

    public int GetIndex()
    {
        return _index[_column, _row];
    }

    private void OnEnable()
    {
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (squareIndexes.Contains(_index[_column, _row]))
        {
            _correct = true;
            _displayedImage.sprite = _correctLetterData.image;
        }
    }


    private void SelectSquare(int column, int row)
    {
        if (_column == column && _row == row)
        {
            _displayedImage.sprite = _selectedLetterData.image;
        }
    }

    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData,
        AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }

    // private void OnMouseDown()
    // {
    //     OnEnableSquareSelection();
    //     GameEvents.EnableSquareSelectionMethod();
    //     CheckSquare();
    //     _displayedImage.sprite = _selectedLetterData.image;
    // }
    //
    // private void OnMouseEnter()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Debug.Log($"pos{gameObject.transform.position}");
    //     }
    //     CheckSquare();
    // }


    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameEvents.GetGridMethod(gameObject);
            _displayedImage.sprite = _normalLetterData.image;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            GameEvents.CheckWordMethod();
        }

        else if (Input.GetMouseButton(0))
        {
            PlaySound();
            GameEvents.GetGridMethod(gameObject);
        }
    }

    private void OnMouseUp()
    {
        GameEvents.CheckWordMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    // public void CheckSquare()
    // {
    //     if (_selected == false && _clicked == true)
    //     {
    //         PlaySound();
    //         _selected = true;
    //         GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
    //     }
    // }

    public void ClearSelection()
    {
        if (_correct == false)
        {
            _displayedImage.sprite = _normalLetterData.image;
        }
        else
        {
            _displayedImage.sprite = _correctLetterData.image;
        }
    }

    private void PlaySound()
    {
        if (SoundManager.instance.IsSoundFxMuted() == false)
            _source.Play();
    }
}