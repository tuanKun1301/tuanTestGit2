using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WordChecker : MonoBehaviour
{
    public GameData currentGameData;
    public GameLevelData gameLevelData;

    private string _word;

    private int _assignedPoints = 0;
    private int _completedWords = 0;
    private Ray _rayUp, _rayDown;
    private Ray _rayLeft, _rayRight;
    private Ray _rayDiagonalLeftUp, _rayDiagonalLeftDown;
    private Ray _rayDiagonalRightUp, _rayDiagonalRightDown;
    private Ray _currentRay = new Ray();
    private Vector3 _rayStartPosition;
    private List<int> _correctSquareList = new List<int>();


    private void OnEnable()
    {
        GameEvents.OnCheckSquare += SquareSelected;
        GameEvents.OnClearSelection += ClearSelection;
        GameEvents.OnLoadNextLevel += LoadNextGameLevel;
        //test logic
        GameEvents.OnGetPosition += GetPosition;
        GameEvents.OnClearPosition += ClearPosition;
    }

    private void OnDisable()
    {
        GameEvents.OnCheckSquare -= SquareSelected;
        GameEvents.OnClearSelection -= ClearSelection;
        GameEvents.OnLoadNextLevel -= LoadNextGameLevel;
        //test logic
        GameEvents.OnGetPosition -= GetPosition;
        GameEvents.OnClearPosition -= ClearPosition;
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
        positionList = new List<Vector2>();
        gameObjectList.Clear();
        positionList.Clear();
    }


    void Update()
    {
        if (_assignedPoints > 0 && Application.isEditor)
        {
            Debug.DrawRay(_rayUp.origin, _rayUp.direction * 4);
            Debug.DrawRay(_rayDown.origin, _rayDown.direction * 4);
            Debug.DrawRay(_rayLeft.origin, _rayLeft.direction * 4);
            Debug.DrawRay(_rayRight.origin, _rayRight.direction * 4);

            Debug.DrawRay(_rayDiagonalLeftUp.origin, _rayDiagonalLeftUp.direction * 4);
            Debug.DrawRay(_rayDiagonalLeftDown.origin, _rayDiagonalLeftDown.direction * 4);
            Debug.DrawRay(_rayDiagonalRightUp.origin, _rayDiagonalRightUp.direction * 4);
            Debug.DrawRay(_rayDiagonalRightDown.origin, _rayDiagonalRightDown.direction * 4);
        }
    }

    private void SquareSelected(string letter, Vector3 position, int squareIndex)
    {
        if (_assignedPoints == 0)
        {
            _rayStartPosition = position;
            _correctSquareList.Add(squareIndex);
            _word += letter;
            // ray +
            _rayUp = new Ray(new Vector2(position.x, position.y), new Vector2(0f, 1));
            _rayDown = new Ray(new Vector2(position.x, position.y), new Vector2(0f, -1));
            _rayLeft = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 0f));
            _rayRight = new Ray(new Vector2(position.x, position.y), new Vector2(1, 0f));
            // ray x
            _rayDiagonalLeftUp = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 1));
            _rayDiagonalLeftDown = new Ray(new Vector2(position.x, position.y), new Vector2(-1, -1));
            _rayDiagonalRightUp = new Ray(new Vector2(position.x, position.y), new Vector2(1, 1));
            _rayDiagonalRightDown = new Ray(new Vector2(position.x, position.y), new Vector2(1, -1));
        }
        else if (_assignedPoints == 1)
        {
            _correctSquareList.Add(squareIndex);
            _currentRay = SelectRay(_rayStartPosition, position);
            GameEvents.SelectSquareMethod(position);
            //Debug.Log($"test 1:({Input.mousePosition.x}, {Input.mousePosition.y})");
            _word += letter;
            CheckWord();
        }
        else
        {
            if (IsPointOnTheRay(_currentRay, position))
            {
                //Debug.Log("test 2:");
                _correctSquareList.Add(squareIndex);
                GameEvents.SelectSquareMethod(position);
                _word += letter;
                CheckWord();
            }
        }

        _assignedPoints++;
    }

    private void CheckWord()
    {
        foreach (var searchingWord in currentGameData.selectedBoardData.SearchWords)
        {
            if (_word == searchingWord.Word && searchingWord.Found == false)
            {
                searchingWord.Found = true;
                GameEvents.CorrectWordMethod(_word, _correctSquareList);
                _completedWords++;
                _word = string.Empty;
                _correctSquareList.Clear();
                CheckBoardCompleted();
                return;
            }
        }
    }

    private bool IsPointOnTheRay(Ray currentRay, Vector3 point)
    {
        var hints = Physics.RaycastAll(currentRay, 100.0f);
        for (int i = 0; i < hints.Length; i++)
        {
            if (hints[i].transform.position == point)
                return true;
        }

        return false;
    }

    private Ray SelectRay(Vector2 firstPosition, Vector2 secondPosition)
    {
        var direction = (secondPosition - firstPosition).normalized;
        float tolerance = 0.01f;
        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance)
            return _rayUp;

        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance)
            return _rayDown;

        if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance)
            return _rayLeft;

        if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance)
            return _rayRight;

        if (direction.x < 0f && direction.y > 0f)
            return _rayDiagonalLeftUp;

        if (direction.x < 0f && direction.y < 0f)
            return _rayDiagonalLeftDown;

        if (direction.x > 0f && direction.y > 0f)
            return _rayDiagonalRightUp;

        if (direction.x > 0f && direction.y < 0f)
            return _rayDiagonalRightDown;

        return _rayDown;
    }

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
    private List<Vector2> positionList;

    private void GetPosition(Vector2 position, GameObject gameObj)
    {
        gameObjectList.Add(gameObj);
        positionList.Add(position);
        //Debug.Log($"position count:{positionList.Count}");
        // Debug.Log($"gameObjectList count:{gameObjectList.Count}");
        // start check ray and draw 

        //get letter
        //GetComponent<GridSquare>().GetLetter();

        if (positionList.Count > 2)
        {
            var firstPosition = positionList[0];
            var secondPosition = positionList[positionList.LastIndexOf(position)];
            //Debug.Log(firstPosition + "<-->" + secondPosition);
            if (firstPosition != secondPosition)
            {
                var direction = (secondPosition - firstPosition).normalized;
                float tolerance = 0.01f;
                if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance)
                {
                    DrawWord(BoardData.Ray.RayUp, firstPosition, secondPosition);
                }

                if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance)
                {
                    DrawWord(BoardData.Ray.RayDown, firstPosition, secondPosition);
                }

                if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance)
                {
                    DrawWord(BoardData.Ray.RayLeft, firstPosition, secondPosition);
                }

                if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance)
                {
                    DrawWord(BoardData.Ray.RayRight, firstPosition, secondPosition);
                }

                if (direction.x < 0f && direction.y > 0f)
                {
                    DrawWord(BoardData.Ray.RayDiagonalLeftUp, firstPosition, secondPosition);
                }

                if (direction.x < 0f && direction.y < 0f)
                {
                    DrawWord(BoardData.Ray.RayDiagonalLeftDown, firstPosition, secondPosition);
                }

                if (direction.x > 0f && direction.y > 0f)
                {
                    DrawWord(BoardData.Ray.RayDiagonalRightUp, firstPosition, secondPosition);
                }

                if (direction.x > 0f && direction.y < 0f)
                {
                    DrawWord(BoardData.Ray.RayDiagonalRightDown, firstPosition, secondPosition);
                }
            }
        }
    }

    int i = 0;

    private void DrawWord(BoardData.Ray ray, Vector2 startPos, Vector2 endPos)
    {
        var list = GetComponent<WordsGrid>().getAllsquarelist();
        switch (ray)
        {
            case BoardData.Ray.RayUp:
                foreach (var square in list)
                {
                    var currentPos = square.transform.position;
                    // ray up check
                    if (currentPos.x == startPos.x && startPos.y <= currentPos.y && currentPos.y <= endPos.y)
                    {
                        GameEvents.SelectSquareMethod(currentPos);
                    }
                }

                break;
            case BoardData.Ray.RayDown:
                foreach (var square in list)
                {
                    var currentPos = square.transform.position;
                    // ray up check
                    if (currentPos.x == startPos.x && startPos.y >= currentPos.y && currentPos.y >= endPos.y)
                    {
                        GameEvents.SelectSquareMethod(currentPos);
                    }
                }

                break;
            case BoardData.Ray.RayLeft:
                foreach (var square in list)
                {
                    var currentPos = square.transform.position;
                    // ray up check
                    if (currentPos.y == startPos.y && startPos.x >= currentPos.x && currentPos.x >= endPos.x)
                    {
                        GameEvents.SelectSquareMethod(currentPos);
                    }
                }

                break;
            case BoardData.Ray.RayRight:
                foreach (var square in list)
                {
                    var currentPos = square.transform.position;
                    // ray up check
                    if (currentPos.y == startPos.y && startPos.x <= currentPos.x && currentPos.x <= endPos.x)
                    {
                        GameEvents.SelectSquareMethod(currentPos);
                    }
                }

                break;
            case BoardData.Ray.RayDiagonalRightUp:
                //Debug.Log("Ray Diagonal Right UP");
                // var canhHuyen = Math.Pow(endPos.x - startPos.x,2) +Math.Pow(endPos.y - startPos.y,2);
                // Debug.Log(Math.Sqrt(canhHuyen));
                
                foreach (var square in list)
                {
                         var currentPos = square.transform.position;
                    //     var hints = Physics.RaycastAll(currentRay, 100.0f);
                    if (currentPos.x / startPos.x == currentPos.y / startPos.y 
                        && endPos.x / currentPos.x == endPos.y / currentPos.y)
                    {
                        GameEvents.SelectSquareMethod(currentPos);
                    }
                }
                break;
        }
    }

    private void ClearPosition()
    {
        gameObjectList.Clear();
        positionList.Clear();
    }
}