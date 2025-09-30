namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class ImpurePurgation(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ImpurePurgationFirst, (uint)AID.ImpurePurgationSecond], new AOEShapeCone(60f, 22.5f.Degrees()), 4, 8);

sealed class MalformedPrayer(BossModule module) : Components.GenericTowers(module)
{
    private readonly ImpurePurgation _aoe = module.FindComponent<ImpurePurgation>()!;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001u)
            return;
        WPos? pos = index switch
        {
            0x1F => new(741f, -201f),
            0x20 => new(748.778f, -197.778f),
            0x21 => new(752f, -190f),
            0x22 => new(748.778f, -182.222f),
            0x23 => new(741f, -179f),
            0x24 => new(733.222f, -182.222f),
            0x25 => new(730f, -190f),
            0x26 => new(733.222f, -197.778f),
            _ => null
        };
        if (pos is WPos origin)
        {
            Towers.Add(new(origin.Quantized(), 4f, activation: WorldState.FutureTime(9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Towers.Count != 0 && spell.Action.ID == (uint)AID.Burst)
        {
            Towers.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Casters.Count != 0 && Towers.Count != 0 && !Towers[0].IsInside(actor))
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 3f));
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.MinSoakers = i > 0 ? 0 : 1;
            t.MaxSoakers = i > 0 ? 0 : 4;
        }
    }
}
