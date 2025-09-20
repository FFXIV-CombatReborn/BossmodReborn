namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarKelpie;

public enum OID : uint
{
    Boss = 0x2537, //R=5.4
    Hydrosphere = 0x255B, //R=1.2
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // Boss/AltarMatanga->player, no cast, single-target

    Torpedo = 13438, // Boss->player, 3.0s cast, single-target
    Innocence = 13439, // Boss->location, 3.0s cast, range 5 circle
    Gallop = 13441, // Boss->location, no cast, ???, movement ability
    RisingSeas = 13440, // Boss->self, 5.0s cast, range 50+R circle, knockback 20, away from source
    BloodyPuddle = 13443, // Hydrosphere->self, 4.0s cast, range 10+R circle
    HydroPush = 13442, // Boss->self, 6.0s cast, range 44+R width 44 rect, knockback 20, dir forward

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    Telega = 9630 // AltarMatanga/Mandragoras->self, no cast, single-target, bonus adds disappear
}

class Innocence(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Innocence, 5f);
class HydroPush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HydroPush, new AOEShapeRect(49.4f, 22f));

class BloodyPuddle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(11.2f);
    public readonly List<AOEInstance> AOEs = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Hydrosphere)
            AOEs.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(8.6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.BloodyPuddle)
            AOEs.Clear();
    }
}

class Torpedo(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Torpedo);
class RisingSeas(BossModule module) : Components.RaidwideCast(module, (uint)AID.RisingSeas);

class RisingSeasKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RisingSeas, 20f, stopAtWall: true)
{
    private readonly BloodyPuddle _aoe = module.FindComponent<BloodyPuddle>()!;
    private static readonly Angle cone = 37.5f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var count = _aoe.AOEs.Count;
            var forbidden = new ShapeDistance[count];
            var center = Arena.Center;
            var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = new SDCone(center, 20f, Angle.FromDirection(aoe.Origin - center), cone);
            }
            if (forbidden.Length != 0)
                hints.AddForbiddenZone(new SDUnion(forbidden), Casters.Ref(0).Activation);
        }
    }
}

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class AltarKelpieStates : StateMachineBuilder
{
    public AltarKelpieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<HydroPush>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<Torpedo>()
            .ActivateOnEnter<RisingSeas>()
            .ActivateOnEnter<RisingSeasKB>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarKelpie.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7589)]
public class AltarKelpie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.AltarMatanga, (uint)OID.GoldWhisker];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
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
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen or (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
