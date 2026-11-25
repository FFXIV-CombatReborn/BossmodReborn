using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.MSQ.DeathUntoDawn.P3;

public enum OID : uint
{
    Boss = 0x3201,
    Helper = 0x233C,
    MoonGana = 0x3219,
    SpiritGana = 0x321A,
    RavanasWill = 0x321B,
}

public enum AID : uint
{
    Explosion = 24046 // 3204->self, 5.0s cast, range 80 width 10 cross
}

public enum SID : uint
{
    Invincibility = 325
}

class GrahaAI(BossModule module, WorldState ws) : QuestBattle.UnmanagedRotation(ws, 25f)
{
    private static readonly uint[] _adds = [(uint)OID.MoonGana, (uint)OID.SpiritGana, (uint)OID.RavanasWill];

    // Ravana's Wills just move to boss, whereas butterflies are only a threat once they start casting
    private bool ShouldBreak(Actor a) => StatusDetails(a, (uint)Roleplay.SID.Break, Player.InstanceID).Left == 0 && (a.OID == (uint)OID.RavanasWill || a.CastInfo != null);

    protected override void Exec(Actor? primaryTarget)
    {
        var alladds = module.Enemies(_adds);
        var count = alladds.Count;
        var adds = new List<Actor>(count);

        for (var i = 0; i < count; ++i)
        {
            var a = adds[i];
            if (a.IsTargetable && !a.IsDead)
            {
                adds.Add(a);
                continue;
            }
            if (ShouldBreak(a) && a.Position.InCircle(Player.Position, 20f))
                UseAction(RID.Break, Player);
        }

        Hints.GoalZones.Add(pos =>
           {
               var count = 0;
               for (var i = 0; i < count; ++i)
               {
                   if (adds[i].Position.InCircle(pos, 20f))
                       ++count;
               }
               return count;
           });

        if (MP >= 1000 && Player.HPMP.CurHP * 3 < Player.HPMP.MaxHP)
            UseAction(RID.CureII, Player);

        if (MP < 800)
            UseAction(RID.AllaganBlizzardIV, primaryTarget);

        if (primaryTarget?.OID == (uint)OID.Boss)
        {
            var thunder = StatusDetails(primaryTarget, Roleplay.SID.ThunderIV, Player.InstanceID);
            if (thunder.Left < 3f)
                UseAction(RID.ThunderIV, primaryTarget);
        }

        switch (ComboAction)
        {
            case RID.FireIV:
                UseAction(RID.FireIV2, primaryTarget);
                break;
            case RID.FireIV2:
                UseAction(RID.FireIV3, primaryTarget);
                break;
            case RID.FireIV3:
                UseAction(RID.Foul, primaryTarget);
                break;
            default:
                UseAction(RID.FireIV, primaryTarget);
                break;
        }
    }
}

class AutoGraha(BossModule module) : QuestBattle.RotationModule<GrahaAI>(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            if (h.Actor.FindStatus((uint)SID.Invincibility) != null)
                h.Priority = AIHints.Enemy.PriorityInvincible;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}
class DirectionalParry(BossModule module) : Components.DirectionalParry(module, [0x3201]);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeCross(80f, 5f), maxCasts: 2);

class LunarRavanaStates : StateMachineBuilder
{
    public LunarRavanaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoGraha>()
            .ActivateOnEnter<DirectionalParry>()
            .ActivateOnEnter<Explosion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10037)]
public class LunarRavana(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144f, 83f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
    protected override bool CheckPull() => Raid.Player()?.InCombat == true;
}
