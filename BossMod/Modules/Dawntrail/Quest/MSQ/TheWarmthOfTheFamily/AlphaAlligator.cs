namespace BossMod.Dawntrail.Quest.MSQ.TheWarmthOfTheFamily.AlphaAlligator;

public enum OID : uint
{
    Boss = 0x4632, // R2.6
    Yeheheceyaa = 0x4633, // R2.0
    Ceratoraptor = 0x4634, // R1.95
    HornedLizard = 0x4635, // R2.2
    AlphaAlligator = 0x4637, // R5.13
    Alligator = 0x4636, // R2.7
}

public enum AID : uint
{
    AutoAttack1 = 870, // Yeheheceyaa/Ceratoraptor/AlphaAlligator/Alligator->player/WukLamat/Koana/Boss, no cast, single-target
    AutoAttack2 = 872, // HornedLizard->player/WukLamat/Koana, no cast, single-target

    ToxicSpitVisual = 40561, // HornedLizard->self, 8.0s cast, single-target
    ToxicSpit = 40562, // HornedLizard->Boss/Koana/WukLamat, no cast, single-target
    CriticalBite = 40563, // Alligator->self, 25.0s cast, range 10 120-degree cone
}

sealed class FeedingTime(BossModule module) : Components.InterceptTether(module, (uint)AID.ToxicSpit, excludedAllies: [(uint)OID.Boss])
{
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.ToxicSpit)
            _activation = Module.CastFinishAt(spell, 1.2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            Actor? source = null;
            var enemies = Module.Enemies((uint)OID.HornedLizard);
            var sourcePosition = new WPos(403, -105); // NPCs always seem to ignore the middle tether
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
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
                hints.AddForbiddenZone(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f), _activation);
        }
    }
}

sealed class CriticalBite(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CriticalBite, new AOEShapeCone(10, 60.Degrees()));

sealed class AlphaAlligatorStates : StateMachineBuilder
{
    public AlphaAlligatorStates(AlphaAlligator module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FeedingTime>()
            .ActivateOnEnter<CriticalBite>()
            .Raw.Update = () =>
            {
                if (module.BossAlphaAlligator?.IsDead ?? false)
                {
                    return true;
                }
                if (AllDestroyed(AlphaAlligator.All))
                {
                    return true;
                }
                return false;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70785, NameID = 13593)]
public sealed class AlphaAlligator(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(423.82f, -140.6f), new(424.36f, -140.47f), new(424.85f, -140.23f), new(425.30f, -139.93f), new(425.72f, -139.59f),
    new(426.11f, -139.22f), new(428.76f, -135.75f), new(431.02f, -134.76f), new(431.56f, -134.39f), new(432.07f, -134.19f),
    new(432.65f, -134.45f), new(433.39f, -134.54f), new(433.85f, -134.25f), new(434.04f, -133.76f), new(433.22f, -132.93f),
    new(433.21f, -132.34f), new(434.86f, -128.51f), new(436.71f, -120.31f), new(436.79f, -119.61f), new(436.78f, -118.91f),
    new(433.47f, -109.77f), new(432.97f, -109.24f), new(427.74f, -105.89f), new(427.25f, -105.74f), new(421.41f, -104.22f),
    new(420.90f, -103.86f), new(420.39f, -102.69f), new(419.25f, -102.22f), new(418.97f, -101.65f), new(406.66f, -81.87f),
    new(401.18f, -81.39f), new(400.67f, -81.47f), new(400.29f, -81.88f), new(397.38f, -88.17f), new(395.23f, -98.38f),
    new(395.06f, -98.95f), new(389.16f, -103.32f), new(389.03f, -103.81f), new(388.7f, -106.85f), new(388.77f, -107.49f),
    new(388.92f, -108.11f), new(389.11f, -108.73f), new(389.38f, -109.32f), new(390.8f, -111.52f), new(404.33f, -119.98f),
    new(405.71f, -121.94f), new(405.65f, -122.46f), new(405.73f, -123.2f), new(406.51f, -124.25f), new(406.71f, -124.75f),
    new(406.61f, -125.40f), new(407.32f, -126.66f), new(407.47f, -127.25f), new(407.42f, -127.82f), new(406.67f, -128.79f),
    new(406.44f, -129.39f), new(407.78f, -134.79f), new(408.15f, -135.35f), new(420.82f, -140.11f), new(423.33f, -140.59f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] All = [(uint)OID.Yeheheceyaa, (uint)OID.AlphaAlligator, (uint)OID.HornedLizard, (uint)OID.Ceratoraptor, (uint)OID.Alligator];
    public Actor? BossAlphaAlligator;

    protected override void UpdateModule()
    {
        BossAlphaAlligator ??= GetActor((uint)OID.AlphaAlligator);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, All);
    }
}
