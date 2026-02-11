using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Izgaradaki boşlukları doldurma, yerçekimi ve yeni blok üretme işlemlerini yönetir.
/// </summary>
public class BoardRefiller : MonoBehaviour
{
    [SerializeField] private BlockPooler pooler;
    [SerializeField] private float fallDuration = 0.4f;
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    /// <summary>
    /// Boşlukları tespit eder, üsttekileri kaydırır ve yeni bloklar üretir.
    /// </summary>
    // Geri dönüş tipini List<int> yaparak hangi sütunların değiştiğini bildirelim.
    public void FillHoles(Block[,] grid, int width, int height, List<ColorData> activeColors, System.Action<List<int>> onComplete)
    {
        List<int> dirtyColumns = new List<int>();

        for (int x = 0; x < width; x++)
        {
            bool columnChanged = false;
            int writeY = 0;

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) 
                {
                    columnChanged = true;
                    continue;
                }

                if (y != writeY)
                {
                    columnChanged = true;
                    Block block = grid[x, y];
                    grid[x, writeY] = block;
                    grid[x, y] = null;
                    block.Init(x, writeY, block.Data);
                    block.transform.DOMove(new Vector3(x, writeY, 0), fallDuration).SetEase(fallEase);
                }
                writeY++;
            }

            // Yeni blok ekleniyorsa da sütun kirlenmiştir.
            if (writeY < height) columnChanged = true;

            for (int y = writeY; y < height; y++)
            {
                SpawnBlockAt(x, y, height + (y - writeY), grid, activeColors);
                grid[x, y].transform.DOMove(new Vector3(x, y, 0), fallDuration).SetEase(fallEase);
            }

            if (columnChanged) dirtyColumns.Add(x);
        }

        DOVirtual.DelayedCall(fallDuration, () => onComplete?.Invoke(dirtyColumns));
    }

    private void SpawnBlockAt(int x, int y, int startY, Block[,] grid, List<ColorData> activeColors)
    {
        int randomColorIndex = Random.Range(0, activeColors.Count);
        Block blockScript = pooler.GetBlock();

        blockScript.transform.position = new Vector3(x, startY, 0);
        blockScript.transform.localScale = Vector3.one;
        blockScript.Init(x, y, activeColors[randomColorIndex]);
        grid[x, y] = blockScript;
    }
}