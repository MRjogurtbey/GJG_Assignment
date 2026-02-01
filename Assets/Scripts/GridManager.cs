using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public LevelConfig config;
    public GameObject blockPrefab;
    private Block[,] _grid;

    // POOLING: Boştaki blokları burada saklayacağız
    private Queue<Block> _pool = new Queue<Block>();

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        _grid = new Block[config.M, config.N]; 
        
        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                SpawnBlockAt(x, y, y); 
            }
        }
        UpdateAllVisuals();
    }

    // Belirtilen koordinatta havuzdan veya yeniden blok yaratır
    private void SpawnBlockAt(int x, int y, int startY)
    {
        int randomColorIndex = Random.Range(0, config.colors.Length);
        ColorData selectedData = config.colors[randomColorIndex];

        // 1. Havuzdan Blok İste
        Block blockScript = GetBlockFromPool();

        // 2. Pozisyonunu ayarla
        blockScript.transform.position = new Vector3(x, startY, 0);
        
        // 3. Verilerini güncelle (Init kullanıyoruz, SetBlock değil!)
        // 'this' diyerek GridManager referansını gönderiyoruz.
        blockScript.Init(x, y, selectedData, this);
        
        // 4. Grid'e kaydet
        _grid[x, y] = blockScript;
    }

    // POOLING: Havuzdan blok çekme mantığı
    private Block GetBlockFromPool()
    {
        if (_pool.Count > 0)
        {
            Block block = _pool.Dequeue();
            block.gameObject.SetActive(true); // Tekrar görünür yap
            return block;
        }
        else
        {
            // Havuz boşsa yeni yarat
            GameObject newObj = Instantiate(blockPrefab, transform);
            return newObj.GetComponent<Block>();
        }
    }

    // POOLING: Bloğu havuza geri gönderme mantığı
    private void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false); // Görünmez yap
        _pool.Enqueue(block); // Havuza ekle
    }

    public void OnBlockClicked(Block clickedBlock)
    {
        List<Block> group = GetGroup(clickedBlock);

        if (group.Count >= 2) 
        {
            BlastGroup(group);
        }
    }

    private void BlastGroup(List<Block> group)
    {
        foreach (Block block in group)
        {
            _grid[block.X, block.Y] = null; 
            ReturnBlockToPool(block);
        }
        
        FillHoles(); 
    }

    private void FillHoles()
    {
        for (int x = 0; x < config.M; x++)
        {
            int writeY = 0;
            
            for (int y = 0; y < config.N; y++)
            {
                if (_grid[x, y] != null)
                {
                    if (y != writeY)
                    {
                        Block block = _grid[x, y];
                        _grid[x, y] = null;
                        _grid[x, writeY] = block;
                        
                        // BURASI DEĞİŞTİ: SetBlock -> Init
                        block.Init(x, writeY, block.Data, this);
                        
                        block.transform.position = new Vector3(x, writeY, 0);
                    }
                    writeY++;
                }
            }

            for (int y = writeY; y < config.N; y++)
            {
                SpawnBlockAt(x, y, config.N + (y - writeY)); 
                _grid[x, y].transform.position = new Vector3(x, y, 0);
            }
        }

        UpdateAllVisuals();
        CheckForDeadlock();
    }

      

    // --- GRUP BULMA VE GÖRSEL GÜNCELLEME (DEĞİŞMEDİ) ---

    private List<Block> GetGroup(Block startBlock)
    {
        List<Block> group = new List<Block>();
        HashSet<Block> visited = new HashSet<Block>();
        Stack<Block> candidates = new Stack<Block>();

        candidates.Push(startBlock);
        visited.Add(startBlock);

        while (candidates.Count > 0)
        {
            Block current = candidates.Pop();
            group.Add(current);

            foreach (Block neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && neighbor.Data == startBlock.Data)
                {
                    visited.Add(neighbor);
                    candidates.Push(neighbor);
                }
            }
        }
        return group;
    }

    private List<Block> GetNeighbors(Block block)
    {
        List<Block> neighbors = new List<Block>();
        int x = block.X;
        int y = block.Y;

        if (x > 0) neighbors.Add(_grid[x - 1, y]);
        if (x < config.M - 1) neighbors.Add(_grid[x + 1, y]);
        if (y > 0) neighbors.Add(_grid[x, y - 1]);
        if (y < config.N - 1) neighbors.Add(_grid[x, y + 1]);

        return neighbors;
    }

    public void UpdateAllVisuals()
    {
        HashSet<Block> visited = new HashSet<Block>();

        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                Block currentBlock = _grid[x, y];
                if (currentBlock == null || visited.Contains(currentBlock)) continue;

                List<Block> group = GetGroup(currentBlock);
                foreach (var b in group) visited.Add(b);

                Sprite iconToSet = GetSpriteForGroupSize(currentBlock.Data, group.Count);
                foreach (var b in group) b.UpdateVisual(iconToSet);
            }
        }
    }

    private Sprite GetSpriteForGroupSize(ColorData data, int groupSize)
    {
        if (groupSize >= config.C) return data.iconC;
        if (groupSize >= config.B) return data.iconB;
        if (groupSize >= config.A) return data.iconA;
        return data.defaultIcon;
    }

    // Hamle kontrolünü FillHoles işlemi bittikten sonra yapmalıyız.
    private void CheckForDeadlock()
    {
        // Az önce yazdığımız Utility sınıfını kullanıyoruz
        if (!MatchUtility.HasAnyMoves(_grid, config.M, config.N))
        {
            Debug.Log("Deadlock Detected! Shuffling...");
            ShuffleBoard();
        }
    }

    private void ShuffleBoard()
    {
        // 1. Mevcut tüm blokların renk verilerini topla
        List<ColorData> allColors = new List<ColorData>();
    
        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                if (_grid[x, y] != null)
                {
                    allColors.Add(_grid[x, y].Data);
                }
            }
        }

        // 2. Listeyi Karıştır (Fisher-Yates Shuffle)
        for (int i = 0; i < allColors.Count; i++)
        {
            ColorData temp = allColors[i];
            int randomIndex = Random.Range(i, allColors.Count);
            allColors[i] = allColors[randomIndex];
            allColors[randomIndex] = temp;
        }

        // 3. Grid'i Yeniden Doldur
        int listIndex = 0;
        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                Block block = _grid[x, y];
            
                // DÜZELTME BURADA: SetBlock yerine Init yazıyoruz ve 'this' ekliyoruz
                block.Init(x, y, allColors[listIndex], this);
            
                listIndex++;
            }
        }
    
        // 4. Deadlock Kontrolü ve Acil Müdahale
        if (!MatchUtility.HasAnyMoves(_grid, config.M, config.N))
        {
            Debug.Log("Shuffle sonrası hamle yok, manuel müdahale yapılıyor.");
            // (0,0) ve (0,1) noktalarını aynı renk yaparak hamleyi GARANTİLE
            if (_grid[0,0] != null && _grid[0,1] != null)
            {
                // DÜZELTME BURADA: SetBlock yerine Init
                _grid[0,1].Init(0, 1, _grid[0,0].Data, this);
            }
        }

        // Görselleri güncelle
        UpdateAllVisuals();
        Debug.Log("Board Shuffled.");
    }
}