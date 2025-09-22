namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE112EternalWatch;

public enum OID : uint
{
    Boss = 0x46CC, // R7.42
    HolySphere = 0x46CD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    RadiantWaveVisual = 41279, // Boss->self, 5.0s cast, single-target
    RadiantWave = 41345, // Helper->self, no cast, ???
    AncientHolyVisual = 41282, // Boss->self, no cast, single-target

    AncientHoly1 = 41284, // Helper->location, 11.0s cast, ???, proximity AOE, multiple casters to circumvent AOE limit
    AncientHoly2 = 41395, // Helper->location, 11.0s cast, ???

    AetherialExchange = 41280, // Boss->self, 3.0+1,0s cast, single-target
    AetherialExchangeStone = 41276, // Helper->HolySphere, 1.0s cast, single-target
    AetherialExchangeWind = 41281, // Helper->HolySphere, 1.0s cast, single-target

    AncientStoneIIIVisual = 41288, // Boss->self, 4.0+1,0s cast, single-target
    AncientStoneIII1 = 41289, // Helper->self, 5.0s cast, range 40 60-degree cone
    AncientStoneIII2 = 41293, // Helper->self, 12.0s cast, range 40 60-degree cone
    AncientAeroIIIVisual = 41286, // Boss->self, 4.0+1,0s cast, single-target
    AncientAeroIII1 = 41287, // Helper->self, 5.0s cast, range 40 60-degree cone
    AncientAeroIII2 = 41292, // Helper->self, 12.0s cast, range 40 60-degree cone

    SandSurge = 41296, // HolySphere->self, 2.0s cast, range 15 circle
    WindSurge = 41295, // HolySphere->self, 2.0s cast, range 15 circle
    LightSurge = 41294, // HolySphere->self, 2.0s cast, range 15 circle

    HolyBlaze = 41297, // Boss->self, 4.0s cast, range 60 width 5 rect
    Scratch = 41301, // Boss->player, 5.0s cast, single-target, tankbuster
    Fissure = 41283, // Boss->self, no cast, single-target

    DoubleCast1 = 41291, // Boss->self, 11.5+0,5s cast, single-target
    DoubleCast2 = 41290 // Boss->self, 11.5+0,5s cast, single-target
}

public enum SID : uint
{
    SphereStatus = 2536 // Helper->HolySphere, extra=0x225/0x224, 224 wind, 225 stone
}

sealed class AncientStoneAncientAeroIIISlow : Components.SimpleAOEGroups
{
    public AncientStoneAncientAeroIIISlow(BossModule module) : base(module, [(uint)AID.AncientAeroIII2, (uint)AID.AncientStoneIII2], WindStoneLightSurge.Cone, 4)
    {
        MaxDangerColor = 2;
    }
}

sealed class AncientStoneAncientAeroIIIFast(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientAeroIII1, (uint)AID.AncientStoneIII1], WindStoneLightSurge.Cone)
{
    private readonly WindStoneLightSurge _aoe = module.FindComponent<WindStoneLightSurge>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.AOEs.Count != 0 && Casters.Count != 0 && (_aoe.AOEs[0].Activation - Casters[0].Activation).TotalSeconds < 1.5d)
        {
            return [];
        }
        return base.ActiveAOEs(slot, actor);
    }
}

sealed class AncientHoly(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientHoly1, 26f);
sealed class RadiantWave(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.RadiantWaveVisual, (uint)AID.RadiantWave, 1.2f);
sealed class Scratch(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Scratch);
sealed class HolyBlaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyBlaze, new AOEShapeRect(60f, 2.5f));

sealed class WindStoneLightSurge(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(12);
    private static readonly AOEShapeCircle circle = new(15f);
    public static readonly AOEShapeCone Cone = new(40f, 30f.Degrees());
    private readonly List<Actor> spheres = new(12);
    private readonly List<Actor> spheresStone = new(6);
    private readonly List<Actor> spheresWind = new(6);
    private readonly AncientHoly _aoe = module.FindComponent<AncientHoly>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);

        ref var aoe0 = ref aoes[0];
        var act0 = aoe0.Activation;

        if (count > 3 && (AOEs.Ref(3).Activation - act0).TotalSeconds > 0.5d)
        {
            for (var i = 0; i < 2; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Color = Colors.Danger;
            }
        }
        var deadline = act0.AddSeconds(2.4d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AncientAeroIII1:
            case (uint)AID.AncientAeroIII2:
                AddAOEs(spheresWind);
                break;
            case (uint)AID.AncientStoneIII1:
            case (uint)AID.AncientStoneIII2:
                AddAOEs(spheresStone);
                break;
            case (uint)AID.AncientHoly1 when spheres.Count == 4:
                var count = spheres.Count;
                var act = Module.CastFinishAt(spell, 2.4d);
                for (var i = 0; i < count; ++i)
                {
                    var sphere = spheres[i];
                    AddAOE(sphere.Position, ref sphere.InstanceID, ref act);
                }
                spheres.Clear();
                break;
        }
        void AddAOE(WPos position, ref readonly ulong instanceID, ref readonly DateTime activation, Angle rotation = default) => AOEs.Add(new(circle, position.Quantized(), rotation, activation, actorID: instanceID));
        void AddAOEs(List<Actor> list)
        {
            var count = list.Count - 1;
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell).AddSeconds(2.4d);
            for (var i = count; i >= 0; --i)
            {
                var sphere = list[i];
                if (Cone.Check(sphere.Position, pos, rot))
                {
                    AddAOE(sphere.Position, ref sphere.InstanceID, ref act);
                    list.RemoveAt(i);
                }
                if (i == 0)
                {
                    AOEs.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.WindSurge or (uint)AID.SandSurge or (uint)AID.LightSurge)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.SphereStatus)
        {
            var extra = status.Extra;
            if (extra == 0x224)
            {
                UpdateList(spheresWind);
            }
            else if (extra == 0x225)
            {
                UpdateList(spheresStone);
            }
            if (spheres.Count == 4 && _aoe.Casters.Count != 0)
            {
                var act = _aoe.Casters.Ref(0).Activation.AddSeconds(2.6d);
                for (var i = 0; i < 4; ++i)
                {
                    var sphere = spheres[i];
                    AOEs.Add(new(circle, sphere.Position.Quantized(), default, act, actorID: sphere.InstanceID));
                }
                spheres.Clear();
            }
        }
        void UpdateList(List<Actor> list)
        {
            list.Add(actor);
            spheres.Remove(actor);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.HolySphere)
        {
            spheres.Add(actor);
        }
    }
}

sealed class CE112EternalWatchStates : StateMachineBuilder
{
    public CE112EternalWatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientStoneAncientAeroIIISlow>()
            .ActivateOnEnter<AncientHoly>()
            .ActivateOnEnter<RadiantWave>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<HolyBlaze>()
            .ActivateOnEnter<WindStoneLightSurge>()
            .ActivateOnEnter<AncientStoneAncientAeroIIIFast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 46)]
public sealed class CE112EternalWatch(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(870.1f, 180f), 24.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
