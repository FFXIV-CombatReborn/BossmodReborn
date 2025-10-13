namespace BossMod.Dawntrail.Quest.Role.BarThePassage.Trash2;

public enum OID : uint
{
    Boss = 0x4660, // R0.5, friendly NPC Tentoawa
    UncannyGedan1 = 0x4668, // R1.65
    UncannyGedan2 = 0x4675, // R1.65
    UncannyMountainBear1 = 0x4669, // R1.7
    UncannyMountainBear2 = 0x4676, // R1.7
    UncannyHuallepen = 0x4667 // R3.2
}

public enum AID : uint
{
    AutoAttack1 = 872, // UncannyGedan1/UncannyHuallepen/UncannyGedan2->allies, no cast, single-target
    AutoAttack2 = 870, // UncannyMountainBear1/UncannyMountainBear2->allies, no cast, single-target
    Teleport = 40966, // UncannyHuallepen->location, no cast, single-target

    FoulBite = 40903, // UncannyGedan1/UncannyGedan2->player/allies, no cast, single-target
    SavageSwipe = 40904, // UncannyMountainBear1/UncannyMountainBear2->self, 3.0s cast, range 9 120-degree cone
    OneOneOneTonzeSwing = 40905, // UncannyHuallepen->self, 3.0s cast, range 10 circle
    OneOneTonzeSwipe = 40906, // UncannyHuallepen->self, 3.0s cast, range 10 120-degree cone

    IceAegis = 40866, // Tentoawa->self, no cast, single-target
    DirtySlashFirst = 40907, // UncannyHuallepen->self, 11.0s cast, range 80 width 70 rect
    DirtySlashRepeat = 40908, // UncannyHuallepen->self, no cast, range 80 width 70 rect
}

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x03 && state == 0x00020001u)
        {
            Arena.Bounds = Trash2.Arena2;
            Arena.Center = Trash2.Arena2Center;
        }
    }
}

sealed class IceAegis(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(5f, 60f.Degrees(), invertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.IceAegis)
        {
            _aoe = [new(cone, caster.Position, caster.Rotation + 180f.Degrees(), default, Colors.SafeFromAOE)];
        }
        else if (id is (uint)AID.DirtySlashFirst or (uint)AID.DirtySlashRepeat)
        {
            if (++NumCasts == 5)
            {
                _aoe = [];
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        hints.Add("Go behind shield!", !aoe.Check(actor.Position));
    }
}

sealed class SavageSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SavageSwipe, new AOEShapeCone(9f, 60f.Degrees()));
sealed class OneOneOneTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OneOneOneTonzeSwing, 10f);
sealed class OneOneTonzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OneOneTonzeSwipe, new AOEShapeCone(10f, 60f.Degrees()));

sealed class Trash2States : StateMachineBuilder
{
    public Trash2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<IceAegis>()
            .ActivateOnEnter<SavageSwipe>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneTonzeSwipe>()
            .Raw.Update = () => !module.PrimaryActor.IsTargetable || module.WorldState.CurrentCFCID != 1016u;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1016u, NameID = 13676u)]
public sealed class Trash2(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 28.5f), new ArenaBoundsRect(34.6f, 23.5f))
{
    public static readonly WPos Arena2Center = new(default, 57f);
    public static readonly ArenaBoundsRect Arena2 = new(34.6f, 24f);
    private static readonly uint[] trash = [(uint)OID.UncannyMountainBear1, (uint)OID.UncannyMountainBear2, (uint)OID.UncannyHuallepen, (uint)OID.UncannyGedan1,
    (uint)OID.UncannyGedan2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, trash);
    }
}
