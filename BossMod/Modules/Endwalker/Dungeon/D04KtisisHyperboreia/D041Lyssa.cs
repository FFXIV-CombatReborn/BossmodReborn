namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D041Lyssa;

public enum OID : uint
{
    Boss = 0x3323, // R=4.0
    IcePillar = 0x3324, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    FrigidStomp = 25181, // Boss->self, 5.0s cast, range 50 circle, raidwide
    FrostbiteAndSeek1 = 28304, // Helper->self, no cast, single-target
    FrostbiteAndSeek2 = 25175, // Boss->self, 3.0s cast, single-target
    HeavySmash = 25180, // Boss->players, 5.0s cast, range 6 circle, stack
    IcePillar = 25179, // IcePillar->self, 3.0s cast, range 4 circle
    Icicall = 25178, // Boss->self, 3.0s cast, single-target
    PillarPierceAOE = 25375, // IcePillar->self, 5.0s cast, range 80 width 4 rect
    PunishingSliceVisual = 25176, // Boss->self, no cast, single-target
    PunishingSliceAOE = 25177, // Helper->self, 2.0s cast, range 50 width 50 rect
    SkullDasher = 25182 // Boss->player, 5.0s cast, single-target, tankbuster
}

sealed class PillarPierceAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierceAOE, new AOEShapeRect(40f, 2f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.HeavySmash)
        {
            var count = Casters.Count;
            var casters = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i)
            {
                ref var c = ref casters[i];
                c.Activation = Module.CastFinishAt(spell);
            }
        }
    }
}

sealed class PunishingSlice(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(50f, 25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        (WPos, Angle) pair = (index, state) switch
        {
            (0x00, 0x00200010) => (new(-154.825f, 42.75f), 60f.Degrees()),
            (0x00, 0x01000080) => (new(-154.825f, 55.25f), 119.997f.Degrees()),
            (0x00, 0x00020001) => (new(-144f, 36.5f), -0.003f.Degrees()),
            (0x01, 0x00200010) => (new(-144f, 61.5f), -180f.Degrees()),
            (0x01, 0x01000080) => (new(-133.175f, 55.25f), -120.003f.Degrees()),
            (0x01, 0x00020001) => (new(-133.175f, 42.75f), -60.005f.Degrees()),
            _ => default
        };
        if (pair != default)
        {
            var activation = NumCasts == 0 ? WorldState.FutureTime(13d) : WorldState.FutureTime(16d);
            _aoe = [new(rect, pair.Item1.Quantized(), pair.Item2, activation)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PunishingSliceAOE)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class IcePillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcePillar, 4f);
sealed class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HeavySmash, 6f, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Stacks.Count;
        if (count == 0)
            return;
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            if (party[i].Type == ActorType.Buddy)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(new(-139f, 47f), 5f), Stacks.Ref(0).Activation);
                return;
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}
sealed class SkullDasher(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SkullDasher);
sealed class FrigidStomp(BossModule module) : Components.RaidwideCast(module, (uint)AID.FrigidStomp);

sealed class D041LyssaStates : StateMachineBuilder
{
    public D041LyssaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PillarPierceAOE>()
            .ActivateOnEnter<PunishingSlice>()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<FrigidStomp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10396)]
public sealed class D041Lyssa(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsCustom arena = new([new Polygon(new(-144f, 49f), 19.5f, 32)], [new Rectangle(new(-144f, 28.852f), 20f, 1.25f), new Rectangle(new(-144f, 69.197f), 20f, 1.25f)]);
}
