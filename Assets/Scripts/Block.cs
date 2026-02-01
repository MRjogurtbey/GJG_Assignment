using UnityEngine;
using System;
/// <summary>
/// Izgaradaki her bir hücreyi temsil eden ve tıklama etkileşimini event tabanlı yöneten sınıftır.
/// </summary>
public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int X { get; private set; }
    public int Y { get; private set; }
    public ColorData Data { get; private set; }
    
    
    /// <summary>
    /// Herhangi bir blok tıklandığında fırlatılan statik olay. 
    /// Loose Coupling (Gevşek Bağlılık) prensibi için kullanılmıştır.
    /// </summary>
    public static event Action<Block> OnBlockClicked;
    
    /// <summary>
    /// Bloğu koordinatları ve rengi ile yapılandırır. 
    /// Object pooling sonrası verileri sıfırlamak/güncellemek için kullanılır.
    /// </summary>
    /// <param name="x">Izgaradaki yeni sütun indeksi.</param>
    /// <param name="y">Izgaradaki yeni satır indeksi.</param>
    /// <param name="data">Atanacak renk verisi.</param>
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
    /// Bloğun görselini (ikonunu) günceller. Grup büyüklüğüne göre ikon değişimi için kullanılır.
    /// </summary>
    /// <param name="newIcon">Atanacak yeni Sprite.</param>
    public void SetIcon(Sprite icon)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = icon;
    }
}