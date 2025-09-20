namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE102FromTimesBygone;

public enum OID : uint
{
    Boss = 0x469B, // R4.6
    MythicMirror = 0x469C, // R2.99
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 41536, // Boss->player, no cast, single-target
    Teleport = 41127, // Helper->location, no cast, single-target

    MysticHeat = 41137, // Boss->self, 5.0s cast, range 40 60-degree cone
    ShiftingShape = 41126, // Boss->self, 3.0s cast, single-target
    Visual1 = 41128, // Boss->self, no cast, single-target
    Visual2 = 41129, // MythicMirror->self, no cast, single-target
    BigBurst = 41130, // MythicMirror->self, 8.0s cast, range 26 circle
    DeathRay = 41133, // MythicMirror->self, 8.0s cast, range 60 90-degree cone
    Steelstrike1 = 41132, // Helper->self, 8.0s cast, range 100 width 10 cross
    Steelstrike2 = 41131, // MythicMirror->self, 8.0s cast, range 100 width 10 cross
    ArcaneDesign = 41134, // Boss->self, 3.0s cast, single-target
    ArcaneOrbTelegraph = 41135, // Helper->location, 1.0s cast, range 6 circle
    ArcaneOrb = 41136, // Helper->location, no cast, range 6 circle
    LotsCastVisual = 41138, // Boss->self, 5.0s cast, single-target
    LotsCastRaidwide = 41764, // Helper->self, no cast, ???, hits only players not targeted by spread or tankbuster
    LotsCastTB = 41139, // Helper->players, 5.0s cast, range 6 circle
    LotsCastSpread = 41140, // Helper->players, 6.0s cast, range 6 circle
    ArcaneLightVisual = 41141, // Boss->self, 5.0s cast, single-target, raidwide
    ArcaneLight = 41142 // Helper->self, no cast, ???
}

sealed class MythicMirror(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeCircle circle = new(26f);
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());
    private static readonly AOEShapeCross cross = new(100f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(5d);

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
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BigBurst => circle,
            (uint)AID.Steelstrike1 or (uint)AID.Steelstrike2 => cross,
            (uint)AID.DeathRay => cone,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BigBurst or (uint)AID.Steelstrike1 or (uint)AID.Steelstrike2 or (uint)AID.DeathRay)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class MysticHeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticHeat, new AOEShapeCone(40f, 30f.Degrees()));
sealed class LotsCast(BossModule module) : Components.RaidwideCast(module, (uint)AID.LotsCastVisual);
sealed class ArcaneLight(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.ArcaneLightVisual, (uint)AID.ArcaneLight, 0.9d);
sealed class LotsCastSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LotsCastSpread, 6f)
{
    public override void Update()
    {
        var count = Spreads.Count;
        if (count != 0) // due to culling players can get removed and added to the object table in any frame, after that we need to update their reference to get the current location again...
        {
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                ref var s = ref spreads[i];
                if (WorldState.Actors.Find(s.Target.InstanceID) is Actor t)
                {
                    s.Target = t;
                }
            }
        }
    }
}

sealed class LotsCastTB(BossModule module) : Components.BaitAwayCast(module, (uint)AID.LotsCastTB, 6f, true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count != 0) // due to culling players can get removed and added to the object table in any frame, after that we need to update their reference to get the current location again...
        {
            var spreads = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var s = ref spreads[i];
                if (WorldState.Actors.Find(s.Target.InstanceID) is Actor t)
                {
                    s.Target = t;
                }
            }
        }
    }
}

sealed class ArcaneOrb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(48);
    private static readonly AOEShapeCircle circle = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 32 ? 32 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var maxC = Math.Min(max, count - NumCasts);
        return aoes.Slice(NumCasts, maxC);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArcaneOrbTelegraph)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, WorldState.FutureTime(8.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArcaneOrb)
        {
            if (++NumCasts == 48)
            {
                NumCasts = 0;
                _aoes.Clear();
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count == 0)
        {
            return;
        }
        hints.AddForbiddenZone(new SDCircle(Arena.Center, 6.6f), WorldState.FutureTime(8d));
    }
}

sealed class CE102FromTimesBygoneStates : StateMachineBuilder
{
    public CE102FromTimesBygoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MysticHeat>()
            .ActivateOnEnter<ArcaneOrb>()
            .ActivateOnEnter<MythicMirror>()
            .ActivateOnEnter<LotsCast>()
            .ActivateOnEnter<LotsCastSpread>()
            .ActivateOnEnter<LotsCastTB>()
            .ActivateOnEnter<ArcaneLight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 39)]
public sealed class CE102FromTimesBygone(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-800f, 245f), 24.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
