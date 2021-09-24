using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    #region Singleton
    private static BoardManager instance = null;
    public static BoardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BoardManager>();
                if (instance == null)
                {
                    Debug.LogError("Fatal Error: BoardManager not Found");
                }
            }
            return instance;
        }
    }
    #endregion

    [Header("Board Properties")]
    public Vector2Int boardSize;
    public Vector2 offsetTile;
    public Vector2 offsetBoard;

    [Header("Tile References")]
    public List<Sprite> tileTypes = new List<Sprite>();
    public GameObject tilePrefab;

    private Vector2 startPos, endPos;
    private TileController[,] tiles;


    public bool IsAnimating
    {
        get
        {
            return IsSwapping;
        }
    }

    public bool IsSwapping { get; set; }

    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);
    }

    private void CreateBoard(Vector2 tileSize)
    {
        tiles = new TileController[boardSize.x, boardSize.y];

        Vector2 totalBoardSize = (tileSize + offsetTile) * (boardSize - Vector2.one);

        startPos = (Vector2)transform.position - (totalBoardSize / 2) + offsetBoard;
        endPos = startPos + totalBoardSize;

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                TileController newTile = Instantiate(tilePrefab, new Vector2(startPos.x + ((tileSize.x + offsetTile.x) * x), startPos.y + ((tileSize.y + offsetTile.y) * y)), tilePrefab.transform.rotation, transform).GetComponent<TileController>();
                tiles[x, y] = newTile;

                List<int> possibleIDs = GetPossibleIDs(x, y);
                int tileID = possibleIDs[Random.Range(0, possibleIDs.Count)];
                newTile.ChangeID(tileID, x, y);

            }
        }
    }

    private List<int> GetPossibleIDs(int x, int y)
    {
        List<int> possibleIDs = new List<int>();

        for (int i = 0; i < tileTypes.Count; i++)
        {
            possibleIDs.Add(i);
        }

        if(x > 1 && tiles[x - 1, y].id == tiles[x - 2, y].id)
        {
            possibleIDs.Remove(tiles[x - 1, y].id);
        }

        if(y > 1 && tiles[x, y - 1].id == tiles[x, y - 2].id)
        {
            possibleIDs.Remove(tiles[x, y - 1].id);
        }

        return possibleIDs;
    }

    #region swap tile

    public IEnumerator SwapTilePosition(TileController tileOne, TileController tileTwo, System.Action OnCompleted)
    {
        IsSwapping = true;

        Vector2Int indexTileOne = GetTileIndex(tileOne);
        Vector2Int indexTileTwo = GetTileIndex(tileTwo);

        tiles[indexTileOne.x, indexTileOne.y] = tileTwo;
        tiles[indexTileTwo.x, indexTileTwo.y] = tileOne;

        tileOne.ChangeID(tileOne.id, indexTileTwo.x, indexTileTwo.y);
        tileTwo.ChangeID(tileTwo.id, indexTileOne.x, indexTileOne.y);

        bool isRoutineTileOneCompleted = false;
        bool isRoutineTileTwoCompleted = false;

        StartCoroutine(tileOne.MoveTilePosition(GetPositionFromIndex(indexTileTwo), () => { isRoutineTileOneCompleted = true; }));
        StartCoroutine(tileTwo.MoveTilePosition(GetPositionFromIndex(indexTileOne), () => { isRoutineTileTwoCompleted = true; }));

        yield return new WaitUntil(() => { return isRoutineTileOneCompleted && isRoutineTileTwoCompleted; });

        OnCompleted?.Invoke();

        IsSwapping = false;
    }

    #endregion

    private Vector2Int GetTileIndex(TileController tile)
    {
        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                if (tile == tiles[x, y]) return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public Vector2 GetPositionFromIndex(Vector2Int index)
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        return new Vector2(startPos.x + ((tileSize.x + offsetTile.x) * index.x), startPos.y + ((tileSize.y + offsetTile.y) * index.y));
    }
}
