namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 4 wreath of thorns
sealed class WreathOfThorns4(BossModule module) : BossComponent(module)
{
    public bool ReadyToBreak;
    private readonly uint[] _playerIcons = new uint[8];
    private readonly Actor?[] _playerTetherSource = new Actor?[8];
    private List<Actor>? _darkOrder; // contains sources
    private int _doneTowers;
    private int _activeTethers;

    private const float _waterExplosionRange = 10f;

    public override void Update()
    {
        if (_darkOrder == null && _activeTethers == 8)
        {
            // build order for dark explosion; TODO: this is quite hacky right now, and probably should be configurable
            // current logic assumes we break N or NW tether first, and then move clockwise
            _darkOrder = [];
            var c = Arena.Center;
            AddAOETargetToOrder(_darkOrder, p => p.Z < c.Z && p.X <= c.X);
            AddAOETargetToOrder(_darkOrder, p => p.X > c.X && p.Z <= c.Z);
            AddAOETargetToOrder(_darkOrder, p => p.Z > c.Z && p.X >= c.X);
            AddAOETargetToOrder(_darkOrder, p => p.X < c.X && p.Z >= c.Z);
        }
        else if (_darkOrder?.Count == _activeTethers + 1 && _darkOrder[0].Tether.Target != 0)
        {
            // update order if unexpected tether was the first one to break
            if (_darkOrder[1].Tether.Target == 0)
            {
                var moved = _darkOrder[0];
                _darkOrder.RemoveAt(0);
                _darkOrder.Add(moved);
            }
            else if (_darkOrder[^1].Tether.Target == 0)
            {
                var moved = _darkOrder[^1];
                _darkOrder.RemoveAt(_darkOrder.Count - 1);
                _darkOrder.Insert(0, moved);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ReadyToBreak)
            return;

        if (_doneTowers < 4)
        {
            if (_playerIcons[slot] == (uint)IconID.AkanthaiWater)
            {
                hints.Add("Break tether!");
                if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _waterExplosionRange))
                {
                    hints.Add("GTFO from others!");
                }
            }
            else if (_playerIcons[slot] == (uint)IconID.AkanthaiDark)
            {
                Actor? soakedTower = null;
                for (var i = 0; i < _playerTetherSource.Length; ++i)
                {
                    var src = _playerTetherSource[i];
                    if (src != null && _playerIcons[i] == (uint)IconID.AkanthaiWater && src.Position.InCircle(actor.Position, P4S2.WreathTowerRadius))
                    {
                        soakedTower = src;
                        break;
                    }
                }
                hints.Add("Soak the tower!", soakedTower == null);
            }
        }
        else
        {
            var nextAOE = NextAOE();
            if (nextAOE != null)
            {
                if (nextAOE.Tether.Target == actor.InstanceID)
                {
                    hints.Add("Break tether!");
                }
                if (actor.Position.InCircle(nextAOE.Position, P4S2.WreathAOERadius))
                {
                    hints.Add("GTFO from AOE!");
                }
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_darkOrder != null && _activeTethers > 0)
        {
            var skip = 4 - Math.Min(_activeTethers, 4);
            hints.Add($"Dark order: {string.Join(" -> ", _darkOrder.Skip(skip).Select(src => WorldState.Actors.Find(src.Tether.Target)?.Name ?? "???"))}");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_doneTowers < 4)
            return;
        var nextAOE = NextAOE();
        if (nextAOE != null)
            Arena.ZoneCircle(nextAOE.Position, P4S2.WreathAOERadius, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw other players
        foreach ((var slot, var player) in Raid.WithSlot(false, true, true))
        {
            if (player == pc)
            {
                continue;
            }
            var icon = _playerIcons[slot];
            var nextBreaking = _doneTowers < 4 ? icon == (uint)IconID.AkanthaiWater : (icon == (uint)IconID.AkanthaiDark && NextAOE()?.Tether.Target == player.InstanceID);
            Arena.Actor(player, nextBreaking ? Colors.Danger : Colors.PlayerGeneric, drawWorld: nextBreaking ? true : null);
        }

        // tether
        var pcTetherSource = _playerTetherSource[pcSlot];
        if (pcTetherSource == null)
            return; // pc is not tethered anymore, nothing to draw...

        var pcIcon = _playerIcons[pcSlot];
        Arena.AddLine(pc.Position, pcTetherSource.Position, pcIcon == (uint)IconID.AkanthaiWater ? Colors.Other8 : Colors.Vulnerable);

        if (_doneTowers < 4)
        {
            if (pcIcon == (uint)IconID.AkanthaiWater)
            {
                // if player has blue => show AOE radius around him and single safe spot
                Arena.ZoneCircleOutline(pc.Position, _waterExplosionRange, Colors.Danger);
                Arena.ZoneCircleOutline(DetermineWaterSafeSpot(pcTetherSource), 1, Colors.Safe);
            }
            else
            {
                // if player has dark => show AOE radius around blue players and single tower to soak
                for (var i = 0; i < _playerIcons.Length; ++i)
                {
                    if (_playerIcons[i] == (uint)IconID.AkanthaiWater && Raid[i] is var player && player != null)
                    {
                        Arena.ZoneCircleOutline(player.Position, _waterExplosionRange, Colors.Danger);
                    }
                }
                var tower = DetermineTowerToSoak(pcTetherSource);
                if (tower != null)
                {
                    Arena.ZoneCircleOutline(tower.Position, P4S2.WreathTowerRadius, Colors.Safe);
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
        {
            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                _playerTetherSource[slot] = source;
                ++_activeTethers;
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
        {
            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                _playerTetherSource[slot] = null;
                --_activeTethers;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkanthaiExplodeTower)
            ++_doneTowers;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerIcons[slot] = iconID;
    }

    private void AddAOETargetToOrder(List<Actor> order, Predicate<WPos> sourcePred)
    {
        var source = _playerTetherSource.Zip(_playerIcons).FirstOrDefault(si => si.Second == (uint)IconID.AkanthaiDark && si.First != null && sourcePred(si.First.Position)).First;
        if (source != null)
            order.Add(source);
    }

    private WPos RotateCW(WPos pos, Angle angle, float radius)
    {
        var dir = Angle.FromDirection(pos - Arena.Center) - angle;
        return Arena.Center + radius * dir.ToDirection();
    }

    private WPos DetermineWaterSafeSpot(Actor source)
    {
        var ccw = Service.Config.Get<P4S2Config>().Act4WaterBreakCCW;
        var dir = (ccw ? -3 : 3) * 45.Degrees();
        return RotateCW(source.Position, dir, 18);
    }

    private Actor? DetermineTowerToSoak(Actor source)
    {
        var ccw = Service.Config.Get<P4S2Config>().Act4DarkSoakCCW;
        var pos = RotateCW(source.Position, (ccw ? -1 : 1) * 45f.Degrees(), 18f);
        for (var i = 0; i < _playerTetherSource.Length; ++i)
        {
            var x = _playerTetherSource[i];
            if (x != null && x.Position.InCircle(pos, 4f))
                return x;
        }
        return null;
    }

    private Actor? NextAOE()
    {
        var nextIndex = Math.Max(0, 4 - _activeTethers);
        return _darkOrder != null && nextIndex < _darkOrder.Count ? _darkOrder[nextIndex] : null;
    }
}
