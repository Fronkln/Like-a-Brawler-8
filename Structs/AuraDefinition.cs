using System;
using DragonEngineLibrary;

[Serializable]
public class AuraDefinition
{
    public EffectEventCharaID Loop;
    public EffectEventCharaID End;

    public AuraDefinition(EffectEventCharaID loop, EffectEventCharaID end)
    {
        Loop = loop;
        End = end;
    }
}