using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private static readonly Color selectedColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color normalColor = Color.white;
    private static readonly float moveDuration = 0.5f;
    private static readonly Vector2[] adjacentDirection = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    public int id;
    private BoardManager boardManager;
    private SpriteRenderer spriteRenderer;

    private static TileController previousSelected = null;
    private bool isSelected = false;

    private void Awake()
    {
        boardManager = BoardManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ChangeID(int _id, int x, int y)
    {
        spriteRenderer.sprite = boardManager.tileTypes[_id];
        id = _id;

        name = "TILE_" + id + " (" + x + ", " + y + ")";
    }

    private void OnMouseDown()
    {
        if (spriteRenderer.sprite == null || boardManager.IsAnimating) return;

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

                    // swap tile 2 kali
                    SwapTile(otherTile, () => {
                        SwapTile(otherTile);
                    });
                }
                else
                {
                    previousSelected.Deselect();
                    Select();
                }

                // run if cant swap (disabled for now)
                //previousSelected.Deselect();
                //Select();
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
}
