namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D100TroianFootman;

public enum OID : uint
{
    Boss = 0x399E, // R4.6
    TroianGuard = 0x397F, // R2.0
    TroianHound = 0x3980, // R3.15
    TroianRider = 0x3981 // R2.8
}

public enum AID : uint
{
    AutoAttack1 = 6497, // TroianHound/TroianGuard/TroianRider->player, no cast, single-target
    AutoAttack2 = 870, // Boss->player, no cast, single-target

    GrimFate = 29713, // Boss->self, 3.0s cast, range 12 120-degree cone
    VoidTrap = 30218, // TroianGuard->location, 3.0s cast, range 6 circle
    GrimHalo = 29712, // Boss->self, 4.0s cast, range 12 circle
    Geirrothr = 30215 // TroianRider->self, 3.0s cast, range 6 120-degree cone
}

sealed class Geirrothr(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Geirrothr, new AOEShapeCone(6f, 60f.Degrees()));
sealed class GrimFate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrimFate, new AOEShapeCone(12f, 60f.Degrees()));
sealed class GrimHalo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrimHalo, 12f);
sealed class VoidTrap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidTrap, 6f);

sealed class D100TroianFootmanStates : StateMachineBuilder
{
    public D100TroianFootmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Geirrothr>()
            .ActivateOnEnter<GrimFate>()
            .ActivateOnEnter<GrimHalo>()
            .ActivateOnEnter<VoidTrap>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D100TroianFootman.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11447, SortOrder = 3)]
