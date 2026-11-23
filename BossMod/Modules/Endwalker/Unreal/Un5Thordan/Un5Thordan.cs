namespace BossMod.Endwalker.Unreal.Un5Thordan;

sealed class AscalonsMight(BossModule module) : Components.Cleave(module, (uint)AID.AscalonsMight, new AOEShapeCone(8 + 3.8f, 45.Degrees()));
sealed class Meteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorainAOE, 6);
sealed class AscalonsMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercy, new AOEShapeCone(34.8f, 10.Degrees()));
sealed class AscalonsMercyHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercyAOE, new AOEShapeCone(34.5f, 10.Degrees()));
sealed class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DragonsRage, 6, 6);
sealed class Heavensflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensflameAOE, 6);
sealed class Conviction(BossModule module) : Components.CastTowers(module, (uint)AID.ConvictionAOE, 3);
sealed class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.HolyChain);
sealed class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.Zephirin);
sealed class LightOfAscalon(BossModule module) : Components.CastCounter(module, (uint)AID.LightOfAscalon); // TODO: show knockback 3 from [-0.8, -16.3]
sealed class UltimateEnd(BossModule module) : Components.CastCounter(module, (uint)AID.UltimateEndAOE);
sealed class HeavenswardLeap(BossModule module) : Components.CastCounter(module, (uint)AID.HeavenswardLeap);
sealed class PureOfSoul(BossModule module) : Components.CastCounter(module, (uint)AID.PureOfSoul);
sealed class AbsoluteConviction(BossModule module) : Components.CastCounter(module, (uint)AID.AbsoluteConviction);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "ported from upstream", GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 963, NameID = 3632, PlanLevel = 90)]
public sealed class Un5Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21));
