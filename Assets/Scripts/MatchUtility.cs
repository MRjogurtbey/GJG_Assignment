using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Oyunun eşleşme ve kilitlenme mantığını yöneten yardımcı sınıf.
/// </summary>
public static class MatchUtility
{
    /// <summary>
    /// Gridi tarayarak yan yana aynı renkte en az bir ikili olup olmadığını kontrol eder.
    /// </summary>
    /// <returns>Hamle varsa true, yoksa false döner.</returns>
    public static bool HasAnyMoves(Block[,] grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block current = grid[x, y];
                if (current == null || !current.gameObject.activeSelf) continue;

                // Sağ komşu kontrolü
                if (x < width - 1)
                {
                    Block right = grid[x + 1, y];
                    if (right != null && right.gameObject.activeSelf && right.Data == current.Data)
                        return true;
                }

                // Üst komşu kontrolü
                if (y < height - 1)
                {
                    Block up = grid[x, y + 1];
                    if (up != null && up.gameObject.activeSelf && up.Data == current.Data)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Tıklanan bloktan başlayarak aynı renkteki tüm komşuları bulur (BFS algoritması).
    /// </summary>
    public static List<Block> GetGroup(Block startBlock, Block[,] grid, int width, int height)
    {
        List<Block> group = new List<Block>();
        if (startBlock == null) return group;

        Queue<Block> checkQueue = new Queue<Block>();
        checkQueue.Enqueue(startBlock);
        group.Add(startBlock);

        ColorData targetColor = startBlock.Data;

        while (checkQueue.Count > 0)
        {
            Block current = checkQueue.Dequeue();
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = current.X + dx[i];
                int ny = current.Y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    Block nb = grid[nx, ny];
                    if (nb != null && nb.gameObject.activeSelf && nb.Data == targetColor && !group.Contains(nb))
                    {
                        group.Add(nb);
                        checkQueue.Enqueue(nb);
                    }
                }
            }
        }
        return group;
    }
}