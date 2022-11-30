using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameColors currentTurn = GameColors.White;
    public Board boardScript;
    public int numMoves = 0;
    private int numWhite = 12;
    private int numBlack = 12;

    private bool captureMadeThisTurn = false;
    public int forcedPiece = -1;
    public Directions forcedDirection;

    public void OnMoveMade(Piece pieceMoved)
    {
        if (!captureMadeThisTurn)
        {
            currentTurn = pieceMoved.GetColor() == GameColors.Black ? GameColors.White : GameColors.Black;
        }
        numMoves++;
        if (numBlack == 0)
        {
            Debug.Log("White Wins!");
        }
        else if (numWhite == 0)
        {
            Debug.Log("Black Wins!");
        }
    }

    public void OnCapture(GameColors colorCaptured, Piece pieceCapturing, int landedOnSquare)
    {
        //TODO: make sure double capture directions are forced,
        //make sure theres a way for player to choose between double capture in both directions
        
        //After every capture, check if the piece that captured can capture further. if so, force that piece
        //(the one on the square that was landed on  in the capture) to make the double capture.
        captureMadeThisTurn = true;
        if (pieceCapturing.CanCapture(Directions.x, landedOnSquare))
        {
            forcedPiece = landedOnSquare;
            forcedDirection = Directions.x;
            currentTurn = pieceCapturing.GetColor();
        }
        else if (pieceCapturing.CanCapture(Directions.y, landedOnSquare))
        {
            forcedPiece = landedOnSquare;
            forcedDirection = Directions.y;
            currentTurn = pieceCapturing.GetColor();
        }
        else
        {
            captureMadeThisTurn = false;
            forcedPiece = -1;
            currentTurn = colorCaptured;
        }
        if (colorCaptured == GameColors.White)
        {
            numWhite--;
        }
        else if (colorCaptured == GameColors.Black)
        {
            numBlack--;
        }
    }
}
