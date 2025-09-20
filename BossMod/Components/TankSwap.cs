namespace BossMod.Components;

// generic tank-swap component for multi-hit tankbusters, with optional aoe
// assume that target of the first hit is locked when mechanic starts, then subsequent targets are selected based on who the boss targets
// TODO: this version assumes that boss cast and first-hit are potentially from different actors; the target lock could also be things like icons, etc - generalize more...
[SkipLocalsInit]
public class TankSwap(BossModule module, uint bossCast, uint firstCast, uint subsequentHit, double delay1, double delay2, AOEShape? shape = null, bool centerAtTarget = false) : GenericBaitAway(module, centerAtTarget: centerAtTarget, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public TankSwap(BossModule module, uint bossCast, uint firstCast, uint subsequentHit, double delay1, double delay2, float radius, bool centerAtTarget = true) : this(module, bossCast, firstCast, subsequentHit, delay1, delay2, new AOEShapeCircle(radius), centerAtTarget) { }

    protected Actor? _source;
    protected ulong _prevTarget; // before first cast, this is the target of the first hit
    public readonly AOEShape? Shape = shape;
    public readonly double Delay1 = delay1;
    public readonly double Delay2 = delay2;
    public readonly uint BossCast = bossCast;
    public readonly uint FirstCast = firstCast;
    public readonly uint SubsequentHit = subsequentHit;

    public override void Update()
    {
        if (_source != null && Shape != null)
        {
            var count = CurrentBaits.Count;
            if (count != 0 && WorldState.Actors.Find(_source.TargetID) is Actor t)
            {
                if (count == 1 && NumCasts == 1)
                {
                    CurrentBaits.Ref(0).Target = t;
                }
                else if (count == 2)
                {
                    CurrentBaits.Ref(1).Target = t;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source?.TargetID == _prevTarget && actor.Role == Role.Tank)
        {
            hints.Add(_prevTarget != actor.InstanceID ? "Provoke!" : "Pass aggro!");
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == BossCast)
        {
            _source = caster;
            if (Shape != null && WorldState.Actors.Find(_source.TargetID) is Actor t)
            {
                AddBait(Delay1);
                AddBait(Delay2);
                void AddBait(double delay = default) => CurrentBaits.Add(new(caster, t, Shape, Module.CastFinishAt(spell, delay)));
            }
        }
        else if (id == FirstCast)
        {
            NumCasts = 0;
            _prevTarget = spell.TargetID;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == FirstCast || id == SubsequentHit)
        {
            ++NumCasts;
            _prevTarget = spell.MainTargetID == caster.InstanceID && spell.Targets.Count != 0 ? spell.Targets.Ref(0).ID : spell.MainTargetID;
            if (CurrentBaits.Count != 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }
}
