using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Match2/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Board Settings")]
    [SerializeField] private int m;
    [SerializeField] private int n;
    [SerializeField] private int k;

    public int M => m; 
    public int N => n;
    public int K => k;

    [Header("Icon Thresholds")]
    [SerializeField] private int a;
    [SerializeField] private int b;
    [SerializeField] private int c;

    public int A => a;
    public int B => b;
    public int C => c;

    [Header("Visuals")]
    public ColorData[] colors; 
}