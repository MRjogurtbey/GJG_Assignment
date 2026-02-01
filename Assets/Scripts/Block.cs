using UnityEngine;
using System;

public class Block : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public ColorData Data { get; private set; }
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private GridManager _gridManager;

    public static event Action<Block> OnBlockClicked;
    
    // SetBlock yerine Init (Başlatma) fonksiyonu yaptık, yöneticiyi de alıyor
    public void Init(int x, int y, ColorData data, GridManager manager)
    {
        X = x;
        Y = y;
        Data = data;
        _gridManager = manager; // Referansı kaydet
        
        spriteRenderer.sprite = data.DefaultIcon;
    }
    
    public void UpdateVisual(Sprite newIcon)
    {
        spriteRenderer.sprite = newIcon;
    }

    private void OnMouseDown()
    {
        OnBlockClicked?.Invoke(this);
    }
}