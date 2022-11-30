using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameColors
{
    White,
    Black
}

public enum Directions
{
    x,
    y
}
public class Piece : MonoBehaviour
{
    //Reference to the game board
    [FormerlySerializedAs("board")] public Board boardScript;
    //The square this piece resides on
    public int currentSquare;
    //Reference to this piece's image component
    public Image image;
    //Boolean representing if the piece is being dragged ("held") by the player
    public bool isHeld;
    //Boolean to tell if piece is a king
    public bool king;
    
    //This piece's color
    private GameColors _pieceColor = GameColors.White;
    public GameColors GetColor()
    {
        return _pieceColor;
    }
    public void SetColor(GameColors color)
    {
        this._pieceColor = color;
    }

    public void Start()
    {
        boardScript = FindObjectOfType<Board>();
        
        //Simply sets the graphical color of the piece based on the color variable
        if (_pieceColor == GameColors.White)
        {
            if (king)
            {
                image.color = boardScript.kingWhite;
            }
            else
            {
                image.color = boardScript.pieceWhite;
            }
        }
        else
        {
            if (king)
            {
                image.color = boardScript.kingBlack;
            }
            else
            {
                image.color = boardScript.pieceBlack;
            }
        }
    }

    private void Update()
    {
        //If the player is already holding an object, but that object isn't this object, return,
        //as this piece can't be moved while another is being moved
        if(isHoldingAlready() && isHeld == false) return;
        
        if (IsPointerOverUIElement(GetEventSystemRaycastResults(), gameObject))
        {
            //Code for dragging
            if (Input.GetMouseButton(0))
            {
                transform.position = Input.mousePosition;
                if (isHeld == false)
                {
                    isHeld = true;
                }
            }

            //Code for dropping - first determines if the player is hovering over a square, then, if so,
            //checks that that square is a) not a white square b) not a square occupied by another piece
            //c) a legal square to move to according to the rules of checkers. If any of these conditions is not met, 
            //the held piece is destroyed and replaced in its original position
            
            if (Input.GetMouseButtonUp(0))
            {
                if (IsPointerOverSquare(GetEventSystemRaycastResults()) == null)
                {
                    boardScript.pieceGFXArray[currentSquare] = null;
                    Destroy(gameObject);
                    return;
                }

                Square targetSquare = IsPointerOverSquare(GetEventSystemRaycastResults());
                if (/*white square*/ targetSquare.squareNum != -1 && /*unoccupied square*/ boardScript.board[targetSquare.squareNum] == 0 && IsLegalMove(targetSquare.squareNum))
                {
                    boardScript.board[currentSquare] = 0;

                    if (!king)
                    {
                        boardScript.board[targetSquare.squareNum] =
                            _pieceColor == GameColors.White ? 1 : -1;
                    }
                    else
                    {
                        boardScript.board[targetSquare.squareNum] =
                            _pieceColor == GameColors.White ? 2 : -2;
                    }
                    if (targetSquare.squareNum <= 3 || targetSquare.squareNum >= 28)
                    {
                        boardScript.board[targetSquare.squareNum] =
                            _pieceColor == GameColors.White ? 2 : -2;
                    }

                    boardScript.pieceGFXArray[currentSquare] = null;
                    boardScript.gameManager.OnMoveMade(this);
                    Destroy(gameObject);
                }
                else 
                {
                    boardScript.pieceGFXArray[currentSquare] = null;
                    Destroy(gameObject);
                }
            }
        }
    }

    //Checks if the player is holding any piece
    private bool isHoldingAlready()
    {
        foreach (var piece in boardScript.pieceGFXArray)
        {
            if(piece == null) continue;
            if (piece.isHeld)
            {
                return true;
            }
        }

        return false;
    }

    //Code for finding the neighbor squares of any given piece - the two squares
    //diagonal and forward to the given square number
    private Vector2Int FindNeighbors(int squareNum, GameColors color)
    {
        int smallMod;
        int bigMod;
        if (boardScript.squareArray[squareNum].squareCoords.y % 2 != 0)
        {
            if (color == GameColors.White)
            {
                smallMod = 3;
                bigMod = 4;
            }
            else
            {
                smallMod = 4;
                bigMod = 5;
            }
        }
        else
        {
            if (color == GameColors.White)
            {
                smallMod = 4;
                bigMod = 5;
            }
            else
            {
                smallMod = 3;
                bigMod = 4;
            }
        }

        Vector2Int neighbors = new Vector2Int();
        if (color == GameColors.White)
        {
            neighbors.x = squareNum - smallMod;
            neighbors.y = squareNum - bigMod;
        }
        else
        {
            neighbors.x = squareNum + smallMod;
            neighbors.y = squareNum + bigMod;
        }

        return neighbors;
    }

