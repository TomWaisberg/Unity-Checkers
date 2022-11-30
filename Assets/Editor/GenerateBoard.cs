using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(BoardEditor))]
public class GenerateBoard : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (GUILayout.Button("Generate Board"))
        {
            Board board = (Board) target;
            board.GenerateBoardUI();
        }
        if (GUILayout.Button("Generate Starting Pieces"))
        {
            Board board = (Board) target;
            board.GenerateStartingPosition();
        }
    }

}
