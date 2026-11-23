namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class DragonsDescent(BossModule module) : Components.GenericKnockback(module, (uint)AID.DragonsDescent)
{
    private Actor? _source;
    private DateTime _activation;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_source != null && _source != actor)
        {
            _oneKnockback[0] = new(_source.Position, 13f, _activation, ignoreImmunes: true);
            return _oneKnockback;
        }
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
        {
            _source = actor;
            _activation = WorldState.FutureTime(8.2f);
        }
    }
}

sealed class DoubleMeteor(BossModule module) : Components.UniformStackSpread(module, default, 15f) // TODO: verify falloff
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DoubleMeteor)
            AddSpread(actor, WorldState.FutureTime(11.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DoubleMeteorAOE1 or (uint)AID.DoubleMeteorAOE2)
        {
            for (var i = Spreads.Count - 1; i >= 0; --i)
            {
                if (Spreads[i].Target.InstanceID == spell.MainTargetID)
                    Spreads.RemoveAt(i);
            }
        }
    }
}

sealed class Explosion(BossModule module) : BossComponent(module)
{
    public bool Done;
    private BitMask _forbidden;
    private Actor? _towerTH;
    private Actor? _towerDD;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var tower = _forbidden[slot] ? null : actor.Class.IsSupport() ? _towerTH : _towerDD;
        if (tower != null)
            hints.Add("Soak the tower!", !actor.Position.InCircle(tower.Position, 4f));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawTower(_towerTH, !_forbidden[pcSlot] && pc.Class.IsSupport());
        DrawTower(_towerDD, !_forbidden[pcSlot] && pc.Class.IsDD());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExplosionDouble:
                _towerTH = caster;
                break;
            case (uint)AID.ExplosionTriple:
                _towerDD = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExplosionDouble:
                _towerTH = null;
                Done = true;
                break;
            case (uint)AID.ExplosionTriple:
                _towerDD = null;
                Done = true;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.DoubleMeteor or (uint)IconID.DragonsDescent)
            _forbidden[Raid.FindSlot(actor.InstanceID)] = true;
    }

    private void DrawTower(Actor? tower, bool safe)
    {
        if (tower != null)
            Arena.ZoneCircleOutline(tower.Position, 4f, safe ? Colors.Safe : 0, 2f);
    }
}

sealed class Cauterize(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Cauterize)
{
    private readonly AOEShapeRect rect = new(50f, 6f);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            CurrentBaits.Add(new(source, target, rect));
        }
    }
}
