namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PasDeSeul;

public enum OID : uint
{
    PasDeSeul = 0x4B90, // R3.000, x?
    SuccubusMage = 0x4B91, // R1.500, x?
    SuccubusKnight = 0x4B92, // R1.500, x?
    Pheromone = 0x4B93, // R1.000, x? Summoned with Beguiling Mist. Hearts tethered together, cast Heart Shatter?
    _Gen_ = 0x4DA2, // R1.000, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50396, // PasDeSeul/SuccubusKnight->player, no cast, single-target
    Teleport = 46931, // PasDeSeul->location, no cast, single-target

    BloodRain = 46925, // PasDeSeul->self, 5.4+0.6s cast, single-target
    BloodRainCircle = 46926, // 233C->self, 6.0s cast, range 8 circle

    VoidAeroII = 46932, // PasDeSeul->self, 4.0s cast, range 60 width 8 rect
    VoidAeroII1 = 46933, // 233C->self, 3.0s cast, range 60 20.000-degree cone
    ColdCaress = 46935, // PasDeSeul->player, 5.0s cast, single-target : Poisons target.
    Summon = 46927, // PasDeSeul->self, 4.0s cast, single-target
    Aero = 50746, // SuccubusMage->player, no cast, single-target
    Fanaticism = 46928, // SuccubusMage->PasDeSeul, 9.0s cast, single-target : Gives target Damage Up buff.
    VoidFireII = 46929, // SuccubusMage->location, 6.0s cast, range 10 circle
    BloodRain2 = 46923, // PasDeSeul->self, 5.0+1.0s cast, single-target
    BloodRainDonut = 46924, // 233C->self, 6.0s cast, range 8-40 donut
    BloodSword = 46934, // PasDeSeul->player, 6.0s cast, single-target : Heals Caster on hit. Interrupt if possible.
    Lifeblood = 49508, // 233C->PasDeSeul, no cast, single-target
    BeguilingMist = 46936, // PasDeSeul->self, 5.0s cast, range 30 circle
    HeartShatter = 46937, // 4B93->self, 1.0s cast, single-target
    HeartShatter1 = 46938, // 233C->self, 1.0s cast, range 24 circle
    SweetSteel = 46930, // SuccubusKnight->self, 6.0s cast, range 10 120.000-degree cone
}

sealed class BloodRainCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BloodRainCircle, 8f);
sealed class BloodRainDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BloodRainDonut, new AOEShapeDonut(8f, 40f));

sealed class VoidAeroIIRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidAeroII, new AOEShapeRect(60f, 4f));

sealed class VoidAeroIICone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidAeroII1, new AOEShapeCone(60f, 10f.Degrees()));

//Prioritizing mage add higher than the knight. Mage Gives Damage up buffs to boss.
sealed class SuccubusMageAdd(BossModule module) : Components.Adds(module, (uint)OID.SuccubusMage, 2);
sealed class SuccubusKnightAdd(BossModule module) : Components.Adds(module, (uint)OID.SuccubusKnight, 1);

sealed class VoidFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireII, 10f);
sealed class Fanaticism(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Fanaticism, showNameInHint: true);
sealed class BloodSword(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BloodSword, "Tankbuster + lifesteal");
sealed class ColdCaress(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ColdCaress, "Tankbuster + poison");
sealed class BeguilingMist(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeguilingMist, "Raidwide + Nightmare");

//HeartShatter1 = 46938, // 233C->self, 1.0s cast, range 24 circle
sealed class HeartShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count == 0 ? [] : CollectionsMarshal.AsSpan(_aoes);
    private readonly List<Actor> _orbs = [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeartShatter)
        {
            _aoes.Clear();
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Pheromone)
        {
            _orbs.Add(actor);

            // tether may come out before both actors are created
            if (_orbs.Count == 2)
            {
                var pos1 = _orbs[0].Position;
                var pos2 = _orbs[1].Position;
                var dir = pos2 - pos1;
                var length = dir.Length();
                var direction = length != 0f ? dir / length : default;
                var estimated = pos1 + direction * (length * 0.5f);
                var activation = WorldState.FutureTime(13.5d);

                _aoes.Add(new(new AOEShapeCircle(24f), estimated, default, activation));
                _orbs.Clear();
            }
        }
    }
}
//SweetSteel = 46930, // SuccubusKnight->self, 6.0s cast, range 10 120.000-degree cone
sealed class SweetSteel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweetSteel, new AOEShapeCone(10f, 60f.Degrees()));

