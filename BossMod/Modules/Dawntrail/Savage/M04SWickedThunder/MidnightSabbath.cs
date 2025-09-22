namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class MidnightSabbath(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShape _shapeCannon = new AOEShapeRect(40f, 5f);
    private static readonly AOEShape _shapeBird = new AOEShapeDonut(5f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.WickedReplica)
            return;
        var (shape, delay) = id switch
        {
            0x11D1 => (_shapeCannon, 8.1d),
            0x11D2 => (_shapeCannon, 12.1d),
            0x11D3 => (_shapeBird, 8.1d),
            0x11D4 => (_shapeBird, 12.1d),
            _ => default
        };
        if (shape != default)
        {
            AOEs.Add(new(shape, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(delay)));
            AOEs.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MidnightSabbathThundering or (uint)AID.MidnightSabbathWickedCannon)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}

class ConcentratedScatteredBurst(BossModule module) : Components.UniformStackSpread(module, 5f, 5f)
{
    public int NumFinishedStacks;
    public int NumFinishedSpreads;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ConcentratedBurst:
                ShowStacks(Module.CastFinishAt(spell, 0.1f));
                break;
            case (uint)AID.ScatteredBurst:
                ShowSpreads(Module.CastFinishAt(spell, 0.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WickedSpark:
                ++NumFinishedSpreads;
                Spreads.Clear();
                if (NumFinishedStacks == 0)
                    ShowStacks(WorldState.FutureTime(3.1d));
                break;
            case (uint)AID.WickedFlare:
                ++NumFinishedStacks;
                Stacks.Clear();
                if (NumFinishedSpreads == 0)
                    ShowSpreads(WorldState.FutureTime(3.1d));
                break;
        }
    }

    private void ShowStacks(DateTime activation)
    {
        // TODO: can target any role
        AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), activation);
    }

    private void ShowSpreads(DateTime activation) => AddSpreads(Raid.WithoutSlot(true, true, true), activation);
}
