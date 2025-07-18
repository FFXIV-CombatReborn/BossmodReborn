﻿namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class MadeMagic(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MadeMagic1, (uint)AID.MadeMagic2], new AOEShapeRect(50f, 15f));
class ScreamingScore(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ScreamingScoreP1, (uint)AID.ScreamingScoreP2]);
class ScatteredMagic(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScatteredMagic, 4);
class DarkerNote(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkerNote, 6);

class UnevenFooting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnevenFooting, new AOEShapeRect(80, 15));
class Crash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crash, new AOEShapeRect(50, 5));
class HeavyArms1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyArms1, new AOEShapeRect(44, 50));
class HeavyArms3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyArms2, new AOEShapeRect(100, 6));
class PlaceOfPower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PlaceOfPower, 6);
class ShockwaveKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShockwaveKB, 35f);
class ShockwaveAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockwaveAOE, 7);
class Towerfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Towerfall, new AOEShapeRect(70, 7));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948, SortOrder = 6)]
public class A35FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700f, -700f), new ArenaBoundsSquare(24.5f))
{
    public Actor? BossBossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossBossP2 == null)
        {
            var b = Enemies((uint)OID.BossP2);
            BossBossP2 = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossBossP2);
    }

    // p2mid: 20 verts, 6 radius
    // towers: 20 verts, 7 radius
}
