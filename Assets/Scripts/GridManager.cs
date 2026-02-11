using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace GJG.Match2048.Core // Profesyonel isimlendirme
{
    /// <summary>
    /// Oyunun ana akışını koordine eden merkez sınıf. 
    /// Hesaplama mantığını MatchUtility, yerçekimini BoardRefiller üzerinden yönetir.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private BlockPooler pooler;
        [SerializeField] private GridCameraController camController;
        [SerializeField] private BoardRefiller refiller;
        public LevelConfig config; // ScriptableObject tabanlı seviye ayarları

        [Header("Settings")]
        [SerializeField] private float blastDuration = 0.2f;

        private Block[,] _grid;
        private List<ColorData> _activeColors;
        private bool _isProcessing;

        #region Unity Lifecycle
        private void Start()
        {
            // Defensive Programming: Bağımlılık kontrolü
            if (config == null || pooler == null || camController == null || refiller == null)
            {
                Debug.LogError("GridManager: Gerekli referanslar eksik! Lütfen Inspector'ı kontrol edin.");
                return;
            }

            _activeColors = config.GetRandomColorsForLevel();
            GenerateGrid();
            camController.Adjust(config.M, config.N);
        }

        private void OnEnable() => Block.OnBlockClicked += HandleBlockClicked;
        private void OnDisable() => Block.OnBlockClicked -= HandleBlockClicked;
        #endregion

     
        private void GenerateGrid()
        {
            _grid = new Block[config.M, config.N]; 
            for (int x = 0; x < config.M; x++)
            {
                for (int y = 0; y < config.N; y++)
                {
                    SpawnBlockInternally(x, y, y); 
                }
            }
            UpdateAllVisuals();
        }

        private void HandleBlockClicked(Block clickedBlock)
        {
            if (_isProcessing) return;

            // BFS Algoritması optimize edilmiş MatchUtility sınıfına taşındı.
            List<Block> group = MatchUtility.GetGroup(clickedBlock, _grid, config.M, config.N);

            if (group.Count >= 2) 
            {
                _isProcessing = true; 
                BlastGroup(group);
            }
        }

        private void BlastGroup(List<Block> group)
        {
            foreach (Block block in group)
            {
                _grid[block.X, block.Y] = null;
                block.PlayBlastAnimation(blastDuration, () => pooler.ReturnBlock(block));
            }
            
            DOVirtual.DelayedCall(blastDuration, () => 
            {
                // Boşlukları doldur ve etkilenen (dirty) sütunların listesini al.
                refiller.FillHoles(_grid, config.M, config.N, _activeColors, (dirtyColumns) => 
                {
                    _isProcessing = false;
                    
                    UpdateAffectedVisuals(dirtyColumns);
                    
                    CheckForDeadlock();
                });
            });
        }

        private void UpdateAffectedVisuals(List<int> dirtyColumns)
        {
            HashSet<Block> visited = new HashSet<Block>();
            HashSet<int> columnsToProcess = new HashSet<int>();

            foreach (int x in dirtyColumns)
            {
                columnsToProcess.Add(x);
                if (x > 0) columnsToProcess.Add(x - 1);
                if (x < config.M - 1) columnsToProcess.Add(x + 1);
            }

            foreach (int x in columnsToProcess)
            {
                for (int y = 0; y < config.N; y++)
                {
                    Block currentBlock = _grid[x, y];
                    if (currentBlock == null || visited.Contains(currentBlock)) continue;

                    List<Block> group = MatchUtility.GetGroup(currentBlock, _grid, config.M, config.N);
                    foreach (var b in group) visited.Add(b);

                    Sprite iconToSet = GetSpriteForGroupSize(currentBlock.Data, group.Count);
                    foreach (var b in group) b.UpdateVisual(iconToSet);
                }
            }
        }

        /// <summary>
        /// Tüm gridi günceller (Startup ve Shuffle durumları için kullanılır).
        /// </summary>
        public void UpdateAllVisuals()
        {
            List<int> allColumns = new List<int>();
            for (int i = 0; i < config.M; i++) allColumns.Add(i);
            UpdateAffectedVisuals(allColumns);
        }

        private void SpawnBlockInternally(int x, int y, int startY)
        {
            int randomColorIndex = Random.Range(0, _activeColors.Count);
            Block blockScript = pooler.GetBlock(); 

            blockScript.transform.position = new Vector3(x, startY, 0);
            blockScript.transform.localScale = Vector3.one;
            blockScript.Init(x, y, _activeColors[randomColorIndex]);
            _grid[x, y] = blockScript;
        }

        private Sprite GetSpriteForGroupSize(ColorData data, int groupSize)
        {
            if (groupSize >= config.C) return data.IconC;
            if (groupSize >= config.B) return data.IconB;
            if (groupSize >= config.A) return data.IconA;
            return data.DefaultIcon;
        }

        private void CheckForDeadlock()
        {
            if (!MatchUtility.HasAnyMoves(_grid, config.M, config.N))
            {
                Debug.Log("<color=yellow>Hamle Kalmadı!</color> Karıştırılıyor...");
                ShuffleBoard();
            }
        }

        private void ShuffleBoard()
        {
            _isProcessing = true;
            List<ColorData> allColors = new List<ColorData>();

            foreach (var block in _grid)
            {
                if (block != null && block.gameObject.activeSelf)
                    allColors.Add(block.Data);
            }

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

            UpdateAllVisuals();
            DOVirtual.DelayedCall(0.2f, () => _isProcessing = false);
        }
    }
}