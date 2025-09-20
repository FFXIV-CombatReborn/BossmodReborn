namespace BossMod.Dawntrail.Alliance.A10Despot;

public enum OID : uint
{
    Boss = 0x4692, // R4.25
    Flamingo1 = 0x4713, // R0.6
    Flamingo2 = 0x4693 // R0.6
}

public enum AID : uint
{
    AutoAttack1 = 870, // Flamingo1/Flamingo2->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target

    WingCutter = 41672, // Flamingo2->self, 2.5s cast, range 6 120-degree cone
    ScraplineStorm = 40650, // Boss->self, 5.0s cast, range 30 circle, pull 10 between centers
    Scrapline = 41393, // Boss->self, 1.0s cast, range 10 circle
    Typhoon = 41902, // Boss->self, 1.5s cast, range 8-40 donut
    IsleDrop = 41699, // Boss->location, 3.0s cast, range 6 circle
    Peck = 41695, // Flamingo2->player, no cast, single-target
    Panzerfaust = 41698, // Boss->player, 5.0s cast, single-target, interruptible tankbuster
    PanzerfaustRepeats = 41353 // Boss->player, no cast, single-target, knockback 10, apply concussion
}

sealed class IsleDrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IsleDrop, 6f);
sealed class WingCutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingCutter, new AOEShapeCone(6f, 60f.Degrees()));
sealed class PanzerfaustHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Panzerfaust, showNameInHint: true);
sealed class Panzerfaust(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Panzerfaust);
sealed class ScraplineStorm(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ScraplineStorm, 10f, kind: Kind.TowardsOrigin)
{
    private readonly ScraplineTyphoon _aoe = module.FindComponent<ScraplineTyphoon>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOEs.Count != 0)
        {
            return _aoe.AOEs.Ref(0).Check(pos);
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var source = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDCircle(source.Origin, 20f), source.Activation);
        }
    }
}

sealed class ScraplineTyphoon(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(8f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScraplineStorm)
        {
            AddAOE(circle, 2.1d);
            AddAOE(donut, 5.6d);
            void AddAOE(AOEShape shape, double delay) => AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Scrapline or (uint)AID.Typhoon)
        {
            AOEs.RemoveAt(0);
        }
    }
}

public sealed class A10DespotStates : StateMachineBuilder
{
    public A10DespotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsleDrop>()
            .ActivateOnEnter<WingCutter>()
            .ActivateOnEnter<ScraplineTyphoon>()
            .ActivateOnEnter<ScraplineStorm>()
            .ActivateOnEnter<Panzerfaust>()
            .ActivateOnEnter<PanzerfaustHint>()
            .Raw.Update = () => AllDeadOrDestroyed(A10Despot.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13608, SortOrder = 6)]
