namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

sealed class TasteOfBloodAndDuelOrDie(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShape _tasteOfBloodShape = new AOEShapeCone(40f, 90f.Degrees());
    public readonly List<Actor> Casters = [];
    public readonly List<Actor> Duelers = [];

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var caster = Casters[i];
            // If the caster did Duel Or Die, the player must get hit by their attack.
            // This is represented by pointing the AOE behind the caster so their front is safe.
            var angle = Duelers.Contains(caster) ? caster.Rotation + 180f.Degrees() : caster.Rotation;
            aoes[i] = new(_tasteOfBloodShape, caster.Position.Quantized(), angle);
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TasteOfBlood)
            Casters.Add(caster);
        else if (spell.Action.ID == (uint)AID.DuelOrDie)
            Duelers.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TasteOfBlood)
        {
            Casters.Remove(caster);
            Duelers.Remove(caster);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var caster in Casters)
        {
            var isDueler = Duelers.Contains(caster);
            Arena.Actor(caster, isDueler ? Colors.Danger : 0, true);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Duelers.Count > 0)
            hints.Add($"Get hit by {Duelers.Count} Duel or Die Taste Of Blood");
    }
}
