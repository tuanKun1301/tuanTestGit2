using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public delegate void EnableSquareSelection();

    public static event EnableSquareSelection OnEnableSquareSelection;

    public static void EnableSquareSelectionMethod()
    {
        if (OnEnableSquareSelection != null)
        {
            OnEnableSquareSelection();
        }
    }

    public delegate void DisableSquareSelection();

    public static event DisableSquareSelection OnDisableSquareSelection;

    public static void DisableSquareSelectionMethod()
    {
        if (OnDisableSquareSelection != null)
        {
            OnDisableSquareSelection();
        }
    }

    public delegate void SelectSquare(Vector3 position);

    public static event SelectSquare OnSelectSquare;

    public static void SelectSquareMethod(Vector3 position)
    {
        if (OnSelectSquare != null)
        {
            OnSelectSquare(position);
        }
    }


    public delegate void CheckSquare(String letter, Vector3 position, int squareIndex);

    public static event CheckSquare OnCheckSquare;

    public static void CheckSquareMethod(String letter, Vector3 position, int squareIndex)
    {
        if (OnCheckSquare != null)
        {
            OnCheckSquare(letter, position, squareIndex);
        }
    }

    public delegate void ClearSelection();

    public static event ClearSelection OnClearSelection;

    public static void ClearSelectionMethod()
    {
        if (OnClearSelection != null)
        {
            OnClearSelection();
        }
    }

    public delegate void CorrectWord(string word, List<int> squareIndexes);

    public static event CorrectWord OnCorrectWord;

    public static void CorrectWordMethod(string word, List<int> squareIndexes)
    {
        if (OnCorrectWord != null)
        {
            OnCorrectWord(word, squareIndexes);
        }
    }


    public delegate void BoardCompleted();

    public static event BoardCompleted OnBoardCompleted;

    public static void BoardCompletedMethod()
    {
        if (OnBoardCompleted != null)
        {
            OnBoardCompleted();
        }
    }

    public delegate void UnlockNextCategory();

    public static event UnlockNextCategory OnUnlockNextCategory;

    public static void UnlockNextCategoryMethod()
    {
        if (OnUnlockNextCategory != null)
        {
            OnUnlockNextCategory();
        }
    }

    public delegate void LoadNextLevel();

    public static event LoadNextLevel OnLoadNextLevel;

    public static void LoadNextLevelMethod()
    {
        int i = 0;
        if (OnLoadNextLevel != null)
        {
            OnLoadNextLevel();
        }
    }


    public delegate void GamOver();

    public static event GamOver OnGamOver;

    public static void GamOverMethod()
    {
        if (OnGamOver != null)
        {
            OnGamOver();
        }
    }


    public delegate void ToggleSoundFx();

    public static event ToggleSoundFx OnToggleSoundFx;

    public static void ToggleSoundFxMethod()
    {
        if (OnToggleSoundFx != null)
        {
            OnToggleSoundFx();
        }
    }
    
    
    // test logic
    public delegate void GetPosition(Vector2 firstPos, GameObject gameObj);

    public static event GetPosition OnGetPosition;

    public static void GetPositionMethod(Vector2 firstPos, GameObject gameObj)
    {
        if (OnGetPosition != null)
        {
            OnGetPosition(firstPos, gameObj);
        }
    }
    public delegate void ClearPosition();

    public static event ClearPosition OnClearPosition;

    public static void ClearPositionMethod()
    {
        if (OnClearPosition != null)
        {
            OnClearPosition();
        }
    }
    
    public delegate void CheckWord();

    public static event CheckWord OnCheckWord;

    public static void CheckWordMethod()
    {
        if (OnCheckWord != null)
        {
            OnCheckWord();
        }
    }
}