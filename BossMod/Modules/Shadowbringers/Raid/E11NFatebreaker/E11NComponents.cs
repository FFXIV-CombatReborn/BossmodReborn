namespace BossMod.Shadowbringers.Raid.E11NFatebreaker;

sealed class BurnishedGlory(BossModule module) : Components.RaidwideCast(module, (uint)AID.BurnishedGlory);
sealed class PowderMark(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PowderMark);

sealed class BurnMark(BossModule module) : Components.UniformStackSpread(module, default, 10f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.PowderMark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.PowderMark)
            Spreads.RemoveAll(s => s.Target == actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BurnMark)
        {
            ++NumFinishedSpreads;
            Spreads.Clear();
        }
    }
}

sealed class BurntStrike(BossModule module) : Components.SimpleAOEGroups(
    module,
    [(uint)AID.BurntStrikeFire, (uint)AID.BurntStrikeLightning, (uint)AID.BurntStrikeHoly,
     (uint)AID.ImageBurntStrikeFire, (uint)AID.ImageBurntStrikeLightning],
    new AOEShapeRect(80f, 5f));

sealed class Burnout(BossModule module) : Components.SimpleAOEGroups(
    module,
    [(uint)AID.Burnout, (uint)AID.ImageBurnout],
    new AOEShapeRect(80f, 10f));

sealed class ShiningBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShiningBlade, 6f);

sealed class Blastburn(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _caster;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo is var cast && cast != null)
        {
            var direction = cast.Rotation;
            var kind = direction.ToDirection().OrthoL().Dot(actor.Position - _caster.Position) > 0f ? Kind.DirLeft : Kind.DirRight;
            return new Knockback[1] { new(_caster.Position, 15f, Module.CastFinishAt(cast), null, direction, kind, ignoreImmunes: true) };
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blastburn or (uint)AID.ImageBlastburn)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blastburn or (uint)AID.ImageBlastburn)
        {
            ++NumCasts;
            _caster = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster?.CastInfo is var cast && cast != null)
            hints.AddForbiddenZone(new SDInvertedRect(_caster.Position, cast.Rotation, 40f, 40f, 7f), Module.CastFinishAt(cast));
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class FireStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FireStack, (uint)AID.Sinsmoke, 6f, 8d, 4, 8);
sealed class HolyBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.HolyBait, (uint)AID.Sinsight, 6f, 8d);

sealed class BoundOfFaithTethers(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private TetherID _tether;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor != _target)
            return;

        switch (_tether)
        {
            case TetherID.Fire:
                hints.Add("Fire tether: stack with party!", false);
                break;
            case TetherID.Lightning:
                hints.Add("Lightning tether on you!", false);
                break;
            case TetherID.Holy:
                hints.Add("Holy tether: bait away from party!", false);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Boss && tether.ID is (uint)TetherID.Fire or (uint)TetherID.Lightning or (uint)TetherID.Holy)
        {
            _target = WorldState.Actors.Find(tether.Target);
            _tether = (TetherID)tether.ID;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Boss)
        {
            _target = null;
            _tether = default;
        }
    }
}

sealed class BrightfireSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrightfireSmall, 5f);
sealed class BrightfireLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrightfireLarge, 10f);
sealed class ResoundingCrack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ResoundingCrack, new AOEShapeCone(40f, 135f.Degrees()));

sealed class PrismaticDeception(BossModule module) : Components.GenericAOEs(module, (uint)AID.BlastingZone)
{
    private static readonly AOEShapeRect Shape = new(50f, 8f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.FatebreakersImage && modelState == 4)
            _aoes.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(14.1d), actorID: actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
