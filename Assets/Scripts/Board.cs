using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    //Game Manager
    public GameManager gameManager;
    
    //colors for pieces and squares
    public Color black;
    public Color white;
    public Color pieceBlack;
    public Color pieceWhite;

    public Color kingBlack;
    public Color kingWhite;
    
    //The board as represented in integers - 0 means the square is empty,
    //1 means there is a white piece there and -1 means there is a black piece there
    public int[] board = new int[32];
    
    //The board's edge pieces - for solving some (literal) edge cases. First item is the top row,
    //second is the right, third is the bottom, and fourth is the left.
    public int[,] edges =
    {
        { 0, 1, 2, 3 },
        { 3, 11, 19, 27 },
        { 31, 30, 29, 28 },
        { 28, 20, 12, 4 }
    };
    
    //Two arrays for holding the classes that represent the graphical representations of the squares
    //and pieces
    public Square[] squareArray = new Square[32];
    public Piece[] pieceGFXArray = new Piece[32];
    
    //Unity GameObjects that are used to show the graphics of the pieces and squares. 
    public GameObject squareObject;
    public GameObject pieceObject;
    
    //Unity GameObject that is the parent object of all piece GameObjects
    public GameObject piecesHolder;

    //Spcaing between squares on the graphical board
    public int spacing;
    
    //Origin point for creating the board
    public Transform topLeftOrigin;

    private void Start()
    {
        GenerateBoardUI();
        GenerateStartingPosition();
    }

    //Function for generating the 8x8 board graphics and attaching each square graphic to it's appropriate
    //index in the board array (only if it's black)
    public void GenerateBoardUI()
    {
        //Index used to keep track of what black square number is being placed
        int blackSquareIndex = 0;
        for (int y = 0; y < 8; y++)
        {
            //Creates a square object to act as the origin at the beginning of each row (y axis), determines if it
            //should be black or white
            GameObject _cur = Instantiate(squareObject, topLeftOrigin.position + new Vector3(0, -spacing * y), Quaternion.identity, topLeftOrigin);
            if (y % 2 == 0)
            {
                _cur.GetComponent<Image>().color = white;
            }
            else
            {
                _cur.GetComponent<Image>().color = black;
                Square _curSquare = _cur.GetComponent<Square>();
                _curSquare.squareNum = blackSquareIndex;
                _curSquare.squareCoords = new Vector2(1, y + 1);
                squareArray[blackSquareIndex] = _curSquare;
                blackSquareIndex++;
            }
            for (int x = 1; x < 8; x++)
            {
                //Creates 7 more squares in each row after the origin has been placed.
                GameObject _new =  Instantiate(squareObject, _cur.transform.position + new Vector3(spacing, 0), Quaternion.identity, topLeftOrigin);
                _cur = _new;
                if ((x + y) % 2 == 0)
                {
                    _cur.GetComponent<Image>().color = white;
                }
                else
                {
                    _cur.GetComponent<Image>().color = black;
                    Square _curSquare = _cur.GetComponent<Square>();
                    _curSquare.squareNum = blackSquareIndex;
                    _curSquare.squareCoords = new Vector2(x,  y+ 1);
                    squareArray[blackSquareIndex] = _curSquare;
                    blackSquareIndex++;
                }
            }
        }
    }

    //Simple function for generating the starting positions of all the pieces on the board.
    //Could be done manually in inspector
    public void GenerateStartingPosition()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (i < 12)
            {
                board[i] = -1;
            }
            else if (i > 19)
            {
                board[i] = 1;
            }
        }
    }

    //Called every frame, this function is used to convert the integer array board into graphical representation.
    //loops over the board, places pieces where needed and adds them to their array.
    private void Update()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == 0)
            {
                if(pieceGFXArray[i] == null) continue;
                Destroy(pieceGFXArray[i].gameObject);
            }
            if (pieceGFXArray[i] != null) continue;
            
            Piece g = Instantiate(pieceObject, squareArray[i].transform.position, Quaternion.identity, piecesHolder.transform)
                .GetComponent<Piece>();
            g.currentSquare = i;
            pieceGFXArray[i] = g;
            if (board[i] == 1 || board[i] == 2)
            {
                g.SetColor(GameColors.White);
            }
            else if(board[i] == -1 || board[i] == -2)
            {
                g.SetColor(GameColors.Black);
            }
            if (board[i] == 2 || board[i] == -2)
            {
                g.SetIsKing(true);
            }
        }
    }
}

static class Extentions
{
    public static T[] GetRow<T>(this T[,] matrix,int rowNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(1))
            .Select(x => matrix[rowNumber, x])
            .ToArray();

    }
}

