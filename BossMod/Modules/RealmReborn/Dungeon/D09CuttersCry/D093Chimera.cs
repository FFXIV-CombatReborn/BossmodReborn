namespace BossMod.RealmReborn.Dungeon.D09CuttersCry.D093Chimera;

public enum OID : uint
{
    Chimera = 0x4900, // R3.7
    Cacophony = 0x4901, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Chimera->player, no cast, single-target

    TheLionsBreath = 44241, // Chimera->self, no cast, range 6+R 120-degree cone
    TheDragonsBreath = 44243, // Chimera->self, 2.0s cast, range 6+R 120-degree cone
    TheRamsBreath = 44242, // Chimera->self, 2.0s cast, range 6+R 120,000-degree cone
    TheRamsVoice = 44237, // Chimera->self, 3.0s cast, range 6+R circle
    TheDragonsVoice = 44238, // Chimera->self, 4.5s cast, range 8-30 donut
    Cacophony = 44239, // Chimera->self, 5.0s cast, single-target
    ChaoticChorus = 44240, // Cacophony->self, no cast, range 6 circle
    LightningStormVisual = 44244, // Chimera->self, 3.0s cast, single-target
    LightningStorm = 44245 // Helper->location, 3.0s cast, range 5 circle
}

public enum IconID : uint
{
    Cacophony = 23 // player->self
}

sealed class TheLionsBreath(BossModule module) : Components.Cleave(module, (uint)AID.TheLionsBreath, new AOEShapeCone(9.7f, 60f.Degrees()));
sealed class TheRamsDragonsBreath(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheRamsBreath, (uint)AID.TheDragonsBreath], new AOEShapeCone(9.7f, 60f.Degrees()));
sealed class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9.7f);
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8f, 30f));
sealed class LightningStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningStorm, 5f);

sealed class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private int target = -1;
    private Actor? source;
    private AOEInstance[] _aoe = [];
    private bool aoeInit;
    private static readonly AOEShapeCircle circle = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (!aoeInit)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        aoe.Origin = source!.Position.Quantized();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Cacophony)
        {
            target = Raid.FindSlot(targetID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChaoticChorus)
        {
            target = -1;
            source = null;
            _aoe = [];
            aoeInit = false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Cacophony)
        {
            source = actor;
            _aoe = [new(circle, actor.Position, default, WorldState.FutureTime(10.5d))];
            aoeInit = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot == target && source is Actor s)
        {
            ref var aoe = ref _aoe[0];
            hints.AddForbiddenZone(new SDCircle(s.Position, 10f), aoe.Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot == target)
        {
            hints.Add("Kite the orb!");
        }
    }
}

sealed class D093ChimeraStates : StateMachineBuilder
{
    public D093ChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<TheRamsDragonsBreath>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<Cacophony>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Chimera, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12u, NameID = 1590u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.RealmReborn, SortOrder = 3)]
public sealed class D093Chimera(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(-183f, -234.78f), new(-180.96f, -234.61f),
    new(-175.02f, -233.03f), new(-172.75f, -232.73f), new(-172.08f, -232.76f),
    new(-170.81f, -232.7f), new(-170.16f, -232.61f), new(-169.5f, -232.44f), new(-152.87f, -219.83f), new(-152.69f, -219.2f),
    new(-152.36f, -217.28f), new(-152.34f, -216.62f), new(-152.49f, -215.99f), new(-153.9f, -213.96f), new(-154.04f, -213.04f),
    new(-153.76f, -212.4f), new(-153.56f, -211.71f), new(-152.99f, -206.75f), new(-153.09f, -206.12f), new(-153.26f, -205.52f),
    new(-153.32f, -204.85f), new(-154.1f, -203.11f), new(-155.34f, -201.65f), new(-155.68f, -201.13f), new(-174.56f, -182.54f),
    new(-175.18f, -182.31f), new(-180.92f, -181.39f), new(-181.56f, -181.4f), new(-184.63f, -182.19f), new(-186.26f, -183.14f),
    new(-187.53f, -182.95f), new(-188.84f, -183.06f), new(-202.09f, -188.94f), new(-202.59f, -189.38f), new(-204.08f, -191.55f),
    new(-205.9f, -197.13f), new(-205.94f, -202.42f), new(-206.11f, -203.06f), new(-206.62f, -207.71f), new(-205.36f, -214.27f),
    new(-205.32f, -214.91f), new(-205.53f, -216.9f), new(-205.55f, -217.58f), new(-205.42f, -218.25f), new(-204.95f, -219.51f),
    new(-204.63f, -220.1f), new(-204.26f, -220.64f), new(-193.31f, -229.15f),
    new(-188.3f, -230.61f), new(-186.84f, -231.89f), new(-186.67f, -232.43f), new(-186.19f, -232.91f), new(-185.79f, -233.39f),
    new(-185.6f, -234f), new(-184.38f, -234.63f), new(-183.69f, -234.75f), new(-183f, -234.78f)])]);
}

