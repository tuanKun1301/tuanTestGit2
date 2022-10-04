using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class BoardData : ScriptableObject
{
    [System.Serializable]
    public class SeachingWord
    {
        [HideInInspector] public bool Found = false;
        public string Word;
    }
    public enum Ray
    {
        rayUp,
        rayDown,
        rayLeft,
        rayRight,
        rayDiagonalLeftUp,
        rayDiagonalLeftDown,
        rayDiagonalRightUp,
        rayDiagonalRightDown,
    }
    [System.Serializable]
    public class BoardRow
    {
        public int Size;
        public string[] Row;

        
        public BoardRow()
        {
        }

        public BoardRow(int size)
        {
            CreateRow(size);
        }

        public void CreateRow(int size)
        {
            Size = size;
            Row = new string[Size];
            ClearRow();
        }

        public void ClearRow()
        {
            for (int i = 0; i < Size; i++)
            {
                Row[i] = " ";
            }
        }
    }

    public float timeInSeconds;
    public int Columns = 0;
    public int Rows = 0;

    public void ClearData()
    {
        foreach (var word in SearchWords)
        {
            word.Found = false;
        }
    }

    public BoardRow[] Board;
    public List<SeachingWord> SearchWords = new List<SeachingWord>();

    public void ClearWithEmptyString()
    {
        for (int i = 0; i < Columns; i++)
        {
            Board[i].ClearRow();
        }
    }

    public List<SeachingWord> GetSearchWords()
    {
        return SearchWords;
    }

    public void CreateNewBoard()
    {
        Board = new BoardRow[Columns];
        for (int i = 0; i < Columns; i++)
        {
            Board[i] = new BoardRow(Rows);
        }
    }
}