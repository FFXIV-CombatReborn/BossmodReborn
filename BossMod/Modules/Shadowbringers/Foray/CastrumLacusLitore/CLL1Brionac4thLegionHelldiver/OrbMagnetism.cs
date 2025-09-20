namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class OrbsAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;
    private readonly List<(Actor Orb, AOEShape Shape)> orbs = new(4);
    public static readonly AOEShapeDonut Donut = new(5f, 20f);
    private static readonly AOEShapeCircle circle = new(12f);
    public readonly List<AOEInstance> AOEs = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsBrionacArena)
        {
            return CollectionsMarshal.AsSpan(AOEs);
        }
        else
        {
            return [];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EnergyGeneration:
                ++NumCasts;
                break;
            case (uint)AID.MagitekMagnetism:
            case (uint)AID.PolarMagnetism:
            case (uint)AID.FalseThunder1:
            case (uint)AID.FalseThunder2:
                AddAOEs(11d);
                break;
            case (uint)AID.PoleShiftVisual:
                AddAOEs(11.9d, true);
                break;
        }
        void AddAOEs(double delay, bool poleshift = false)
        {
            var count = orbs.Count;
            var activation = WorldState.FutureTime(delay);
            for (var i = 0; i < count; ++i)
            {
                var orb = orbs[i];
                var shape = orb.Shape;
                AddAOE(poleshift ? shape == Donut ? circle : Donut : shape, orb.Orb, activation);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Lightburst or (uint)AID.ShadowBurst)
        {
            AOEs.Clear();
            orbs.Clear();
        }
    }

    private void AddAOE(AOEShape shape, Actor actor, DateTime activation) => AOEs.Add(new(shape, actor.Position.Quantized(), default, activation));

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.Lightsphere => Donut,
            (uint)OID.Shadowsphere => circle,
            _ => null
        };
        if (shape != null)
        {
            if (NumCasts > 2)
            {
                orbs.Add((actor, shape));
            }
            else
            {
                AddAOE(shape, actor, WorldState.FutureTime(11d));
            }
        }
    }
}

sealed class Magnetism(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Knockback?[] _sources = new Knockback?[8];
    private readonly byte[] playerPoles = new byte[8];
    private readonly List<(ulong ActorID, WPos Position, List<Actor> Targets, byte Pole)> orbsData = []; // Pole 1: plus, Pole 2: minus
    private BitMask tethered;
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly WPos pos1 = new(63f, -222f), pos2 = new(97f, -222f);
    private readonly OrbsAOE _aoe = module.FindComponent<OrbsAOE>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (slot > 7) // we don't support the random allied NPCs
            return [];
        if (_sources[slot] is Knockback source)
        {
            return new Knockback[1] { source };
        }
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Lightsphere or (uint)OID.Shadowsphere)
        {
            orbsData.Add((actor.InstanceID, actor.Position, [], 0));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        byte poleOrb = iconID switch
        {
            (uint)IconID.OrbPlus => 1,
            (uint)IconID.OrbMinus => 2,
            _ => default
        };
        if (poleOrb != default)
        {
            var count = orbsData.Count;
            var orbs = CollectionsMarshal.AsSpan(orbsData);
            var id = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var orb = ref orbs[i];
                if (orb.ActorID == id)
                {
                    orb.Pole = poleOrb;
                    InitIfReady();
                    return;
                }
            }
        }
        if (iconID == (uint)IconID.PlayerPlus)
        {
            var slot = Raid.FindSlot(targetID);
            if (slot < 0)
                return;
            playerPoles[slot] = 1;
            InitIfReady();
        }
        else if (iconID == (uint)IconID.PlayerMinus)
        {
            var slot = Raid.FindSlot(targetID);
            if (slot < 0)
                return;
            playerPoles[slot] = 2;
            InitIfReady();
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (tethered != default && spell.Action.ID is (uint)AID.MagnetismKnockback or (uint)AID.MagnetismPull)
        {
            Array.Clear(_sources);
            Array.Clear(playerPoles);
            tethered = default;
            orbsData.Clear();
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            if (slot < 0)
                return;
            tethered[slot] = true;
            var count = orbsData.Count;
            var target = tether.Target;
            for (var i = 0; i < count; ++i)
            {
                if (orbsData[i].ActorID == target)
                {
                    orbsData[i].Targets.Add(source);
                }
            }
        }
        else if (tether.ID == (uint)TetherID.PoleShift)
        {
            Array.Clear(_sources);
            var target = tether.Target;
            var sourceID = source.InstanceID;

            var orbs = CollectionsMarshal.AsSpan(orbsData);
            var len = orbs.Length;
            var sourceIndex = -1;
            var targetIndex = -1;

            for (var i = 0; i < len; ++i)
            {
                ref readonly var oID = ref orbs[i].ActorID;
                if (oID == sourceID)
                    sourceIndex = i;
                else if (oID == target)
                    targetIndex = i;
            }
            if (sourceIndex != -1 && targetIndex != -1)
            {
                (orbs[targetIndex].Position, orbs[sourceIndex].Position) = (orbs[sourceIndex].Position, orbs[targetIndex].Position);
            }

            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (tethered != default)
        {
            var party = Raid.WithSlot(true, true, true);
            var len = party.Length;
            var count = orbsData.Count;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var pSlot = ref party[i].Item1;
                ref readonly var pPlayer = ref party[i].Item2;
            next:
                if (_sources[pSlot] == null && tethered[pSlot] && playerPoles[pSlot] != default)
                {
                    for (var j = 0; j < count; ++j)
                    {
                        var orb = orbsData[j];
                        var pole = orb.Pole;
                        if (pole == 0)
                            continue;
                        var countT = orb.Targets.Count;
                        for (var k = 0; k < countT; ++k)
                        {
                            var target = orb.Targets[k];
                            if (target == pPlayer)
                            {
                                AddSource(pSlot, orb.Position, pole == playerPoles[pSlot]);
                                goto next;
                            }
                        }
                    }
                }
            }
        }
        void AddSource(int slot, WPos position, bool isKnockback) => _sources[slot] = new(position, 30f, WorldState.FutureTime(8.2d), kind: isKnockback ? Kind.AwayFromOrigin : Kind.TowardsOrigin, ignoreImmunes: true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot > 7) // we don't support the random allied NPCs
            return;
        if (_sources[slot] is Knockback source)
        {
            if (source.Kind == Kind.TowardsOrigin)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(source.Origin, 35f), source.Activation);
            }
            else
            {
                var oX = source.Origin.X > 90f;
                var opposite = oX ? pos1 : pos2;
                var angle = oX ? a90 : -a90;
                var dir = angle.ToDirection();
                if (opposite != default)
                {
                    hints.AddForbiddenZone(new SDInvertedRect(opposite + 35f * dir, -dir, 5f, default, 0.5f), source.Activation);
                }
            }
        }
    }
}