    //Code for finding ALL adjacent squares of any given square, in an order that allows for easy checking of move legality
    int[] FindNeighborsKing(int squareNum, GameColors color)
        {
            int smallModFwd;
            int bigModFwd;
            int smallModBck;
            int bigModBck;
            
            if (boardScript.squareArray[squareNum].squareCoords.y % 2 != 0)
            {
                if (color == GameColors.White)
                {
                    smallModFwd = 3;
                    bigModFwd = 4;
                    smallModBck = 4;
                    bigModBck = 5;
                }
                else
                {
                    smallModFwd = 4;
                    bigModFwd = 5;
                    smallModBck = 3;
                    bigModBck = 4;
                }
            }
            else
            {
                if (color == GameColors.White)
                {
                    smallModFwd = 4;
                    bigModFwd = 5;
                    smallModBck = 3;
                    bigModBck = 4;
                }
                else
                {
                    smallModFwd = 3;
                    bigModFwd = 4;
                    smallModBck = 4;
                    bigModBck = 5;
                }
            }

        int[] neighbors = new int[4];

        if (color == GameColors.White)
        {
            neighbors[0] = squareNum - smallModFwd;
            neighbors[1] = squareNum - bigModFwd;
            neighbors[2] = squareNum + smallModBck;
            neighbors[3] = squareNum + bigModBck;
        }
        else
        {
            neighbors[0] = squareNum + smallModFwd;
            neighbors[1] = squareNum + bigModFwd;
            neighbors[2] = squareNum - smallModBck;
            neighbors[3] = squareNum - bigModBck;
        }
        
        return neighbors;
    }
    
    //Checks if the move of this piece to the target square is legal.
    //TODO: finish this function, add jumping over pieces and creating a king
    private bool IsLegalMove(int targetSquare)
    {
        if (GameManager.currentTurn != _pieceColor) return false;
        
        //If the player HAS to move a piece (meaning the variable is not equal to -1) and that piece is not this, the move is
        //illegal.
        int forcedPieceCurrent = boardScript.gameManager.forcedPiece;
        if (forcedPieceCurrent != -1 && forcedPieceCurrent != currentSquare)
        {
            return false;
        }

        Vector2Int neighbors = FindNeighbors(currentSquare, _pieceColor);
        int[] kingNeighbors = FindNeighborsKing(currentSquare, _pieceColor);

        //If the target square is adjacent to the current piece, the move is legal.
        if (king)
        {
            if (kingNeighbors.Contains(targetSquare))
            {
                return true;
            }
        }
        if (targetSquare == neighbors.x || targetSquare == neighbors.y)
        {
            return true;
        }

        //If the current piece is a king, loop through all it's "king neighbors", and if it CanCapture any of them,
        //execute the capture and register the move is legal.
        if (king)
        {
            for (int i = 0; i < kingNeighbors.Length; i++)
            {
                if (CanCapture(Directions.x, currentSquare, i)
                    && targetSquare == FindNeighborsKing(kingNeighbors[i], _pieceColor)[i])
                {
                    boardScript.gameManager.OnCapture(GameColors.Black, this, targetSquare);
                    boardScript.board[kingNeighbors[i]] = 0;
                    return true;
                }
            }
        }
        //If the piece is not a king, manually check both possible capture directions, if either returns true the move will be legal
        //and execute the capture.
        else
        {
            if (CanCapture(Directions.x, currentSquare) 
                && targetSquare == FindNeighbors(neighbors.x, _pieceColor).x)
            {
                boardScript.gameManager.OnCapture(_pieceColor == GameColors.White
                    ? GameColors.Black
                    : GameColors.White, this, targetSquare);
                
                boardScript.board[neighbors.x] = 0;
                return true;
            }
            else if (CanCapture(Directions.y, currentSquare)
                     && targetSquare == FindNeighbors(neighbors.y, _pieceColor).y)
            {
                if (king)
                {
                    return false;
                }
                boardScript.gameManager.OnCapture(_pieceColor == GameColors.White
                    ? GameColors.Black
                    : GameColors.White, this, targetSquare);
                
                boardScript.board[neighbors.y] = 0;
                return true;
            }
        }
        return false;
    }

