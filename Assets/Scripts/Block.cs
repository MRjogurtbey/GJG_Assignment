using UnityEngine;
using System;
using DG.Tweening;
/// <summary>
/// Izgaradaki her bir hücreyi temsil eder. 
/// Veriyi tutar ve görsel geri bildirimleri (animasyon, ikon) yönetir.
/// </summary>
public class Block : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Kapsülleme (Encapsulation): Dışarıdan okunabilir ama sadece içeriden değiştirilebilir.
    public int X { get; private set; }
    public int Y { get; private set; }
    public ColorData Data { get; private set; }

    // Event: GridManager bu event'i dinleyerek hangi bloğa tıklandığını anlar.
    public static event Action<Block> OnBlockClicked;

    public void Init(int x, int y, ColorData data)
    {
        X = x;
        Y = y;
        Data = data;

        if (spriteRenderer != null && data != null)
            spriteRenderer.sprite = data.DefaultIcon;
            
        // Başlangıçta minik bir "belirme" animasyonu profesyonel durur.
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Bloğun görselini (ikonunu) günceller.
    /// </summary>
    public void UpdateVisual(Sprite newIcon)
    {
        if (spriteRenderer != null && spriteRenderer.sprite != newIcon)
        {
            // İkon değişirken hafif bir yanıp sönme efekti
            spriteRenderer.DOFade(0.5f, 0.1f).OnComplete(() => {
                spriteRenderer.sprite = newIcon;
                spriteRenderer.DOFade(1f, 0.1f);
            });
        }
    }

    /// <summary>
    /// Blok patladığında oynayacak animasyon.
    /// </summary>
    public void PlayBlastAnimation(float duration, Action onComplete)
    {
        transform.DOScale(Vector3.zero, duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void OnMouseDown()
    {
        // Tıklama bilgisini dış dünyaya (GridManager'a) yayar.
        OnBlockClicked?.Invoke(this);
    }
}
