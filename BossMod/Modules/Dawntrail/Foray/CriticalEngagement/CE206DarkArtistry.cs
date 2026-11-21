namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE206DarkArtistry;

public enum OID : uint
{
    PhantomNecromancer = 0x4BC1,
    LongDeadExplorer = 0x4BC2, // R1.000, x0 (spawn during fight)
    LongDeadPirate = 0x4BC3, // R2.600, x0 (spawn during fight)
    PhantomNecromancer1 = 0x4C75, // R1.000, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50761, // PhantomNecromancer->player, no cast, single-target
    Ability = 47173, // 4C75->self, no cast, ???
    DarkII = 47181, // PhantomNecromancer->self, 5.0s cast, range 50 width 50 rect
    DarkFlareCast = 47182, // PhantomNecromancer->self, 5.0s cast, single-target
    DarkFlare = 47183, // Helper->self, no cast, ???
    ArcaneRevelation = 47179, // PhantomNecromancer->self, 3.0s cast, single-target
    Necrosurge = 47180, // Helper->self, 7.0s cast, range 70 width 12 rect

    RiseOfTheFallen = 47174, // PhantomNecromancer->self, 3.0s cast, single-target
    LongDeadExplorerExplosion = 47175, // 4BC2->self, 2.0s cast, range 8 circle
    LongDeadPirateExplosion = 47176, // 4BC3->self, 4.0s cast, range 80 width 7 cross
}

public enum SID : uint
{
    ExplosionTimer = 2056, // none->LongDeadExplorer, extra=0x26B
}

sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeRect(50f, 25f));
sealed class DarkFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkFlareCast);
sealed class Necrosurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrosurge, new AOEShapeRect(70f, 6f));

sealed class LongDeadExplorer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LongDeadExplorerExplosion, 8f, riskyWithSecondsLeft: 4d)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.LongDeadExplorer && id == 4564)
        {
            Casters.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(7.1d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }

            index++;
        }

        var max = index * 2 > count ? count : index * 2;

        if (RiskyWithSecondsLeft != default)
        {
            var time = WorldState.CurrentTime;
            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Risky = aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
                aoe.Color = i < index ? Colors.Danger : Colors.AOE;
            }
        }

        return aoes[..max];
    }
}

sealed class LongDeadPirate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LongDeadPirateExplosion, new AOEShapeCross(80f, 3.5f), 4, 5d)
{

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.LongDeadPirate && id == 4561)
        {
            Casters.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.0f), actorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }
}

sealed class CE206DarkArtistryStates : StateMachineBuilder
{
    public CE206DarkArtistryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<LongDeadExplorer>()
            .ActivateOnEnter<LongDeadPirate>()
            .ActivateOnEnter<Necrosurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.PhantomNecromancer, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1093u, NameID = 57u)]
public sealed class CE206DarkArtistry(WorldState ws, Actor primary) : BossModule(ws, primary, new(224f, -860f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 20f);
}