public sealed class D100TroianFootman(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-163.01f, 2.62f), new(-162.53f, 3.15f), new(-162.08f, 3.49f), new(-160.91f, 3.72f), new(-161.11f, 4.28f),
    new(-160.95f, 4.75f), new(-158.03f, 5.95f), new(-157.5f, 5.57f), new(-157.26f, 4.97f), new(-156.85f, 4.38f),
    new(-156.21f, 4.64f), new(-155.74f, 5.16f), new(-155.45f, 5.82f), new(-154.99f, 6.2f), new(-154.51f, 6.66f),
    new(-153.88f, 6.93f), new(-153.34f, 6.9f), new(-152.99f, 6.41f), new(-152.44f, 5.95f), new(-149.3f, 9.12f),
    new(-148.99f, 9.52f), new(-149.36f, 9.94f), new(-149.89f, 10.34f), new(-149.84f, 11f), new(-149.47f, 11.56f),
    new(-149.23f, 12.05f), new(-149.87f, 12.43f), new(-149.88f, 12.95f), new(-149.07f, 14.67f), new(-148.89f, 15.33f),
    new(-148.41f, 15.79f), new(-147.9f, 15.67f), new(-146.79f, 15.31f), new(-146.33f, 16.41f), new(-146.2f, 16.93f),
    new(-146.77f, 17.22f), new(-146.77f, 17.81f), new(-146.67f, 18.42f), new(-146.51f, 19.04f), new(-146.21f, 19.49f),
    new(-145.6f, 19.7f), new(-145.6f, 24.21f), new(-146.12f, 24.47f), new(-146.47f, 24.84f), new(-146.59f, 25.43f),
    new(-146.83f, 26.04f), new(-146.8f, 26.73f), new(-146.36f, 27.04f), new(-146.37f, 27.73f), new(-147.92f, 31.43f),
    new(-148.56f, 31.48f), new(-149.02f, 31.77f), new(-149.36f, 32.29f), new(-149.82f, 32.76f), new(-149.99f, 33.34f),
    new(-149.71f, 33.78f), new(-149.98f, 34.42f), new(-152.8f, 37.24f), new(-153.43f, 37.08f), new(-153.94f, 37.12f),
    new(-154.46f, 37.45f), new(-154.94f, 37.62f), new(-155.38f, 37.18f), new(-155.92f, 37.34f), new(-158.81f, 38.51f),
    new(-159.1f, 38.95f), new(-158.66f, 40.12f), new(-159.17f, 40.55f), new(-159.83f, 40.76f), new(-160.1f, 40.33f),
    new(-160.76f, 40.25f), new(-161.41f, 40.38f), new(-161.9f, 39.95f), new(-162.52f, 39.95f), new(-163.06f, 40.08f),
    new(-163.18f, 40.65f), new(-163.18f, 42.81f), new(-163.3f, 43.45f), new(-162.65f, 43.76f), new(-162.46f, 44.23f),
    new(-162.05f, 44.72f), new(-161.4f, 44.96f), new(-161.4f, 58.55f), new(-161.59f, 59.23f), new(-162.22f, 59.32f),
    new(-162.44f, 62.11f), new(-162.28f, 62.7f), new(-162.07f, 63.23f), new(-161.59f, 64.07f), new(-161.61f, 68.14f),
    new(-161.9f, 68.7f), new(-162.28f, 69.1f), new(-162.28f, 70.5f), new(-162.2f, 71.11f), new(-161.61f, 71.29f),
    new(-161.61f, 73.48f), new(-162.38f, 73.61f), new(-163.09f, 73.6f), new(-163.6f, 73.84f), new(-163.65f, 75.79f),
    new(-163.73f, 76.39f), new(-163.63f, 77.06f), new(-163.11f, 77.43f), new(-162.43f, 77.44f), new(-162.41f, 78.01f),
    new(-162.93f, 78.02f), new(-163.43f, 78.25f), new(-163.81f, 78.75f), new(-163.97f, 79.35f), new(-164.22f, 83.75f),
    new(-166.53f, 83.75f), new(-167.01f, 83.93f), new(-167.06f, 87.44f), new(-167.31f, 87.94f), new(-167.02f, 89.9f),
    new(-166.5f, 90.2f), new(-166.04f, 90.63f), new(-165.52f, 90.78f), new(-164.92f, 90.74f), new(-164.33f, 90.93f),
    new(-164.17f, 92.24f), new(-164.04f, 92.85f), new(-163.67f, 94.05f), new(-163.02f, 94.11f), new(-162.3f, 93.94f),
    new(-162.28f, 94.46f), new(-162.48f, 94.94f), new(-163.06f, 94.88f), new(-163.43f, 95.37f), new(-163.9f, 96.41f),
    new(-164.22f, 99.62f), new(-163.95f, 100.06f), new(-163.42f, 100.36f), new(-162.91f, 100.56f), new(-162.49f, 100.3f),
    new(-161.97f, 100.23f), new(-162.4f, 101.39f), new(-167.44f, 101.39f), new(-167.81f, 100.84f), new(-169.2f, 100.72f),
    new(-169.7f, 100.42f), new(-170.13f, 100.83f), new(-170.46f, 101.41f), new(-172.36f, 101.41f), new(-172.55f, 100.68f),
    new(-172.52f, 100.06f), new(-172.78f, 99.54f), new(-174.84f, 99.47f), new(-175.5f, 99.38f), new(-176.15f, 99.46f),
    new(-176.58f, 100.73f), new(-177f, 100.31f), new(-178.08f, 99.77f), new(-178.67f, 99.62f), new(-179.18f, 99.74f),
    new(-179.3f, 100.26f), new(-179.17f, 100.91f), new(-179.19f, 103.38f), new(-178.77f, 103.77f), new(-178.49f, 104.96f),
    new(-178.28f, 105.43f), new(-177.7f, 105.49f), new(-177.42f, 107.74f), new(-177.45f, 109.85f), new(-177.58f, 110.54f),
    new(-178.17f, 110.86f), new(-178.31f, 111.68f), new(-178.78f, 112.14f), new(-178.71f, 112.68f), new(-178.27f, 113.08f),
    new(-177.7f, 113.29f), new(-177.44f, 115.53f), new(-177.43f, 117.54f), new(-177.59f, 118.21f), new(-178f, 118.66f),
    new(-178.3f, 119.16f), new(-178.34f, 119.73f), new(-178.26f, 120.96f), new(-177.83f, 121.28f), new(-176.28f, 121.28f),
    new(-175.6f, 121.36f), new(-175.21f, 121.88f), new(-175.17f, 122.49f), new(-174.94f, 122.99f), new(-174.03f, 123.91f),
    new(-173.57f, 124.22f), new(-172.95f, 124.37f), new(-170.63f, 128.46f), new(-170.75f, 128.96f), new(-170.76f, 129.56f),
    new(-170.64f, 130.23f), new(-170.41f, 130.88f), new(-170.05f, 131.29f), new(-169.76f, 131.84f), new(-169.75f, 136.2f),
    new(-170.18f, 136.76f), new(-170.45f, 137.32f), new(-170.8f, 138.6f), new(-170.7f, 139.13f), new(-170.7f, 139.72f),
    new(-173f, 143.66f), new(-173.64f, 143.79f), new(-175.04f, 145.11f), new(-175.12f, 145.63f), new(-175.11f, 146.25f),
    new(-175.64f, 146.65f), new(-177.9f, 146.73f), new(-178.3f, 147.24f), new(-178.35f, 148.5f), new(-178.2f, 149.11f),
    new(-177.7f, 149.28f), new(-177.4f, 150.3f), new(-177.42f, 154.14f), new(-177.8f, 154.69f), new(-178.27f, 155f),
    new(-178.27f, 156.72f), new(-178.1f, 157.21f), new(-177.48f, 157.29f), new(-177.39f, 161.87f), new(-177.92f, 162.35f),
    new(-178.42f, 162.72f), new(-178.49f, 163.96f), new(-181.75f, 164.24f), new(-183.27f, 164.21f), new(-183.52f, 162.95f),
    new(-183.72f, 162.36f), new(-183.72f, 148.6f), new(-183.48f, 147.98f), new(-183.57f, 147.4f), new(-183.94f, 146.92f),
    new(-184.79f, 146.88f), new(-195.29f, 142.73f), new(-196.98f, 142.73f), new(-199.24f, 125.26f), new(-195.36f, 125.27f),
    new(-185.17f, 121.25f), new(-184.3f, 121.1f), new(-183.8f, 120.91f), new(-183.72f, 120.33f), new(-183.73f, 105.47f),
    new(-183.52f, 104.09f), new(-183.13f, 103.76f), new(-182.84f, 102.03f), new(-182.85f, 100.22f), new(-183.86f, 99.95f),
    new(-184.39f, 100.16f), new(-184.7f, 100.72f), new(-186.27f, 100.67f), new(-187.11f, 99.84f), new(-187.68f, 99.73f),
    new(-188.92f, 99.96f), new(-189.48f, 99.6f), new(-189.69f, 98.93f), new(-190.18f, 98.58f), new(-190.75f, 98.53f),
    new(-191.7f, 99.32f), new(-192.28f, 99.08f), new(-192.69f, 98.55f), new(-193.16f, 98.22f), new(-193.76f, 98.35f),
    new(-194.37f, 98.55f), new(-195.07f, 98.54f), new(-195.44f, 99.09f), new(-195.3f, 99.71f), new(-195.37f, 100.4f),
    new(-195.69f, 101.03f), new(-196.26f, 101.41f), new(-199.48f, 101.39f), new(-199.87f, 100.81f), new(-200.4f, 100.49f),
    new(-200.4f, 95.54f), new(-199.98f, 95.25f), new(-199.72f, 94.22f), new(-199.46f, 93.67f), new(-199.72f, 93.07f),
    new(-200.16f, 92.71f), new(-200.28f, 87.38f), new(-199.76f, 87.04f), new(-199.66f, 85.22f), new(-199.98f, 84.74f),
    new(-200.25f, 79.54f), new(-199.77f, 79.08f), new(-199.7f, 77.89f), new(-199.5f, 77.39f), new(-200.22f, 76.31f),
    new(-199.67f, 76.32f), new(-199.16f, 76.1f), new(-198.67f, 75.64f), new(-197.59f, 75.02f), new(-197.15f, 74.53f),
    new(-195.08f, 70.68f), new(-195.36f, 70.23f), new(-195.94f, 69.92f), new(-195.9f, 69.41f), new(-195.67f, 68.92f),
    new(-195.8f, 68.34f), new(-195.61f, 67.76f), new(-194.41f, 67.21f), new(-194.12f, 66.76f), new(-193.98f, 66.09f),
    new(-193.57f, 65.55f), new(-193.08f, 65.06f), new(-192.45f, 64.99f), new(-192.14f, 64.55f), new(-191.86f, 63.99f),
    new(-191.48f, 63.64f), new(-191.06f, 63.97f), new(-190.5f, 64.23f), new(-189.93f, 64.24f), new(-188.96f, 63.5f),
    new(-188.33f, 63.26f), new(-186.96f, 63.67f), new(-186.38f, 63.41f), new(-186.25f, 68.11f), new(-186.11f, 68.61f),
    new(-176.95f, 68.7f), new(-176.3f, 68.83f), new(-175.79f, 68.51f), new(-175.75f, 64.71f), new(-170.03f, 64.44f),
    new(-169.54f, 64.21f), new(-169.49f, 63.38f), new(-168.22f, 63.27f), new(-167.76f, 63.05f), new(-167.55f, 62.49f),
    new(-167.51f, 59.96f), new(-167.61f, 59.42f), new(-168.6f, 59.1f), new(-168.58f, 45.17f), new(-167.56f, 44.43f),
    new(-167.39f, 43.78f), new(-166.86f, 43.54f), new(-166.84f, 40.16f), new(-167.93f, 39.95f), new(-168.6f, 39.99f),
    new(-169.16f, 39.88f), new(-169.68f, 40.16f), new(-170.08f, 40.87f), new(-174.25f, 39.15f), new(-174.46f, 38.69f),
    new(-174.63f, 38.08f), new(-175.07f, 37.62f), new(-175.61f, 37.19f), new(-176.14f, 37.06f), new(-176.77f, 37.3f),
    new(-177.4f, 37.01f), new(-177.92f, 36.5f), new(-177.67f, 36.04f), new(-177.21f, 35.57f), new(-176.94f, 35.1f),
    new(-177.25f, 34.62f), new(-179.64f, 32.28f), new(-180.3f, 32.55f), new(-180.61f, 32.15f), new(-181.06f, 31.66f),
    new(-181.7f, 31.61f), new(-182.21f, 31.17f), new(-183.7f, 27.5f), new(-183.41f, 26.95f), new(-183.23f, 26.32f),
    new(-183.35f, 25.69f), new(-183.26f, 25.18f), new(-182.59f, 25.11f), new(-182.41f, 24.64f), new(-182.38f, 23.09f),
    new(-182.11f, 22.59f), new(-182.55f, 21.45f), new(-183.99f, 21.36f), new(-184.39f, 20.82f), new(-184.39f, 19.97f),
    new(-184.2f, 19.5f), new(-183.63f, 19.39f), new(-183.46f, 18.85f), new(-183.22f, 17.65f), new(-183.28f, 17.14f),
    new(-183.83f, 16.76f), new(-182.38f, 13.32f), new(-181.89f, 12.81f), new(-181.31f, 12.51f), new(-180.36f, 11.63f),
    new(-179.7f, 11.51f), new(-179.23f, 11.26f), new(-178.85f, 10.73f), new(-178.47f, 10.38f), new(-178.23f, 10.92f),
    new(-177.66f, 10.86f), new(-173.89f, 9.36f), new(-173.21f, 9.23f), new(-172.73f, 8.94f), new(-172.4f, 8.5f),
    new(-173.54f, 5.59f), new(-173.86f, 5.19f), new(-174.46f, 5.34f), new(-174.19f, 4.8f), new(-170.15f, 3.16f),
    new(-169.92f, 3.66f), new(-169.37f, 3.78f), new(-168.46f, 3.64f), new(-168.12f, 4.23f), new(-164.76f, 4.35f),
    new(-164.17f, 4.29f), new(-163.92f, 3.65f), new(-163.92f, 2.77f), new(-163.01f, 2.62f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.TroianGuard, (uint)OID.TroianRider, (uint)OID.TroianHound];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
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
