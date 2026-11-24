using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.MSQ.DeathUntoDawn.P2;

public enum OID : uint
{
    Boss = 0x3200,
    Fetters = 0x3218
}

public enum AID : uint
{
    LunarGungnir = 24025, // LunarOdin->31EC, 12.0s cast, range 6 circle
    LunarGungnir1 = 24026, // LunarOdin->2E2E, 25.0s cast, range 6 circle
    GungnirAOE = 24698, // 233C->self, 10.0s cast, range 10 circle
    Gagnrath = 24030, // 321C->self, 3.0s cast, range 50 width 4 rect
    GungnirSpread = 24029, // 321C->self, no cast, range 10 circle
    LeftZantetsuken = 24034, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
    RightZantetsuken = 24032, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
}

class UriangerAI(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 25f)
{
    public const ushort StatusParam = 158;

    private float HeliosLeft(Actor p) => p.IsTargetable ? StatusDetails(p, 836, Player.InstanceID).Left : float.MaxValue;

    protected override void Exec(Actor? primaryTarget)
    {
        var party = World.Party.WithoutSlot(false, true);
        var len = party.Length;
        var positions = new WPos[len];
        for (var i = 0; i < len; ++i)
        {
            positions[i] = party[i].Position;
        }

        Hints.GoalZones.Add(pos =>
           {
               var count = 0;
               for (var i = 0; i < len; ++i)
               {
                   ref readonly var p = ref positions[i];
                   if (p.InCircle(pos, 16f))
                       ++count;
               }
               return count;
           });

        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (!(HeliosLeft(p) < 1 && p.Position.InCircle(Player.Position, 15.5f + p.HitboxRadius)))
            {
                UseAction(RID.AspectedHelios, Player);
                break;
            }
        }

        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p.HPMP.CurHP < p.HPMP.MaxHP * 0.4f)
            {
                UseAction(RID.Benefic, p);
                break;
            }
        }

        UseAction(RID.MaleficIII, primaryTarget);

        if (Player.FindStatus((uint)Roleplay.SID.DestinyDrawn) != null)
        {
            if (ComboAction == RID.DestinyDrawn)
                UseAction(RID.LordOfCrowns, primaryTarget, -100f);

            if (ComboAction == RID.DestinysSleeve)
                UseAction(RID.TheScroll, Player, -100f);
        }
        else
        {
            UseAction(RID.DestinyDrawn, Player, -100f);
            UseAction(RID.DestinysSleeve, Player, -100f);
        }

        UseAction(RID.FixedSign, Player, -150);
    }
}

class AutoUrianger(BossModule module) : QuestBattle.RotationModule<UriangerAI>(module);
class Fetters(BossModule module) : Components.Adds(module, (uint)OID.Fetters);

class GunmetalSoul(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(4f, 100f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var enemies = Module.Enemies(0x1EB1D5);
        var count = 0;
        var countE = enemies.Count;
        for (var i = 0; i < countE; ++i)
        {
            if (enemies[i].EventState != 7)
                ++count;
        }

        if (count == 0)
            return [];

        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;

        for (var i = 0; i < countE; ++i)
        {
            var e = enemies[i];
            if (e.EventState != 7)
                aoes[index++] = new(donut, e.Position);
        }
        return aoes.AsSpan(0, aoesLen);
    }
}
class LunarGungnir1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.LunarGungnir, 6f);
class LunarGungnir2(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.LunarGungnir1, 6f);
class Gungnir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GungnirAOE, 10f);
class Gagnrath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gagnrath, new AOEShapeRect(50f, 2f));
class GungnirSpread(BossModule module) : Components.BaitAwayIcon(module, 10f, 189u, (uint)AID.GungnirSpread, 5.3f);
class Zantetsuken(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightZantetsuken, (uint)AID.LeftZantetsuken], new AOEShapeRect(70f, 19.5f));

public class LunarOdinStates : StateMachineBuilder
{
    public LunarOdinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoUrianger>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<Gungnir>()
            .ActivateOnEnter<Gagnrath>()
            .ActivateOnEnter<GungnirSpread>()
            .ActivateOnEnter<GunmetalSoul>()
            .ActivateOnEnter<LunarGungnir1>()
            .ActivateOnEnter<LunarGungnir2>()
            .ActivateOnEnter<Zantetsuken>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10034)]
public class LunarOdin(WorldState ws, Actor primary) : BossModule(ws, primary, new(146.5f, 84.5f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => Raid.Player()?.InCombat == true;
}
