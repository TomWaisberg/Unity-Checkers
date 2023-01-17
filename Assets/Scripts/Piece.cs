using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extras;
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
    X,
    Y,
    None,
    Both
}
public class Piece : MonoBehaviour
{
    //Reference to the game board (hence public)
    [FormerlySerializedAs("board")] public Board boardScript;
    //The square this piece resides on
    private int currentSquare;
    //Reference to this piece's image component (hence public)
    public Image image;
    //Boolean representing if the piece is being dragged ("held") by the player
    private bool isHeld;
    //Boolean to tell if piece is a king
    private bool king;
    
    //This piece's color
    private GameColors pieceColor = GameColors.White;

    public Square targetSquare;
    
    //This bool is used to decide if a capture is needed based on the move the player chose, and the Piece variable
    //is used to feed the captured piece to the game manager
    
    bool doCapture;
    Piece capturedPiece = null;

    //Getters and Setters
    public int GetCurrentSquare()
    {
        return currentSquare;
    }
    public void SetCurrentSquare(int value)
    {
        currentSquare = value;
    }

    public bool GetIsKing()
    {
        return king;
    }
    public void SetIsKing(bool value)
    {
        king = value;
    }
    
    public GameColors GetColor()
    {
        return pieceColor;
    }
    public void SetColor(GameColors color)
    {
        this.pieceColor = color;
    }

