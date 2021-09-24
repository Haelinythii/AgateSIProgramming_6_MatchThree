using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private static readonly Color selectedColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color normalColor = Color.white;
    private static readonly float moveDuration = 0.5f;
    private static readonly Vector2[] adjacentDirection = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private static readonly float destroyBigDuration = .1f;
    private static readonly float destroySmallDuration = .4f;
    private static readonly Vector2 sizeBig = Vector2.one * 1.2f;
    private static readonly Vector2 sizeSmall = Vector2.zero;
    private static readonly Vector2 sizeNormal = Vector2.one;

    public int id;
    private BoardManager boardManager;
    private GameFlowManager gameFlowManager;
    private SpriteRenderer spriteRenderer;

    private static TileController previousSelected = null;
    private bool isSelected = false;
    public bool IsDestroyed { get; private set; }

    private void Awake()
    {
        boardManager = BoardManager.Instance;
        gameFlowManager = GameFlowManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        IsDestroyed = false;
    }

    public void ChangeID(int _id, int x, int y)
    {
        spriteRenderer.sprite = boardManager.tileTypes[_id];
        id = _id;

        name = "TILE_" + id + " (" + x + ", " + y + ")";
    }

    private void OnMouseDown()
    {
        if (spriteRenderer.sprite == null || boardManager.IsAnimating || gameFlowManager.IsGameOver) return;

        if (isSelected)
        {
            Deselect();
        }
        else
        {
            if(previousSelected == null)
            {
                Select();
            }
            else
            {
                if (GetAllAdjacentTiles().Contains(previousSelected))
                {
                    TileController otherTile = previousSelected;
                    previousSelected.Deselect();
                    
                    SwapTile(otherTile, () => {
                        if(boardManager.GetAllMatches().Count > 0)
                        {
                            Debug.Log("Match");
                            boardManager.Process();
                        }
                        else
                        {
                            SwapTile(otherTile);
                        }
                    });
                }
                else
                {
                    previousSelected.Deselect();
                    Select();
                }
            }
        }
    }

    public void SwapTile(TileController otherTile, System.Action onCompleted = null)
    {
        StartCoroutine(boardManager.SwapTilePosition(this, otherTile, onCompleted));
    }

    public IEnumerator MoveTilePosition(Vector2 targetPos, System.Action OnCompleted)
    {
        Vector2 startPos = transform.position;
        float time = 0f;

        yield return new WaitForEndOfFrame();

        while(time < moveDuration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, time / moveDuration);
            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPos;
        OnCompleted?.Invoke();
    }

    public IEnumerator SetDestroyed(System.Action OnCompleted)
    {
        id = -1;
        name = "TILE_NULL";
        IsDestroyed = true;

        Vector2 startSize = transform.localScale;
        float timer = 0f;

        while(timer < destroyBigDuration)
        {
            transform.localScale = Vector2.Lerp(startSize, sizeBig, timer / destroyBigDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = sizeBig;

        startSize = transform.localScale;
        timer = 0f;
        while (timer < destroySmallDuration)
        {
            transform.localScale = Vector2.Lerp(startSize, sizeSmall, timer / destroySmallDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = sizeSmall;

        spriteRenderer.sprite = null;
        OnCompleted?.Invoke();
    }

    private void Select()
    {
        isSelected = true;
        spriteRenderer.color = selectedColor;
        previousSelected = this;
    }

    private void Deselect()
    {
        isSelected = false;
        spriteRenderer.color = normalColor;
        previousSelected = null;
    }

    public List<TileController> GetAllAdjacentTiles()
    {
        List<TileController> adjacentTiles = new List<TileController>();

        for (int i = 0; i < adjacentDirection.Length; i++)
        {
            adjacentTiles.Add(GetAdjacentTile(adjacentDirection[i]));
        }
        return adjacentTiles;
    }

    private TileController GetAdjacentTile(Vector2 raycastDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection, spriteRenderer.size.x);
        return hit ? hit.collider.GetComponent<TileController>() : null;
    }

    #region Check Match

    public List<TileController> GetAllMatches()
    {
        if (IsDestroyed) return null;

        List<TileController> matchingTiles = new List<TileController>();

        List<TileController> horizontalMatchingTiles = GetOneLineMatch(new Vector2[2] { Vector2.right, Vector2.left });
        List<TileController> verticalMatchingTiles = GetOneLineMatch(new Vector2[2] { Vector2.up, Vector2.down });

        if(horizontalMatchingTiles != null)
        {
            matchingTiles.AddRange(horizontalMatchingTiles);
        }
        if(verticalMatchingTiles != null)
        {
            matchingTiles.AddRange(verticalMatchingTiles);
        }

        //add diri sendiri jika ada match dengan vertical dan horizontal
        if(matchingTiles != null && matchingTiles.Count >= 2)
        {
            matchingTiles.Add(this);
        }
        return matchingTiles;
    }

    private List<TileController> GetOneLineMatch(Vector2[] raycastDirections)
    {
        List<TileController> matchingTiles = new List<TileController>();

        for (int i = 0; i < raycastDirections.Length; i++)
        {
            matchingTiles.AddRange(GetMatch(raycastDirections[i]));
        }

        if(matchingTiles.Count >= 2)
        {
            return matchingTiles;
        }
        return null;
    }

    private List<TileController> GetMatch(Vector2 raycastDirection)
    {
        List<TileController> matchingTiles = new List<TileController>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection, spriteRenderer.size.x);

        while (hit)
        {
            TileController hitTile = hit.collider.GetComponent<TileController>();
            if(hitTile.id != id || hitTile.IsDestroyed)
            {
                break;
            }

            matchingTiles.Add(hitTile);
            hit = Physics2D.Raycast(hitTile.transform.position, raycastDirection, spriteRenderer.size.x);
        }

        return matchingTiles;
    }

    #endregion

    public void GenerateRandomTile(int x, int y)
    {
        transform.localScale = sizeNormal;
        IsDestroyed = false;

        ChangeID(Random.Range(0, boardManager.tileTypes.Count), x, y);
    }
}
