namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE113ScourgeOfMind;

public enum OID : uint
{
    Boss = 0x46B5, // R3.25
    Tentacle = 0x46B7, // R7.2-12.024
    JestingJackanapes = 0x46B6, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 41173, // Boss->player, no cast, single-target

    DarkII = 41170, // Boss->self, 6.0s cast, range 65 90-degree cone
    Summon = 41168, // Boss->self, 3.0s cast, single-target
    Recharge = 41169, // Boss->self, 4.0s cast, single-target

    MindBlastVisual = 41167, // Boss->self, 4.0+1,0s cast, single-target, raidwide
    MindBlast = 41166, // Helper->self, 5.0s cast, ???
    ArcaneBlastVisual = 41171, // Boss->self, 4.0+1,0s cast, single-target, raidwide
    ArcaneBlast = 41174, // Helper->self, 5.0s cast, ???
    VoidThunderIII = 41172, // Boss->player, 5.0s cast, single-target, tankbuster

    FireTrap = 41250, // JestingJackanapes->self, 4.0s cast, range 8 circle
    BlizzardTrap = 41251, // JestingJackanapes->self, 4.0s cast, range 8 circle

    TentacleVisual = 41255, // Tentacle->self, no cast, single-target

    Wallop1 = 41257, // Tentacle->self, 11.0s cast, range 60 width 20 rect
    Wallop2 = 41314, // Tentacle->self, 7.0s cast, range 60 width 10 rect
    Wallop3 = 41256, // Tentacle->self, 11.0s cast, range 60 width 10 rect

    SurpriseAttack = 41254, // Helper->location, 12.0s cast, width 6 rect charge
    SurpriseAttackTeleport1 = 41252, // JestingJackanapes->location, 14.0s cast, single-target
    SurpriseAttackTeleport2 = 41253 // JestingJackanapes->location, no cast, single-target
}

public enum SID : uint
{
    PlayingWithIce = 4212, // none->player, extra=0x0
    PlayingWithFire = 4211, // none->player, extra=0x0
    ImpElement = 2193 // none->JestingJackanapes, extra=0x344/0x345, 344 fire, 345 ice
}

sealed class WallopNarrow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Wallop2, (uint)AID.Wallop3], new AOEShapeRect(60f, 5f));
sealed class WallopWide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop1, new AOEShapeRect(60f, 10f));
sealed class SurpriseAttack(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SurpriseAttack, 3f);
sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeCone(65f, 45f.Degrees()));
sealed class MindBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.MindBlast);
sealed class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcaneBlast);
sealed class VoidThunderIII(BossModule module) : Components.SingleTargetCast(module, (uint)AID.VoidThunderIII);

sealed class FireIceTrap(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4), _firePlayers = new(6), _icePlayers = new(6);
    private static readonly AOEShapeCircle circleTrap = new(8f), circleBig = new(38f); // seduced lasts 5s * 6y/s - traps activate 0.3s-1s before seduced ends, but who cares
    private readonly List<(WPos position, bool isFire)> jesters = new(4);
    private bool first = true;
    private BitMask hasFireElement;
    private BitMask hasIceElement;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (hasFireElement[slot])
        {
            return CollectionsMarshal.AsSpan(_firePlayers);
        }
        if (hasIceElement[slot])
        {
            return CollectionsMarshal.AsSpan(_icePlayers);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ImpElement:
                if (actor.OID == (uint)OID.JestingJackanapes)
                {
                    var position = actor.Position;
                    var isFire = status.Extra == 0x344;
                    if (first)
                    {
                        AddTraps(position.Quantized(), WorldState.FutureTime(10d), isFire);
                    }
                    jesters.Add((position, isFire));
                }
                break;
            case (uint)SID.PlayingWithFire:
                hasFireElement.Set(Raid.FindSlot(actor.InstanceID));
                activation = status.ExpireAt;
                break;
            case (uint)SID.PlayingWithIce:
                hasIceElement.Set(Raid.FindSlot(actor.InstanceID));
                activation = status.ExpireAt;
                break;
        }
    }

    private void AddTraps(WPos position, DateTime activation, bool isFire)
    {
        AddAOE(_icePlayers, circleTrap);
        AddAOE(_firePlayers, circleTrap);
        AddAOE(_aoes, circleTrap);
        AddAOE(isFire ? _firePlayers : _icePlayers, circleBig);
        void AddAOE(List<AOEInstance> aoes, AOEShapeCircle shape) => aoes.Add(new(shape, position, default, activation));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PlayingWithFire:
                hasFireElement.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.PlayingWithIce:
                hasIceElement.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SurpriseAttack)
        {
            var imps = CollectionsMarshal.AsSpan(jesters);
            var len = imps.Length;
            var pos = caster.Position;
            for (var i = 0; i < len; ++i)
            {
                ref var jester = ref imps[i];
                if (jester.position.AlmostEqual(pos, 1f))
                {
                    jester.position = spell.LocXZ;
                    if (++NumCasts > 4)
                    {
                        AddTraps(spell.LocXZ, activation, jester.isFire);
                    }
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireTrap or (uint)AID.BlizzardTrap)
        {
            _aoes.Clear();
            _icePlayers.Clear();
            _firePlayers.Clear();
            jesters.Clear();
            NumCasts = 0;
            first = false;
        }
    }
}

sealed class CE113ScourgeOfMindStates : StateMachineBuilder
{
    public CE113ScourgeOfMindStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WallopNarrow>()
            .ActivateOnEnter<WallopWide>()
            .ActivateOnEnter<SurpriseAttack>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<MindBlast>()
            .ActivateOnEnter<ArcaneBlast>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<FireIceTrap>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 33)]
public sealed class CE113ScourgeOfMind(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(300f, 730f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
