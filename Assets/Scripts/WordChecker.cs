using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Profiling.Experimental;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WordChecker : MonoBehaviour
{
    public GameData currentGameData;
    public GameLevelData gameLevelData;

    private string _word;

    private int _assignedPoints = 0;
    private int _completedWords = 0;

    private Ray _currentRay = new Ray();
    private Vector3 _rayStartPosition;
    private List<int> _correctSquareList = new List<int>();

    private enum Ray
    {
        _rayUp,
        _rayDown,
        _rayLeft,
        _rayRight,
        _rayDiagonalLeftUp,
        _rayDiagonalLeftDown,
        _rayDiagonalRightUp,
        _rayDiagonalRightDown
    }

    private void OnEnable()
    {
        //GameEvents.OnCheckSquare += SquareSelected;
        GameEvents.OnClearSelection += ClearSelection;
        GameEvents.OnLoadNextLevel += LoadNextGameLevel;
        //test logic
        GameEvents.OnGetPosition += GetPosition;
        GameEvents.OnClearPosition += ClearPosition;
        GameEvents.OnCheckWord += CheckWord;
    }

    private void OnDisable()
    {
        //GameEvents.OnCheckSquare -= SquareSelected;
        GameEvents.OnClearSelection -= ClearSelection;
        GameEvents.OnLoadNextLevel -= LoadNextGameLevel;
        //test logic
        GameEvents.OnGetPosition -= GetPosition;
        GameEvents.OnClearPosition -= ClearPosition;
        GameEvents.OnCheckWord -= CheckWord;
    }

    private void LoadNextGameLevel()
    {
        SceneManager.LoadScene("GameScene");
    }


    void Start()
    {
        currentGameData.selectedBoardData.ClearData();
        _assignedPoints = 0;
        _completedWords = 0;
        gameObjectList = new List<GameObject>();
        letterList = new List<int>();
        gameObjectList.Clear();
        letterList.Clear();
    }


    // void Update()
    // {
    //     if (_assignedPoints > 0 && Application.isEditor)
    //     {
    //         Debug.DrawRay(_rayUp.origin, _rayUp.direction * 4);
    //         Debug.DrawRay(_rayDown.origin, _rayDown.direction * 4);
    //         Debug.DrawRay(_rayLeft.origin, _rayLeft.direction * 4);
    //         Debug.DrawRay(_rayRight.origin, _rayRight.direction * 4);
    //
    //         Debug.DrawRay(_rayDiagonalLeftUp.origin, _rayDiagonalLeftUp.direction * 4);
    //         Debug.DrawRay(_rayDiagonalLeftDown.origin, _rayDiagonalLeftDown.direction * 4);
    //         Debug.DrawRay(_rayDiagonalRightUp.origin, _rayDiagonalRightUp.direction * 4);
    //         Debug.DrawRay(_rayDiagonalRightDown.origin, _rayDiagonalRightDown.direction * 4);
    //     }
    // }
    //
    // private void SquareSelected(string letter, Vector3 position, int squareIndex)
    // {
    //     if (_assignedPoints == 0)
    //     {
    //         _rayStartPosition = position;
    //         _correctSquareList.Add(squareIndex);
    //         _word += letter;
    //         // ray +
    //         _rayUp = new Ray(new Vector2(position.x, position.y), new Vector2(0f, 1));
    //         _rayDown = new Ray(new Vector2(position.x, position.y), new Vector2(0f, -1));
    //         _rayLeft = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 0f));
    //         _rayRight = new Ray(new Vector2(position.x, position.y), new Vector2(1, 0f));
    //         // ray x
    //         _rayDiagonalLeftUp = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 1));
    //         _rayDiagonalLeftDown = new Ray(new Vector2(position.x, position.y), new Vector2(-1, -1));
    //         _rayDiagonalRightUp = new Ray(new Vector2(position.x, position.y), new Vector2(1, 1));
    //         _rayDiagonalRightDown = new Ray(new Vector2(position.x, position.y), new Vector2(1, -1));
    //     }
    //     else if (_assignedPoints == 1)
    //     {
    //         _correctSquareList.Add(squareIndex);
    //         _currentRay = SelectRay(_rayStartPosition, position);
    //         GameEvents.SelectSquareMethod(position);
    //         //Debug.Log($"test 1:({Input.mousePosition.x}, {Input.mousePosition.y})");
    //         _word += letter;
    //         CheckWord();
    //     }
    //     else
    //     {
    //         if (IsPointOnTheRay(_currentRay, position))
    //         {
    //             //Debug.Log("test 2:");
    //             _correctSquareList.Add(squareIndex);
    //             GameEvents.SelectSquareMethod(position);
    //             _word += letter;
    //             CheckWord();
    //         }
    //     }
    //
    //     _assignedPoints++;
    // }
    //
    private void CheckWord()
    {
        foreach (var index in letterList.Distinct())
        {
            foreach (var gameObj in GetComponent<WordsGrid>().getAllsquarelist())
            {
                //_word += gameObject.GetComponent<GridSquare>().GetLetter(index);
                if (index == gameObj.GetComponent<GridSquare>().GetIndex())
                {
                    //Debug.Log(index);
                    _word += gameObj.GetComponent<GridSquare>().GetLetter();
                }
            }
        }

        foreach (var searchingWord in currentGameData.selectedBoardData.SearchWords)
        {
            if ((Reverse(_word.Trim()) == searchingWord.Word || _word.Trim() == searchingWord.Word) &&
                searchingWord.Found == false)
            {
                Debug.Log("come here");
                searchingWord.Found = true;
                GameEvents.CorrectWordMethod(_word, _correctSquareList);
                _completedWords++;
                _word = string.Empty;
                _correctSquareList.Clear();
                CheckBoardCompleted();
                return;
            }
        }

        letterList.Clear();
    }

    private String Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        Debug.Log(new string(charArray));
        return new string(charArray);
    }
    //
    // private bool IsPointOnTheRay(Ray currentRay, Vector3 point)
    // {
    //     var hints = Physics.RaycastAll(currentRay, 100.0f);
    //     for (int i = 0; i < hints.Length; i++)
    //     {
    //         if (hints[i].transform.position == point)
    //             return true;
    //     }
    //
    //     return false;
    // }

    // private Ray SelectRay(Vector2 startPos, Vector2 endPos)
    // {
    //     var direction = (endPos - startPos).normalized;
    //     float tolerance = 0.01f;
    //     if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance)
    //         return _rayUp;
    //
    //     if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance)
    //         return _rayDown;
    //
    //     if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance)
    //         return _rayLeft;
    //
    //     if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance)
    //         return _rayRight;
    //
    //     if (direction.x < 0f && direction.y > 0f)
    //         return _rayDiagonalLeftUp;
    //
    //     if (direction.x < 0f && direction.y < 0f)
    //         return _rayDiagonalLeftDown;
    //
    //     if (direction.x > 0f && direction.y > 0f)
    //         return _rayDiagonalRightUp;
    //
    //     if (direction.x > 0f && direction.y < 0f)
    //         return _rayDiagonalRightDown;
    //
    //     return _rayDown;
    // }

    private void ClearSelection()
    {
        _assignedPoints = 0;
        _correctSquareList.Clear();
        _word = string.Empty;
    }

    private void CheckBoardCompleted()
    {
        bool loadNextCategory = false;
        if (currentGameData.selectedBoardData.SearchWords.Count == _completedWords)
        {
            // Save current level progress
            var categoryName = currentGameData.selectedCategoryName;
            var currentBoardIndex = DataSaver.ReadCategoryCurrentIndexValues(categoryName);
            var nextBoardIndex = -1;
            var currentCategoryIndex = 0;
            bool readNextLevelName = false;
            for (int index = 0; index < gameLevelData.data.Count; index++)
            {
                if (readNextLevelName)
                {
                    nextBoardIndex = DataSaver.ReadCategoryCurrentIndexValues(gameLevelData.data[index].categoryName);
                    readNextLevelName = false;
                }

                if (gameLevelData.data[index].categoryName == categoryName)
                {
                    readNextLevelName = true;
                    currentCategoryIndex = index;
                }
            }

            var currentLevelSize = gameLevelData.data[currentCategoryIndex].boardData.Count;
            if (currentBoardIndex < currentLevelSize)
            {
                currentBoardIndex += 1;
            }

            DataSaver.SaveCategoryData(categoryName, currentBoardIndex);

            //unlock next category if currentBoardIndex = currentLevelSize
            if (currentBoardIndex >= currentLevelSize)
            {
                int i = 0;
                currentCategoryIndex++;
                if (currentCategoryIndex < gameLevelData.data.Count)
                {
                    categoryName = gameLevelData.data[currentCategoryIndex].categoryName;
                    currentBoardIndex = 0;
                    loadNextCategory = true;
                    if (nextBoardIndex <= 0)
                        DataSaver.SaveCategoryData(categoryName, currentBoardIndex);
                }
                else
                    SceneManager.LoadScene("SelectCategory");
            }
            else
                GameEvents.BoardCompletedMethod();

            if (loadNextCategory)
                GameEvents.UnlockNextCategoryMethod();
        }
    }

    //test logic
    private List<GameObject> gameObjectList;
    private List<int> letterList;

    private void GetPosition(Vector2 position, GameObject gameObj)
    {
        gameObjectList.Add(gameObj);
        var uniqueList = gameObjectList.Distinct().ToList();

        if (uniqueList.Count >= 2)
        {
            var startPos = uniqueList[0].transform.position;
            var endPos = uniqueList[uniqueList.LastIndexOf(gameObj)].transform.position;

            var list = GetComponent<WordsGrid>().getAllsquarelist();
            //Debug.Log(startPos + "<-->" + endPos);
            if (startPos != endPos)
            {
                var direction = (endPos - startPos).normalized;
                float tolerance = 0.01f;

                GameEvents.SelectSquareMethod(startPos);
                GameEvents.SelectSquareMethod(endPos);
                letterList.Add(gameObj.GetComponent<GridSquare>().GetIndex());
                //ray Up
                if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance)
                {
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;
                        square.GetComponent<GridSquare>().GetIndex();
                        // ray up check
                        if (currentPos.x == startPos.x && startPos.y <= currentPos.y && currentPos.y <= endPos.y)
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray down
                else if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance)
                {
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;
                        // ray down check
                        if (currentPos.x == startPos.x && startPos.y >= currentPos.y && currentPos.y >= endPos.y)
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray left
                else if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance)
                {
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;
                        // ray left check
                        if (currentPos.y == startPos.y && startPos.x >= currentPos.x && currentPos.x >= endPos.x)
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray right
                else if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance)
                {
                    
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;
                        // ray right check
                        if (currentPos.y == startPos.y && startPos.x <= currentPos.x && currentPos.x <= endPos.x)
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray diagonal left up
                else if (direction.x < 0f && direction.y > 0f)
                {
                    letterList.Add(gameObj.GetComponent<GridSquare>().GetIndex());
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;

                        if (checkAngle(startPos, currentPos, endPos))
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray diagonal left down
                else if (direction.x < 0f && direction.y < 0f)
                {
                    letterList.Add(gameObj.GetComponent<GridSquare>().GetIndex());
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;

                        if (checkAngle(startPos, currentPos, endPos))
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray diagonal right up
                else if (direction.x > 0f && direction.y > 0f)
                {
                    letterList.Add(gameObj.GetComponent<GridSquare>().GetIndex());
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;

                        if (checkAngle(startPos, currentPos, endPos))
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                }
                // ray diagonal right down
                else if (direction.x > 0f && direction.y < 0f)
                {
                    letterList.Add(gameObj.GetComponent<GridSquare>().GetIndexByPos(startPos));
                    foreach (var square in list)
                    {
                        var currentPos = square.transform.position;

                        if (checkAngle(startPos, currentPos, endPos))
                        {
                            letterList.Add(square.GetComponent<GridSquare>().GetIndex());
                            GameEvents.SelectSquareMethod(currentPos);
                        }
                    }
                    letterList.Add(gameObj.GetComponent<GridSquare>().GetIndexByPos(endPos));
                }
                //Debug.Log(letterList.Count);
            }
        }
    }

    private bool checkAngle(Vector2 start, Vector2 current, Vector2 end)
    {
        if (Math.Round(Mathf.Atan2((end.x - start.x), (end.y - start.y)), 2) ==
            Math.Round(Mathf.Atan2((current.x - start.x), (current.y - start.y)), 2)
            && Math.Round(Mathf.Atan2((end.x - current.x), (end.y - current.y)), 2) ==
            Math.Round(Mathf.Atan2((current.x - start.x), (current.y - start.y)), 2))
            return true;
        return false;
    }


    private void ClearPosition()
    {
        gameObjectList.Clear();
    }
}