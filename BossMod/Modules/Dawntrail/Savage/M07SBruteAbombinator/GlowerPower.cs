namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class GlowerPower(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(65f, 7f);
    public AOEInstance[] AOE = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BrutishSwingCone1:
            case (uint)AID.BrutishSwingDonutSegment2:
                AddAOE(spell, 4.8d);
                break;
            case (uint)AID.GlowerPower1:
            case (uint)AID.GlowerPower2:
                AddAOE(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GlowerPower1:
            case (uint)AID.GlowerPower2:
                AOE = [];
                break;
            case (uint)AID.BrutishSwingCone2:
            case (uint)AID.BrutishSwingDonutSegment1:
                AddAOE(spell, 3d);
                break;
        }
    }

    private void AddAOE(ActorCastInfo spell, double delay = default) => AOE = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay))];
}

sealed class ElectrogeneticForce(BossModule module) : Components.GenericStackSpread(module, false)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BrutishSwingCone1:
            case (uint)AID.BrutishSwingDonutSegment2:
                AddSpreads(Module.CastFinishAt(spell, 2.9d));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BrutishSwingCone2:
            case (uint)AID.BrutishSwingDonutSegment1:
                AddSpreads(Module.CastFinishAt(spell, 4.7d));
                break;
        }
    }

    private void AddSpreads(DateTime activation)
    {
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            Spreads.Add(new(party[i], 6f, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrogeneticForce)
        {
            ++NumCasts;
        }
    }
}
