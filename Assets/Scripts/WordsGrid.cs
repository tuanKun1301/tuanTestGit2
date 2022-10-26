using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordsGrid: MonoBehaviour {
  public GameData currentGameData;
  public GameObject gridSquarePrefab;
  public AlphabetData alphabetData;

  public float squareOffset = 0.0f;
  public float topPosition;

  private List<GameObject> _squareList = new List<GameObject>();

  private GameObject GameDataInstance;


  // Start is called before the first frame update
  void Start() {
    SpawnGridSquares();
    SetSquarePosition();
  }

  private void SetSquarePosition() {
    var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
    var squareTransform = _squareList[0].GetComponent<Transform>();

    var offset = new Vector2 {
      x = (squareRect.width * squareTransform.localScale.x + squareOffset) * 0.01f,
      y = (squareRect.height * squareTransform.localScale.y + squareOffset) * 0.01f
    };

    var startPosition = GetFirstSquarePosition();
    int columnNumber = 0;
    int rowNumber = 0;

    foreach (var square in _squareList) {
      if (rowNumber + 1 > currentGameData.selectedBoardData.Rows) {
        columnNumber++;
        rowNumber = 0;
      }

      var positionX = startPosition.x + offset.x * columnNumber;
      var positionY = startPosition.y - offset.y * rowNumber;

      square.GetComponent<Transform>().position = new Vector2(positionX, positionY);
      // create board with Two-dimensional arrays and save indexes along with arrays demension
      square.GetComponent<GridSquare>().CreateArray(currentGameData.selectedBoardData.Columns,
        currentGameData.selectedBoardData.Rows);
      square.GetComponent<GridSquare>().SetColumn(columnNumber);
      square.GetComponent<GridSquare>().SetRow(rowNumber);
      square.GetComponent<GridSquare>().SetIndex(_squareList.IndexOf(square));

      rowNumber++;
    }
  }


  private Vector2 GetFirstSquarePosition() {
    var startPosition = new Vector2(0f, transform.position.y);
    var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
    var squareTransform = _squareList[0].GetComponent<Transform>();
    var squareSize = new Vector2(0f, 0f);

    squareSize.x = squareRect.width * squareTransform.localScale.x;
    squareSize.y = squareRect.height * squareTransform.localScale.y;

    var midWidthPosition = (((currentGameData.selectedBoardData.Columns - 1) * squareSize.x) / 2) * 0.01f;
    var midWidthHeight = (((currentGameData.selectedBoardData.Rows - 1) * squareSize.y) / 2) * 0.01f;

    startPosition.x = (midWidthPosition != 0) ? midWidthPosition * -1 : midWidthPosition;
    startPosition.y += midWidthHeight;

    return startPosition;
  }

  #region Spawn_Grid_Square

  private void SpawnGridSquares() {
    if (currentGameData != null) {
      var squareScale = GetSquareScale(new Vector3(1.5f, 1.5f, 0.1f));
      foreach (var squares in currentGameData.selectedBoardData.Board) {
        foreach (var squareLetter in squares.Row) {
          var normalLetterData = alphabetData.AlphabetNormal.Find(data => data.letter == squareLetter);
          var selectedLetterData = alphabetData.AlphabetHighlighted.Find(data => data.letter == squareLetter);
          var correctLetterData = alphabetData.AlphabetWrong.Find(data => data.letter == squareLetter);

          if (normalLetterData.image == null || selectedLetterData.image == null) {
            Debug.LogError("All fields in your array should have some letters. Press Fill up with random");
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying) {
              UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
          }
          else {
            _squareList.Add(Instantiate(gridSquarePrefab));
            _squareList[_squareList.Count - 1].GetComponent<GridSquare>()
              .SetSprite(normalLetterData, correctLetterData, selectedLetterData);
            _squareList[_squareList.Count - 1].transform.SetParent(this.transform);
            _squareList[_squareList.Count - 1].GetComponent<Transform>().position = new Vector3(0f, 0f, 0f);
            _squareList[_squareList.Count - 1].transform.localScale = squareScale;
          }
        }
      }
    }
  }

  #endregion


  private Vector3 GetSquareScale(Vector3 defaultScale) {
    var finalScale = defaultScale;
    var adjustment = 0.01f;

    while (ShouldScaleDown(finalScale)) {
      finalScale.x -= adjustment;
      finalScale.y -= adjustment;

      if (finalScale.x <= 0 || finalScale.y <= 0) {
        finalScale.x = adjustment;
        finalScale.y = adjustment;
        return finalScale;
      }
    }

    return finalScale;
  }

  private bool ShouldScaleDown(Vector3 targetScale) {
    var squareRect = gridSquarePrefab.GetComponent<SpriteRenderer>().sprite.rect;
    var squareSize = new Vector2(0f, 0f);
    var startPosition = new Vector2(0f, 0f);

    squareSize.x = (squareRect.width * targetScale.x) + squareOffset;
    squareSize.y = (squareRect.height * targetScale.y) + squareOffset;

    var midWidthPosition = ((currentGameData.selectedBoardData.Columns * squareSize.x) / 2) * 0.01f;
    var midWidthHeight = ((currentGameData.selectedBoardData.Rows * squareSize.y) / 2) * 0.01f;

    startPosition.x = (midWidthPosition != 0) ? midWidthPosition * -1 : midWidthPosition;
    startPosition.y += midWidthHeight;

    return (startPosition.x < GetHalfScreenWidth() * -1 || startPosition.y > topPosition);
  }

  private float GetHalfScreenWidth() {
    float height = Camera.main.orthographicSize * 2;
    float width = (1.7f * height) * Screen.width / Screen.height;
    return width / 2;
  }

  public List<GameObject> getAllsquarelist() { return _squareList; }

  /// <summary>
  /// get all square index according to column number and row number
  /// </summary>
  /// <param name="column">collumn number in 2 demensional array</param>
  /// <param name="row">row number in 2 demensional array</param>
  /// <returns></returns>
  public int GetSquareIndex(int column, int row) {
    foreach (var square in getAllsquarelist()) {
      if (square.GetComponent<GridSquare>().GetColumn() == column && square.GetComponent<GridSquare>().GetRow() == row)
        return square.GetComponent<GridSquare>().GetIndex();
    }

    return 0;
  }
}