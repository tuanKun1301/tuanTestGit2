using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Random = UnityEngine.Random;


[CustomEditor(typeof(BoardData), false)]
[CanEditMultipleObjects]
[System.Serializable]
// drawing board data in editor
public class BoardDataDrawer : Editor
{
    private BoardData GameDataInstance => target as BoardData;
    private ReorderableList _dataList;
    public BoardData.Ray ray;

    private void OnEnable()
    {
        InitializeRecordableList(ref _dataList, "SearchWords", " Searching Words");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GameDataInstance.timeInSeconds =
            EditorGUILayout.FloatField("Max Game Time (in Seconds)", GameDataInstance.timeInSeconds);
        DrawColumnsRowsInputFields();
        EditorGUILayout.Space();
        if (GUILayout.Button("To Upper"))
            ConvertToUpperButton();

        if (GameDataInstance.Board != null && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
            DrawBoardTable();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Board"))
            ClearBoardButton();
        if (GUILayout.Button("Fill Up With Random"))
            FillUpWithRandomLettersButton();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        _dataList.DoLayoutList();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Fill Words"))
            AutoFillRandomWord();
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(GameDataInstance);
        }
    }

    private void DrawColumnsRowsInputFields()
    {
        var columnsTemp = GameDataInstance.Columns;
        var rowsTemp = GameDataInstance.Rows;

        GameDataInstance.Columns = EditorGUILayout.IntField("Columns", GameDataInstance.Columns);
        GameDataInstance.Rows = EditorGUILayout.IntField("Rows", GameDataInstance.Rows);

        if ((GameDataInstance.Columns != columnsTemp || GameDataInstance.Rows != rowsTemp) &&
            GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
        {
            GameDataInstance.CreateNewBoard();
        }
    }

    private void DrawBoardTable()
    {
        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 35;

        var columnStyle = new GUIStyle();
        columnStyle.fixedWidth = 50;

        var rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        rowStyle.fixedWidth = 40;
        rowStyle.alignment = TextAnchor.MiddleCenter;

        var textFieldStyle = new GUIStyle();
        textFieldStyle.normal.background = Texture2D.grayTexture;
        textFieldStyle.normal.textColor = Color.white;
        textFieldStyle.fontStyle = FontStyle.Bold;
        textFieldStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.BeginHorizontal(tableStyle);

        for (var x = 0; x < GameDataInstance.Columns; x++)
        {
            EditorGUILayout.BeginVertical(x == -1 ? headerColumnStyle : columnStyle);
            for (var y = 0; y < GameDataInstance.Rows; y++)
            {
                if (x >= 0 && y >= 0)
                {
                    EditorGUILayout.BeginHorizontal(rowStyle);
                    var character = (string)EditorGUILayout.TextArea(GameDataInstance.Board[x].Row[y], textFieldStyle);
                    if (GameDataInstance.Board[x].Row[y].Length > 1)
                    {
                        character = GameDataInstance.Board[x].Row[y].Substring(0, 1);
                    }

                    GameDataInstance.Board[x].Row[y] = character;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void InitializeRecordableList(ref ReorderableList list, string propertyName, string listLabel)
    {
        int i = 0;
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(propertyName),
            true, true, true, true);
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, listLabel); };
        var l = list;
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Word"), GUIContent.none);
        };
           
    }

    private void ConvertToUpperButton()
    {
        for (var i = 0; i < GameDataInstance.Columns; i++)
        {
            for (var j = 0; j < GameDataInstance.Rows; j++)
            {
                var errorCounter = Regex.Matches(GameDataInstance.Board[i].Row[j], @"[a-z]").Count;

                if (errorCounter > 0)
                {
                    GameDataInstance.Board[i].Row[j] = GameDataInstance.Board[i].Row[j].ToUpper();
                }
            }
        }

        foreach (var searchWord in GameDataInstance.SearchWords)
        {
            var errorCounter = Regex.Matches(searchWord.Word, @"[a-z]").Count;
            if (errorCounter > 0)
            {
                searchWord.Word = searchWord.Word.ToUpper();
            }
        }
    }

    private void ClearBoardButton()
    {
        for (int i = 0; i < GameDataInstance.Columns; i++)
        {
            for (int j = 0; j < GameDataInstance.Rows; j++)
            {
                GameDataInstance.Board[i].Row[j] = "";
            }
        }
    }

    private void FillUpWithRandomLettersButton()
    {
        for (int i = 0; i < GameDataInstance.Columns; i++)
        {
            for (int j = 0; j < GameDataInstance.Rows; j++)
            {
                int errorCounter = Regex.Matches(GameDataInstance.Board[i].Row[j], @"[a-zA-Z]").Count;
                string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int index = UnityEngine.Random.Range(0, letters.Length);

                if (errorCounter == 0)
                {
                    GameDataInstance.Board[i].Row[j] = letters[index].ToString();
                }
            }
        }
    }

    /// <summary>
    /// list all drawable ray after check, then random in listDrawableRays to draw word
    /// drawnWord to listed all word which is successfully drawn on board, using to compare to get new word
    /// </summary>
    

    private List<BoardData.Ray> listDrawableRays = new List<BoardData.Ray>();

    // auto generate word
    private void AutoFillRandomWord()
    {
        ConvertToUpperButton();
        ClearBoardButton();
        var listSearchWords = GameDataInstance.SearchWords;

        if (listSearchWords != null)
        {
            for (int i = 0; i < listSearchWords.Count; i++)
            {
                //get letter of words
                var randomColumn = Random.Range(0, GameDataInstance.Columns - 1);
                var randomRow = Random.Range(0, GameDataInstance.Rows - 1);
                var word = listSearchWords[i].Word.ToUpper().ToCharArray();
                //CheckWord(BoardData.Ray.RayUp, word, 0, 5);

                RayCheck(randomColumn, randomRow, word);

                if (listDrawableRays.Count > 0)
                {
                    int index = Random.Range(0, listDrawableRays.Count);
                   
                    //if(index < listDrawableRays.Count)
                    DrawWord(listDrawableRays[index], word, randomColumn, randomRow);
                    listDrawableRays.Clear();
                }
                else
                {
                    RayCheck(Random.Range(0, GameDataInstance.Columns - 1),
                        Random.Range(0, GameDataInstance.Rows - 1), word);
                    i--;
                }
                //
            }
            FillUpWithRandomLettersButton();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="randomColumn"></param>
    /// <param name="randomRow"></param>
    /// <param name="word"></param>
    /// <returns></returns>
    public List<BoardData.Ray> RayCheck(int randomColumn, int randomRow, char[] word)
    {
        listDrawableRays.Clear();

        // minus 1 cuz first letter start in rand position
        // check if RayUp drawable
        if (randomRow - (word.Length - 1) >= 0)
        {
            if (CheckWord(BoardData.Ray.RayUp, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayUp);
            }
        }

        // ray down
        if (randomRow + (word.Length - 1) < GameDataInstance.Rows)
        {
            if (CheckWord(BoardData.Ray.RayDown, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayDown);
            }
        }

        // ray left
        if (randomColumn - (word.Length - 1) >= 0)
        {
            if (CheckWord(BoardData.Ray.RayLeft, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayLeft);
            }
        }

        // ray right
        if (randomColumn + (word.Length - 1) < GameDataInstance.Columns)
        {
            if (CheckWord(BoardData.Ray.RayRight, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayRight);
            }
        }

        // ray diagonal left up
        if (randomRow - (word.Length - 1) >= 0 && randomColumn - (word.Length - 1) >= 0)
        {
            if (CheckWord(BoardData.Ray.RayDiagonalLeftUp, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayDiagonalLeftUp);
            }
        }

        // ray diagonal left down
        if (randomRow + (word.Length - 1) < GameDataInstance.Rows && randomColumn - (word.Length - 1) >= 0)
        {
            if (CheckWord(BoardData.Ray.RayDiagonalLeftDown, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayDiagonalLeftDown);
            }
        }

        // ray diagonal right up
        if (randomRow - (word.Length - 1) >= 0 && randomColumn + (word.Length - 1) < GameDataInstance.Columns)
        {
            if (CheckWord(BoardData.Ray.RayDiagonalRightUp, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayDiagonalRightUp);
            }
        }

        //ray diagonal right down
        if (randomRow + (word.Length - 1) < GameDataInstance.Rows &&
            randomColumn + (word.Length - 1) < GameDataInstance.Columns)
        {
            if (CheckWord(BoardData.Ray.RayDiagonalRightDown, word, randomColumn, randomRow))
            {
                listDrawableRays.Add(BoardData.Ray.RayDiagonalRightDown);
            }
        }

        listDrawableRays.Distinct().ToList();
        return listDrawableRays;
    }

    public Boolean CheckWord(BoardData.Ray ray, char[] word, int ranCol, int ranRow)
    {
        switch (ray)
        {
            case BoardData.Ray.RayUp:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow--;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayDown:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow++;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayLeft:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranCol--;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayRight:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranCol++;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayDiagonalLeftUp:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow--;
                        ranCol--;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayDiagonalLeftDown:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow++;
                        ranCol--;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayDiagonalRightUp:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow--;
                        ranCol++;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            case BoardData.Ray.RayDiagonalRightDown:
                foreach (var letter in word)
                {
                    if (letter.ToString().Equals(GameDataInstance.Board[ranCol].Row[ranRow].Trim().ToUpper()) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[ranCol].Row[ranRow]))
                    {
                        ranRow++;
                        ranCol++;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            default:
                return false;
        }
    }

    public void DrawWord(BoardData.Ray ray, char[] word, int column, int row)
    {
        switch (ray)
        {
            case BoardData.Ray.RayUp:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row--;
                }

                break;
            case BoardData.Ray.RayDown:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row++;
                }

                break;
            case BoardData.Ray.RayLeft:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    column--;
                }

                break;
            case BoardData.Ray.RayRight:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    column++;
                }

                break;
            case BoardData.Ray.RayDiagonalLeftUp:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row--;
                    column--;
                }

                break;
            case BoardData.Ray.RayDiagonalLeftDown:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row++;
                    column--;
                }

                break;
            case BoardData.Ray.RayDiagonalRightUp:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row--;
                    column++;
                }

                break;
            case BoardData.Ray.RayDiagonalRightDown:
                foreach (var letter in word)
                {
                    GameDataInstance.Board[column].Row[row] = letter.ToString().ToUpper();
                    row++;
                    column++;
                }

                break;
        }
    }
}