    public void Start()
    {
        boardScript = FindObjectOfType<Board>();
        
        //Simply sets the graphical color of the piece based on the color variable
        if (pieceColor == GameColors.White)
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
        
        //If piece is black, return
        if (pieceColor == GameColors.Black && boardScript.gameManager.useAI)
        {
            return;
        }
        
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

                targetSquare = IsPointerOverSquare(GetEventSystemRaycastResults());
                if (IsLegalMove(targetSquare.squareNum))
                {
                    boardScript.board[currentSquare] = 0;
                    if (doCapture)
                    {
                        boardScript.gameManager.OnCapture(capturedPiece, this, targetSquare.squareNum);
                        doCapture = false;
                        capturedPiece = null;
                    }

                    if (!king)
                    {
                        boardScript.board[targetSquare.squareNum] =
                            pieceColor == GameColors.White ? 1 : -1;
                    }
                    else
                    {
                        boardScript.board[targetSquare.squareNum] =
                            pieceColor == GameColors.White ? 2 : -2;
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
    public Vector2Int FindNeighbors(int squareNum, GameColors color)
    {
        int smallMod = 0;
        int bigMod = 0;
        
        //Check what the modifiers that should be applied to find the neighbors will be and make sure the square checked is
        //inside the bounds of the board array
        if (squareNum < boardScript.board.Length && squareNum >= 0)
        {
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
        }
        else
        {
            if (pieceColor == GameColors.White)
            {
                smallMod = squareNum + 1;
                bigMod = squareNum + 1;
            }
            else
            {
                smallMod = squareNum - 1;
                bigMod = squareNum - 1;
            }
        }
        Vector2Int neighbors = new Vector2Int();
        
        //Applying the modifiers in a way that accounts for the exceptions: pieces on the edge of the board have some
        //neighbors marked as -1, that is, the neighbors that do not exist
        if (color == GameColors.White)
        {
            if (boardScript.edges.GetRow(0).Contains(squareNum))
            {
                neighbors.x = -1;
                neighbors.y = -1;
            }
            else if (boardScript.edges.GetRow(1).Contains(squareNum))
            {
                neighbors.x = -1;
                neighbors.y = squareNum - bigMod;
            }
            else if (boardScript.edges.GetRow(3).Contains(squareNum))
            {
                neighbors.y = -1;
                neighbors.x = squareNum - smallMod;
            }
            else
            {
                neighbors.x = squareNum - smallMod;
                neighbors.y = squareNum - bigMod;
            }
        }
        else
        {
            if (boardScript.edges.GetRow(2).Contains(squareNum))
            {
                neighbors.x = -1;
                neighbors.y = -1;
            }
            else if(boardScript.edges.GetRow(1).Contains(squareNum))
            {
                neighbors.x = squareNum + smallMod;
                neighbors.y = -1;
            }
            else if(boardScript.edges.GetRow(3).Contains(squareNum))
            {
                neighbors.x = -1;
                neighbors.y = squareNum + bigMod;
            }
            else
            {
                neighbors.x = squareNum + smallMod;
                neighbors.y = squareNum + bigMod;
            }
        }

        return neighbors;
    }

    //Code for finding ALL adjacent squares of any given square, in an order that allows for easy checking of move legality
    public int[] FindNeighborsKing(int squareNum, GameColors color)
        {
            int smallModFwd = 0;
            int bigModFwd = 0;
            int smallModBck = 0;
            int bigModBck = 0;
            
            //Check what the modifiers that should be applied to find the neighbors will be
            if (squareNum < boardScript.board.Length && squareNum >= 0)
            {
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
            }
            else
            {
                if (pieceColor == GameColors.White)
                {
                    smallModFwd = squareNum + 1;
                    bigModFwd = squareNum + 1;
                    smallModBck = squareNum - 1;
                    bigModBck = squareNum - 1;
                }
                else
                {
                    smallModFwd = squareNum - 1;
                    bigModFwd = squareNum - 1;
                    smallModBck = squareNum + 1;
                    bigModBck = squareNum + 1;
                }
            }
            int[] neighbors = new int[4];

        //Applying the modifiers in a way that accounts for the exceptions: pieces on the edge of the board have some
        //neighbors marked as -1, that is, the neighbors that do not exist
        if (color == GameColors.White)
        {
            if (boardScript.edges.GetRow(0).Contains(squareNum))
            {
                neighbors[0] = -1;
                neighbors[1] = -1;
                neighbors[2] = squareNum + smallModBck;
                neighbors[3] = squareNum + bigModBck;
            }
            else if (boardScript.edges.GetRow(1).Contains(squareNum))
            {
                neighbors[0] = -1;
                neighbors[1] = squareNum - bigModFwd;
                neighbors[2] = -1;
                neighbors[3] = squareNum + bigModBck;
            }
            else if (boardScript.edges.GetRow(2).Contains(squareNum))
            {
                neighbors[0] = squareNum - smallModFwd;
                neighbors[1] = squareNum - bigModFwd;
                neighbors[2] = -1;
                neighbors[3] = -1;
            }
            else if (boardScript.edges.GetRow(3).Contains(squareNum))
            {
                neighbors[0] = squareNum - smallModFwd;
                neighbors[1] = -1;
                neighbors[2] = squareNum + smallModBck;
                neighbors[3] = -1;
            }
            else
            {
                neighbors[0] = squareNum - smallModFwd;
                neighbors[1] = squareNum - bigModFwd;
                neighbors[2] = squareNum + smallModBck;
                neighbors[3] = squareNum + bigModBck;
            }
        }
        else
        {
            if (boardScript.edges.GetRow(0).Contains(squareNum))
            {
                neighbors[0] = squareNum + smallModFwd;
                neighbors[1] = squareNum + bigModFwd;
                neighbors[2] = -1;
                neighbors[3] = -1;
            }
            else if (boardScript.edges.GetRow(1).Contains(squareNum))
            {
                neighbors[0] = squareNum + smallModFwd;
                neighbors[1] = -1;
                neighbors[2] = squareNum - smallModBck;
                neighbors[3] = -1;
            }
            else if (boardScript.edges.GetRow(2).Contains(squareNum))
            {
                neighbors[0] = -1;
                neighbors[1] = -1;
                neighbors[2] = squareNum - smallModBck;
                neighbors[3] = squareNum - bigModBck;
            }
            else if (boardScript.edges.GetRow(3).Contains(squareNum))
            {
                neighbors[0] = -1;
                neighbors[1] = squareNum + bigModFwd;
                neighbors[2] = -1;
                neighbors[3] = squareNum - bigModBck;
            }
            else
            {
                neighbors[0] = squareNum + smallModFwd;
                neighbors[1] = squareNum + bigModFwd;
                neighbors[2] = squareNum - smallModBck;
                neighbors[3] = squareNum - bigModBck;
            }
        }
        
        return neighbors;
    }
    
    //Checks if the move of this piece to the target square is legal.
    private bool IsLegalMove(int targetSquare)
    {
        if (GameManager.currentTurn != pieceColor) return false;
        
        if(targetSquare > boardScript.board.Length || targetSquare < 0)
        {
            return false;
        }
        
        if (boardScript.board[targetSquare] != 0)
        {
            return false;
        }

        //If the player HAS to move a piece (meaning the variable is not equal to -1) and that piece is not this, the move is
        //illegal.
        int forcedPieceCurrent = boardScript.gameManager.GetForcedPiece();
        if (forcedPieceCurrent != -1 && forcedPieceCurrent != currentSquare)
        {
            return false;
        }
        
        Vector2Int neighbors = FindNeighbors(currentSquare, pieceColor);
        int[] kingNeighbors = FindNeighborsKing(currentSquare, pieceColor);
        
        //If the player has to move in a certain direction, do not allow any move in a different direction. same for 
        //kings
        Directions forcedDirCurrent = boardScript.gameManager.GetForcedDirection();
        int[] forcedDirectionsKing = boardScript.gameManager.GetForcedDirectionsKing();
        if (forcedDirCurrent != Directions.None && forcedDirCurrent != Directions.Both)
        {
            if (targetSquare == neighbors.x && forcedDirCurrent != Directions.X)
            {
                return false;
            }
            else if (targetSquare == neighbors.y && forcedDirCurrent != Directions.Y)
            {
                return false;
            }
        }

        if (king)
        {
            if (forcedPieceCurrent != -1)
            {
                for (int i = 0; i < forcedDirectionsKing.Length; i++)
                {
                    if (kingNeighbors[i] == targetSquare && forcedDirectionsKing[i] != 1)
                    {
                        return false;
                    }
                }
            }
        }

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
                if (!(kingNeighbors[i] >= boardScript.board.Length) && kingNeighbors[i] >= 0)
                {
                    if (CanCapture(Directions.X, currentSquare, i)
                        && targetSquare == FindNeighborsKing(kingNeighbors[i], pieceColor)[i])
                    {
                        doCapture = true;
                        capturedPiece = boardScript.pieceGFXArray[kingNeighbors[i]];
                        return true;
                    }
                }
            }
        }
        //If the piece is not a king, manually check both possible capture directions, if either returns true the move will be legal
        //and execute the capture.
        else
        {
            if (CanCapture(Directions.X, currentSquare) 
                && targetSquare == FindNeighbors(neighbors.x, pieceColor).x)
            {
                doCapture = true;
                capturedPiece = boardScript.pieceGFXArray[neighbors.x];
                return true;
            }
            else if (CanCapture(Directions.Y, currentSquare)
                     && targetSquare == FindNeighbors(neighbors.y, pieceColor).y)
            {
                if (king)
                {
                    return false;
                }

                doCapture = true;
                capturedPiece = boardScript.pieceGFXArray[neighbors.y];
                return true;
            }
        }
        return false;
    }

    //Method that returns true if the piece at the location on the board squareToCheck can capture the piece in given direction
    public bool CanCapture(Directions directionToCapture, int squareToCheck, int kingDir = -1)
    {
        Vector2Int neighbors = FindNeighbors(squareToCheck, pieceColor);
        int[] kingNeighbors = FindNeighborsKing(squareToCheck, pieceColor);

        //check that the neighbors of the piece are not past the edges of the board, in which case it is on the edge
        //and will not be able to capture
        if (king)
        {
            if((kingNeighbors[kingDir] < 0 || boardScript.board.Length <= kingNeighbors[kingDir]) &&
            (FindNeighborsKing(kingNeighbors[kingDir], pieceColor)[kingDir] < 0 || 
             boardScript.board.Length <= FindNeighborsKing(kingNeighbors[kingDir], pieceColor)[kingDir]))
            {
                return false;
            }
        }
        else
        {
            if ((neighbors.x < 0 || boardScript.board.Length <= neighbors.x ||
                 FindNeighbors(neighbors.x, pieceColor).x < 0 ||
                 FindNeighbors(neighbors.x, pieceColor).x >= boardScript.board.Length) 
                && directionToCapture == Directions.X)
            {
                return false;
            }

            if ((neighbors.y < 0 || boardScript.board.Length <= neighbors.y ||
                 FindNeighbors(neighbors.y, pieceColor).y < 0 ||
                 FindNeighbors(neighbors.y, pieceColor).y >= boardScript.board.Length) 
                && directionToCapture == Directions.Y)
            {
                return false;
            }
        }

        //Actual checking if the piece can capture: if the current piece's neighbor in the
        //direction that the piece is trying to capture in is equal to the opposite color's
        //value (1 or -1, 2 or -2) AND there is an open space behind the target, the piece can capture.
        if (pieceColor == GameColors.White)
        {
            if (king)
            {
                if (boardScript.board[kingNeighbors[kingDir]] == -1 || boardScript.board[kingNeighbors[kingDir]] == -2
                    && boardScript.board[FindNeighborsKing(kingNeighbors[kingDir], pieceColor)[kingDir]] == 0)
                {
                        return true;
                }
            }
            else
            {
                switch (directionToCapture)
                {
                    case Directions.X:
                        return (boardScript.board[neighbors.x] == -1 || boardScript.board[neighbors.x] == -2) 
                               && boardScript.board[FindNeighbors(neighbors.x, pieceColor).x] == 0;
                    case Directions.Y:
                        return (boardScript.board[neighbors.y] == -1 || boardScript.board[neighbors.y] == -2)
                               && boardScript.board[FindNeighbors(neighbors.y, pieceColor).y] == 0;
                }
            }
        }
        else
        {
            if (king)
            {
                if (boardScript.board[kingNeighbors[kingDir]] == 1 || boardScript.board[kingNeighbors[kingDir]] == 2
                    && boardScript.board[FindNeighborsKing(kingNeighbors[kingDir], pieceColor)[kingDir]] == 0)
                {
                    return true;
                }
            }
            else
            {
                switch (directionToCapture)
                {
                    case Directions.X:
                        return (boardScript.board[neighbors.x] == 1 || boardScript.board[neighbors.x] == 2)
                               && boardScript.board[FindNeighbors(neighbors.x, pieceColor).x] == 0;
                    case Directions.Y:
                        return (boardScript.board[neighbors.y] == 1 || boardScript.board[neighbors.y] == 2)
                               && boardScript.board[FindNeighbors(neighbors.y, pieceColor).y] == 0;
                }
            }
        }
        return false;
    }

    //A function to get all the legal moves of this piece
    // there are conditionals to check that the neighbors of the piece are not past the edges of the board, in which case it is on the edge
    //the move will not be added to the list
    public List<Move> GetAllLegalMoves()
    {
        List<Move> allMoves = new List<Move>();
        
        int[] kingNeighbors = FindNeighborsKing(currentSquare, pieceColor);
        Vector2Int neighbors = FindNeighbors(currentSquare, pieceColor);
        
        if (king)
        {
            for (int i = 0; i < kingNeighbors.Length; i++)
            {
                if (IsLegalMove(kingNeighbors[i]))
                {
                    allMoves.Add(new Move(currentSquare, kingNeighbors[i]));
                }

                if (IsLegalMove(FindNeighborsKing(kingNeighbors[i], pieceColor)[i]))
                {
                    allMoves.Add(new Move(currentSquare, FindNeighborsKing(kingNeighbors[i], pieceColor)[i], capturedPiece));
                }
            }
        }
        else
        {
            if (IsLegalMove(neighbors.x))
            {
                allMoves.Add(new Move(currentSquare, neighbors.x));
            }
            if (IsLegalMove(FindNeighbors(neighbors.x, pieceColor).x))
            {
                allMoves.Add(new Move(currentSquare, FindNeighbors(neighbors.x, pieceColor).x, capturedPiece));
            }

            if (IsLegalMove(neighbors.y))
            {
                allMoves.Add(new Move(currentSquare, neighbors.y));
            }
            if (IsLegalMove(FindNeighbors(neighbors.y, pieceColor).y))
            {
                allMoves.Add(new Move(currentSquare, FindNeighbors(neighbors.y, pieceColor).y, capturedPiece));
            }
        }

        return allMoves;
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
