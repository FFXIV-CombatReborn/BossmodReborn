namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL04thLegionColossusRubricatus;

public enum OID : uint
{
    Boss = 0x326C // R3.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    MagitekSlashCCWFirst = 24226, // Boss->self, 5.0s cast, range 20 60-degree cone
    MagitekSlashCWFirst = 24225, // Boss->self, 5.0s cast, range 20 60-degree cone
    MagitekSlash = 24227, // Boss->self, no cast, range 20 60-degree cone

    CeruleumVent = 24224, // Boss->self, 5.0s cast, range 40 circle
    MagitekField = 24221 // Boss->self, 5.0s cast, single-target
}

sealed class MagitekSlash(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(20f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rotation = spell.Action.ID switch
        {
            (uint)AID.MagitekSlashCWFirst => -60f.Degrees(),
            (uint)AID.MagitekSlashCCWFirst => 60f.Degrees(),
            _ => default
        };
        if (rotation != default)
        {
            Sequences.Add(new(cone, spell.LocXZ, spell.Rotation, rotation, Module.CastFinishAt(spell), 1.6d, 6));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MagitekSlashCWFirst or (uint)AID.MagitekSlashCCWFirst or (uint)AID.MagitekSlash)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // stay close to the origin
        if (Sequences.Count != 0)
        {
            var sequences = CollectionsMarshal.AsSpan(Sequences);
            ref readonly var sequence = ref sequences[0];
            base.AddAIHints(slot, actor, assignment, hints);
            hints.AddForbiddenZone(new SDInvertedCircle(sequence.Origin, 5f), sequence.NextActivation);
        }
    }
}

sealed class CeruleumVent(BossModule module) : Components.RaidwideCast(module, (uint)AID.CeruleumVent);

sealed class DAL04thLegionColossusRubricatusStates : StateMachineBuilder
{
    public DAL04thLegionColossusRubricatusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CeruleumVent>()
            .ActivateOnEnter<MagitekSlash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 9432, SortOrder = 4)]
public sealed class DAL04thLegionColossusRubricatus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos rect = new(650f, -556f);
    private static readonly ArenaBoundsCustom arena = new([new Rectangle(new(650f, -546.1f), 18.81f, 18.51f), new Rectangle(rect, 2.85f, 17.6f), new Rectangle(rect, 2.6f, 18.56f)]);
}
