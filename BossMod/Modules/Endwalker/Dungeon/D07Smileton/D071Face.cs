namespace BossMod.Endwalker.Dungeon.D07Smileton.D071Face;

public enum OID : uint
{
    Boss = 0x34CF, // R=12.5
    Helper = 0x233C,
    RelativelySmallFace = 0x34D0, // R3.060, x8
}

public enum AID : uint
{
    AutoAttack = 26433, // Boss->player, no cast, single-target
    LinesOfFire = 26421, // Boss->self, 3.0s cast, single-target

    OffMyLawn1 = 26430, // Boss->self, 5.0s cast, single-target
    OffMyLawnKnockback = 27742, // Helper->self, 5.0s cast, range 31 width 30 rect //Knockback 8

    MixedFeelings = 26424, // Helper->self, 7.0s cast, range 60 width 2 rect
    FrownyFace = 26422, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect
    SmileyFace = 26423, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect

    TempersFlare = 26435, // Boss->self, 5.0s cast, range 60 circle //Raidwide
    TemperTemperSpread = 26432, // Helper->player, 5.0s cast, range 5 circle //Spread mechanic

    UnknownAbility1 = 26426, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility2 = 26427, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility3 = 26428, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility4 = 26429, // RelativelySmallFace->self, no cast, single-target
    UpsideDown = 26425, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    SmileyFace = 2763, // 34D0->player, extra=0x1
    FrownyFace = 2764, // 34D0->player, extra=0x1/0x2
    DownForTheCount = 783, // 34D0->player, extra=0xEC7
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon136 = 136, // player
    Icon96 = 96, // player
    Icon137 = 137, // player
}

public enum TetherID : uint
{
    Tether169 = 169, // 34D0->Boss
}
class OffMyLawnKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.OffMyLawnKnockback, 8f, shape: new AOEShapeRect(31f, 15f), kind: Kind.DirForward, stopAtWall: true);
class TemperTemperSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TemperTemperSpread, 5f);
class MixedFeelings(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MixedFeelings, new AOEShapeRect(60f, 1f));

sealed class FaceLines(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(45f, 3f);
    private readonly List<(Actor Caster, bool Smiley)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var hasSmiley = actor.FindStatus((uint)SID.SmileyFace) != null;
        var hasFrowny = actor.FindStatus((uint)SID.FrownyFace) != null;
        for (var i = 0; i < count; ++i)
        {
            var (c, smiley) = _casters[i];
            if (c.CastInfo is not ActorCastInfo cast)
            {
                continue;
            }
            var risky = smiley ? hasSmiley : hasFrowny;
            _aoes.Add(new(_shape, c.Position, cast.Rotation, Module.CastFinishAt(cast), risky ? default : Colors.SafeFromAOE, risky));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SmileyFace or (uint)AID.FrownyFace)
        {
            _casters.Add((caster, spell.Action.ID == (uint)AID.SmileyFace));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SmileyFace or (uint)AID.FrownyFace)
        {
            for (var i = _casters.Count - 1; i >= 0; --i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.RemoveAt(i);
                }
            }
        }
    }
}

class TempersFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.TempersFlare);

class D071FaceStates : StateMachineBuilder
{
    public D071FaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OffMyLawnKnockback>()
            .ActivateOnEnter<MixedFeelings>()
            .ActivateOnEnter<FaceLines>()
            .ActivateOnEnter<TemperTemperSpread>()
            .ActivateOnEnter<TempersFlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10331)]
public class D071Face(WorldState ws, Actor primary) : BossModule(ws, primary, new(-45, -20), new ArenaBoundsSquare(20));
