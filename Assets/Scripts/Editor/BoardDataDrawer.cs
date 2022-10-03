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
        ConvertToUpperButton();

        if (GameDataInstance.Board != null && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
            DrawBoardTable();

        GUILayout.BeginHorizontal();
        ClearBoardButton();
        FillUpWithRandomLettersButton();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        _dataList.DoLayoutList();

        GUILayout.BeginHorizontal();
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
            //Debug.Log(propertyName);
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Word"), GUIContent.none);
        };
        // foreach (var data in list.l)
        // {
        //     Debug.Log("Show Name:" + data );
        // }    
    }

    private void ConvertToUpperButton()
    {
        if (GUILayout.Button("To Upper"))
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
    }

    private void ClearBoardButton()
    {
        if (GUILayout.Button("Clear Board"))
        {
            for (int i = 0; i < GameDataInstance.Columns; i++)
            {
                for (int j = 0; j < GameDataInstance.Rows; j++)
                {
                    GameDataInstance.Board[i].Row[j] = "";
                }
            }
        }
    }

    private void FillUpWithRandomLettersButton()
    {
        if (GUILayout.Button("Fill Up With Random"))
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
    }

    private BoardData boardData;

    // auto generate word
    private void AutoFillRandomWord()
    {
        //var listWord = _dataList.li;
        if (GUILayout.Button("Auto Fill Words"))
        {
            var list = GameDataInstance.SearchWords;

            var randomColumn = UnityEngine.Random.Range(0, GameDataInstance.Columns - 1);
            var randomRow = UnityEngine.Random.Range(0, GameDataInstance.Rows - 1);
            if (list != null)
            {
                foreach (var data in list)
                {
                    //get letter of words
                    var letters = data.Word.ToCharArray();
                    //Debug.Log("Show letter:"+randomRay);
                }
            }
        }
    }

    /// <summary>
    /// check ray if -
    /// - out of range
    /// - similar word or not
    /// </summary>
    public BoardData.Ray RayCheck(int randomColumn, int randomRow, char[] lettersInWord)
    {
        //ray = (BoardData.Ray)Random.Range(0, Enum.GetValues(typeof(BoardData.Ray)).Length);
        ray = BoardData.Ray.rayUp;
        switch (ray)
        {
            case BoardData.Ray.rayUp:
                //ray up
                if (lettersInWord.Length + randomRow < GameDataInstance.Rows)
                {
                    foreach (var letter in lettersInWord)
                    {
                        if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                            string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                        {
                            randomRow++;
                            //return BoardData.Ray.rayUp;
                        }
                    }

                    if (randomRow == lettersInWord.Length)
                    {
                        return ray;
                    }
                }

                break;
            case BoardData.Ray.rayDown:
                // ray down
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString().ToUpper();
                        randomRow++;
                    }
                }

                break;
            case BoardData.Ray.rayLeft:
                // ray left
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn--;
                    }
                }

                break;
            case BoardData.Ray.rayRight:
                // ray right
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn++;
                    }
                }

                break;
            case BoardData.Ray.rayDiagonalLeftUp:
                // diagonal left up
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn--;
                        randomRow--;
                    }
                }

                break;
            case BoardData.Ray.rayDiagonalLeftDown:
                // diagonal left down
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn--;
                        randomRow++;
                    }
                }

                break;
            case BoardData.Ray.rayDiagonalRightUp:
                // diagonal right up
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn++;
                        randomRow--;
                    }
                }

                break;
            case BoardData.Ray.rayDiagonalRightDown:
                // diagonal right down
                foreach (var letter in lettersInWord)
                {
                    if (letter.Equals(GameDataInstance.Board[randomColumn].Row[randomRow]) ||
                        string.IsNullOrEmpty(GameDataInstance.Board[randomColumn].Row[randomRow]))
                    {
                        GameDataInstance.Board[randomColumn].Row[randomRow] = letter.ToString();
                        randomColumn++;
                        randomRow++;
                    }
                }

                break;
            default:
                break;
        }

        return ray;
    }
}