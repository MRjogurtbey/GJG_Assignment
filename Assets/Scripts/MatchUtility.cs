using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyunun eşleşme mantığını yöneten, bellek optimizasyonu yapılmış yardımcı sınıf.
/// Statik koleksiyonlar kullanarak Garbage Collector yükünü minimize eder.
/// </summary>
public static class MatchUtility
{
    // Bellek Optimizasyonu: Koleksiyonları statik yaparak her çağrıda 'new' yükünden kurtuluyoruz.
    private static readonly List<Block> _reusableGroup = new List<Block>();
    private static readonly HashSet<Block> _visited = new HashSet<Block>();
    private static readonly Queue<Block> _checkQueue = new Queue<Block>();

    /// <summary>
    /// Tıklanan bloktan başlayarak aynı renkteki grubu bulur.
    /// Reusable collections kullanarak GC alloc oluşturmaz.
    /// </summary>
    public static List<Block> GetGroup(Block startBlock, Block[,] grid, int width, int height)
    {
        // 1. Önceki aramadan kalan verileri temizle
        _reusableGroup.Clear();
        _visited.Clear();
        _checkQueue.Clear();

        if (startBlock == null) return new List<Block>();

        _checkQueue.Enqueue(startBlock);
        _visited.Add(startBlock);

        ColorData targetColor = startBlock.Data;

        while (_checkQueue.Count > 0)
        {
            Block current = _checkQueue.Dequeue();
            _reusableGroup.Add(current);

            int x = current.X;
            int y = current.Y;

            CheckNeighbor(x - 1, y, grid, width, height, targetColor);
            CheckNeighbor(x + 1, y, grid, width, height, targetColor);
            CheckNeighbor(x, y - 1, grid, width, height, targetColor);
            CheckNeighbor(x, y + 1, grid, width, height, targetColor);
        }

        return new List<Block>(_reusableGroup);
    }

    /// <summary>
    /// Belirli bir koordinattaki bloğun eşleşme durumunu kontrol eder ve kuyruğa ekler.
    /// </summary>
    private static void CheckNeighbor(int nx, int ny, Block[,] grid, int width, int height, ColorData targetColor)
    {
        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
        {
            Block nb = grid[nx, ny];
            if (nb != null && nb.gameObject.activeSelf && nb.Data == targetColor && !_visited.Contains(nb))
            {
                _visited.Add(nb);
                _checkQueue.Enqueue(nb);
            }
        }
    }

    public static bool HasAnyMoves(Block[,] grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block current = grid[x, y];
                if (current == null || !current.gameObject.activeSelf) continue;

                if (x < width - 1 && IsMatch(current, grid[x + 1, y])) return true;
                if (y < height - 1 && IsMatch(current, grid[x, y + 1])) return true;
            }
        }
        return false;
    }

    private static bool IsMatch(Block a, Block b)
    {
        return b != null && b.gameObject.activeSelf && a.Data == b.Data;
    }
}