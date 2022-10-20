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
    private int _index = -1;
    private bool _correct;

    private AudioSource _source;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public int GetIndex()
    {
        return _index;
    }

    public String GetLetter()
    {
        return _normalLetterData.letter;
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
        if (_selected && squareIndexes.Contains(_index))
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

    private void SelectSquare(Vector3 position)
    {
        if (this.gameObject.transform.position == position)
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
            
            //PlaySound();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            secondPos = gameObject.transform.position;
            GameEvents.ClearPositionMethod();
        }
 
        else if (Input.GetMouseButton(0))
        {
            PlaySound();
            GameEvents.GetPositionMethod(gameObject.transform.position,gameObject);
            
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
        GameEvents.DisableSquareSelectionMethod();
    }

    public void CheckSquare()
    {
        if (_selected == false && _clicked == true)
        {
            PlaySound();
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
        }
    }

    private void PlaySound()
    {
        if (SoundManager.instance.IsSoundFxMuted() == false)
        {
            _source.Play();
        }
    }

    
    
}