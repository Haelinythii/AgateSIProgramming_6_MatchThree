using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public int id;
    private BoardManager boardManager;
    private SpriteRenderer spriteRenderer;

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
}
