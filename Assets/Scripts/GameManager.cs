using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Extras;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public bool useAI;
    
    public static GameColors currentTurn = GameColors.White;
    public Board boardScript;
    private int numMoves = 0;
    private int numWhite = 12;
    private int numBlack = 12;
    
    private bool captureMadeThisTurn = false;
    private int forcedPiece = -1;
    private Directions forcedDirection = Directions.None;
    private int[] forcedDirectionsKing = {0, 0, 0, 0};

    private void Awake()
    {
        captureMadeThisTurn = false;
        forcedPiece = -1;
        forcedDirection = Directions.None;
        forcedDirectionsKing = new[] {0, 0, 0, 0};
    }

    public int GetForcedPiece()
    {
        return forcedPiece;
    }
    public void SetForcedPiece(int value)
    {
        forcedPiece = value;
    }
    
    public Directions GetForcedDirection()
    {
        return forcedDirection;
    }
    public void SetForcedDirection(Directions value)
    {
        forcedDirection = value;
    }
    
    public int[] GetForcedDirectionsKing()
    {
        return forcedDirectionsKing;
    }
    public void SetForcedDirectionsKing(int[] value)
    {
        forcedDirectionsKing = value;
    }

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
        
        if (pieceMoved.GetColor() == GameColors.White && 
            boardScript.edges.GetRow(0).Contains(pieceMoved.targetSquare.squareNum))
        {
            boardScript.board[pieceMoved.targetSquare.squareNum] = 2;
        }
        else if (pieceMoved.GetColor() == GameColors.Black && 
            boardScript.edges.GetRow(2).Contains(pieceMoved.targetSquare.squareNum))
        {
            boardScript.board[pieceMoved.targetSquare.squareNum] = -2;
        }
        
        //Every turn, calculate all enemy's moves. Chose one of them to play TODO: make it smart
        Invoke(nameof(DoEnemyMove), 0.3f);
    }

    
    //TODO: black king has problems with forced capture direction
    public void OnCapture(Piece pieceCaptured, Piece pieceCapturing, int landedOnSquare)
    {
        //After every capture, check if the piece that captured can capture further. if so, force that piece
        //(the one on the square that was landed on  in the capture) to make the double capture.
        captureMadeThisTurn = true;
        boardScript.board[pieceCaptured.GetCurrentSquare()] = 0;
        if (pieceCapturing.GetIsKing())
        {
            int[] kingNeighbors = pieceCapturing.FindNeighborsKing(landedOnSquare, pieceCapturing.GetColor());
            bool foundJump = false;
            for (int i = 0; i < kingNeighbors.Length; i++)
            {
                if (!(kingNeighbors[i] >= boardScript.board.Length) && kingNeighbors[i] >= 0 && 
                    !(pieceCapturing.FindNeighborsKing(kingNeighbors[i], pieceCapturing.GetColor())[i] < 0 || 
                      boardScript.board.Length <= pieceCapturing.FindNeighborsKing(kingNeighbors[i], pieceCapturing.GetColor())[i]))
                {
                    if (pieceCapturing.CanCapture(Directions.X, landedOnSquare, i) 
                        && pieceCapturing.FindNeighborsKing(kingNeighbors[i], pieceCapturing.GetColor())[i] != pieceCapturing.GetCurrentSquare()
                        && boardScript.board[pieceCapturing.FindNeighborsKing(kingNeighbors[i], pieceCapturing.GetColor())[i]] == 0)
                    {
                        forcedPiece = landedOnSquare;
                        forcedDirectionsKing[i] = 1;
                        currentTurn = pieceCapturing.GetColor();
                        foundJump = true;
                    }
                }
            }

            if (!foundJump)
            {
                captureMadeThisTurn = false;
                forcedDirectionsKing = new []{0, 0, 0, 0};
                forcedPiece = -1;
                currentTurn = pieceCaptured.GetColor();
            }
        }
        else if (pieceCapturing.CanCapture(Directions.X, landedOnSquare))
        {
            forcedPiece = landedOnSquare;
            forcedDirection = Directions.X;
            currentTurn = pieceCapturing.GetColor();
        }
        else if (pieceCapturing.CanCapture(Directions.Y, landedOnSquare))
        {
            forcedPiece = landedOnSquare;
            forcedDirection =  Directions.Y;
            currentTurn = pieceCapturing.GetColor();
        }
        else
        {
            captureMadeThisTurn = false;
            forcedDirection = Directions.None;
            forcedPiece = -1;
            currentTurn = pieceCaptured.GetColor();
        }
        
        switch (pieceCaptured.GetColor())
        {
            case GameColors.White:
                numWhite--;
                break;
            case GameColors.Black:
                numBlack--;
                break;
        }
    }

    void DoEnemyMove()
    {
        if (currentTurn == GameColors.Black && useAI)
        {
            List<Move> allEnemyMoves = new List<Move>();
            foreach (var piece in boardScript.pieceGFXArray)
            {
                if (piece != null)
                {
                    if (piece.GetColor() == GameColors.Black)
                    {
                        foreach (var move in piece.GetAllLegalMoves())
                        {
                            allEnemyMoves.Add(move);
                        }
                    }
                }
            }
            Move chosenMove = allEnemyMoves[UnityEngine.Random.Range(0, allEnemyMoves.Count)];
            
            //Do another check to see that the randomly chosen piece has not been captured this turn, as the gfx 
            //entity is deleted only slightly after.
            if (boardScript.board[chosenMove.GetOriginSquare()] != 0)
            {
                if (!(chosenMove.GetPieceCaptured() is null))
                {
                    OnCapture(chosenMove.GetPieceCaptured(), boardScript.pieceGFXArray[chosenMove.GetOriginSquare()],chosenMove.GetTargetSquare());
                }
                boardScript.pieceGFXArray[chosenMove.GetOriginSquare()].targetSquare = 
                    boardScript.squareArray[chosenMove.GetTargetSquare()];

                if (boardScript.board[chosenMove.GetOriginSquare()] == -2)
                {
                    boardScript.board[chosenMove.GetTargetSquare()] = -2;
                }
                else
                {
                    boardScript.board[chosenMove.GetTargetSquare()] = -1;
                }
                boardScript.board[chosenMove.GetOriginSquare()] = 0;
                OnMoveMade(boardScript.pieceGFXArray[chosenMove.GetOriginSquare()]);
            }
        }
    }
}
