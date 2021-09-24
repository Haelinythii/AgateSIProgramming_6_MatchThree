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
            return IsSwapping || IsProcessing;
        }
    }

    public bool IsSwapping { get; set; }
    public bool IsProcessing { get; set; }

    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);

        IsProcessing = false;
        IsSwapping = false;
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

        if (x > 1 && tiles[x - 1, y].id == tiles[x - 2, y].id)
        {
            possibleIDs.Remove(tiles[x - 1, y].id);
        }

        if (y > 1 && tiles[x, y - 1].id == tiles[x, y - 2].id)
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

    #region match

    public List<TileController> GetAllMatches()
    {
        List<TileController> matchingTiles = new List<TileController>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                List<TileController> matchingTilesFromOneTile = tiles[x, y].GetAllMatches();

                //kalau tidak ada matching dari tile tersebut, lewati
                if (matchingTilesFromOneTile == null || matchingTilesFromOneTile.Count == 0)
                {
                    continue;
                }

                foreach (TileController tile in matchingTilesFromOneTile)
                {
                    if (!matchingTiles.Contains(tile))
                    {
                        matchingTiles.Add(tile);
                    }
                }
            }
        }

        return matchingTiles;
    }

    public void Process()
    {
        IsProcessing = true;
        ProcessMatches();
    }

    private void ProcessMatches()
    {
        List<TileController> allMatchingTiles = GetAllMatches();

        if (allMatchingTiles == null || allMatchingTiles.Count == 0)
        {
            IsProcessing = false;
            return;
        }
        StartCoroutine(ClearMatches(allMatchingTiles, ProcessDrop));
    }

    private IEnumerator ClearMatches(List<TileController> matchingTiles, System.Action OnCompleted)
    {
        List<bool> isAllClearingCompleted = new List<bool>();

        for (int i = 0; i < matchingTiles.Count; i++)
        {
            isAllClearingCompleted.Add(false);
        }

        for (int i = 0; i < matchingTiles.Count; i++)
        {
            int index = i;
            StartCoroutine(matchingTiles[i].SetDestroyed(() => { isAllClearingCompleted[index] = true; }));
        }
        yield return new WaitUntil(() => { return IsProcessCompleted(isAllClearingCompleted); });

        OnCompleted?.Invoke();
    }

    public bool IsProcessCompleted(List<bool> b)
    {
        foreach (bool item in b)
        {
            if (item == false) return false;
        }
        return true;
    }

    #endregion

    #region drop
    private void ProcessDrop()
    {
        Dictionary<TileController, int> droppingTiles = GetAllDroppingTiles();
        StartCoroutine(DropTiles(droppingTiles, ProcessDestroyAndFill));
    }

    private IEnumerator DropTiles(Dictionary<TileController, int> droppingTiles, System.Action OnCompleted)
    {
        foreach (KeyValuePair<TileController, int> droppingTileInfo in droppingTiles)
        {
            Vector2Int tileIndex = GetTileIndex(droppingTileInfo.Key);

            TileController tile = droppingTileInfo.Key;
            tiles[tileIndex.x, tileIndex.y] = tiles[tileIndex.x, tileIndex.y - droppingTileInfo.Value];
            tiles[tileIndex.x, tileIndex.y - droppingTileInfo.Value] = tile;

            tile.ChangeID(tile.id, tileIndex.x, tileIndex.y - droppingTileInfo.Value);
        }
        yield return null;

        OnCompleted?.Invoke();
    }

    private Dictionary<TileController, int> GetAllDroppingTiles()
    {
        Dictionary<TileController, int> droppingTiles = new Dictionary<TileController, int>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                if (tiles[x, y].IsDestroyed)
                {
                    for (int i = y + 1; i < boardSize.y; i++)
                    {
                        if (tiles[x, i].IsDestroyed) continue;

                        if (droppingTiles.ContainsKey(tiles[x, i])) // kalau tile yang ada diatasnya udah pernah ke add sebelumnya
                        {
                            droppingTiles[tiles[x, i]]++; //naikin drop rangenya
                        }
                        else //kalau belum ada dalam droppingTiles
                        {
                            droppingTiles.Add(tiles[x, i], 1); //add dengan drop rangenya 1
                        }
                    }
                }
            }
        }
        return droppingTiles;
    }
    #endregion

    #region destroy and fill

    private void ProcessDestroyAndFill()
    {
        List<TileController> destroyedTiles = GetAllDestroyedTiles();
        StartCoroutine(DestroyAndFillTiles(destroyedTiles, ProcessReposition));
    }

    private List<TileController> GetAllDestroyedTiles()
    {
        List<TileController> destroyedTiles = new List<TileController>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                if (tiles[x, y].IsDestroyed)
                {
                    destroyedTiles.Add(tiles[x, y]);
                }
            }
        }
        return destroyedTiles;
    }

    private IEnumerator DestroyAndFillTiles(List<TileController> destroyedTiles, System.Action OnCompleted)
    {
        List<int> highestIndex = new List<int>();

        for (int i = 0; i < boardSize.x; i++)
        {
            highestIndex.Add(boardSize.y - 1);
        }

        float spawnHeight = endPos.y + tilePrefab.GetComponent<SpriteRenderer>().size.y + offsetTile.y;

        foreach (TileController tile in destroyedTiles)
        {
            Vector2Int tileIndex = GetTileIndex(tile);
            Vector2Int targetIndex = new Vector2Int(tileIndex.x, highestIndex[tileIndex.x]);
            highestIndex[tileIndex.x]--;

            tile.transform.position = new Vector2(tile.transform.position.x, spawnHeight);
            tile.GenerateRandomTile(targetIndex.x, targetIndex.y);
        }
        yield return null;
        OnCompleted?.Invoke();
    }

    #endregion

    #region reposition

    private void ProcessReposition()
    {
        StartCoroutine(RepositionTiles(ProcessMatches));
    }

    private IEnumerator RepositionTiles(System.Action OnCompleted)
    {
        List<bool> isAllRepositionCompleted = new List<bool>();

        int i = 0;
        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                Vector2 targetPosition = GetPositionFromIndex(new Vector2Int(x, y));

                if((Vector2)tiles[x, y].transform.position == targetPosition)//kalau udah ditempat lanjut ke selanjutnya
                {
                    continue;
                }

                isAllRepositionCompleted.Add(false);

                int index = i;
                StartCoroutine(tiles[x, y].MoveTilePosition(targetPosition, () => { isAllRepositionCompleted[index] = true; }));

                i++;
            }
        }
        yield return new WaitUntil(() => { return IsProcessCompleted(isAllRepositionCompleted); });

        OnCompleted?.Invoke();
    }

    #endregion
}
