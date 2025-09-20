namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D130BlizzardDragon;

public enum OID : uint
{
    Boss = 0x1612, // R10.0
    HolyWyvern = 0x1613, // R3.6
    BlizzardDragon1 = 0x1611, // R5.0
    BlizzardDragon2 = 0x1636, // R5.0
    Bridge = 0x1EA032, // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 872, // HolyWyvern->player, no cast, single-target
    AutoAttack2 = 870, // BlizzardDragon1->player, no cast, single-target

    Fireball = 4724, // HolyWyvern->location, 3.0s cast, range 4 circle
    Cauterize = 6241, // BlizzardDragon2->self, 6.0s cast, range 48+R width 20 rect
    SheetOfIce = 4794, // BlizzardDragon1->location, 3.0s cast, range 5 circle
    Touchdown = 564, // BlizzardDragon1->self, no cast, range 10 circle
}

class BridgeCreation(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && actor.OID == (uint)OID.Bridge)
        {
            Arena.Bounds = D130BlizzardDragon.Arena2;
            Arena.Center = D130BlizzardDragon.Arena2.Center;
        }
    }
}

class Touchdown(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cauterize)
        {
            _aoe = [new(circle, new(364.523f, -225.727f), default, Module.CastFinishAt(spell, 6.7d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Touchdown)
        {
            _aoe = [];
        }
    }
}

class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(53f, 10f));
class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 4f);
class SheetOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SheetOfIce, 5f);

class D130BlizzardDragonStates : StateMachineBuilder
{
    public D130BlizzardDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BridgeCreation>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<Touchdown>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.BlizzardDragon1);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDead)
                        return false;
                }
                return module.PrimaryActor.IsDestroyed;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171u, NameID = 4942u, SortOrder = 5)]
