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

    void Start()
    {
        _selected = false;
        _clicked = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
        _source = GetComponent<AudioSource>();
    }

    void Update()
    {
        
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
            _displayedImage.sprite = _selectedLetterData.image;
    }

    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData,
        AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }

    private void OnMouseDown()
    {
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.image;
    }
    private void OnMouseEnter()
    {
        //Debug.Log($"mouse enter here: {gameObject.transform.position}");
        CheckSquare();
    }

    private void OnMouseUp()
    {
        //Debug.Log($"pos:{gameObject.transform.position}");
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    public void CheckSquare()
    {
        if (_selected == false && _clicked == true)
        {
            // play sound
            if (SoundManager.instance.IsSoundFxMuted() == false)
            {
                _source.Play();
            }

            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
        }
    }

    
    private Vector3 mousePosition;
    private Vector3 position = new Vector3(0f, 0f,0f);
    
    public BoardData.Ray PredictionRay(Vector2 firstPosition, Vector2 secondPosition)
    {
        int a = 0;
        if (firstPosition != secondPosition)
        {
            //Debug.Log($"first pos:{firstPosition} --- second pos:{secondPosition}");
            var direction = secondPosition - firstPosition;
            Debug.Log($"direction: {direction}");
            if (direction.x > 0 && direction.y == 0)
            {
                return BoardData.Ray.RayRight;
            }
            if (direction.x < 0 && direction.y == 0)
            {
                return BoardData.Ray.RayLeft;
            }
            if (direction.x == 0 && direction.y < 0)
            {
                return BoardData.Ray.RayUp;
            }
            if (direction.x == 0 && direction.y > 0)
            {
                return BoardData.Ray.RayDown;
            }
            if (direction.x > 0 && direction.y > 0 && direction.x == direction.y)
            {
                return BoardData.Ray.RayDiagonalRightDown;
            }
            if (direction.x < 0 && direction.y > 0 && direction.x == direction.y)
            {
                return BoardData.Ray.RayDiagonalLeftDown;
            }
            if (direction.x > 0 && direction.y < 0 && direction.x == direction.y)
            {
                return BoardData.Ray.RayDiagonalRightUp;
            }
            if (direction.x < 0 && direction.y < 0 && direction.x == direction.y)
            {
                return BoardData.Ray.RayDiagonalLeftUp;
            }
        }

        return BoardData.Ray.RayDown;
    }

    public void DrawWordInPrediction(BoardData.Ray ray)
    {
        switch (ray)
        {
        }
    }
}