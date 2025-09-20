namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD30Ningishzida;

public enum OID : uint
{
    Boss = 0x16AC, // R4.8
    FireVoidzone = 0x1E8D9B, // R0.5
    IceVoidzone = 0x1E8D9C // R0.5
}

public enum AID : uint
{
    AutoAttack = 6501, // Boss->player, no cast, range 6+R ?-degree cone

    Dissever = 6426, // Boss->self, no cast, range 6+R 90-degree cone
    BallOfFire = 6427, // Boss->location, no cast, range 6 circle
    BallOfIce = 6428, // Boss->location, no cast, range 6 circle
    FearItself = 6429 // Boss->self, 6.0s cast, range 54+R circle
}

class Dissever(BossModule module) : Components.Cleave(module, (uint)AID.Dissever, new AOEShapeCone(10.8f, 45f.Degrees()), activeWhileCasting: false);

abstract class Voidzones(BossModule module, uint aid, uint oid) : Components.VoidzoneAtCastTarget(module, 6, aid, m => GetVoidzones(m, oid), 2.1f)
{
    private static Actor[] GetVoidzones(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class BallofFire(BossModule module) : Voidzones(module, (uint)AID.BallOfFire, (uint)OID.FireVoidzone);
class BallofIce(BossModule module) : Voidzones(module, (uint)AID.BallOfIce, (uint)OID.IceVoidzone);

class FearItself(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FearItself, new AOEShapeDonut(5f, 50f));

class Hints(BossModule module) : BossComponent(module)
{
    // arena is like a weird octagon and the boss also doesn't cast from the center
    private static readonly WPos FearCastSource = new(-300f, -236f);
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BallOfFire or (uint)AID.BallOfIce or (uint)AID.FearItself)
            ++NumCasts;

        if (NumCasts >= 5)
        {
            NumCasts = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NumCasts < 4)
            hints.Add($"Bait the boss away from the middle of the arena. \n{Module.PrimaryActor.Name} will cast x2 Fire Puddles & x2 Ice Puddles. \nAfter the 4th puddle is dropped, run to the middle.");
        else if (NumCasts >= 4)
            hints.Add($"Run to the middle of the arena! \n{Module.PrimaryActor.Name} is about to cast a donut AOE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (NumCasts < 4)
            hints.AddForbiddenZone(new SDCircle(FearCastSource, 11f), WorldState.FutureTime(10d));
        else
            hints.AddForbiddenZone(new SDInvertedCircle(FearCastSource, 5f), WorldState.FutureTime(10d));
    }
}

class DD30NingishzidaStates : StateMachineBuilder
{
    public DD30NingishzidaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dissever>()
            .ActivateOnEnter<BallofFire>()
            .ActivateOnEnter<BallofIce>()
            .ActivateOnEnter<FearItself>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 176, NameID = 5012)]
public class DD30Ningishzida(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-292.06f, -260.52f), new(-291.70f, -259.98f), new(-283f, -253.77f), new(-278.9f, -248.62f), new(-276.9f, -243.92f),
    new(-276.7f, -243.32f), new(-275.95f, -237.58f), new(-275.96f, -236.90f), new(-276.71f, -231.18f), new(-279.11f, -225.31f),
    new(-282.9f, -220.35f), new(-283.44f, -219.89f), new(-287.65f, -216.66f), new(-288.24f, -216.30f), new(-293.88f, -214f),
    new(-299.76f, -213.22f), new(-300.41f, -213.24f), new(-306.18f, -214f), new(-312.06f, -216.44f), new(-316.71f, -220.01f),
    new(-317.2f, -220.47f), new(-320.75f, -225.10f), new(-321.04f, -225.68f), new(-323.28f, -231.07f), new(-324.03f, -236.80f),
    new(-324.06f, -237.44f), new(-323.29f, -243.30f), new(-323.08f, -243.94f), new(-321.31f, -248.22f), new(-317.93f, -252.79f),
    new(-317.48f, -253.28f), new(-312.6f, -257.12f), new(-307.27f, -259.75f), new(-307.11f, -260.30f), new(-292.06f, -260.52f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)], [new Rectangle(new(-299.312f, -261.765f), 20f, 1.325f)]);
}
