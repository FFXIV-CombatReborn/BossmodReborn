namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE24PatriotGames;

public enum OID : uint
{
    Boss = 0x2E14, // R4.5
    MagitekBartizan = 0x2F02, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 20456, // Boss->player, no cast, single-target

    PlasmaGun = 20451, // Boss->player, 4.0s cast, single-target
    MagitekBartizan = 20441, // Boss->self, 5.0s cast, single-target
    ElectrochemicalTransfer = 20442, // Boss->self, 7.0s cast, single-target
    ElectrochemicalReaction = 20443, // MagitekBartizan->self, 1.0s cast, range 50 width 25 rect
    LightningRodVisual = 20447, // Boss->self, 5.0s cast, single-target
    LightningRod = 20448, // Helper->self, no cast, single-target, mine spawn
    Explosion = 20449, // Helper->self, no cast, range 8 circle
    MassiveExplosion = 20450, // Helper->self, no cast, range 40 circle, mine fail

    FiringOrders = 20445, // Boss->self, 5.0s cast, single-target
    Neutralization = 20444, // Boss->self, 4.0s cast, range 30 120-degree cone
    OrderedFire = 20446, // Helper->location, 8.0s cast, range 10 circle
    ElectrifyingConductionVisual = 20452, // Boss->self, 4.0s cast, single-target
    ElectrifyingConduction = 20453, // Helper->self, 4.0s cast, range 40 circle
    SearingConductionVisual = 20454, // Boss->self, 3.0s cast, single-target
    SearingConduction = 20455 // Helper->self, 3.0s cast, range 15 circle
}

public enum TetherID : uint
{
    Laser1 = 114, // MagitekBartizan->Boss
    Laser2 = 115 // MagitekBartizan->Boss
}

sealed class PlasmaGun(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PlasmaGun);
sealed class Neutralization(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Neutralization, new AOEShapeCone(30f, 60f.Degrees()));
sealed class OrderedFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OrderedFire, 10f);
sealed class SearingConduction(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SearingConduction, 15f);
sealed class ElectrifyingConduction(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ElectrifyingConduction);

sealed class LightningRod(BossModule module) : Components.GenericTowers(module)
{
    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        Span<Tower> updated = new Tower[len];

        for (var i = 0; i < len; ++i)
        {
            ref readonly var tower = ref towers[i];
            updated[i] = tower.ForbiddenSoakers[slot] ? new(tower.Position, 8f, forbiddenSoakers: tower.ForbiddenSoakers) : tower;
        }
        return updated;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningRod)
        {
            Towers.Add(new(caster.Position.Quantized(), 4f)); // no activation time because player might need to do multiple mines within 12s
        }
        else if (spell.Action.ID is (uint)AID.Explosion or (uint)AID.MassiveExplosion)
        {
            var count = Towers.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (Towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var party = Module.Raid.WithSlot(true, true, true);
        var len = party.Length;
        BitMask forbidden = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (p.Item2.Role != Role.Tank && p.Item2.HPMP.CurHP < 40000u) // might lead to tanks sacrificing themselves, but oh well...
            {
                forbidden.Set(p.Item1);
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.ForbiddenSoakers = forbidden;
        }
    }
}

sealed class ElectrochemicalReaction(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private static readonly AOEShapeRect rect = new(50f, 12.5f);
    private readonly List<(WPos position, int charges)> bartizans = [with(4)];
    private readonly LightningRod _towers = module.FindComponent<LightningRod>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.Laser1 or (uint)TetherID.Laser2)
        {
            var count = bartizans.Count;
            if (count != 4)
            {
                var loc = source.Position;
                for (var i = 0; i < count; ++i)
                {
                    var laser = bartizans[i];
                    if (laser.position.AlmostEqual(loc, 1f))
                    {
                        goto skip; // each bartizan got 2 actors, don't add duplicates
                    }
                }
                bartizans.Add((loc.Quantized(), default));
            }
        skip:
            var pos = source.Position;
            var lasers = CollectionsMarshal.AsSpan(bartizans);
            var len = lasers.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var laser = ref lasers[i];
                if (laser.position.AlmostEqual(pos, 1f))
                {
                    ++laser.charges;
                    if (laser.charges == 2)
                    {
                        _aoes.Add(new(rect, laser.position, source.Rotation, WorldState.FutureTime(10.2d)));
                        laser.charges = default;
                    }
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrochemicalReaction)
        {
            _aoes.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towers.Towers.Count != 0)
        {
            if (!_towers.Towers[0].ForbiddenSoakers[slot])
            {
                return;
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class CE24PatriotGamesStates : StateMachineBuilder
{
    public CE24PatriotGamesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PlasmaGun>()
            .ActivateOnEnter<Neutralization>()
            .ActivateOnEnter<OrderedFire>()
            .ActivateOnEnter<SearingConduction>()
            .ActivateOnEnter<ElectrifyingConduction>()
            .ActivateOnEnter<LightningRod>()
            .ActivateOnEnter<ElectrochemicalReaction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 10)]
public sealed class CE24PatriotGames(WorldState ws, Actor primary) : BossModule(ws, primary, new(-239.999f, 413.999f), new ArenaBoundsSquare(24.5f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 25f);
}
