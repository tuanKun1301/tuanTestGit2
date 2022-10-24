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

    private bool _selected;
    private bool _clicked;
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
        _selected = false;
        _clicked = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
        _source = GetComponent<AudioSource>();
        positionSave = new List<Vector2>();
    }

    public void CreatePoss(int col, int row)
    {
        _index = new int[col, row];
    }

    public void SetPoss(int index)
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
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (_selected && squareIndexes.Contains(_index[_column, _row]))
        {
            _correct = true;
            _displayedImage.sprite = _correctLetterData.image;
        }

        _selected = false;
        _clicked = false;
    }

    public void OnEnableSquareSelection()
    {
        _clicked = true;
        _selected = false;
    }

    public void OnDisableSquareSelection()
    {
        _clicked = false;
        _selected = false;
        if (_correct == true)
            _displayedImage.sprite = _correctLetterData.image;
        else
            _displayedImage.sprite = _normalLetterData.image;
    }

    private void SelectSquare(int column, int row)
    {
        if (_column == column && _row == row)
        {
            _displayedImage.sprite = _selectedLetterData.image;
        }
        // _displayedImage.sprite = _selectedLetterData.image;
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

    private Vector3 secondPos;
    private List<Vector2> positionSave;

    private void OnMouseOver()
    {
        Vector3 firstPos;
        if (Input.GetMouseButtonDown(0))
        {
            firstPos = gameObject.transform.position;
            positionSave.Add(firstPos);
            GameEvents.GetPositionMethod(firstPos, gameObject);
            _displayedImage.sprite = _normalLetterData.image;
            //PlaySound();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            secondPos = gameObject.transform.position;
            GameEvents.CheckWordMethod();
            GameEvents.ClearPositionMethod();
        }

        else if (Input.GetMouseButton(0))
        {
            PlaySound();
            GameEvents.GetPositionMethod(gameObject.transform.position, gameObject);
        }
    }

    private void OnMouseExit()
    {
        Console.Clear();

        //GameEvents.ClearPositionMethod();
    }

    private void OnMouseUp()
    {
        GameEvents.ClearSelectionMethod();
        GameEvents.ClearPositionMethod();
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

    private void PlaySound()
    {
        if (SoundManager.instance.IsSoundFxMuted() == false)
        {
            _source.Play();
        }
    }
}