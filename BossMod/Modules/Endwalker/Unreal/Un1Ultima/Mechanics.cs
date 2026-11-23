namespace BossMod.Endwalker.Unreal.Un1Ultima;

// common mechanics that are used for entire fight
// TODO: consider splitting into multiple components, at least for mechanics that start in later phases...
sealed class Mechanics(BossModule module) : BossComponent(module)
{
    private readonly int[] _tankStacks = new int[PartyState.MaxPartySize];

    private readonly HashSet<ulong> _orbsSharedExploded = [];
    // TODO: think how to associate kiters with orbs
    private readonly HashSet<ulong> _orbsKitedExploded = [];
    private readonly List<ulong> _orbKiters = [];

    private Angle? _magitekOffset;

    private readonly AOEShapeCircle _aoeCleave = new(2f);
    private readonly AOEShapeCone _aoeDiffractive = new(12f, 60f.Degrees());
    private readonly AOEShapeRect _aoeAssaultCannon = new(45f, 1f);
    private readonly AOEShapeRect _aoeMagitekRay = new(40f, 3f);
    //private static readonly float _homingLasersRange = 4;
    //private static readonly float _ceruleumVentRange = 8;
    private const float _orbSharedRange = 8f;
    private const float _orbFixateRange = 6f;

    public override void Update()
    {
        // TODO: this is bad, we need to find a way to associate orb to kiter...
        if (_orbKiters.Count > 0 && Module.Enemies((uint)OID.Aetheroplasm).Count == 0)
            _orbKiters.Clear();
    }

    private bool BitHits(Actor actor)
    {
        var bits = Module.Enemies((uint)OID.MagitekBit);
        for (var i = 0; i < bits.Count; ++i)
        {
            var bit = bits[i];
            if (bit.CastInfo != null && _aoeAssaultCannon.Check(actor.Position, bit))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var mtSlot = WorldState.Party.FindSlot(Module.PrimaryActor.TargetID);
        if (actor.Role == Role.Tank)
        {
            if (Module.PrimaryActor.TargetID == actor.InstanceID)
            {
                if (_tankStacks[slot] >= 4)
                    hints.Add("Pass aggro to co-tank!");
            }
            else
            {
                if (mtSlot >= 0 && _tankStacks[mtSlot] >= 4)
                    hints.Add("Taunt boss!");
            }
        }

        var mt = WorldState.Party[mtSlot];
        if (slot != mtSlot && mt != null && (_aoeCleave.Check(actor.Position, mt) || _aoeDiffractive.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(mt.Position - Module.PrimaryActor.Position))))
        {
            hints.Add("GTFO from tank!");
        }

        // TODO: reconsider whether we really care about spread for vents/lasers...
        //if (actor.Role is Role.Healer or Role.Ranged && GeometryUtils.InCircle(actor.Position - Module.PrimaryActor.Position, _ceruleumVentRange))
        //{
        //    hints.Add("Move from boss");
        //}

        //if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _homingLasersRange))
        //{
        //    hints.Add("Spread");
        //}

        if (_magitekOffset != null && _aoeMagitekRay.Check(actor.Position, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + _magitekOffset.Value))
        {
            hints.Add("GTFO from ray aoe!");
        }

        if (_orbKiters.Contains(actor.InstanceID))
        {
            hints.Add("Kite the orb!");
        }

        if (BitHits(actor))
        {
            hints.Add("GTFO from bit aoe!");
        }

        // TODO: large detonations
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_magitekOffset != null)
            _aoeMagitekRay.Draw(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + _magitekOffset.Value);

        foreach (var bit in Module.Enemies((uint)OID.MagitekBit))
            if (bit.CastInfo != null)
                _aoeAssaultCannon.Draw(Arena, bit);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var mt = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        foreach (var player in Raid.WithoutSlot(false, true, true))
        {
            if (!(player != pc))
                continue;
            if (player == pc)
            {
                continue;
            }
            var isKiter = _orbKiters.Contains(player.InstanceID);
            Arena.Actor(player, isKiter ? Colors.Danger : player == mt ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isKiter || player == mt ? true : null);
        }
        if (mt != null)
            Arena.ZoneCircleOutline(mt.Position, _aoeCleave.Radius, Colors.Danger);

        //if (pc.Role is Role.Healer or Role.Ranged)
        //    Arena.ZoneCircleOutline(Module.PrimaryActor.Position, _ceruleumVentRange, Colors.Danger);

        foreach (var orb in Module.Enemies((uint)OID.Ultimaplasm))
        {
            if (!(!_orbsSharedExploded.Contains(orb.InstanceID)))
                continue;
            // TODO: line between paired orbs
            Arena.Actor(orb, Colors.Danger, true);
            Arena.ZoneCircleOutline(orb.Position, _orbSharedRange, Colors.Safe);
        }

        foreach (var orb in Module.Enemies((uint)OID.Aetheroplasm))
        {
            if (!(!_orbsKitedExploded.Contains(orb.InstanceID)))
                continue;
            // TODO: line from corresponding target
            Arena.Actor(orb, Colors.Danger, true);
            Arena.ZoneCircleOutline(orb.Position, _orbFixateRange, Colors.Danger);
        }

        foreach (var bit in Module.Enemies((uint)OID.MagitekBit))
        {
            Arena.Actor(bit, Colors.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ViscousAetheroplasm)
            SetTankStacks(actor, status.Extra);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ViscousAetheroplasm)
            SetTankStacks(actor, 0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? ray = spell.Action.ID switch
        {
            (uint)AID.MagitekRayCenter => default,
            (uint)AID.MagitekRayLeft => 45f.Degrees(),
            (uint)AID.MagitekRayRight => -45f.Degrees(),
            _ => null
        };
        if (ray == null)
            return;
        if (_magitekOffset != null)
            ReportError("Several concurrent magitek rays");
        _magitekOffset = ray.Value;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MagitekRayCenter or (uint)AID.MagitekRayLeft or (uint)AID.MagitekRayRight)
            _magitekOffset = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AetheroplasmBoom:
                _orbsSharedExploded.Add(caster.InstanceID);
                break;
            case (uint)AID.AetheroplasmFixated:
                _orbsKitedExploded.Add(caster.InstanceID);
                break;
            case (uint)AID.OrbFixate:
                _orbKiters.Add(spell.MainTargetID);
                break;
        }
    }

    private void SetTankStacks(Actor actor, int stacks)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _tankStacks[slot] = stacks;
    }
}