    //Method that returns true if the piece at the location on the board squareToCheck can capture the piece in given direction
    public bool CanCapture(Directions directionToCapture, int squareToCheck, int kingDir = -1)
    {
        Vector2Int neighbors = FindNeighbors(squareToCheck, _pieceColor);
        int[] kingNeighbors = FindNeighborsKing(squareToCheck, _pieceColor);

        //check that the neighbors of the piece are not past the edges of the board, in which case it is on the edge
        //and will not be able to capture
        //TODO: fix bug where king cant ever capture when on edges
        print(kingDir);
        if (king &&
            (kingNeighbors[kingDir] < 0 || boardScript.board.Length <= kingNeighbors[kingDir]) &&
            (FindNeighborsKing(kingNeighbors[kingDir], _pieceColor)[kingDir] < 0 || 
             boardScript.board.Length <= FindNeighborsKing(kingNeighbors[kingDir], _pieceColor)[kingDir]))
        {
            return false;
        }

        if ((neighbors.x < 0 || boardScript.board.Length <= neighbors.x ||
                FindNeighbors(neighbors.x, _pieceColor).x < 0 ||
                FindNeighbors(neighbors.x, _pieceColor).x >= boardScript.board.Length) 
                && directionToCapture == Directions.x)
        {
            return false;
        }

        if ((neighbors.y < 0 || boardScript.board.Length <= neighbors.y ||
             FindNeighbors(neighbors.y, _pieceColor).y < 0 ||
             FindNeighbors(neighbors.y, _pieceColor).y >= boardScript.board.Length) 
             && directionToCapture == Directions.y)
        {
            return false;
        }
        
        //Actual checking if the piece can capture: if the current piece's neighbor in the
        //direction that the piece is trying to capture in is equal to the opposite color's
        //value (1 or -1, 2 or -2) AND there is an open space behind the target, the piece can capture.
        if (_pieceColor == GameColors.White)
        {
            if (king)
            {
                if (boardScript.board[kingNeighbors[kingDir]] == -1 || boardScript.board[kingNeighbors[kingDir]] == -2
                    && boardScript.board[FindNeighborsKing(kingNeighbors[kingDir], _pieceColor)[kingDir]] == 0)
                {
                        print(boardScript.board[kingNeighbors[kingDir]] + " at " + kingNeighbors[kingDir]);
                        return true;
                }
            }
            switch (directionToCapture)
            {
                case Directions.x:
                    return (boardScript.board[neighbors.x] == -1 || boardScript.board[neighbors.x] == -2) 
                           && boardScript.board[FindNeighbors(neighbors.x, _pieceColor).x] == 0;
                case Directions.y:
                    return (boardScript.board[neighbors.y] == -1 || boardScript.board[neighbors.y] == -2)
                           && boardScript.board[FindNeighbors(neighbors.y, _pieceColor).y] == 0;
            }
        }
        else
        {
            if (king)
            {
                for (int i = 0; i < kingNeighbors.Length; i++)
                {
                    if (boardScript.board[kingNeighbors[i]] == 1 || boardScript.board[kingNeighbors[i]] == 2
                        && boardScript.board[FindNeighborsKing(kingNeighbors[i], _pieceColor)[i]] == 0)
                    {
                        return true;
                    }
                }
            }
            switch (directionToCapture)
            {
                case Directions.x:
                    return (boardScript.board[neighbors.x] == 1 || boardScript.board[neighbors.x] == 2)
                           && boardScript.board[FindNeighbors(neighbors.x, _pieceColor).x] == 0;
                case Directions.y:
                    return (boardScript.board[neighbors.y] == 1 || boardScript.board[neighbors.y] == 2)
                           && boardScript.board[FindNeighbors(neighbors.y, _pieceColor).y] == 0;
            }
        }
        return false;
    }
    
    //Checks if pointer is over given UI element
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, GameObject element)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject == element)
                return true;
        }
        return false;
    }
    
    //Checks if pointer is over UI element with Square component
    private Square IsPointerOverSquare(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.TryGetComponent<Square>(out var sq))
                return sq;
        }
        return null;
    }
    
    //Creates a raycast from the pointer to find what it is above
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

}
