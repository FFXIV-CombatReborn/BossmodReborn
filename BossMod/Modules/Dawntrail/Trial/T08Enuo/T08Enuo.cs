namespace BossMod.Dawntrail.Trial.T08Enuo;

public enum OID : uint
{
    Enuo = 0x4DB9,
    YawningVoid = 0x4DBA,
    Helper = 0x233C,
    _Gen_ = 0x4DB8, // R5.000, x2
    _Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    NaughtHuntChaser = 0x4EB4, // R0.850, x0 (spawn during fight), Helper type : Endless chase caster
    _Gen_Void1 = 0x4DBB, // R0.850, x0 (spawn during fight), Helper type :
    BeaconInTheDark = 0x4DBE,
    UncastShadow = 0x4DBD, // R5.000, x0 (spawn during fight)
    LoomingShadow = 0x4DBC, // R12.500, x0 (spawn during fight)
    Zero = 0x4DBF,
    Golbez = 0x4DC0,
}

public enum AID : uint
{
    _AutoAttack_ = 49936, // Enuo->player, no cast, single-target
    Meteorain = 49971, // Enuo->self, 5.0s cast, range 40 circle, raidwide
    _Ability_ = 49927, // Enuo->location, no cast, single-target
    NaughtGrowsCastVisual = 49931, // Enuo->self, no cast, single-target
    NaughtGrowsAOE = 49933, // 4DBA (Yawning Void)->self, 6.7s cast, range 40 circle
    NaughtWakes = 49929, // Enuo->self, no cast, single-target melee hit
    _Weaponskill_ = 49930, // YawningVoid->location, no cast, single-target
    GazeOfTheVoidVisual = 49950, // Enuo->self, 7.0+1.0s cast, single-target
    GazeOfTheVoid1 = 49952, // Helper->self, 8.0s cast, single-target
    GazeOfTheVoidCones = 49953, // Helper->self, 8.0s cast, range 40 ?-degree cone
    DeepFreezeCast = 49965, // Enuo->self, 5.0+1.0s cast, single-target
    DeepFreeze = 49966, // Helper->players, 6.0s cast, range 40 circle
    NaughtHunts = 49939, // Enuo->self, 6.0+1.0s cast, single-target
    EndlessChaseFirst = 48474, // _Gen_Void->self, 6.0s cast, range 6 circle : chasing aoe
    EndlessChaseRest = 49940, // _Gen_Void->location, no cast, range 6 circle
    NaughtHuntsAnother = 49941, // Enuo->self, 5.0+1.0s cast, single-target
    ShroudedHolyVisual = 49967, // Enuo->self, 4.0+1.0s cast, single-target
    ShroudedHolyStack = 49968, // Helper->players, 5.0s cast, range 6 circle
    MeltdownVisual = 49962, // Enuo->self, 4.0+1.0s cast, single-target
    MeltdownAOE = 49963, // Helper->location, 5.0s cast, range 5 circle
    MeltdownSpread = 49964, // Helper->players, 5.0s cast, range 5 circle
    VacuumSingleTargetVisual = 49942, // Enuo->self, 2.0+1.0s cast, single-target
    // Start points for these vacuum arcs come from the outside and terminate at the center
    SilentTorrentVisual1 = 49944, // _Gen_Void1->location, 3.5s cast, single-target Vacuum arc
    SilentTorrentVisual2 = 49943, // _Gen_Void1->location, 3.5s cast, single-target Vacuum arc
    SilentTorrentVisual = 49945, // _Gen_Void1->location, 3.5s cast, single-target Vacuum arc
    SilentTorrentMedium = 49947, // Helper->self, 4.0s cast, range ?-19 donut vacuum -medium arc x 4 (nw, sw, se, ne)
    SilentTorrentLarge = 49948, // Helper->self, 4.0s cast, range ?-19 donut vacuum  - shortest arc x  (n)
    SilentTorrentSmall = 49946, // Helper->self, 4.0s cast, range ?-19 donut vacuum - longest arc x 3 (,s,w, e)
    VacuumCircle = 49949, // _Gen_Void1->self, 1.5s cast, range 7 circle
    AllForNaughtVisual = 49954, // Enuo->self, 5.0s cast, single-target
    LoomingEmptinessVisual = 49955, // _Gen_LoomingShadow->self, 6.0s cast, single-target
    LoomingEmptinessAOE = 49981, // Helper->self, 7.0s cast, range 100 circle
    EmptyShadowVisual = 49956, // _Gen_UncastShadow->self, 7.0s cast, single-target
    _AutoAttack_1 = 49957, // _Gen_LoomingShadow->player, no cast, single-target
    EmptyShadowAOE = 50667, // Helper->self, 8.0s cast, range 10 circle
    Nothingness = 49958, // _Gen_UncastShadow->self, 3.0s cast, range 100 width 4 rect
    _Weaponskill_1 = 49938, // _Gen_UncastShadow/_Gen_LoomingShadow->self, no cast, single-target
    LightlessWorld = 49959, // Enuo->self, 5.0s cast, single-target
    LightlessWorld1 = 49960, // Helper->self, no cast, range 40 circle
    LightlessWorldAOE = 49961, // Helper->self, no cast, range 40 circle
    _AutoAttack_2 = 50775, // 4DC0->Enuo, no cast, single-target
    _AutoAttack_Attack = 870, // 4DBF->Enuo, no cast, single-target
    SpellFlame = 50796, // 4DC0->Enuo, no cast, range 25 width 4 rect
    ShineBraver = 50505, // 4DBF->Enuo, no cast, range 5 circle
    WarlocksTide = 50776, // 4DC0->Enuo, no cast, single-target
    LightSlash = 50777, // 4DBF->Enuo, no cast, single-target
    PaladinForce = 50798, // 4DBF->Enuo, no cast, range 5 circle
    NaughtGrows = 49932, // Enuo->self, 6.0+1.0s cast, single-target
    NaughtGrowsAOESmall = 49934, // Helper->self, 7.0s cast, range 12 circle
    ShieldSlam = 50797, // 4DBF->Enuo, no cast, single-target
    SacredSword = 50778, // 4DBF->Enuo, no cast, single-target
    AuthorityOfTheAnointed = 50779, // 4DBF->Enuo, no cast, single-target
    Almagest = 49928, // Enuo->self, 5.0s cast, range 40 circle
    DimensionZeroRect = 49969, // Enuo->self, 5.0s cast, single-target
    DimensionZeroMarker = 49970, // Enuo->self, no cast, range 60 width 8 rect
    GazeOfTheVoid2 = 49951, // Helper->self, 8.0s cast, single-target
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 1789, // YawningVoid/_Gen_Void1/_Gen_UncastShadow/Helper->player, extra=0x1/0x2
    _Gen_DirectionalDisregard = 3808, // none->Enuo, extra=0x0
    _Gen_2056 = 2056, // none->_Gen_UncastShadow/_Gen_LoomingShadow, extra=0x46B
    _Gen_Unbecoming = 4882, // none->player, extra=0x0
    _Gen_LightVision = 5343, // none->player, extra=0x28
    _Gen_InEvent = 1268, // none->player, extra=0x0
    _Gen_Preoccupied = 1619, // none->player, extra=0x0
    _Gen_2552 = 2552, // none->Enuo/4DBF, extra=0x487/0x488/0x489
    _Gen_VulnerabilityDown = 5467, // none->player/4DC0/4DBF, extra=0x0
    _Gen_2160 = 2160, // none->4DC0/4DBF, extra=0xC98/0x3152

}