sealed class PasDeSeulStates : StateMachineBuilder
{
    public PasDeSeulStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fanaticism>()
            .ActivateOnEnter<BloodSword>()
            .ActivateOnEnter<ColdCaress>()
            .ActivateOnEnter<BeguilingMist>()
            .ActivateOnEnter<BloodRainCircle>()
            .ActivateOnEnter<BloodRainDonut>()
            .ActivateOnEnter<VoidAeroIIRect>()
            .ActivateOnEnter<VoidAeroIICone>()
            .ActivateOnEnter<SuccubusMageAdd>()
            .ActivateOnEnter<SuccubusKnightAdd>()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<HeartShatter>()
            .ActivateOnEnter<SweetSteel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    PrimaryActorOID = (uint)OID.PasDeSeul,
    Contributors = "wen",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken,
    GroupID = 1088u,
    NameID = 14541u,
    SortOrder = 6)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Succubus Mage's Fanaticism")]
[PreferredKinship(BST.Kinship.Wave, "Remove Pas de Seul's Damage Up if Fanaticism goes off")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Poison from Cold Caress")]
public sealed class PasDeSeul : BossModule
{
    public PasDeSeul(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    // Constructor so we can build arena
    private PasDeSeul(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static readonly WPos ArenaCenter = new(520f, -420f);

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        ArenaBoundsCustom arena = new(
            [
                new Rectangle(ArenaCenter, 19.70f, 23.50f)
            ],
            [
                new Rectangle(new WPos(502.88f, -398.45f), 3.41f, 5.42f, -7.380f.Degrees()),
                new Rectangle(new WPos(510.94f, -396.70f), 1.30f, 0.50f),
                new Rectangle(new WPos(515.42f, -396.61f), 0.75f, 0.50f),
                new Rectangle(new WPos(524.58f, -396.61f), 0.75f, 0.50f),
                new Rectangle(new WPos(528.84f, -397.06f), 1.57f, 1.01f),
                new Rectangle(new WPos(539.61f, -408.00f), 0.77f, 1.20f),
                new Rectangle(new WPos(537.57f, -412.33f), 2.50f, 3.20f),
                new Rectangle(new WPos(541.41f, -420.00f), 2.50f, 1.20f),
                new Rectangle(new WPos(538.77f, -426.12f), 2.41f, 3.97f),
                new Rectangle(new WPos(539.57f, -431.99f), 0.75f, 1.20f),
                new Rectangle(new WPos(539.31f, -443.61f), 0.47f, 0.79f),
                new Rectangle(new WPos(505.35f, -443.55f), 1.52f, 0.79f),
                new Rectangle(new WPos(501.05f, -440.73f), 3.75f, 5.00f, 5.790f.Degrees()),
                new Rectangle(new WPos(500.56f, -432.03f), 0.60f, 1.25f),
                new Rectangle(new WPos(500.56f, -420.00f), 0.60f, 1.25f),
                new Rectangle(new WPos(500.60f, -408.02f), 0.60f, 1.25f),
                new Rectangle(new WPos(539.50f, -396.55f), 0.70f, 0.60f),
                new Rectangle(new WPos(502.11f, -424.06f), 2.50f, 2.90f),
                new Rectangle(new WPos(501.36f, -415.86f), 2.30f, 1.80f, -5.800f.Degrees()),
                new Rectangle(new WPos(500.13f, -413.02f), 1.10f, 1.30f)
            ]);
        return (ArenaCenter, arena);
    }
}
