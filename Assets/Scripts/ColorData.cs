using UnityEngine;

[CreateAssetMenu(fileName = "New Color Data", menuName = "Match2/Color Data")]
public class ColorData: ScriptableObject
{
    [SerializeField] private string _colorName;
    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private Sprite _iconA;      
    [SerializeField] private Sprite _iconB;      
    [SerializeField] private Sprite _iconC;    
    
    public string ColorName => _colorName;
    public Sprite DefaultIcon => _defaultIcon;
    public Sprite IconA => _iconA;
    public Sprite IconB => _iconB;
    public Sprite IconC => _iconC;
}
