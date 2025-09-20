namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D100TroianScavanger;

public enum OID : uint
{
    Boss = 0x397D, // R3.9
    TroianSentry = 0x397C, // R1.6
    TroianPawn = 0x397E // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 6497, // TroianPawn/TroianSentry->player, no cast, single-target
    AutoAttack2 = 28630, // Boss->player, no cast, single-target

    Condemnation = 30212, // TroianPawn->self, 3.0s cast, range 6 90-degree cone
    EvilPhlegm = 30214, // TroianSentry->location, 3.0s cast, range 5 circle
    DarkArrivisme = 30213 // Boss->location, 3.0s cast, range 5 circle
}

sealed class Condemnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Condemnation, new AOEShapeCone(6f, 45f.Degrees()));
sealed class EvilPhlegm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilPhlegm, 5f);
sealed class DarkArrivisme(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkArrivisme, 5f);

sealed class D100TroianScavangerStates : StateMachineBuilder
{
    public D100TroianScavangerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Condemnation>()
            .ActivateOnEnter<EvilPhlegm>()
            .ActivateOnEnter<DarkArrivisme>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D100TroianScavanger.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11358, SortOrder = 1)]
public sealed class D100TroianScavanger(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? arena1.Center : arena2.Center, IsArena1(primary) ? arena1 : arena2)
{
    private static bool IsArena1(Actor primary) => primary.PosRot.Z > 170f;
    private static readonly WPos[] vertices1 = [new(58.65f, 113.75f), new(59.33f, 113.92f), new(60.07f, 115.82f), new(60.42f, 116.25f), new(60.46f, 116.87f),
    new(60.72f, 117.46f), new(62.58f, 119.22f), new(63.19f, 119.03f), new(63.58f, 118.63f), new(63.96f, 118.98f),
    new(64.48f, 119.34f), new(65.69f, 119.96f), new(66.19f, 120.47f), new(70.77f, 125.4f), new(73.23f, 126.28f),
    new(74.46f, 126.44f), new(75.12f, 126.42f), new(78.81f, 125.5f), new(79.44f, 125.4f), new(81f, 125.39f),
    new(81.46f, 125.61f), new(81.82f, 126.09f), new(82.33f, 126.11f), new(82.11f, 127.29f), new(82.09f, 128f),
    new(82.34f, 128.53f), new(82.83f, 128.89f), new(83.48f, 128.92f), new(83.8f, 128.49f), new(83.8f, 126.88f),
    new(84.24f, 126.37f), new(84.82f, 126.1f), new(84.9f, 125.58f), new(84.88f, 124.97f), new(85.04f, 124.31f),
    new(85.96f, 123.54f), new(86.27f, 123f), new(86.43f, 122.44f), new(86.4f, 121.78f), new(87.6f, 120.18f),
    new(88.14f, 119.76f), new(88.78f, 119.79f), new(89.08f, 119.33f), new(89.22f, 118.75f), new(89.66f, 118.21f),
    new(90.18f, 118.27f), new(90.76f, 118.09f), new(91.47f, 116.99f), new(92.02f, 116.68f), new(92.64f, 116.88f),
    new(93.2f, 116.79f), new(93.69f, 116.31f), new(94.64f, 115.88f), new(95.98f, 115.85f), new(96.65f, 115.72f),
    new(97.34f, 115.86f), new(98.67f, 115.85f), new(99.81f, 116.04f), new(100.14f, 116.46f), new(100.74f, 116.59f),
    new(101.25f, 116.12f), new(101.8f, 116.15f), new(102.68f, 115.48f), new(102.98f, 115.95f), new(102.8f, 116.65f),
    new(102.95f, 117.28f), new(103.37f, 117.76f), new(103.94f, 118.02f), new(104.55f, 118.01f), new(105.09f, 117.76f),
    new(105.49f, 117.32f), new(106.02f, 117.37f), new(106.7f, 117.36f), new(107.36f, 117.44f), new(108.43f, 118.24f),
    new(108.71f, 118.84f), new(109.05f, 119.38f), new(109.48f, 119.92f), new(109.8f, 120.52f), new(110.54f, 121.38f),
    new(111.76f, 122.02f), new(112.44f, 123.22f), new(112.6f, 123.71f), new(112.77f, 125.84f), new(113.13f, 126.3f),
    new(113.68f, 126.59f), new(113.88f, 127.92f), new(113.91f, 128.42f), new(113.52f, 128.93f), new(112.95f, 129.18f),
    new(112.46f, 129.54f), new(112.3f, 131.46f), new(112.33f, 131.99f), new(112.86f, 132.2f), new(113.52f, 132.25f),
    new(113.8f, 132.83f), new(113.07f, 135.38f), new(112.48f, 135.64f), new(111.05f, 138.27f), new(110.72f, 139.46f),
    new(109.34f, 140.93f), new(108.78f, 141.38f), new(108.11f, 141.41f), new(107.67f, 141.71f), new(107.39f, 142.29f),
    new(106.94f, 142.78f), new(106.28f, 143.18f), new(106.07f, 142.67f), new(105.7f, 142.2f), new(105.11f, 141.94f),
    new(104.55f, 141.85f), new(104.01f, 142.18f), new(103.6f, 142.62f), new(103.42f, 143.18f), new(103.51f, 143.79f),
    new(102.9f, 144.07f), new(102.25f, 143.95f), new(101.63f, 143.98f), new(101.14f, 144.21f), new(100.78f, 144.77f),
    new(100.66f, 145.39f), new(100.83f, 145.99f), new(100.32f, 146.42f), new(99.72f, 146.59f), new(99.58f, 149.56f),
    new(99.78f, 150.04f), new(100.3f, 150.12f), new(100.78f, 149.89f), new(100.83f, 147.84f), new(101.64f, 147.54f),
    new(102.28f, 147.78f), new(102.74f, 148.21f), new(102.74f, 149.3f), new(102.87f, 149.8f), new(104.06f, 149.9f),
    new(104.73f, 150.15f), new(104.81f, 150.67f), new(105.09f, 151.26f), new(105.54f, 151.6f), new(106.11f, 151.7f),
    new(106.77f, 151.53f), new(107.24f, 151.24f), new(107.49f, 150.67f), new(107.52f, 149.51f), new(108.03f, 149.51f),
    new(110.69f, 150.1f), new(111.36f, 150.07f), new(111.86f, 149.72f), new(116.01f, 149.53f), new(116.5f, 150.07f),
    new(116.47f, 153.94f), new(116.01f, 154.47f), new(115.87f, 155.05f), new(116.07f, 155.62f), new(116.48f, 156.15f),
    new(116.48f, 159.78f), new(116.09f, 160.35f), new(115.87f, 160.88f), new(116f, 161.51f), new(116.4f, 161.94f),
    new(116.49f, 165.75f), new(116.1f, 166.32f), new(115.85f, 166.86f), new(115.95f, 167.45f), new(116.39f, 167.95f),
    new(116.49f, 171.77f), new(115.91f, 172.09f), new(115.87f, 172.72f), new(115.93f, 173.37f), new(116.45f, 173.99f),
    new(115.27f, 174.29f), new(114.83f, 174.68f), new(114.57f, 175.21f), new(114.59f, 175.78f), new(114.82f, 176.33f),
    new(114.46f, 177.6f), new(114.65f, 178.16f), new(112.19f, 178.5f), new(111.63f, 178.07f), new(111.09f, 177.83f),
    new(110.48f, 177.99f), new(110.03f, 178.4f), new(109.89f, 177.23f), new(109.64f, 176.65f), new(109.2f, 176.24f),
    new(108.59f, 176.09f), new(107.98f, 176.18f), new(107.48f, 176.53f), new(106.29f, 176.57f), new(105.74f, 176.76f),
    new(105.31f, 177.18f), new(105.09f, 177.77f), new(103.17f, 178.08f), new(102.74f, 178.57f), new(102.22f, 178.98f),
    new(96.2f, 179.19f), new(95.65f, 178.82f), new(95.22f, 178.33f), new(94.76f, 178.08f), new(93.9f, 178.08f),
    new(93.26f, 177.91f), new(92.62f, 177.9f), new(92.13f, 178.27f), new(88.65f, 178.5f), new(87.96f, 178.41f),
    new(87.48f, 177.95f), new(86.91f, 177.87f), new(86.36f, 178.08f), new(85.83f, 178.5f), new(82.14f, 178.48f),
    new(81.55f, 178f), new(81.58f, 174.1f), new(82f, 173.52f), new(82.14f, 172.9f), new(81.91f, 172.35f),
    new(81.56f, 171.83f), new(81.56f, 168.17f), new(81.96f, 167.58f), new(82.14f, 167.03f), new(81.99f, 166.47f),
    new(81.56f, 166.01f), new(81.56f, 162.34f), new(82.09f, 161.25f), new(82.07f, 160.66f), new(81.78f, 160.13f),
    new(81.57f, 159.51f), new(81.57f, 156.25f), new(81.93f, 155.67f), new(82.12f, 155.07f), new(82.01f, 154.49f),
    new(81.55f, 154.01f), new(81.55f, 150.13f), new(81.97f, 149.59f), new(85.89f, 149.51f), new(86.41f, 149.95f),
    new(86.95f, 150.15f), new(87.54f, 149.99f), new(87.97f, 149.65f), new(88.27f, 150.8f), new(88.74f, 151.25f),
    new(89.31f, 151.46f), new(90.57f, 151.34f), new(91.15f, 151.59f), new(91.78f, 151.62f), new(92.32f, 151.4f),
    new(92.74f, 150.96f), new(92.94f, 150.4f), new(93.53f, 150.04f), new(94.79f, 149.9f), new(95.41f, 149.66f),
    new(96.06f, 149.75f), new(96.63f, 149.68f), new(97.16f, 149.37f), new(97.49f, 148.83f), new(97.58f, 148.25f),
    new(97.56f, 147.56f), new(97.94f, 147.17f), new(97.87f, 146.62f), new(96.04f, 146.43f), new(95.51f, 146.06f),
    new(95.27f, 145.46f), new(95.26f, 144.62f), new(94.89f, 144.11f), new(93.85f, 144.11f), new(93.42f, 143.82f),
    new(93.56f, 143.16f), new(93.56f, 142.55f), new(93.37f, 141.97f), new(93f, 141.6f), new(92.4f, 141.41f),
    new(91.76f, 141.45f), new(91.2f, 141.73f), new(90.57f, 141.64f), new(89.45f, 140.95f), new(88.96f, 140.48f),
    new(88.64f, 139.91f), new(88.4f, 139.37f), new(87.3f, 138.74f), new(86.69f, 138.47f), new(86.23f, 137.97f),
    new(85.9f, 137.42f), new(85.68f, 136.81f), new(85.51f, 135.58f), new(85.05f, 135.11f), new(85.1f, 134.44f),
    new(84.9f, 133.95f), new(84.24f, 133.71f), new(84.43f, 133.22f), new(84.52f, 132.64f), new(84.41f, 132.05f),
    new(84.2f, 131.51f), new(83.72f, 131.17f), new(83.21f, 131.04f), new(82.75f, 131.35f), new(82.7f, 132.78f),
    new(82.4f, 133.39f), new(82.04f, 133.89f), new(81.58f, 134.13f), new(81.17f, 134.65f), new(78.59f, 135.15f),
    new(77.92f, 135.16f), new(75.97f, 134.8f), new(75.32f, 134.55f), new(72.93f, 133.43f), new(72.3f, 133.23f),
    new(71.63f, 133.09f), new(71.01f, 133.18f), new(70.4f, 133.33f), new(69.86f, 133.63f), new(67.9f, 135.19f),
    new(67.48f, 134.86f), new(66.99f, 134.68f), new(65.36f, 135.88f), new(64.91f, 136.32f), new(64.5f, 136.88f),
    new(64.01f, 137.32f), new(63.96f, 137.9f), new(63.77f, 138.37f), new(62.7f, 139.33f), new(61.93f, 140.32f),
    new(52.89f, 140.71f), new(52.32f, 139.64f), new(45.42f, 134.07f), new(44.79f, 133.9f), new(44.2f, 133.84f),
    new(44.41f, 124.87f), new(44.97f, 124.69f), new(45.52f, 124.31f), new(45.87f, 123.85f), new(47.9f, 118.39f),
    new(48.22f, 117.81f), new(48.6f, 117.29f), new(48.92f, 116.11f), new(50.49f, 114.04f)];
    private static readonly WPos[] vertices2 = [new(169.07f, 152.18f), new(171.16f, 152.21f), new(171.55f, 152.72f), new(171.72f, 153.4f), new(171.96f, 153.86f),
    new(173.07f, 153.9f), new(173.68f, 154.07f), new(174.32f, 154.08f), new(174.83f, 153.8f), new(178.68f, 153.5f),
    new(178.94f, 153.95f), new(178.9f, 154.68f), new(179.01f, 155.32f), new(179.37f, 155.8f), new(179.87f, 156.07f),
    new(180.43f, 156.15f), new(180.98f, 155.97f), new(182.24f, 155.7f), new(182.81f, 155.53f), new(183.45f, 155.16f),
    new(183.18f, 156.35f), new(183.32f, 156.93f), new(183.64f, 157.46f), new(184.28f, 157.64f), new(184.93f, 157.63f),
    new(185.49f, 157.5f), new(185.32f, 158.08f), new(184.94f, 158.61f), new(184.9f, 159.21f), new(185.17f, 159.78f),
    new(185.48f, 163.78f), new(185.07f, 164.38f), new(184.85f, 164.95f), new(185.01f, 165.53f), new(185.48f, 166.01f),
    new(185.47f, 169.79f), new(185.07f, 170.38f), new(184.88f, 170.91f), new(184.99f, 171.51f), new(185.38f, 171.94f),
    new(185.47f, 175.83f), new(185.06f, 176.4f), new(184.85f, 176.97f), new(185.02f, 177.54f), new(185.48f, 178f),
    new(185.48f, 181.83f), new(185.07f, 182.35f), new(181.35f, 182.48f), new(180.73f, 182.12f), new(180.14f, 181.88f),
    new(179.52f, 181.97f), new(179.06f, 182.34f), new(175.29f, 182.49f), new(174.3f, 181.91f), new(173.67f, 181.93f),
    new(172.29f, 182.1f), new(171.78f, 182.32f), new(171.73f, 184.37f), new(170.49f, 184.59f), new(169.21f, 185.04f),
    new(169.06f, 185.74f), new(170.24f, 186.4f), new(170.14f, 189.14f), new(170.33f, 190.41f), new(170.65f, 191.6f),
    new(170.86f, 192.18f), new(171.81f, 193.92f), new(172.28f, 195.22f), new(172.52f, 197.24f), new(172.16f, 200.5f),
    new(169.47f, 210.34f), new(169.39f, 211.03f), new(169.03f, 211.39f), new(168.34f, 212.57f), new(167.7f, 212.83f),
    new(163.86f, 212.44f), new(163.37f, 212.03f), new(162.68f, 211.93f), new(162.14f, 210f), new(161.87f, 209.42f),
    new(161.54f, 208.86f), new(161.15f, 208.34f), new(160.15f, 207.52f), new(159.63f, 207.17f), new(159.04f, 207.11f),
    new(158.4f, 206.9f), new(157.88f, 206.82f), new(157.22f, 206.97f), new(155.34f, 207.06f), new(154.68f, 207.04f),
    new(153.43f, 207.38f), new(149.89f, 208.8f), new(149.27f, 208.96f), new(146.02f, 209.09f), new(145.59f, 209.5f),
    new(145.56f, 210.14f), new(145.6f, 210.67f), new(145.06f, 210.63f), new(144.63f, 210.95f), new(144.6f, 214f),
    new(143.86f, 215.84f), new(143.78f, 216.4f), new(144.09f, 216.94f), new(144.55f, 217.13f), new(144.02f, 217.54f),
    new(143.62f, 218.02f), new(143.46f, 218.58f), new(143.45f, 219.24f), new(142.43f, 220.04f), new(141.76f, 220.01f),
    new(141.13f, 219.91f), new(140.65f, 220.12f), new(139.62f, 220.88f), new(136.98f, 221.46f), new(136.47f, 221.7f),
    new(135.98f, 222.09f), new(135.73f, 222.54f), new(135.78f, 223.05f), new(135.24f, 223.17f), new(134.55f, 223.16f),
    new(132.01f, 222.59f), new(131.37f, 222.52f), new(130.77f, 222.72f), new(130.08f, 222.87f), new(128.02f, 223.01f),
    new(127.6f, 222.53f), new(126.4f, 222.4f), new(125.72f, 222.25f), new(125.39f, 221.78f), new(124.87f, 221.42f),
    new(123f, 221.06f), new(122.45f, 220.76f), new(121.82f, 220.74f), new(121.18f, 220.77f), new(120.89f, 221.24f),
    new(112.64f, 205.63f), new(113.12f, 197.15f), new(114.25f, 192.7f), new(114.84f, 192.42f), new(118.21f, 192.41f),
    new(118.75f, 192.48f), new(119.24f, 192.72f), new(119.74f, 193.26f), new(120.4f, 193.19f), new(121.07f, 193.41f),
    new(121.65f, 193.78f), new(121.84f, 194.44f), new(122.34f, 194.57f), new(124.67f, 193.54f), new(124.83f, 193.04f),
    new(124.85f, 191.72f), new(125.27f, 191.4f), new(125.91f, 191.1f), new(127.91f, 190.49f), new(129.99f, 190.18f),
    new(130.69f, 190.17f), new(131.31f, 190.54f), new(131.29f, 191.07f), new(131.79f, 191.43f), new(132.37f, 191.57f),
    new(134.96f, 190.97f), new(135.58f, 191.24f), new(136.15f, 191.67f), new(136.94f, 192.81f), new(137.41f, 193.14f),
    new(138.06f, 193.3f), new(139.33f, 193.33f), new(140.68f, 193.78f), new(141.14f, 194.02f), new(141.75f, 194.11f),
    new(142.32f, 194.47f), new(144.03f, 194.56f), new(144.29f, 195.19f), new(143.9f, 195.77f), new(143.62f, 196.34f),
    new(143.74f, 196.87f), new(144.1f, 197.25f), new(144.43f, 197.82f), new(144.43f, 198.53f), new(143.97f, 199.07f),
    new(143.69f, 199.64f), new(143.84f, 200.2f), new(144.28f, 200.56f), new(144.58f, 201.18f), new(144.37f, 202.57f),
    new(144.68f, 203.09f), new(145.28f, 203.34f), new(145.26f, 203.88f), new(145.3f, 204.51f), new(145.75f, 204.89f),
    new(147.01f, 204.83f), new(149.7f, 204.83f), new(150.29f, 204.63f), new(150.96f, 204.5f), new(155.51f, 202.13f),
    new(156.8f, 201.77f), new(158.04f, 201.51f), new(158.72f, 201.47f), new(161.33f, 201.71f), new(162.03f, 201.69f),
    new(162.66f, 201.59f), new(163.9f, 201.12f), new(165.05f, 200.41f), new(165.44f, 199.95f), new(167.22f, 197.19f),
    new(167.62f, 195.33f), new(167.65f, 194.65f), new(167.52f, 194.08f), new(167.07f, 192.97f), new(166.35f, 191.92f),
    new(166.03f, 191.32f), new(165.6f, 190.04f), new(165.58f, 189.39f), new(165.5f, 188.76f), new(166.08f, 188.37f),
    new(166.75f, 188.25f), new(167.08f, 187.86f), new(167.08f, 185.75f), new(166.97f, 185.2f), new(166.49f, 184.95f),
    new(165.84f, 184.76f), new(164.66f, 184.6f), new(164.44f, 183.97f), new(164.51f, 183.26f), new(164.48f, 182.57f),
    new(164.04f, 182.14f), new(162.81f, 182.09f), new(162.13f, 181.88f), new(161.53f, 181.96f), new(161.07f, 182.34f),
    new(157.81f, 182.49f), new(156.51f, 181.98f), new(155.89f, 181.87f), new(155.34f, 182.1f), new(154.82f, 182.5f),
    new(151f, 182.48f), new(150.57f, 181.88f), new(150.57f, 178.19f), new(150.92f, 177.64f), new(151.16f, 177.09f),
    new(151.03f, 176.52f), new(150.65f, 176.05f), new(150.55f, 175.56f), new(150.57f, 171.96f), new(151.02f, 171.5f),
    new(151.11f, 170.87f), new(150.9f, 170.33f), new(150.55f, 169.77f), new(150.55f, 167.03f), new(150.98f, 165.65f),
    new(151.14f, 164.96f), new(150.94f, 164.38f), new(150.56f, 163.82f), new(150.56f, 160.66f), new(150.65f, 160.16f),
    new(151.23f, 159.83f), new(151.26f, 159.23f), new(151.05f, 158.58f), new(150.66f, 158.1f), new(150.55f, 154.04f),
    new(151.02f, 153.5f), new(154.92f, 153.5f), new(155.45f, 153.98f), new(156.08f, 154.14f), new(156.61f, 153.94f),
    new(157.05f, 153.51f), new(160.9f, 153.51f), new(161.42f, 153.95f), new(162.02f, 154.14f), new(162.64f, 153.94f),
    new(163.9f, 153.9f), new(164.26f, 153.37f), new(164.46f, 152.72f), new(164.94f, 152.18f), new(169.07f, 152.18f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)]);
    private static readonly ArenaBoundsCustom arena2 = new([new PolygonCustom(vertices2)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.TroianSentry, (uint)OID.TroianPawn];

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
