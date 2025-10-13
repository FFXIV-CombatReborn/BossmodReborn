namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class Roar(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Roar1, (uint)AID.Roar2, (uint)AID.Roar3]);
sealed class ForgedFury(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ForgedFury1, (uint)AID.ForgedFury2, (uint)AID.ForgedFury3]);
sealed class WhiteFlash(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.WhiteFlash, 6f, 4, 4);
sealed class Dragonspark(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Dragonspark, 6f, 4, 4);
sealed class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WildEnergy, 6f);
sealed class WyvernsRadianceGuardianResonanceCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsRadianceCircle, (uint)AID.GuardianResonanceAOE], 6f);
sealed class WyvernsRadianceChainbladeCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WyvernsRadianceChainbladeCharge, 12f);
sealed class WyvernsOuroblade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsOuroblade1, (uint)AID.WyvernsOuroblade2,
(uint)AID.WyvernsOuroblade3, (uint)AID.WyvernsOuroblade4], new AOEShapeCone(40f, 90f.Degrees()));
sealed class SteeltailThrust(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SteeltailThrust1, (uint)AID.SteeltailThrust2], new AOEShapeRect(60f, 3f));
sealed class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ChainbladeCharge, (uint)AID.ChainbladeCharge, 6f, 8.4d, PartyState.MaxPartySize, PartyState.MaxPartySize);

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(Ex6GuardianArkveldStates),
ConfigType = typeof(Ex6GuardianArkveldConfig),
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.GuardianArkveld,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Extreme,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1044u,
NameID = 14237u,
SortOrder = 1,
PlanLevel = 100)]
public sealed class Ex6GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsCustom([new Polygon(arenaCenter, 20.030838f, 40)]))
{
    private static readonly WPos arenaCenter = new(100f, 100f);
}
