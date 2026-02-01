using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Oyunun ana ızgara yönetimini, animasyonlarını, eşleşme kontrolünü ve oyun akışını yöneten merkezi sınıf.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Level Config")]
    public LevelConfig config;
    public GameObject blockPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float fallDuration = 0.4f;
    [SerializeField] private float blastDuration = 0.2f;
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    private Block[,] _grid;
    private Queue<Block> _pool = new Queue<Block>();
    private List<ColorData> _activeColors;
    private Camera _mainCam;
    private bool _isProcessing = false;

    private void Awake()
    {
        // Performance: Camera.main her çağrıldığında sahnede arama yapar, cachelemek CPU dostudur.
        _mainCam = Camera.main;
    }

    private void Start()
    {
        if (config == null) return;

        // Seviye başlangıcında havuzdan rastgele K tane renk seçiyoruz.
        _activeColors = config.GetRandomColorsForLevel();
        GenerateGrid();
        AdjustCamera();
    }

    private void OnEnable() => Block.OnBlockClicked += HandleBlockClicked;
    private void OnDisable() => Block.OnBlockClicked -= HandleBlockClicked;

    /// <summary>
    /// Seviye tasarımına uygun olarak ızgarayı oluşturur ve başlangıç bloklarını yerleştirir.
    /// </summary>
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

    /// <summary>
    /// Event üzerinden gelen tıklama sinyalini karşılar ve eşleşme sürecini başlatır.
    /// </summary>
    private void HandleBlockClicked(Block clickedBlock)
    {
        if (_isProcessing) return;

        List<Block> group = GetGroup(clickedBlock);

        if (group.Count >= 2) 
        {
            _isProcessing = true; 
            BlastGroup(group);
        }
    }

    /// <summary>
    /// Belirlenen gruptaki blokları havuz sistemine geri gönderir ve patlama animasyonunu tetikler.
    /// </summary>
    private void BlastGroup(List<Block> group)
    {
        foreach (Block block in group)
        {
            _grid[block.X, block.Y] = null;
            
            // DOTween: Blokları küçülterek yok eder.
            block.transform.DOScale(Vector3.zero, blastDuration)
                .OnComplete(() => ReturnBlockToPool(block));
        }
        
        DOVirtual.DelayedCall(blastDuration, () => FillHoles());
    }

    /// <summary>
    /// Boşlukları tespit ederek üstteki blokları aşağı kaydırır ve yeni bloklar üretir (Two-Pointer tabanlı).
    /// </summary>
    private void FillHoles()
    {
        _isProcessing = true;

        for (int x = 0; x < config.M; x++)
        {
            int writeY = 0;

            for (int y = 0; y < config.N; y++)
            {
                if (_grid[x, y] == null) continue;

                if (y != writeY)
                {
                    Block block = _grid[x, y];
                    _grid[x, writeY] = block;
                    _grid[x, y] = null;

                    // Loose Coupling: Init artık manager referansı beklemez.
                    block.Init(x, writeY, block.Data);
                    block.transform.DOMove(new Vector3(x, writeY, 0), fallDuration).SetEase(fallEase);
                }
                writeY++;
            }

            for (int y = writeY; y < config.N; y++)
            {
                int spawnOffset = config.N + (y - writeY);
                SpawnBlockAt(x, y, spawnOffset);
            
                _grid[x, y].transform.DOMove(new Vector3(x, y, 0), fallDuration).SetEase(fallEase);
            }
        }

        UpdateAllVisuals();

        DOVirtual.DelayedCall(fallDuration, () =>
        {
            _isProcessing = false;
            CheckForDeadlock();
        });
    }

    /// <summary>
    /// Belirtilen koordinatlarda havuza uygun bir blok üretir veya mevcut olanı canlandırır.
    /// </summary>
    private void SpawnBlockAt(int x, int y, int startY)
    {
        int randomColorIndex = Random.Range(0, _activeColors.Count);
        ColorData selectedData = _activeColors[randomColorIndex];
        Block blockScript = GetBlockFromPool();

        blockScript.transform.position = new Vector3(x, startY, 0);
        blockScript.transform.localScale = Vector3.one;
        blockScript.Init(x, y, selectedData);
    
        _grid[x, y] = blockScript;
    }

    /// <summary>
    /// Object Pooling: Bellek performansını korumak için objeleri yeniden kullanır.
    /// </summary>
    private Block GetBlockFromPool()
    {
        if (_pool.Count > 0)
        {
            Block block = _pool.Dequeue();
            block.gameObject.SetActive(true); 
            return block;
        }
        return Instantiate(blockPrefab, transform).GetComponent<Block>();
    }

    private void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false); 
        _pool.Enqueue(block); 
    }

    /// <summary>
    /// Tıklanan bloktan başlayarak BFS (Breadth-First Search) algoritması ile aynı renkteki grubu bulur.
    /// </summary>
    private List<Block> GetGroup(Block startBlock)
    {
        List<Block> group = new List<Block>();
        if (startBlock == null) return group;

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
                if (neighbor != null && !visited.Contains(neighbor) && neighbor.Data == startBlock.Data)
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

    /// <summary>
    /// Tüm griddeki blokların ikonlarını mevcut grup büyüklüklerine (A, B, C) göre günceller.
    /// </summary>
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

    /// <summary>
    /// Grid üzerinde yapılabilecek hamle kalıp kalmadığını kontrol eder.
    /// </summary>
    private void CheckForDeadlock()
    {
        if (!MatchUtility.HasAnyMoves(_grid, config.M, config.N))
        {
            Debug.Log("<color=yellow>Deadlock Detected!</color> Shuffling...");
            ShuffleBoard();
        }
    }

    /// <summary>
    /// Hamle kalmadığında tüm renkleri toplar, Fisher-Yates ile karıştırır ve gridi yeniden dağıtır.
    /// </summary>
    private void ShuffleBoard()
    {
        _isProcessing = true;
        List<ColorData> allColors = new List<ColorData>();

        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                if (_grid[x, y] != null && _grid[x, y].gameObject.activeSelf)
                    allColors.Add(_grid[x, y].Data);
            }
        }

        // Fisher-Yates Shuffle
        for (int i = allColors.Count - 1; i > 0; i--)
        {
            int rIndex = Random.Range(0, i + 1);
            ColorData temp = allColors[i];
            allColors[i] = allColors[rIndex];
            allColors[rIndex] = temp;
        }

        int listIndex = 0;
        for (int x = 0; x < config.M; x++)
        {
            for (int y = 0; y < config.N; y++)
            {
                if (_grid[x, y] != null && listIndex < allColors.Count)
                {
                    _grid[x, y].Init(x, y, allColors[listIndex]);
                    listIndex++;
                }
            }
        }

        // Emergency Fix: Eğer hala hamle yoksa (0,0) ve (0,1)'i eşitle.
        if (!MatchUtility.HasAnyMoves(_grid, config.M, config.N))
        {
            if (config.M > 0 && config.N > 1)
                _grid[0, 1].Init(0, 1, _grid[0, 0].Data);
        }

        UpdateAllVisuals();
        DOVirtual.DelayedCall(0.2f, () => {
            _isProcessing = false;
            CheckForDeadlock(); 
        });
    }
    
    /// <summary>
    /// Kamerayı grid boyutlarına ve ekran oranına göre otomatik olarak konumlandırır ve ölçeklendirir.
    /// </summary>
    private void AdjustCamera()
    {
        float centerX = (config.M - 1) / 2f;
        float centerY = (config.N - 1) / 2f;
        _mainCam.transform.position = new Vector3(centerX, centerY, -10f);

        float aspect = (float)Screen.width / Screen.height;
        float padding = 1.5f;
        float vSize = (config.N / 2f) + padding;
        float hSize = ((config.M / 2f) / aspect) + padding;

        _mainCam.orthographicSize = Mathf.Max(vSize, hSize);
    }
}