public enum IconID : uint
{
    DeepFreezeFlareIcon = 327, // player->self triangle bait away candidate deep freeze
    EndlessChaseIcon = 172, // player->self : endless chase
    ShroudedHolyStackIcon = 161, // player->self
    MeltdownSpreadIcon = 558, // player->self
    DimensionZeroIcon = 719, // Enuo->player : Line Stack Icon?
}

public enum TetherID : uint
{
    _Gen_Tether_chn_z5fd07_0a1 = 391, // _Gen_->Enuo
    NaughtHuntsTether = 404, // _Gen_Void->player : Naught hunts tether
    NaughtHuntsAnotherTether = 405, // player->player : Naught hunts another tether with arrows on it.
    _Gen_Tether_chn_tergetfix1f = 284, // _Gen_LoomingShadow->player
    GolbezZeroTether = 425, // 4DC0->4DBF
    _Gen_Tether_chn_z5fd08_0a1 = 392, // _Gen_->Enuo
}

class Meteorain(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteorain);

sealed class NaughtGrowsAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsAOE, new AOEShapeCircle(40f));

// cone degree is an estimate.
sealed class GazeOfTheVoidCones(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GazeOfTheVoidCones, (uint)AID.GazeOfTheVoid1], new AOEShapeCone(20f, 22.5f.Degrees()), 7, 10);

// Flare marker : tank buster magic damage
sealed class DeepFreeze(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DeepFreeze, new AOEShapeCircle(8), true, true);

