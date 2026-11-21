namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class ElementaryChemistry(BossModule module) : BossComponent(module)
{
    // players get ElementaryDeficiency status with extra 1-3
    // getting orb on a panel gives element resist down status
    // spawns x3 element orb actors 4E03/4/5 every 3s
    // does player have to stand on glowing spot or can it be intercepted any time?
    // glowing spot roughly 9f from center, enum says 1.5R, manually create as there's no marker actor for each spot
    // want to move to one of the 1st spawned orbs

    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly List<Actor> _actors = [];
    private BitMask _deficiency = new();
    private BitMask _fire = new();
    private BitMask _ice = new();
    private BitMask _lightning = new();
    private readonly WPos[] _soakSpots = [
        new(0f, -619f),
        new(7.794f, -623.5f),
        new(7.794f, -632.5f),
        new(0f, -637f),
        new(-7.794f, -632.5f),
        new(-7.794f, -623.5f),
        ];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.ChemistryBallOfFire or (uint)OID.ChemistrySwirlingOrb or (uint)OID.ChemistryBallOfLevin)
        {
            _actors.Add(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        switch (status.ID)
        {
            case (uint)SID.ElementaryDeficiency:
                _deficiency.Set(slot);
                break;
            case (uint)SID.FireResistanceDownII:
                _fire.Set(slot);
                break;
            case (uint)SID.IceResistanceDownII:
                _ice.Set(slot);
                break;
            case (uint)SID.LightningResistanceDownII:
                _lightning.Set(slot);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        switch (status.ID)
        {
            case (uint)SID.ElementaryDeficiency:
                _deficiency.Clear(slot);
                break;
            case (uint)SID.FireResistanceDownII:
                _fire.Clear(slot);
                break;
            case (uint)SID.IceResistanceDownII:
                _ice.Clear(slot);
                break;
            case (uint)SID.LightningResistanceDownII:
                _lightning.Clear(slot);
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_deficiency[pcSlot])
        {
            var sds = _panels.GetElementSDs(_fire[pcSlot], _ice[pcSlot], _lightning[pcSlot]);
            var scount = sds.Length;

            for (var i = 0; i < 6; ++i)
            {
                var taken = false;
                var spot = _soakSpots[i];
                for (var j = 0; j < scount; ++j)
                {
                    var sd = sds[j];
                    if (sd.Contains(spot))
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                {
                    Arena.ZoneCircleOutline(spot, 1.5f);
                }
            }
        }

#if DEBUG
        var count = _actors.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; ++i)
            {
                var actor = _actors[i];
                var color = actor.OID switch
                {
                    (uint)OID.ChemistryBallOfFire => 0xFF0000FF,
                    (uint)OID.ChemistrySwirlingOrb => 0xFFFF0000,
                    (uint)OID.ChemistryBallOfLevin => 0xFF00FF00,
                    _ => 0xFF000000
                };

                Arena.ZoneCircle(actor.Position, 1f, color);
            }
        }
#endif
    }

    private readonly List<string> _colorsBuffer = [];
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.ElementaryDeficiency) != null)
        {
            var colors = _colorsBuffer;
            colors.Clear();
            if (actor.FindStatus((uint)SID.FireResistanceDownII) == null)
            {
                colors.Add("fire");
            }
            if (actor.FindStatus((uint)SID.IceResistanceDownII) == null)
            {
                colors.Add("ice");
            }
            if (actor.FindStatus((uint)SID.LightningResistanceDownII) == null)
            {
                colors.Add("lightning");
            }
            hints.Add($"Grab {string.Join(" & ", colors)} orb{(colors.Count > 1 ? "s" : "")}!", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_deficiency[slot])
        {
            var sds = _panels.GetElementSDs(_fire[slot], _ice[slot], _lightning[slot]);
            var scount = sds.Length;
            for (var i = 0; i < scount; ++i)
            {
                var sd = sds[i];
                hints.AddForbiddenZone(sd);
            }

            // if all goal zones are equal, should move to closest while avoiding forbidden zones
            // need it to nav to one of the 1st spawned orbs the move CW
            for (var i = 0; i < 6; ++i)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(_soakSpots[i], 1.5f));
            }
        }
    }
}
