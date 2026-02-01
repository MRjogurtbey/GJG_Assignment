using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Match2/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Grid Settings")]
    [SerializeField] private int _m;
    [SerializeField] private int _n;
    
    [Header("Threshold Settings")]
    [SerializeField] private int _a;
    [SerializeField] private int _b;
    [SerializeField] private int _c;

    [Header("Dynamic Color Settings")]
    [SerializeField] private ColorPool _globalPool; 
    [SerializeField] private int _k; 

    public int M => _m;
    public int N => _n;
    public int A => _a;
    public int B => _b;
    public int C => _c;
    public int K => _k;

    public List<ColorData> GetRandomColorsForLevel()
    {
        if (_globalPool == null || _globalPool.AllColors.Count == 0) return new List<ColorData>();

        List<ColorData> result = new List<ColorData>(_globalPool.AllColors);
        int poolSize = result.Count;

        for (int i = 0; i < poolSize; i++)
        {
            int randomIndex = Random.Range(i, poolSize);
            ColorData temp = result[i];
            result[i] = result[randomIndex];
            result[randomIndex] = temp;
        }

        int countToPick = Mathf.Min(_k, poolSize);
        if (result.Count > countToPick)
        {
            result.RemoveRange(countToPick, result.Count - countToPick);
        }

        return result;
    }

    private void OnValidate()
    {
        if (_globalPool != null)
        {
            _k = Mathf.Clamp(_k, 1, _globalPool.AllColors.Count);
        }
    }
}