using static BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver.CLL1Brionac4thLegionHelldiver;

namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class InfraredBlast(BossModule module) : Components.InterceptTether(module, (uint)AID.InfraredBlast, (uint)TetherID.InfraredBlast)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;
    private DateTime _activation;
    private readonly List<Actor> players = [];
    private BitMask fire;
    private readonly Actor tunnelmachine = module.Enemies((uint)OID.TunnelArmor)[0];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        if (tether.ID == (uint)TetherID.InfraredBlast)
        {
            if (players.Count == 0)
            {
                foreach (var a in WorldState.Actors.Actors.Values)
                {
                    if (a.OID == default && ArenaBottom.Contains(a.Position - ArenaCenterBottom))
                    {
                        players.Add(a);
                    }
                }
            }
            if (_activation == default)
            {
                _activation = WorldState.FutureTime(3.7d);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts = 0;
            _activation = default;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active || _arena.IsBrionacArena)
        {
            return;
        }
        var count = _tethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var side = _tethers[i];
            Arena.AddLine(side.Enemy.Position, side.Player.Position, players.Contains(side.Player) ? Colors.Safe : default);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active || _arena.IsBrionacArena)
        {
            return;
        }
        if (!_tetheredPlayers[slot])
        {
            var count = _tethers.Count;
            var untakenTethers = false;
            for (var i = 0; i < count; ++i)
            {
                if (_tethers[i].Player.OID != default)
                {
                    untakenTethers = true;
                    break;
                }
            }
            if (untakenTethers && !fire[slot])
            {
                hints.Add(hint);
            }
            else if (fire[slot])
            {
                hints.Add("Avoid taking tether!");
            }
        }
    }

    public override (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDBad)
        {
            return null;
        }
        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return null;
        }
        var (player, enemy) = players.Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FireResistanceDownII)
        {
            fire.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FireResistanceDownII)
        {
            fire.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active && !_arena.IsBrionacArena)
        {
            var count = _tethers.Count;
            var forbidden = new List<ShapeDistance>(2);
            var target = tunnelmachine;
            for (var i = 0; i < count; ++i)
            {
                var t = _tethers[i];
                if (t.Player.OID != default || t.Player == actor)
                {
                    var source = t.Enemy;
                    forbidden.Add(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f));
                }
            }
            if (forbidden.Count != 0)
            {
                hints.AddForbiddenZone(new SDIntersection([.. forbidden]), _activation);
            }
        }
    }
}
