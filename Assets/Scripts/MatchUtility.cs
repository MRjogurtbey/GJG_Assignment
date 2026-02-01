using UnityEngine;

// "static" yaparak bu sınıfın bir nesne olarak yaratılmasını engelliyoruz.
// Sadece kütüphane gibi fonksiyonlarını kullanacağız.
public static class MatchUtility
{
    /// <summary>
    /// Board üzerinde patlatılabilir (yan yana en az 2 aynı renk) grup var mı kontrol eder.
    /// Senior Notu: Bu algoritma O(M*N) karmaşıklığındadır, gayet hızlıdır.
    /// </summary>
    public static bool HasAnyMoves(Block[,] grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block current = grid[x, y];
                
                // Eğer blok boşsa veya havuza gönderildiyse atla
                if (current == null || !current.gameObject.activeSelf) continue;

                // Sadece Sağa ve Yukarı baksak yeterli (Geriye bakmaya gerek yok)
                // Böylece işlem sayısını yarıya indiririz.
                
                // 1. Sağ Kontrol
                if (x < width - 1)
                {
                    Block right = grid[x + 1, y];
                    if (right != null && right.gameObject.activeSelf && right.Data == current.Data)
                        return true; // Hamle bulundu, döngüden çık
                }

                // 2. Yukarı Kontrol
                if (y < height - 1)
                {
                    Block up = grid[x, y + 1];
                    if (up != null && up.gameObject.activeSelf && up.Data == current.Data)
                        return true; // Hamle bulundu, döngüden çık
                }
            }
        }

        return false; // Hiçbir eşleşme yok, Deadlock!
    }
}