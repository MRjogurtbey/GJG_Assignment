using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // DOTween kütüphanesini ekledik

public class GridManager : MonoBehaviour
{
    public LevelConfig config;
    public GameObject blockPrefab;
    private Block[,] _grid;
    private Queue<Block> _pool = new Queue<Block>();
    private List<ColorData> _activeColors;
    private Camera _mainCam;

    [Header("Animation Settings")]
    [SerializeField] private float fallDuration = 0.4f;
    [SerializeField] private float blastDuration = 0.2f;
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void Start()
    {
        _activeColors = config.GetRandomColorsForLevel();
        GenerateGrid();
        AdjustCamera();
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

    private void SpawnBlockAt(int x, int y, int startY)
    {
        int randomColorIndex = Random.Range(0, _activeColors.Count);
        ColorData selectedData = _activeColors[randomColorIndex];
        Block blockScript = GetBlockFromPool();

        blockScript.transform.position = new Vector3(x, startY, 0);
        blockScript.transform.localScale = Vector3.one;
        blockScript.Init(x, y, selectedData, this);
    
        _grid[x, y] = blockScript;
    }

    private Block GetBlockFromPool()
    {
        if (_pool.Count > 0)
        {
            Block block = _pool.Dequeue();
            block.gameObject.SetActive(true); 
            return block;
        }
        GameObject newObj = Instantiate(blockPrefab, transform);
        return newObj.GetComponent<Block>();
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
            
            // ANIMASYON: Küçülerek yok olma
            block.transform.DOScale(Vector3.zero, blastDuration)
                .OnComplete(() => {
                    ReturnBlockToPool(block);
                });
        }
        
        // Patlama bittikten hemen sonra taşları düşür
        DOVirtual.DelayedCall(blastDuration, () => FillHoles());
    }

    private void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false); 
        _pool.Enqueue(block); 
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
                        block.Init(x, writeY, block.Data, this);
                        
                        // ANIMASYON: Aşağı süzülme
                        block.transform.DOMove(new Vector3(x, writeY, 0), fallDuration).SetEase(fallEase);
                    }
                    writeY++;
                }
            }

            // Yeni blokları yukarıdan yağdır
            for (int y = writeY; y < config.N; y++)
            {
                int spawnOffset = config.N + (y - writeY);
                SpawnBlockAt(x, y, spawnOffset);
                
                // ANIMASYON: Yeni gelen taşların düşüşü
                _grid[x, y].transform.DOMove(new Vector3(x, y, 0), fallDuration).SetEase(fallEase);
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
        if (groupSize >= config.C) return data.IconC;
        if (groupSize >= config.B) return data.IconB;
        if (groupSize >= config.A) return data.IconA;
        return data.DefaultIcon;
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
    private void OnEnable() 
    {
        Block.OnBlockClicked += HandleBlockClicked;
    }

    private void OnDisable() 
    {
        Block.OnBlockClicked -= HandleBlockClicked;
    }

    private void HandleBlockClicked(Block clickedBlock)
    {
        List<Block> group = GetGroup(clickedBlock);
        if (group.Count >= 2) 
        {
            BlastGroup(group);
        }
    }
    
    private void AdjustCamera()
    {
        float centerX = (config.M - 1) / 2f;
        float centerY = (config.N - 1) / 2f;
    
        _mainCam.transform.position = new Vector3(centerX, centerY, -10f);

        float aspectRatio = (float)Screen.width / Screen.height;
        float padding = 1.5f;
        float verticalSize = (config.N / 2f) + padding;
        float horizontalSize = ((config.M / 2f) / aspectRatio) + padding;

        _mainCam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
    
    
}