public sealed class A10Despot(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-585.1f, -640.25f), new(-584.42f, -640.14f), new(-556.15f, -628.13f), new(-556.17f, -627.43f), new(-556.81f, -626.37f),
    new(-556.72f, -625.74f), new(-556.38f, -624.57f), new(-555.35f, -623.79f), new(-554.92f, -623.21f), new(-556.88f, -618.59f),
    new(-556.76f, -618.1f), new(-532.93f, -607.98f), new(-532.34f, -607.67f), new(-531.78f, -607.29f), new(-531.16f, -607.05f),
    new(-530.49f, -606.98f), new(-530.01f, -607.45f), new(-526.74f, -615.15f), new(-526.43f, -615.55f), new(-504, -606.01f),
    new(-503.69f, -605.41f), new(-503.74f, -604.11f), new(-503.65f, -603.47f), new(-503.43f, -602.86f), new(-503.01f, -602.43f),
    new(-502.58f, -601.89f), new(-502.1f, -601.59f), new(-501.41f, -601.61f), new(-500.72f, -601.68f), new(-500.05f, -601.82f),
    new(-499.4f, -602.04f), new(-497.02f, -603.35f), new(-495.18f, -607.55f), new(-494.72f, -608.11f), new(-493.47f, -608.77f),
    new(-493.13f, -608.34f), new(-492.9f, -607.7f), new(-492.78f, -607.07f), new(-492.47f, -606.51f), new(-491.09f, -605.03f),
    new(-490.59f, -604.6f), new(-490, -604.29f), new(-489.34f, -604.14f), new(-488.75f, -604.33f), new(-484, -607.26f),
    new(-483.55f, -607.67f), new(-482.54f, -610.05f), new(-481.96f, -610.45f), new(-480.12f, -611.24f), new(-475.91f, -614.53f),
    new(-475.99f, -613.97f), new(-481.73f, -600.44f), new(-482.11f, -599.84f), new(-482.62f, -599.44f), new(-483.05f, -598.99f),
    new(-483.28f, -598.38f), new(-483.22f, -597.79f), new(-482.95f, -597.2f), new(-482.4f, -596.83f), new(-482.56f, -596.32f),
    new(-492.75f, -573.42f), new(-492.65f, -572.93f), new(-487.62f, -569.97f), new(-487.26f, -569.61f), new(-487.45f, -566.88f),
    new(-487.39f, -566.19f), new(-487.2f, -565.48f), new(-486.95f, -564.82f), new(-486.64f, -564.13f), new(-486.01f, -561.99f),
    new(-485.78f, -561.53f), new(-485.22f, -560.66f), new(-485f, -560f), new(-498.54f, -538.05f), new(-499.21f, -537.84f),
    new(-507.26f, -536.82f), new(-507.98f, -536.85f), new(-509.25f, -538.49f), new(-509.72f, -538.95f), new(-510.24f, -539.05f),
    new(-511.53f, -538.97f), new(-512.14f, -538.89f), new(-512.7f, -538.69f), new(-513.01f, -538.17f), new(-513.51f, -537.7f),
    new(-517.45f, -535.31f), new(-519.94f, -532.16f), new(-520.52f, -531.83f), new(-521.18f, -531.89f), new(-521.83f, -532.08f),
    new(-522.41f, -532.45f), new(-525.15f, -535.26f), new(-525.61f, -535.68f), new(-526.13f, -535.99f), new(-526.7f, -536.2f),
    new(-527.37f, -536.27f), new(-527.91f, -536.18f), new(-530.31f, -533.18f), new(-530.6f, -532.66f), new(-530.63f, -532.02f),
    new(-543.41f, -515.55f), new(-544.1f, -515.68f), new(-555.29f, -524.27f), new(-555.32f, -524.91f), new(-543.33f, -540.26f),
    new(-542.97f, -540.82f), new(-541.82f, -541.41f), new(-540.94f, -542.3f), new(-539.96f, -543.1f), new(-539.51f, -543.54f),
    new(-539.23f, -544.06f), new(-539.07f, -544.64f), new(-539.1f, -545.25f), new(-539.21f, -545.86f), new(-539.63f, -546.23f),
    new(-542.42f, -547.97f), new(-543.03f, -548.24f), new(-545.06f, -548.75f), new(-546.33f, -548.68f), new(-546.94f, -548.49f),
    new(-547.44f, -548.05f), new(-550.57f, -546.55f), new(-551.17f, -546.91f), new(-552.21f, -547.72f), new(-554.33f, -565.17f),
    new(-554.29f, -565.67f), new(-553.44f, -567.42f), new(-553.24f, -568), new(-553.48f, -568.46f), new(-575f, -585.35f),
    new(-575.48f, -585.85f), new(-576.07f, -585.86f), new(-576.48f, -585.42f), new(-579.7f, -581.35f), new(-580.39f, -581.19f),
    new(-581.01f, -581.22f), new(-581.59f, -580.82f), new(-582.12f, -580.35f), new(-582.41f, -579.83f), new(-582.31f, -579.14f),
    new(-581.86f, -578.57f), new(-585.24f, -574.25f), new(-585.66f, -573.94f), new(-592.4f, -579.21f), new(-592.88f, -579.69f),
    new(-593.98f, -580.42f), new(-594.46f, -580.26f), new(-596.61f, -577.68f), new(-608.31f, -586.89f), new(-608.07f, -587.53f),
    new(-591f, -609.36f), new(-590.71f, -609.86f), new(-593.18f, -623.19f), new(-589.12f, -632.73f), new(-588.73f, -633.32f),
    new(-588.21f, -633.36f), new(-587.48f, -633.3f), new(-586.78f, -633.16f), new(-584.14f, -632.28f), new(-583.52f, -632.53f),
    new(-583.31f, -633.17f), new(-583.26f, -633.79f), new(-583.79f, -634.2f), new(-585.11f, -634.48f), new(-585.79f, -634.7f),
    new(-586.45f, -635.02f), new(-587.01f, -635.5f), new(-587.48f, -636.06f), new(-587.43f, -636.72f), new(-586.1f, -639.84f),
    new(-585.55f, -640.25f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Flamingo1, (uint)OID.Flamingo2];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
