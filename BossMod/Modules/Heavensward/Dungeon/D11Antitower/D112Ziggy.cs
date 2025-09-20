namespace BossMod.Heavensward.Dungeon.D11Antitower.D112Ziggy;

public enum OID : uint
{
    Boss = 0x3D82, // R2.700, x1
    Stardust = 0x3D83 // R2.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 31836, // Boss->location, no cast, single-target

    GyratingGlare = 31835, // Boss->self, 5.0s cast, range 40 circle
    ShinySummoning = 31831, // Boss->self, no cast, single-target
    MysticLight = 31838, // Stardust->self, 6.0s cast, range 12 circle
    JitteringGlare = 31832, // Boss->self, 3.0s cast, range 40 30-degree cone
    JitteringJounceVisual = 31833, // Boss->self, 6.0s cast, single-target
    JitteringJounce = 31840, // Boss->players/Stardust, no cast, width 6 rect charge
    DeepFracture = 31839, // Stardust->self, 4.0s cast, range 11 circle
    JitteringJab = 31837 // Boss->player, 5.0s cast, single-target, takbuster
}

public enum TetherID : uint
{
    JitteringJounce = 2 // Boss->player/Stardust
}

class GyratingGlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.GyratingGlare);
class MysticLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticLight, 12f);
class DeepFracture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepFracture, 11f);
class JitteringGlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JitteringGlare, new AOEShapeCone(40f, 15f.Degrees()));
class JitteringJab(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.JitteringJab);

class JitteringJounceBait(BossModule module) : Components.BaitAwayChargeTether(module, 3f, default, (uint)AID.JitteringJounce, tetherIDBad: (uint)TetherID.JitteringJounce)
{
    public Actor? Target;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        Target ??= WorldState.Actors.Find(tether.Target);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.JitteringJounce)
            Target = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

class JitteringJounceLOS(BossModule module) : Components.GenericAOEs(module)
{
    private readonly JitteringJounceBait _bait = module.FindComponent<JitteringJounceBait>()!;
    private static readonly Angle halfAngle = Angle.Asin(1.5f / 11);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_bait.Target == actor)
        {
            var stardust = GetStardust(Module);
            var count = stardust.Count;
            var cones = new DonutSegmentHA[count];
            for (var i = 0; i < count; ++i)
            {
                cones[i] = new(D112Ziggy.ArenaCenter, 11.1f, 20f, Angle.FromDirection(Module.PrimaryActor.DirectionTo(stardust[i])), halfAngle);
            }
            return new AOEInstance[1] { new(new AOEShapeCustom([.. cones], invertForbiddenZone: true), Arena.Center, color: Colors.SafeFromAOE) };
        }
        return [];
    }

    public static List<Actor> GetStardust(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Stardust);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var kegs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                kegs.Add(z);
        }
        return kegs;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_bait.Target == actor)
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var risky = true;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(actor.Position))
                {
                    risky = false;
                    break;
                }
            }
            hints.Add("Hide behind meteor!", risky);
        }
    }
}

class D112ZiggyStates : StateMachineBuilder
{
    public D112ZiggyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GyratingGlare>()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<DeepFracture>()
            .ActivateOnEnter<JitteringGlare>()
            .ActivateOnEnter<JitteringJab>()
            .ActivateOnEnter<JitteringJounceBait>()
            .ActivateOnEnter<JitteringJounceLOS>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4808)]
public class D112Ziggy(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(185.8f, 137.5f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 20.06f * CosPI.Pi36th, 36)], [new Rectangle(new(166, 138), 1.1f, 20f),
    new Rectangle(new(207, 137), 2.6f, 20f)]);
}
