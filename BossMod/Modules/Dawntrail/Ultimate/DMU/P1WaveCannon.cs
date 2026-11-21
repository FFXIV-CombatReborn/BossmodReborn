namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class WaveCannon : Components.BaitAwayEveryone
{
    private readonly DateTime activation;
    private const float baitActivation = 4.2f;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public WaveCannon(BossModule module) : base(module, module.Enemies((uint)OID.StatueWaveCannon) is var statues && statues.Count != 0 ? statues[0] : null, new AOEShapeRect(100.0f, 3.0f))
    {
        activation = WorldState.FutureTime(baitActivation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaveCannon)
        {
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);

        if (!dmuConfig.P1WaveCannonHints || !dmuConfig.P1WaveCannonAssignment.Validate())
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];
        var order = dmuConfig.P1WaveCannonAssignment[assignment];
        var safeSpot = P1WaveCannonData.safeSpots[order];
        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.PRECISE, Colors.Safe, 2.0f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }

        if (!dmuConfig.P1WaveCannonAssignment.Validate())
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        var order = dmuConfig.P1WaveCannonAssignment[assignment];
        var safeSpot = P1WaveCannonData.safeSpots[order];

        var remaining = (activation - WorldState.CurrentTime).TotalSeconds;
        if (remaining <= 0.7f)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.PRECISE, PositionWeights.MECHANIC));
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), activation);
    }
}

sealed class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4.0f)
{
    private readonly DateTime[] debuffs = new DateTime[PartyState.MaxPartySize];
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                debuffs[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (Towers.Count == 4)
        {
            Towers.Sort((t1, t2) => t1.Position.X.CompareTo(t2.Position.X));

            // Case: party assignments are not set up so, we just assign towers base on debuffs
            var slots = partyConfig.SlotsPerAssignment(Raid);
            if (slots.Length == 0)
            {
                setupDefaultForbiddenPlayers();
                return;
            }

            // Case: party assignment are set up so we can solve it correctly
            setupSpecificForbiddenPlayers();
        }
    }

    // Custom AIHints since towers can overlap, we have to stand where the tower is not overlapping + go to the front of the tower as much as possible
    private readonly List<ShapeDistance> _forbiddenInverted = [];
    private readonly List<(ShapeDistance zone, DateTime activation)> _forbidden = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = ActiveTowers(slot, actor);
        var len = towers.Length;
        if (len == 0)
        {
            return;
        }

        var forbiddenInverted = _forbiddenInverted;
        var forbidden = _forbidden;
        forbiddenInverted.Clear();
        forbidden.Clear();

        for (int i = 0; i < len; i++)
        {
            ref readonly var tower = ref towers[i];
            if (tower.ForbiddenSoakers[slot])
            {
                forbidden.Add((tower.ShapeDistance ?? tower.Shape.Distance(tower.Position, tower.Rotation), tower.Activation));
            }

            if (!tower.ForbiddenSoakers[slot])
            {
                forbiddenInverted.Add(tower.InvertedShapeDistance ?? tower.Shape.InvertedDistance(tower.Position, tower.Rotation));
                var center = (Arena.Center - tower.Position).Normalized();

                // If the tower is inside the boss' hitbox then we should aim for the outer radius of the tower (back of tower)
                if (tower.Position.InCircle(Module.PrimaryActor.Position, Radius + Module.PrimaryActor.HitboxRadius))
                {
                    center = -center;
                }

                // If the tower is outside the boss's hitbox then we should aim for the inner radius of the tower (front of tower)
                var front = tower.Position + center * Radius;
                hints.GoalZones.Add(AIHints.GoalProximity(front, Arena.Bounds.Radius, PositionWeights.PRE_POSITION));
            }
        }

        if (forbiddenInverted.Count != 0)
        {
            hints.AddForbiddenZone(new SDIntersection([.. forbiddenInverted]), towers[0].Activation);
        }

        for (int i = 0; i < forbidden.Count; i++)
        {
            var (zone, activation) = forbidden[i];
            hints.AddForbiddenZone(zone, activation);

            // Donut cones east and west to help with players standing near the boss and pre-position for the next mechanic
            hints.GoalZones.Add(p => p.InDonutCone(Module.PrimaryActor.Position, 5.0f, 8.0f, Angle.AnglesCardinals[0], 60.0f.Degrees()) ?
                PositionWeights.PRE_POSITION : 0.0f);

            hints.GoalZones.Add(p => p.InDonutCone(Module.PrimaryActor.Position, 5.0f, 8.0f, Angle.AnglesCardinals[3], 60.0f.Degrees()) ?
                PositionWeights.PRE_POSITION : 0.0f);
        }
    }

    private void setupDefaultForbiddenPlayers()
    {
        for (int i = 0; i < Towers.Count; i++)
        {
            var tower = Towers[i];
            BitMask forbiddenPlayers = default;

            for (int k = 0; k < debuffs.Length; k++)
            {
                var playerDebuff = debuffs[k];
                if (playerDebuff > tower.Activation)
                {
                    forbiddenPlayers.Set(k);
                }
            }

            tower.ForbiddenSoakers = forbiddenPlayers;
            Towers[i] = tower;
        }
    }

    private void setupSpecificForbiddenPlayers()
    {
        var activation = Towers[0].Activation;
        List<(int slot, int order)> soakers = [];

        // Find the four players without debuffs
        for (int i = 0; i < debuffs.Length; i++)
        {
            if (debuffs[i] > activation)
            {
                continue;
            }

            var assignment = partyConfig[Raid.Members[i].ContentId];
            var order = dmuConfig.P1WaveCannonAssignment[assignment];
            soakers.Add((i, order));
        }

        soakers.Sort((a, b) => a.order.CompareTo(b.order));

        for (int i = 0; i < Towers.Count; i++)
        {
            var tower = Towers[i];
            BitMask forbiddenPlayers = default;

            if (i < soakers.Count)
            {
                var soaker = soakers[i];
                forbiddenPlayers.Set(soaker.slot);
                tower.ForbiddenSoakers = ~forbiddenPlayers;
            }
            else
            {
                tower.ForbiddenSoakers = ~forbiddenPlayers;
            }

            Towers[i] = tower;
        }
    }
}
