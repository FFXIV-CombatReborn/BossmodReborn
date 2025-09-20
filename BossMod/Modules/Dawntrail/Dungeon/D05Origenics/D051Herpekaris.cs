namespace BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;

public enum OID : uint
{
    Boss = 0x4185, // R8.4
    PoisonVoidzone = 0x1E9E3C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36520, // Boss->location, no cast, single-target

    StridentShriek = 36519, // Boss->self, 5.0s cast, range 60 circle  raidwide

    Vasoconstrictor = 36459, // Boss->self, 3.0+1.2s cast, single-target, spawns poison
    PoisonHeartVoidzone = 36460, // Helper->location, 4.2s cast, range 2 circle
    PoisonHeartSpread = 37921, // Helper->player, 8.0s cast, range 5 circle

    PodBurst1 = 38518, // Helper->location, 5.0s cast, range 6 circle
    PodBurst2 = 38519, // Helper->location, 4.0s cast, range 6 circle

    Venomspill1 = 37451, // Boss->self, 5.0s cast, single-target
    Venomspill2 = 36452, // Boss->self, 5.0s cast, single-target
    Venomspill3 = 36453, // Boss->self, 4.0s cast, single-target
    Venomspill4 = 36454, // Boss->self, 4.0s cast, single-target

    WrithingRiotVisual = 36463, // Boss->self, 9.0s cast, single-target
    RightSweepTelegraph = 36465, // Helper->self, 2.0s cast, range 25 210-degree cone
    LeftSweepTelegraph = 36466, // Helper->self, 2.0s cast, range 25 210-degree cone
    RearSweepTelegraph = 36467, // Helper->self, 2.0s cast, range 25 90-degree cone
    RightSweep = 36469, // Boss->self, no cast, range 25 210-degree cone
    LeftSweep = 36470, // Boss->self, no cast, range 25 210-degree cone
    RearSweep = 36471, // Boss->self, no cast, range 25 90-degree cone

    CollectiveAgonyMarker = 36474, // Helper->player, no cast, single-target
    CollectiveAgony = 36473, // Boss->self/players, 5.5s cast, range 50 width 8 rect
    ConvulsiveCrush = 36518, // Boss->player, 5.0s cast, single-target, tb
}

sealed class CollectiveAgony(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.CollectiveAgonyMarker, (uint)AID.CollectiveAgony, 5.6d);
sealed class StridentShriek(BossModule module) : Components.RaidwideCast(module, (uint)AID.StridentShriek);
sealed class ConvulsiveCrush(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.ConvulsiveCrush);
sealed class PoisonHeartSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PoisonHeartSpread, 5f);
sealed class PoisonHeartVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 2f, (uint)AID.PoisonHeartVoidzone, GetVoidzones, 0.9d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PoisonVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class PodBurst(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PodBurst1, (uint)AID.PodBurst2], 6f);

sealed class WrithingRiot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCone coneLeftRight = new(25f, 105f.Degrees());
    private static readonly AOEShapeCone coneRear = new(25f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else if (aoe.Shape == aoes[0].Shape)
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, WorldState.FutureTime(7.3d - _aoes.Count * 1d)));
        switch (spell.Action.ID)
        {
            case (uint)AID.RightSweepTelegraph:
            case (uint)AID.LeftSweepTelegraph:
                AddAOE(coneLeftRight);
                break;
            case (uint)AID.RearSweepTelegraph:
                AddAOE(coneRear);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.RearSweep:
                case (uint)AID.LeftSweep:
                case (uint)AID.RightSweep:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle if there is next imminent aoe
        if (_aoes.Count > 1)
        {
            ref var aoe = ref _aoes.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 3f), aoe.Activation);
        }
    }
}

sealed class D051HerpekarisStates : StateMachineBuilder
{
    public D051HerpekarisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CollectiveAgony>()
            .ActivateOnEnter<StridentShriek>()
            .ActivateOnEnter<PoisonHeartSpread>()
            .ActivateOnEnter<PoisonHeartVoidzone>()
            .ActivateOnEnter<PodBurst>()
            .ActivateOnEnter<WrithingRiot>()
            .ActivateOnEnter<ConvulsiveCrush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12741, SortOrder = 1)]
public sealed class D051Herpekaris(WorldState ws, Actor primary) : BossModule(ws, primary, new(-88f, -180f), new ArenaBoundsSquare(17.5f));