public class D130BlizzardDragon(WorldState ws, Actor primary) : BossModule(ws, primary, arena1.Center, arena1)
{
    private static readonly WPos[] vertices1 = [new(442.92f, -223.66f), new(443.59f, -223.55f), new(444.20f, -223.30f), new(448.14f, -221.02f), new(448.67f, -220.68f),
    new(449.18f, -220.27f), new(449.58f, -219.74f), new(449.83f, -219.13f), new(449.94f, -218.48f), new(449.90f, -217.78f),
    new(449.69f, -217.12f), new(449.40f, -216.54f), new(449.05f, -215.96f), new(448.73f, -215.37f), new(448.76f, -214.83f),
    new(448.63f, -214.15f), new(448.29f, -213.55f), new(447.90f, -212.98f), new(448.39f, -212.78f), new(448.89f, -213.07f),
    new(449.25f, -213.57f), new(449.78f, -213.38f), new(450.12f, -212.80f), new(450.39f, -212.19f), new(450.55f, -211.59f),
    new(450.69f, -210.98f), new(452.11f, -205.69f), new(452.11f, -205.07f), new(452.05f, -204.39f), new(450.38f, -198.16f),
    new(450.10f, -197.55f), new(449.77f, -196.98f), new(449.33f, -196.52f), new(445.08f, -192.27f), new(444.51f, -191.85f),
    new(443.91f, -191.50f), new(438.37f, -190.02f), new(437.70f, -189.92f), new(437.08f, -189.71f), new(436.39f, -189.68f),
    new(435.79f, -189.72f), new(433.94f, -189.21f), new(433.35f, -188.19f), new(420.74f, -188.89f), new(421.03f, -189.41f),
    new(421.62f, -189.81f), new(422.32f, -190.04f), new(423.01f, -190.29f), new(423.71f, -190.56f), new(424.34f, -190.83f),
    new(425.00f, -191.16f), new(425.57f, -191.54f), new(426.68f, -192.32f), new(426.56f, -192.91f), new(424.09f, -194.44f),
    new(424.21f, -194.95f), new(424.44f, -195.47f), new(424.21f, -195.96f), new(423.48f, -197.02f), new(423.15f, -197.61f),
    new(422.88f, -198.25f), new(421.23f, -204.39f), new(421.15f, -205.09f), new(421.17f, -205.79f), new(421.36f, -206.50f),
    new(421.56f, -207.18f), new(422.95f, -212.36f), new(423.26f, -212.95f), new(423.64f, -213.53f), new(428.01f, -217.90f),
    new(428.49f, -218.34f), new(429.09f, -218.70f), new(429.71f, -218.95f), new(436.03f, -220.64f), new(436.72f, -220.67f),
    new(437.42f, -220.64f), new(437.46f, -220.03f), new(437.28f, -219.44f), new(436.99f, -218.81f), new(437.52f, -218.83f),
    new(438.09f, -219.20f), new(438.56f, -219.70f), new(439.01f, -220.24f), new(439.42f, -220.77f), new(440.09f, -221.90f),
    new(440.46f, -222.47f), new(440.91f, -222.97f), new(441.45f, -223.34f), new(442.04f, -223.58f), new(442.69f, -223.67f)];
    private static readonly WPos[] vertices2 = [new(365.73f, -244.29f), new(366.38f, -244.21f), new(367.01f, -244.02f), new(367.57f, -243.69f), new(368.07f, -243.22f),
    new(368.45f, -242.62f), new(368.66f, -241.98f), new(368.74f, -241.35f), new(368.74f, -239.60f), new(368.92f, -238.95f),
    new(368.93f, -238.27f), new(368.98f, -237.59f), new(369.04f, -237.03f), new(369.53f, -237.19f), new(369.96f, -237.56f),
    new(370.08f, -238.17f), new(370.18f, -238.71f), new(370.69f, -238.47f), new(371.26f, -238.13f), new(371.78f, -237.74f),
    new(376.30f, -233.22f), new(376.70f, -232.69f), new(377.01f, -232.12f), new(377.20f, -231.49f), new(378.83f, -225.40f),
    new(378.84f, -224.71f), new(378.85f, -224.07f), new(379.12f, -223.65f), new(380.94f, -223.28f), new(381.48f, -223.28f),
    new(381.66f, -223.81f), new(382.24f, -223.96f), new(382.84f, -223.76f), new(383.45f, -223.61f), new(384.06f, -223.42f),
    new(384.66f, -223.20f), new(388.18f, -221.82f), new(388.75f, -221.56f), new(389.30f, -221.29f), new(389.85f, -221.00f),
    new(390.40f, -220.74f), new(390.98f, -220.51f), new(393.21f, -219.45f), new(393.81f, -219.19f), new(397.33f, -217.75f),
    new(397.95f, -217.54f), new(400.62f, -216.72f), new(401.29f, -216.56f), new(402.65f, -216.27f), new(405.44f, -215.76f),
    new(408.22f, -215.45f), new(409.58f, -215.33f), new(411.66f, -215.19f), new(412.34f, -215.12f), new(413.54f, -214.93f),
    new(414.22f, -214.89f), new(414.82f, -214.83f), new(415.42f, -214.75f), new(418.04f, -214.31f), new(419.38f, -214.06f),
    new(420.08f, -213.87f), new(420.20f, -213.19f), new(420.72f, -212.72f), new(421.89f, -212.21f), new(422.44f, -212.00f),
    new(422.87f, -212.28f), new(423.20f, -212.85f), new(423.60f, -213.46f), new(423.02f, -211.74f), new(423.00f, -211.07f),
    new(422.91f, -210.39f), new(422.75f, -209.71f), new(422.57f, -209.05f), new(421.33f, -204.71f), new(421.16f, -205.26f),
    new(421.16f, -205.91f), new(420.76f, -206.37f), new(420.11f, -206.51f), new(419.51f, -206.65f), new(418.87f, -206.75f),
    new(418.35f, -206.39f), new(418.22f, -205.88f), new(415.86f, -206.61f), new(415.24f, -206.83f), new(414.61f, -207.11f),
    new(413.37f, -207.58f), new(412.75f, -207.85f), new(410.93f, -208.61f), new(408.46f, -209.75f), new(407.85f, -210.04f),
    new(407.23f, -210.36f), new(405.44f, -211.14f), new(402.45f, -212.31f), new(401.27f, -212.70f), new(399.46f, -213.26f),
    new(397.45f, -213.73f), new(394.01f, -214.28f), new(392.66f, -214.43f), new(391.96f, -214.53f), new(391.29f, -214.60f),
    new(390.62f, -214.65f), new(387.30f, -214.93f), new(384.53f, -215.23f), new(383.93f, -215.36f), new(383.24f, -215.49f),
    new(382.54f, -215.59f), new(381.27f, -215.87f), new(380.63f, -215.99f), new(379.97f, -216.14f), new(379.79f, -216.79f),
    new(379.30f, -217.27f), new(378.08f, -217.81f), new(377.50f, -218.01f), new(377.11f, -217.67f), new(376.80f, -217.13f),
    new(376.48f, -216.59f), new(371.92f, -212.03f), new(371.41f, -211.60f), new(370.86f, -211.25f), new(369.01f, -207.42f),
    new(369.01f, -185.29f), new(368.77f, -184.63f), new(368.51f, -184.00f), new(368.24f, -183.39f), new(367.50f, -181.60f),
    new(367.20f, -180.97f), new(364.77f, -179.96f), new(364.16f, -179.74f), new(363.55f, -179.48f), new(362.85f, -179.59f),
    new(361.64f, -180.09f), new(361.04f, -180.37f), new(359.83f, -180.87f), new(359.32f, -181.34f), new(357.86f, -184.88f),
    new(357.72f, -192.30f), new(357.72f, -209.77f), new(357.65f, -210.36f), new(357.13f, -210.64f), new(355.99f, -211.19f),
    new(355.42f, -211.53f), new(354.87f, -211.96f), new(352.37f, -214.45f), new(351.90f, -214.95f), new(350.53f, -216.32f),
    new(350.09f, -216.83f), new(349.77f, -217.41f), new(349.54f, -218.06f), new(348.02f, -223.75f), new(347.89f, -224.40f),
    new(347.89f, -225.27f), new(348.03f, -225.95f), new(349.58f, -231.68f), new(349.81f, -232.30f), new(350.11f, -232.85f),
    new(350.55f, -233.36f), new(355.12f, -237.92f), new(355.67f, -238.27f), new(356.29f, -238.57f), new(356.64f, -238.13f),
    new(356.95f, -236.86f), new(357.03f, -236.33f), new(357.50f, -236.59f), new(357.92f, -239.24f), new(357.99f, -241.23f),
    new(358.05f, -241.89f), new(358.23f, -242.52f), new(358.58f, -243.12f), new(359.06f, -243.62f), new(359.62f, -243.98f),
    new(360.28f, -244.20f), new(360.96f, -244.29f), new(365.73f, -244.29f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)]);
    public static readonly ArenaBoundsCustom Arena2 = new([new PolygonCustom(vertices1), new PolygonCustom(vertices2)]);

    public static readonly uint[] Trash = [(uint)OID.HolyWyvern, (uint)OID.BlizzardDragon1];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
