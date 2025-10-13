namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos Center = new(945f, -945f);
    private static readonly ArenaBoundsSquare squareBounds = new(24f);
    private static readonly ArenaBoundsCircle smallerBounds = new(30f);
    public static readonly ArenaBoundsCircle BigBounds = new(35f);
    private static readonly AOEShapeCustom transitionSquare = new([new Square(Center, 30f)], [new Square(Center, 24f)]);
    private static readonly AOEShapeDonut transitionSmallerBounds = new(30f, 35f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004u)
            {
                Arena.Bounds = BigBounds;
            }
            else if (state == 0x00100001u)
            {
                Arena.Bounds = smallerBounds;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Hieroglyphika)
        {
            _aoe = [new(transitionSquare, Center, default, Module.CastFinishAt(spell))];
        }
        else if (id == (uint)AID.Whorl)
        {
            _aoe = [new(transitionSmallerBounds, Center, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Hieroglyphika)
        {
            Arena.Bounds = squareBounds;
            _aoe = [];
        }
        else if (id == (uint)AID.Whorl)
        {
            Arena.Bounds = smallerBounds;
            _aoe = [];
        }
    }
}

sealed class Sunbeam(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SunbeamAOE, 6f);
sealed class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DestructiveBoltAOE, 6f, 8);

sealed class HandOfTheDestroyer(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HandOfTheDestroyerWrathAOE, (uint)AID.HandOfTheDestroyerJudgmentAOE], new AOEShapeRect(90f, 20f));

sealed class SoaringMinuet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoaringMinuet, new AOEShapeCone(40f, 135f.Degrees()));
sealed class EudaimonEorzea(BossModule module) : Components.CastCounter(module, (uint)AID.EudaimonEorzeaAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962u, NameID = 11301u, SortOrder = 7, PlanLevel = 90)]
public sealed class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.Center, ArenaChanges.BigBounds);
