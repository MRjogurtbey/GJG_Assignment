using UnityEngine;

public class Block : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public ColorData Data { get; private set; }
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // CACHE: Yöneticiyi burada saklayacağız
    private GridManager _gridManager;

    // SetBlock yerine Init (Başlatma) fonksiyonu yaptık, yöneticiyi de alıyor
    public void Init(int x, int y, ColorData data, GridManager manager)
    {
        X = x;
        Y = y;
        Data = data;
        _gridManager = manager; // Referansı kaydet
        
        spriteRenderer.sprite = data.defaultIcon;
    }
    
    public void UpdateVisual(Sprite newIcon)
    {
        spriteRenderer.sprite = newIcon;
    }

    private void OnMouseDown()
    {
        // ARTIK ARAMA YOK! Doğrudan hafızadaki referansı kullanıyoruz.
        if (_gridManager != null)
        {
            _gridManager.OnBlockClicked(this);
        }
    }
}