//Naughthunts
/** The solution credit here goes to @Endings.  Any mistakes in implementation from
 * here belong to @wen.
*/
sealed class NaughtHunts(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6f), (uint)AID.EndlessChaseRest, (uint)AID.EndlessChaseRest, 2.9f, 0.7d, 13, icon: (uint)IconID.EndlessChaseIcon, activationDelay: 6d)
{
    private int _tethercount = 0;
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.NaughtHuntsTether)
        {
            var p = WorldState.Actors.Find(tether.Target);
            if (_tethercount == 2)
            {
                Chasers.Clear();
            }
            if (p != null)
            {
                Chasers.Add(new(Shape, p, source.Position, 0, MaxCasts, WorldState.FutureTime(ActivationDelay), SecondsBetweenActivations));
                ++_tethercount;
            }
            if (_tethercount == 4)
            {
                _tethercount = 0;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.NaughtHuntChaser)
        {
            Chasers.Clear(); // This'll happen twice per cycle but who cares?
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // Overriding to remove the 'add' behaviour here, chasers are added from tethers instead.
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) // Don't count the first action so that we can have the same count between the two.
    {
        if (spell.Action.ID is var id && id == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position.Quantized() : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetTargets)
            {
                Targets.Clear();
                NumCasts = 0;
            }
        }
    }
}

//enshrouded holy - stack marker
sealed class ShroudedHoly(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShroudedHolyStack, 7, 4);

sealed class MeltdownSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MeltdownSpread, 5f);
sealed class MeltdownAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeltdownAOE, new AOEShapeCircle(5f));

sealed class SilentTorrentSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentSmall, new AOEShapeDonutSector(17f, 19f, 10f.Degrees()));
sealed class SilentTorrentMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentMedium, new AOEShapeDonutSector(17f, 19f, 20f.Degrees()));
sealed class SilentTorrentLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentLarge, new AOEShapeDonutSector(17f, 19f, 30f.Degrees()));

sealed class VacuumCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumCircle, new AOEShapeCircle(7f));

class ArenaSwitcher : BossComponent
{
    public Actor? Beacon;

    public ArenaSwitcher(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public bool IntermissionOver => Beacon is { IsDead: true };

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeaconInTheDark)
            Beacon = actor;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state == 0x00020001)
            // intermission arena is actually a 500x500 square
            // square is huge so as not to run off the radar
            Arena.Bounds = new ArenaBoundsSquare(80);

        if (index == 0 && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(20);
    }
}

// Void arena actors
class LoomingShadow(BossModule module) : Components.Adds(module, (uint)OID.LoomingShadow);
class Shadows(BossModule module) : Components.Adds(module, (uint)OID.UncastShadow, 1);
class Beacon(BossModule module) : Components.Adds(module, (uint)OID.BeaconInTheDark, 1, true);

class EmptyShadow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyShadowAOE, new AOEShapeCircle(10f));
// big explosion For when Looming emptiness spawns. Went a little small: proximity AOE
class LoomingEmptinessAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LoomingEmptinessAOE, 42f);

class Nothingness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nothingness, new AOEShapeRect(100f, 2f));

// back to circle arena

sealed class LightlessWorld(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.LightlessWorld, 40f, false)
{
    // Use this to find Zero and make a blocker to hide behind
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.Zero));
}

//almagest
class Almagest(BossModule module) : Components.RaidwideCast(module, (uint)AID.Almagest);

sealed class NaughtGrows(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsAOESmall, 12f);

sealed class DimensionZero(BossModule module) : Components.LineStack(module, iconID: (uint)IconID.DimensionZeroIcon, (uint)AID.DimensionZeroMarker, 5d, 60f, 4f, 8, PartyState.MaxPartySize, 3);

sealed class EnuoStates : StateMachineBuilder
{
    public EnuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsAOE>()
            .ActivateOnEnter<GazeOfTheVoidCones>()
            .ActivateOnEnter<DeepFreeze>()
            .ActivateOnEnter<NaughtHunts>()
            .ActivateOnEnter<ShroudedHoly>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownAOE>()
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<VacuumCircle>()

            // Arena change
            .ActivateOnEnter<ArenaSwitcher>()

            // Shadows area
            // actors
            .ActivateOnEnter<LoomingShadow>()
            .ActivateOnEnter<Shadows>()
            .ActivateOnEnter<Beacon>()
            // spells
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<LoomingEmptinessAOE>()
            .ActivateOnEnter<Nothingness>()

            //back to circle arena
            .ActivateOnEnter<LightlessWorld>()
            .ActivateOnEnter<NaughtGrows>()
            .ActivateOnEnter<Almagest>()
            .ActivateOnEnter<DimensionZero>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(EnuoStates),
ConfigType = null, // replace null with typeof(EnuoConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null, // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Enuo,
Contributors = "Wen",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Trial,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1115u,
NameID = 14749u,
SortOrder = 1,
PlanLevel = 0)]
public sealed class Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
