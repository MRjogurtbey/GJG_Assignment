using UnityEngine;
using System;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int X { get; private set; }
    public int Y { get; private set; }
    public ColorData Data { get; private set; }
    
    public static event Action<Block> OnBlockClicked;
    
    // SetBlock yerine Init (Başlatma) fonksiyonu yaptık, yöneticiyi de alıyor
    public void Init(int x, int y, ColorData data)
    {
        X = x;
        Y = y;
        Data = data;
        
        if (spriteRenderer != null && data != null)
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
    /// <summary>
    /// Bloğun ikonunu grup büyüklüğüne göre günceller.
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = icon;
    }
}