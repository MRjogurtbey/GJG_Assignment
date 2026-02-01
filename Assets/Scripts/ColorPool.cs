using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GlobalColorPool", menuName = "Match2/Color Pool")]
public class ColorPool : ScriptableObject
{
    [SerializeField] private List<ColorData> _allColors;
    public List<ColorData> AllColors => _allColors;
}
