using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling.Experimental;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WordChecker: MonoBehaviour {
  public GameData currentGameData;
  public GameLevelData gameLevelData;

  private string _word;

  private int _assignedPoints = 0;
  private int _completedWords = 0;

  private Vector3 _rayStartPosition;
  private List<int> _SquareIndexList;
  private List<int> _correctWordList;

  private void OnEnable() {
    GameEvents.OnClearSelection += ClearSelection;
    GameEvents.OnLoadNextLevel += LoadNextGameLevel;
    GameEvents.OnGetGrid += GetGrid;
    GameEvents.OnCheckWord += CheckWord;
  }

  private void OnDisable() {
    GameEvents.OnClearSelection -= ClearSelection;
    GameEvents.OnLoadNextLevel -= LoadNextGameLevel;
    GameEvents.OnGetGrid -= GetGrid;
    GameEvents.OnCheckWord -= CheckWord;
  }

  private void LoadNextGameLevel() { SceneManager.LoadScene("GameScene"); }

  void Start() {
    currentGameData.selectedBoardData.ClearData();
    _assignedPoints = 0;
    _completedWords = 0;
    gameObjectList = new List<GameObject>();
    _SquareIndexList = new List<int>();
    _correctWordList = new List<int>();
    gameObjectList.Clear();
    _SquareIndexList.Clear();
    _correctWordList.Clear();
  }

  private void CheckWord() {
    // for loop to get word according to index
    foreach (var index in _SquareIndexList.Distinct()) {
      foreach (var gameObject in GetComponent<WordsGrid>().getAllsquarelist()) {
        _word += gameObject.GetComponent<GridSquare>().GetLetter(index);
      }
    }

    foreach (var searchingWord in currentGameData.selectedBoardData.SearchWords) {
      if ((Reverse(_word.Trim()) == searchingWord.Word || _word.Trim() == searchingWord.Word) &&
          searchingWord.Found == false) {
        searchingWord.Found = true;
        GameEvents.CorrectWordMethod(_word, _SquareIndexList.Distinct().ToList());
        _correctWordList.AddRange(_SquareIndexList.Distinct().ToList());
        _completedWords++;
        _word = string.Empty;
        _SquareIndexList.Clear();
        CheckBoardCompleted();
        return;
      }
    }

    ClearSelection();
  }

  /// <summary>
  /// reverse word to check word from forward and backward selection
  /// </summary>
  /// <param name="s"></param>
  /// <returns></returns>
  private String Reverse(string s) {
    char[] charArray = s.ToCharArray();
    Array.Reverse(charArray);
    return new string(charArray);
  }

  private void ClearSelection() {
    _assignedPoints = 0;
    _SquareIndexList.Clear();
    _word = string.Empty;
    gameObjectList.Clear();
  }

  private void CheckBoardCompleted() {
    bool loadNextCategory = false;
    if (currentGameData.selectedBoardData.SearchWords.Count == _completedWords) {
      // Save current level progress
      var categoryName = currentGameData.selectedCategoryName;
      var currentBoardIndex = DataSaver.ReadCategoryCurrentIndexValues(categoryName);
      var nextBoardIndex = -1;
      var currentCategoryIndex = 0;
      bool readNextLevelName = false;
      for (int index = 0; index < gameLevelData.data.Count; index++) {
        if (readNextLevelName) {
          nextBoardIndex = DataSaver.ReadCategoryCurrentIndexValues(gameLevelData.data[index].categoryName);
          readNextLevelName = false;
        }

        if (gameLevelData.data[index].categoryName == categoryName) {
          readNextLevelName = true;
          currentCategoryIndex = index;
        }
      }

      var currentLevelSize = gameLevelData.data[currentCategoryIndex].boardData.Count;
      if (currentBoardIndex < currentLevelSize) {
        currentBoardIndex += 1;
      }

      DataSaver.SaveCategoryData(categoryName, currentBoardIndex);

      //unlock next category if currentBoardIndex = currentLevelSize
      if (currentBoardIndex >= currentLevelSize) {
        int i = 0;
        currentCategoryIndex++;
        if (currentCategoryIndex < gameLevelData.data.Count) {
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

  /// <summary>
  /// get selected gridsquare at mouse down and mouse up event then draw word from start to end,
  /// if mouse up start check word
  /// </summary>
  /// <param name="gameObj"> </param>
  private void GetGrid(GameObject gameObj) {
    gameObjectList.Add(gameObj);
    var uniqueList = gameObjectList.Distinct().ToList();

    if (uniqueList.Count >= 2) {
      // start and end column 
      var startCol = uniqueList[0].GetComponent<GridSquare>().GetColumn();
      var endCol = uniqueList[uniqueList.LastIndexOf(gameObj)].GetComponent<GridSquare>().GetColumn();
      // start and end row
      var startRow = uniqueList[0].GetComponent<GridSquare>().GetRow();
      var endRow = uniqueList[uniqueList.LastIndexOf(gameObj)].GetComponent<GridSquare>().GetRow();
      // start and end position 
      var startPos = uniqueList[0].transform.position;
      var endPos = uniqueList[uniqueList.LastIndexOf(gameObj)].transform.position;


      if (startPos != endPos) {
        var direction = (endPos - startPos).normalized;
        float tolerance = 0.01f;

        _SquareIndexList.Add(uniqueList[0].GetComponent<GridSquare>().GetIndex());
        // distance from start to end
        var distance = endRow == startRow ? Math.Abs(endCol - startCol) + 1 : Math.Abs(endRow - startRow) + 1;
        //ray Up
        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance) {
          ClearWrongRaySelected();
          // for loop 
          for (int i = 0; i < distance; i++) {
            // ray up start draw word
            if (startCol == endCol && startRow >= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startRow--;
            }
          }
        }

        //ray down
        else if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (startCol == endCol && startRow <= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startRow++;
            }
          }
        }

        //ray left
        else if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (startRow == endRow && startCol >= endCol) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol--;
            }
          }
        }

        // ray right
        else if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (startRow == endRow && startCol <= endCol) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol++;
            }
          }
        }

        //ray diagonal left up
        else if (direction.x < 0f && direction.y > 0f) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (Math.Abs(endCol - startCol) == Math.Abs(endRow - startRow) && startCol >= endCol &&
                startRow >= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol--;
              startRow--;
            }
          }
        }

        // ray diagonal left down
        else if (direction.x < 0f && direction.y < 0f) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (Math.Abs(endCol - startCol) == Math.Abs(endRow - startRow) && startCol >= endCol &&
                startRow <= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol--;
              startRow++;
            }
          }
        }

        // ray diagonal right up
        else if (direction.x > 0f && direction.y > 0f) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (Math.Abs(endCol - startCol) == Math.Abs(endRow - startRow) && startCol <= endCol &&
                startRow >= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol++;
              startRow--;
            }
          }
        }

        //ray diagonal right down
        else if (direction.x > 0f && direction.y < 0f) {
          ClearWrongRaySelected();
          for (int i = 0; i < distance; i++) {
            // ray up check
            if (Math.Abs(endCol - startCol) == Math.Abs(endRow - startRow) && startCol <= endCol &&
                startRow <= endRow) {
              _SquareIndexList.Add(GetComponent<WordsGrid>().GetSquareIndex(startCol, startRow));
              GameEvents.SelectSquareMethod(startCol, startRow);
              startCol++;
              startRow++;
            }
          }
        }
      }
    }
  }

  /// <summary>
  /// clear all wrong selected square
  /// </summary>
  private void ClearWrongRaySelected() {
    _SquareIndexList.RemoveRange(1, _SquareIndexList.Count - 1);
    var list = GetComponent<WordsGrid>().getAllsquarelist();
    Debug.Log($"come here {_correctWordList.Count}");
    foreach (var gameObject in list) {
      gameObject.GetComponent<GridSquare>().ClearSelection();
    }
  }
}