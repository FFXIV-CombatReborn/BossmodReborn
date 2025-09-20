namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE101BlackRegiment;

public enum OID : uint
{
    Boss = 0x46A7, // R1.32
    BlackStar = 0x46A5, // R1.98
    BlackChocobo1 = 0x46A8, // R1.32
    BlackChocobo2 = 0x46A6, // R1.32
    BlackChocobo3 = 0x45D0, // R1.32
    BlackChocobo4 = 0x46A9, // R1.32
    Deathwall = 0x4861, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 43032, // BlackChocobo2->player, no cast, single-target
    AutoAttack2 = 43260, // BlackStar->player, no cast, single-target

    ChocoAero = 41165, // Boss->player, no cast, single-target
    Deathwall = 41394, // Deathwall->self, no cast, ???

    ChocoBeak = 41163, // BlackChocobo1->location, 5.0s cast, width 4 rect charge
    ChocoMaelfeather = 41164, // BlackChocobo6->self, 5.0s cast, range 8 circle
    ChocoWindstorm = 41147, // BlackStar->self, 7.0s cast, range 16 circle
    ChocoCyclone = 41148, // BlackStar->self, 7.0s cast, range 8-30 donut

    ChocoSlaughterVisual = 41149, // BlackStar->self, 5.0s cast, single-target
    ChocoSlaughterFirst = 41151, // Helper->self, 5.0s cast, range 5 circle, exaflare
    ChocoSlaughterRest = 41152, // Helper->location, no cast, range 5 circle

    ChocoDoublades = 41153, // BlackStar->self, 5.0s cast, single-target
    ChocoBlades1 = 41155, // Helper->self, 5.0s cast, range 40 45-degree cone
    ChocoBlades2 = 41156, // Helper->self, 8.0s cast, range 40 45-degree cone
    Visual = 41154, // BlackStar->self, no cast, single-target

    ChocoAeroII = 41162 // Helper->location, 3.0s cast, range 4 circle
}

sealed class ChocoSlaughter(BossModule module) : Components.SimpleExaflare(module, 5f, (uint)AID.ChocoSlaughterFirst, (uint)AID.ChocoSlaughterRest, 5f, 1.1d, 5, 3, true);

sealed class ChocoBeak(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeRect rect = new(50f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1 && actor.OID == (uint)OID.BlackChocobo1)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(9.1d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ChocoBeak)
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

sealed class ChocoAeroII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChocoAeroII, 4f);
sealed class ChocoMaelfeather(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChocoMaelfeather, 8f);
sealed class ChocoWindstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChocoWindstorm, 16f, riskyWithSecondsLeft: 4d);
sealed class ChocoCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChocoCyclone, new AOEShapeDonut(8f, 30f));
sealed class ChocoDoublades(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChocoBlades1, (uint)AID.ChocoBlades2], new AOEShapeCone(40f, 22.5f.Degrees()), 4, 8);

sealed class CE101BlackRegimentStates : StateMachineBuilder
{
    public CE101BlackRegimentStates(CE101BlackRegiment module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChocoSlaughter>()
            .ActivateOnEnter<ChocoAeroII>()
            .ActivateOnEnter<ChocoDoublades>()
            .ActivateOnEnter<ChocoBeak>()
            .ActivateOnEnter<ChocoMaelfeather>()
            .ActivateOnEnter<ChocoWindstorm>()
            .ActivateOnEnter<ChocoCyclone>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.BlackChocobo1);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDestroyed)
                    {
                        return false;
                    }
                }
                return module.BossBlackStar?.IsDeadOrDestroyed ?? true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 34)]
public sealed class CE101BlackRegiment(WorldState ws, Actor primary) : BossModule(ws, primary, new(450f, 357f), new ArenaBoundsSquare(20f))
{
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.BlackChocobo2, (uint)OID.BlackChocobo4];
    public Actor? BossBlackStar;

    protected override void UpdateModule()
    {
        BossBlackStar ??= GetActor((uint)OID.BlackStar);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
        Arena.Actor(BossBlackStar);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f); // not targetable at start
}
