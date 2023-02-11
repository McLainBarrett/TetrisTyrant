/*
    Recreate tetris

    Build Neural Network trainer to play tetris
        Training is x-minute game, prioritize getting more points faster

    Mess with NN
        Give it all one piece (s shape)
        Give it non standard piece (5 block pieces)
        Give it a pre-played(badly) board
        
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetris : MonoBehaviour {

    public bool[,] Gameboard = new bool[10,20];
    public Vector2Int[] Tetromino = new Vector2Int[4];

    public int score = 0;
    public int curTet = 0;//I J L O S T Z
    protected int curTetX = 2;
    protected int curTetY = 20;
    protected int curTetR = 0;

    public int[] queue = new int[3];
    public int heldTet = -1;
    public bool usedHeld;

    public float tickLength = 0.5f;
    protected float tickTimer;


    public GameObject Pixel;
    public SpriteRenderer[,] RenderMatrix = new SpriteRenderer[10,20];
    public SpriteRenderer[,] HoldMatrix = new SpriteRenderer[4,4];
    public SpriteRenderer[,,] QueueMatrices = new SpriteRenderer[3,4,4];


    /*  To Do:
        https://tetris.fandom.com/wiki/Tetris_Guideline
    */


    void Start() {
        tickTimer = tickLength;
        for(int i = 0; i < queue.Length; i++)
            queue[i] = Random.Range(0,7);
        instantiateRenderers();
        spawnTet();
    }

    void Update() {
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0) {
            tickTimer = tickLength;

            if (!willCollide(Tetromino, curTetX,curTetY-1)) {
                curTetY -= 1;
            } else {
                //grace period of one tick?
                tetToBoard();
            }
        }


        //Color
        for (int i = 0; i < Tetromino.Length; i++)
            if (iB(0, curTetX+Tetromino[i].x, 10) && iB(0, curTetY+Tetromino[i].y, 20))
                    RenderMatrix[curTetX+Tetromino[i].x,curTetY+Tetromino[i].y].color = getColor(curTet);

        var myCol = getColor(heldTet);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                HoldMatrix[x,y].color = myCol;
        
        for (int i = 0; i < queue.Length; i++) {
            myCol = getColor(queue[i]);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    QueueMatrices[i,x,y].color = myCol; }


        //Controls
        if (Input.GetKeyDown(KeyCode.F) && !usedHeld) {
            int temp = curTet;
            curTet = heldTet;
            heldTet = temp;

            curTetX = 2;
            curTetY = 20;
            curTetR = 0;
            if (curTet == -1)
                curTet = chooseTet();
            Tetromino = getTet(curTet);
            
            usedHeld = true;
        }
        moveTet();
        rotateTet();

        //render
        renderBoard();
    }

    void spawnTet() {
        lineClearCheck();
        curTet = chooseTet();
        Tetromino = getTet(curTet);
        usedHeld = false;

        curTetX = 2;
        curTetY = 20;
        curTetR = 0;
    }

    int chooseTet() {
        int chosenTet = queue[0];
        for(int i = 0; i < queue.Length-1; i++)
            queue[i] = queue[i+1];
        queue[queue.Length-1] = Random.Range(0, 7);//Grab bag to be implemented later :)
        return chosenTet;
    }

    Vector2Int[] getTet(int ID) {
        switch (ID) {
            case 0:
                return new Vector2Int[] { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) };
            case 1:
                return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
            case 2:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) };
            case 3:
                return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
            case 4:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
            case 5:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0) };
            case 6:
                return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) };
            default:
                return new Vector2Int[] { new Vector2Int(0, 0) };
        }
    }

    bool iB(int lowerBound, int value, int higherBound) {//in Bounds
        return (lowerBound <= value && value < higherBound);
    }

    bool willCollide(Vector2Int[] tet, int tX, int tY) {
        //loop through tetromino matrix, if T && G, it is a collision
        for (int i = 0; i < tet.Length; i++) {
            var pos = tet[i];
            if (20 <= (tY+pos.y))
                    continue;
            if (!(iB(0, tX+pos.x, 10) && 0 <= (tY+pos.y)) || Gameboard[tX+pos.x,tY+pos.y])
                return true;
        }
        /*for (int x = 0; x < 4; x++) {
            for (int y = 0; y < 4; y++) {
                if (20 <= (tY+y))
                    continue;
                if (tet[x,y] && (!(iB(0, tX+x, 10) && 0 <= (tY+y)) || Gameboard[tX+x,tY+y] && tet[x,y]))
                    return true;
            }
        }*/
        return false;
    }
    
    void moveTet() {
        if (Input.GetKeyDown(KeyCode.A) && !willCollide(Tetromino, curTetX-1,curTetY)) {
            curTetX -= 1;
        } else if (Input.GetKeyDown(KeyCode.D) && !willCollide(Tetromino, curTetX+1,curTetY)) {
            curTetX += 1;
        } else if (Input.GetKeyDown(KeyCode.S) && !willCollide(Tetromino, curTetX,curTetY-1)) {
            curTetY -= 1;
        } else if (Input.GetKeyDown(KeyCode.Space)) {
            for (int y = curTetY; -4 <= y; y--) {
                if (willCollide(Tetromino, curTetX, y)) {
                    curTetY = y+1;
                    break;
                }
            }        
        }

    }

    void rotateTet() {
        /*  Rotate the tet around specified point.
            If collision occurs, offset by one block in each direction until resolved, else don't rotate.
        */

        int rot = (Input.GetKeyDown(KeyCode.Q) ? -1 : 0) + (Input.GetKeyDown(KeyCode.E) ? 1 : 0);
        Vector2Int[] transTet = new Vector2Int[Tetromino.Length];
        if (rot != 0) {
            for (int i = 0; i < Tetromino.Length; i++)
                    transTet[i] = new Vector2Int(rot * Tetromino[i].y, rot * -Tetromino[i].x);

            //Wallkicking
            Vector2Int[] offsets = { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(-1,0), new Vector2Int(0,-1), new Vector2Int(1,0) };
            for (int i = 0; i < offsets.Length; i++)
                if (!willCollide(transTet, curTetX + offsets[i].x, curTetY + offsets[i].y)) {
                    Tetromino = transTet;
                    curTetX += offsets[i].x;
                    curTetY += offsets[i].y;
                }  
        }
    }

    void tetToBoard() {
        for (int i = 0; i < Tetromino.Length; i++) {
            if (20 <= curTetY+Tetromino[i].y) {
                failGame();
                return;
            }
            
            Gameboard[curTetX+Tetromino[i].x,curTetY+Tetromino[i].y] = true;
        }
        spawnTet();


        /*for (int x = 0; x < 4; x++) {
            for (int y = 0; y < 4; y++) {
                if (20 <= curTetY+y) {
                    failGame();
                    return;
                }
                    
                if (Tetromino[x,y])
                    Gameboard[curTetX+x,curTetY+y] = true;
            }
        }
        spawnTet();*/
    }

    void lineClearCheck() {
        bool[] lines = new bool[20];
        //Find filled lines
        for (int y = 0; y < 20; y++) {
            bool clear = true;
            for (int x = 0; x < 10; x++) {
                if (!Gameboard[x,y]) {
                    clear = false;
                    break;
                }
            }
            if (clear)
                lines[y] = true;
        }

        //Move lines down
        for (int l = 19; l >= 0; l--) {
            if (lines[l]) {
                for (int y = l+1; y < 20; y++)
                    for (int x = 0; x < 10; x++)
                        Gameboard[x,y-1] = Gameboard[x,y];
            }
        }

        //Clear last line
        for (int x = 0; x < 10; x++)
            Gameboard[x, 19] = false;

        //Move colors down
        for (int l = 19; l >= 0; l--)
            if (lines[l])
                for (int y = l+1; y < 20; y++)
                    for (int x = 0; x < 10; x++)
                        RenderMatrix[x,y-1].color = RenderMatrix[x,y].color;
        for (int x = 0; x < 10; x++)
            RenderMatrix[x, 19].color = Color.black;

        //Count/score cleared lines
        int count = 0;
        for (int y = 0; y < 20; y++)
            if (lines[y])
                count++;
        
        switch(count) {
            case 0:
                return;
            case 1:
                score += 1;
                return;
            case 2:
                score += 3;
                return;
            case 3:
                score += 5;
                return;
            case 4:
                score += 8;
                return;
            default:
                score += 16;
                return;
        }
    }

    void failGame() {
        print("You Won! Final Score: " + score);
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    //----Rendering----//

    Color getColor(int ID) {
        switch (ID) {//I J L O S T Z
            case 0:
                return Color.cyan;
            case 1:
                return Color.blue;
            case 2:
                return new Color(1, 0.65f, 0);
            case 3:
                return Color.yellow;
            case 4:
                return Color.green;
            case 5:
                return new Color(1, 0, 1);
            case 6:
                return Color.red;
            default:
                return Color.black;
        }
    }

    void instantiateRenderers() {
        //Main board
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 20; y++) {
                var go = Instantiate(Pixel, transform);
                RenderMatrix[x,y] = go.GetComponent<SpriteRenderer>();
                go.transform.position = new Vector2(x+0.5f,y+0.5f);
                go.SetActive(false);
            }
        }

        //Hold
        Vector2 holdOffset = new Vector2(12, 16);
        for (int x = 0; x < 4; x++) {
            for (int y = 0; y < 4; y++) {
                var go = Instantiate(Pixel, transform);
                HoldMatrix[x,y] = go.GetComponent<SpriteRenderer>();
                go.transform.position = new Vector2(x+0.5f,y+0.5f) + holdOffset;
                go.SetActive(false);
            }
        }

        //Queue
        Vector2 queueOffset = new Vector2(12, 11);
        for (int i = 0; i < queue.Length; i++) {
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    var go = Instantiate(Pixel, transform);
                    QueueMatrices[i,x,y] = go.GetComponent<SpriteRenderer>();
                    go.transform.position = new Vector2(x+0.5f,y+0.5f) + queueOffset - i * new Vector2(0, 5);
                    go.SetActive(false);
                }
            }
        }
    }

    void renderBoard() {
        //Render board
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 20; y++)
                RenderMatrix[x,y].gameObject.SetActive(Gameboard[x,y]);
        
        //Render tet
        for (int i = 0; i < Tetromino.Length; i++)
            if ((curTetY+Tetromino[i].y) < 20)
                RenderMatrix[curTetX+Tetromino[i].x,curTetY+Tetromino[i].y].gameObject.SetActive(true);

        /*for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                if (Tetromino[x,y] && (curTetY+y) < 20)
                    RenderMatrix[curTetX+x,curTetY+y].gameObject.SetActive(true);*/

        //Render hold
        var holdTet = getTet(heldTet);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                HoldMatrix[x,y].gameObject.SetActive(false);
        for (int i = 0; i < holdTet.Length; i++)
            HoldMatrix[holdTet[i].x+1, holdTet[i].y+1].gameObject.SetActive(true);
        
        
        //Render queue
        for (int i = 0; i < queue.Length; i++) {
            var queueTet = getTet(queue[i]);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    QueueMatrices[i,x,y].gameObject.SetActive(false);
            for (int j = 0; j < queueTet.Length; j++)
                QueueMatrices[i, queueTet[j].x+1, queueTet[j].y+1].gameObject.SetActive(true);
        }




        /*for (int i = 0; i < queue.Length; i++) {
            var queueTet = getTet(queue[i]);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    QueueMatrices[i,x,y].gameObject.SetActive(queueTet[x,y]);
        }*/
    }
}