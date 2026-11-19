namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class FlarePlay(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlarePlay);

sealed class Vidohunir(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.Vidohunir1, 5f);

sealed class EmpiricalResearch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmpiricalResearch1, new AOEShapeRect(80f, 6f));

sealed class SuperiorStoneIITelegraph(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SuperiorStoneII1, new AOEShapeRect(21f, 6.5f));

sealed class SuperiorStoneIIArena(BossModule module) : BossComponent(module)
{
    public sealed class Cutoff(WPos wpos, Angle angle, Vector4 posrot)
    {
        public WPos Position = wpos;
        public Angle Rotation = angle;
        public Vector4 PosRot = posrot;
    }

    private readonly List<Cutoff> cutoffs = [];

    private readonly Rectangle _basearena = new(new(0f, -720f), 24f, 30f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuperiorStoneII1)
        {
            cutoffs.Add(new(caster.Position, caster.Rotation, caster.PosRot));
            //Service.Log($"Adding cutoff {caster.Position}, {caster.Rotation}");
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuperiorStoneII)
        {
            var _rects = new Rectangle[cutoffs.Count];
            for (var i = 0; i < cutoffs.Count; ++i)
            {
                var pos = cutoffs[i].Position + cutoffs[i].Rotation.ToDirection() * 10.5f;
                _rects[i] = new Rectangle(pos, 6.1f, 10.5f, cutoffs[i].Rotation); //This functions but could probably be more accurate.
                //Service.Log($"Adding Rectangle {_rects[i]}");
            }

            var newbounds = new ArenaBoundsCustom([_basearena], _rects);
            Arena.Bounds = newbounds;
        }
        if (spell.Action.ID == (uint)AID.GroundbreakingQuake1)
        {
            Arena.Bounds = new ArenaBoundsRect(24f, 30f);
            cutoffs.Clear();
        }
    }
}

sealed class GroundBreakingQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundbreakingQuake1, new AOEShapeRect(30f, 6f));

/*
 * Use tethers to trace the path from one circumscribed fire to the next.
 * It is kind of like a chasing AOE except the tether isn't attached to a pc, it leads to next AOE donut.
 */
sealed class DiagrammaticDoorway(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    /*
     * convert _predicted to ReadOnlySpan. If empty return empty span, else return only 0th index so aoe does not overlap.
     */
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Count != 0 ? CollectionsMarshal.AsSpan(_predicted)[..1] : [];

    private static readonly AOEShapeDonut donut = new(6f, 70f);

    private void AddNext(ulong targetID)
    {
        if (_predicted.Count != 0 && WorldState.Actors.Find(targetID) is Actor target)
        {
            _predicted.Add(new(donut, target.Position, default, _predicted[^1].Activation.AddSeconds(3.1d)));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch ((uint)(TetherID)tether.ID)
        {
            case (uint)TetherID.Purple:
                // First cast is a long cast, else normal short cast.
                if (_predicted.Count == 0)
                {
                    _predicted.Add(new(donut, source.Position, default, WorldState.FutureTime(10.1d)));
                }
                AddNext(tether.Target);
                break;
            case (uint)TetherID.PurpleCircle:
                AddNext(tether.Target);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CircumscribedFire or (uint)AID.CircumscribedFire1)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

sealed class LocalizedBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LocalizedBlizzard, new AOEShapeCircle(10f));

sealed class ThunderAndError(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ThunderAndError, 5f);

sealed class SmallSpecimen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmallSpecimen, new AOEShapeCircle(6f));

sealed class LargeSpecimen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LargeSpecimen, new AOEShapeCircle(13f));

sealed class StardustSpecimen(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.StardustSpecimen, 6f);

sealed class Shockwave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Shockwave);

sealed class FallingRubble(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble, new AOEShapeRect(35f, 5f));

sealed class FallingRubble1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble1, new AOEShapeCircle(12f));

sealed class FallingRubble2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble2, new AOEShapeCircle(8f));

sealed class FallingRubble3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble3, new AOEShapeRect(25f, 3f));

sealed class FinalExam(BossModule module) : Components.UniformStackSpread(module, 6f, default)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FinalExam1 && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            NumCasts = 0;
            AddStack(target, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FinalExam1 or (uint)AID.FinalExam2 && ++NumCasts >= 3)
        {
            Stacks.Clear();
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team, HerStolenLight", PrimaryActorOID = (uint)OID.ShantottoTheDemon, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14778u)]
public sealed class A30Shantoto(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(24f, 30f))
{
    public static readonly WPos ArenaCenter = new(0f, -720f);
}
