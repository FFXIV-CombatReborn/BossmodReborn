namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretCladoselache;

public enum OID : uint
{
    SecretCladoselache = 0x3027, //R=2.47
    SecretShark = 0x3028, //R=3.0 
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // SecretShark->player, no cast, single-target
    AutoAttack2 = 872, // SecretCladoselache->player, no cast, single-target

    TidalGuillotine = 21704, // SecretCladoselache->self, 4.0s cast, range 13 circle
    ProtolithicPuncture = 21703, // SecretCladoselache->player, 4.0s cast, single-target

    PelagicCleaver = 21705, // SecretCladoselache->self, 3.5s cast, range 40 60-degree cone
    PelagicCleaverFirst = 21706, // SecretCladoselache->self, 5.0s cast, range 40 60-degree cone
    PelagicCleaverRest = 21707, // SecretCladoselache->self, no cast, range 40 60-degree cone

    BiteAndRun = 21709, // SecretShark->player, 5.0s cast, width 5 rect charge
    AquaticLance = 21708, // SecretCladoselache->player, 5.0s cast, range 8 circle

    Telega = 9630, // KeeperOfKeys->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

sealed class PelagicCleaverRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -60f.Degrees(),
            (uint)IconID.RotateCCW => 60f.Degrees(),
            _ => default
        };
        InitIfReady(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PelagicCleaverFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PelagicCleaverFirst or (uint)AID.PelagicCleaverRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(cone, source.Position.Quantized(), _rotation, _increment, _activation, 2.1d, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

sealed class PelagicCleaver(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PelagicCleaver, new AOEShapeCone(40f, 30f.Degrees()));
sealed class TidalGuillotine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TidalGuillotine, 13f);
sealed class ProtolithicPuncture(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ProtolithicPuncture);
sealed class BiteAndRun(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.BiteAndRun, 2.5f);
sealed class AquaticLance(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AquaticLance, 8f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

sealed class SecretCladoselacheStates : StateMachineBuilder
{
    public SecretCladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<PelagicCleaverRotation>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<BiteAndRun>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretCladoselache.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(SecretCladoselacheStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.SecretCladoselache,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9778u,
SortOrder = 4,
PlanLevel = 0)]
public sealed class SecretCladoselache : THTemplate
{
    public SecretCladoselache(WorldState ws, Actor primary) : base(ws, primary)
    {
        sharks = Enemies((uint)OID.SecretShark);
    }
    private readonly List<Actor> sharks;
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.SecretCladoselache, (uint)OID.SecretShark, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(sharks);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretShark => 1,
                _ => 0
            };
        }
    }
}
