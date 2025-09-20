namespace BossMod.Dawntrail.Quest.Role.BarThePassage.Trash1;

public enum OID : uint
{
    Boss = 0x465D, // R0.5, friendly NPC Loashkhana
    UncannyHornedLizard = 0x4664, // R2.486
    UncannyTumbleclaw1 = 0x4665, // R1.76
    UncannyTumbleclaw2 = 0x46AF, // R1.76
    UncannyRroneek = 0x4661, // R2.08
    UncannyCactuar1 = 0x4666, // R1.2
    UncannyCactuar2 = 0x46B0, // R1.2
    UncannyNopalitender1 = 0x4663, // R2.35
    UncannyNopalitender2 = 0x46AD, // R2.35
    UncannyFlyingPopoto1 = 0x4662, // R0.9
    UncannyFlyingPopoto2 = 0x46AC, // R0.9
    UncannyYeheheceyaa1 = 0x473A, // R4.0
    UncannyYeheheceyaa2 = 0x46BC, // R2.6, untargetable helpers
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // UncannyFlyingPopoto1/UncannyRroneek/UncannyNopalitender1/UncannyFlyingPopoto2/UncannyNopalitender2->allies, no cast, single-target
    AutoAttack2 = 870, // UncannyTumbleclaw1/UncannyHornedLizard/UncannyCactuar1/UncannyYeheheceyaa1/UncannyTumbleclaw2/UncannyCactuar2->allies, no cast, single-target

    NopalFan = 40896, // UncannyNopalitender1->self, 3.0s cast, range 15 width 4 rect
    PulverizingPound = 40898, // UncannyRroneek->self, 3.0s cast, range 5 circle
    BodySlam = 40901, // UncannyHornedLizard->self, 4.0s cast, range 6 circle
    Tumblecleave = 40895, // UncannyTumbleclaw1->allies, no cast, single-target
    AdventitiousLash = 40899, // UncannyFlyingPopoto1->allies, no cast, single-target
    ZillionNeedles = 40900, // UncannyCactuar1->self, 3.0s cast, range 4 circle

    PredatorySwoopVisual = 41958, // UncannyYeheheceyaa2->Tepeke, 8.0s cast, single-target
    PredatorySwoop = 41787, // UncannyYeheheceyaa2->location, 4.0s cast, range 6 circle
    PredatorySwoopTepeke = 42026, // Helper->Tepeke, no cast, single-target

    FlockFrenzy = 41960, // UncannyYeheheceyaa1->self, 3.0s cast, range 50 circle
    Visual = 41959, // UncannyYeheheceyaa2->self, no cast, single-target
    ToxicSpitVisual = 41125, // UncannyHornedLizard->self, 11.0s cast, single-target
    ToxicSpit = 40902 // UncannyHornedLizard->Boss, no cast, single-target
}

public enum TetherID : uint
{
    ToxicSpit = 84, // UncannyHornedLizard->Boss/player
    ToxicSpitTepeke = 17, // UncannyHornedLizard->Tepeke
}

sealed class NopalFan(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NopalFan, new AOEShapeRect(15f, 2f));
sealed class PulverizingPound(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PulverizingPound, 5f);
sealed class BodySlamPredatorySwoop(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BodySlam, (uint)AID.PredatorySwoop], 6f);
sealed class ZillionNeedles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ZillionNeedles, 4f);
sealed class FlockFrenzy(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlockFrenzy);

sealed class ToxicSpit(BossModule module) : Components.InterceptTether(module, (uint)AID.ToxicSpitVisual, excludedAllies: [(uint)OID.Boss])
{
    private DateTime _activation;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.ToxicSpitVisual)
            _activation = Module.CastFinishAt(spell, 1.1d);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            Actor? source = null;
            var enemies = Module.Enemies((uint)OID.UncannyHornedLizard);
            var sourcePosition = new WPos(13, -302); // Tepeke always seems to ignore this lizard
            var count = enemies.Count;
            for (var i = 0; i < count; ++i)
            {
                var enemy = enemies[i];
                if (enemy.Position.AlmostEqual(sourcePosition, 1f))
                {
                    source = enemy;
                    break;
                }
            }

            if (source == null)
                return;
            var target = Module.Enemies((uint)OID.Boss)[0];
            hints.AddForbiddenZone(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f), _activation);
        }
    }
}

sealed class Trash1States : StateMachineBuilder
{
    public Trash1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NopalFan>()
            .ActivateOnEnter<PulverizingPound>()
            .ActivateOnEnter<BodySlamPredatorySwoop>()
            .ActivateOnEnter<ZillionNeedles>()
            .ActivateOnEnter<FlockFrenzy>()
            .ActivateOnEnter<ToxicSpit>()
            .Raw.Update = () => !module.PrimaryActor.IsTargetable || module.WorldState.CurrentCFCID != 1016u;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1016, NameID = 13720)]
public sealed class Trash1(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -336.3f), new ArenaBoundsRect(34.6f, 43.7f))
{
    private static readonly uint[] trash = [(uint)OID.UncannyHornedLizard, (uint)OID.UncannyCactuar1, (uint)OID.UncannyCactuar2, (uint)OID.UncannyFlyingPopoto1,
    (uint)OID.UncannyFlyingPopoto2, (uint)OID.UncannyYeheheceyaa1, (uint)OID.UncannyRroneek, (uint)OID.UncannyTumbleclaw1, (uint)OID.UncannyTumbleclaw2,
    (uint)OID.UncannyNopalitender1, (uint)OID.UncannyNopalitender2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, trash);
    }
}
