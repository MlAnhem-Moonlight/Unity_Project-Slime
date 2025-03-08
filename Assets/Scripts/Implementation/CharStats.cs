using System.Collections;
using System.Collections.Generic;


public class CharStats
{
    public float BaseValue { get; set; }
    
    private readonly List<StatModifier> statModifiers;

    private CharStats(float baseValue)
    {
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
    }
}
