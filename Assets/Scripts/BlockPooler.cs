using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Blokların bellekte verimli yönetilmesini sağlayan havuz sistemi.
/// </summary>
public class BlockPooler : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int initialPoolSize = 50; // Grid boyutuna göre ayarla
    private Queue<Block> _pool = new Queue<Block>();

    private void Start()
    {
        // Pre-warming: Oyun başladığında objeleri sessizce üret ve hazırla
        for (int i = 0; i < initialPoolSize; i++)
        {
            Block newBlock = CreateNewBlock();
            newBlock.gameObject.SetActive(false);
            _pool.Enqueue(newBlock);
        }
    }

    public Block GetBlock()
    {
        if (_pool.Count > 0)
        {
            Block block = _pool.Dequeue();
            block.gameObject.SetActive(true);
            return block;
        }
        return CreateNewBlock();
    }

    private Block CreateNewBlock()
    {
        return Instantiate(blockPrefab, transform).GetComponent<Block>();
    }

    public void ReturnBlock(Block block)
    {
        block.gameObject.SetActive(false);
        _pool.Enqueue(block);
    }
}