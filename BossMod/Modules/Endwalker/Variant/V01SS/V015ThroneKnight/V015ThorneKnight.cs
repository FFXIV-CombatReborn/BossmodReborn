﻿namespace BossMod.Endwalker.Variant.V01SS.V015ThorneKnight;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11419)]
public class V015ThorneKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsRect(new(289, -230), 20, 20, 45.Degrees()));