namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.UltimateEmbrace, 5f);

sealed class Forsaken(BossModule module) : Components.RaidwideCast(module, (uint)AID.Forsaken);

sealed class LightOfJudgmentP2(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightOfJudgmentP2);

// Used for towers' spawn locations and marking them as SW or SE depending on the spawn point.
sealed class PathOfLight(BossModule module) : Components.GenericTowers(module, (uint)AID.ThePathOfLight)
{
    public int CurrentSW = -1;
    public int CurrentSE = -1;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index >= 0x01 && index <= 0x08 && state == 0x00020001u)
        {
            var angle = (180f - (index - 1) * 45f).Degrees();
            Towers.Add(new(Arena.Center + angle.ToDirection() * 8f, 4f, 2, 2, default, WorldState.FutureTime(10d)));
            UpdateCurrentTowers();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                    break;
                }
            }
            UpdateCurrentTowers();
        }
    }
    public override void Update() => UpdateCurrentTowers();
    public void UpdateCurrentTowers()
    {
        if (Towers.Count != 2)
        {
            return;
        }

        var towers = CollectionsMarshal.AsSpan(Towers);
        var tower1 = towers[0].Position;
        var tower2 = towers[1].Position;

        var middleOfTowers = new WPos((tower1.X + tower2.X) * 0.5f, (tower1.Z + tower2.Z) * 0.5f);
        var southDirection = (middleOfTowers - Arena.Center).Normalized();

        if ((tower1 - middleOfTowers).Dot(southDirection.OrthoL()) <= (tower2 - middleOfTowers).Dot(southDirection.OrthoL()))
        {
            CurrentSW = 0;
            CurrentSE = 1;
        }
        else
        {
            CurrentSW = 1;
            CurrentSE = 0;
        }
    }
}

// Used for setting up each player's role, such as the shape the player has, the pair the player belongs, if the player is a helper or soaker, etc.
sealed class ForsakenShapes(BossModule module) : BossComponent(module)
{
    private static readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    public int currentTowerSet = 1; // We start on odd tower set
    private int pathOfLightCasts;

    public enum Shape { None, Spread, Cone, Stack }
    public Shape[] shapes = new Shape[8];
    public enum TowerRole { Unknown, Helper, Taker }

    public BitMask swSoakers;
    public BitMask seSoakers;
    public BitMask supportHelpers;
    public BitMask dpsHelpers;

    // TODO merge these together
    public bool pairsLocked;
    private bool pairsSwapped;

    public sealed class PairInfo(PartyRolesConfig.Assignment player1, PartyRolesConfig.Assignment player2, bool isSupport)
    {
        public PartyRolesConfig.Assignment player1Assignment = player1;
        public PartyRolesConfig.Assignment player2Assignment = player2;
        public bool isSupport = isSupport;
        public TowerRole role = TowerRole.Unknown;
    }

    public readonly PairInfo[] pairs = [
        new(PartyRolesConfig.Assignment.MT, PartyRolesConfig.Assignment.H1, true),
        new(PartyRolesConfig.Assignment.OT, PartyRolesConfig.Assignment.H2, true),
        new(PartyRolesConfig.Assignment.M1, PartyRolesConfig.Assignment.R1, false),
        new(PartyRolesConfig.Assignment.M2, PartyRolesConfig.Assignment.R2, false),
    ];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.ThePathOfLight)
        {
            return;
        }

        if (++pathOfLightCasts < 2)
        {
            return;
        }

        pathOfLightCasts = 0;
        ++currentTowerSet;

        if (currentTowerSet is 4 or 8)
        {
            pairsSwapped = false;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shape = iconID switch
        {
            (uint)IconID.TowerSpreadIcon => Shape.Spread,
            (uint)IconID.TowerConeIcon => Shape.Cone,
            (uint)IconID.TowerStackIcon => Shape.Stack,
            _ => default
        };

        if (shape != default)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                shapes[slot] = shape;
            }
        }
    }

    public override void Update()
    {
        swSoakers = default;
        seSoakers = default;
        supportHelpers = default;
        dpsHelpers = default;

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        SetupPairs(slots);

        if ((currentTowerSet == 4 || currentTowerSet == 8) && !pairsSwapped)
        {
            foreach (var pair in pairs)
            {
                pair.role = pair.role switch
                {
                    TowerRole.Helper => TowerRole.Taker,
                    TowerRole.Taker => TowerRole.Helper,
                    _ => pair.role
                };
            }

            pairsSwapped = true;
        }

        foreach (var pair in pairs)
        {
            var slotPlayer1 = slots[(int)pair.player1Assignment];
            var slotPlayer2 = slots[(int)pair.player2Assignment];
            var shapeA = shapes[slotPlayer1];
            var shapeB = shapes[slotPlayer2];

            if (pair.role == TowerRole.Helper)
            {
                if (pair.isSupport)
                {
                    supportHelpers.Set(slotPlayer1);
                    supportHelpers.Set(slotPlayer2);
                }
                else
                {
                    dpsHelpers.Set(slotPlayer1);
                    dpsHelpers.Set(slotPlayer2);
                }
            }

            if (pair.role == TowerRole.Taker && dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                // First set of towers (tower set odd)
                if ((currentTowerSet & 1) != 0)
                {
                    // Case: for the first set of towers no adjustment is needed between the Melee & Tank
                    if (currentTowerSet == 1)
                    {
                        if (shapeA == Shape.Cone || shapeB == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread || shapeB == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                    }

                    // Case: every odd tower beyond the first set, the Melee & Tank may need to adjust base on pair shapes
                    if (currentTowerSet > 1)
                    {
                        // Cones and spreads are forced, where cone is always SW and spread is always SE
                        if (shapeA == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer2);
                        }

                        // If the pairs has the same shape, an adjustment is needed
                        if (shapeA == shapeB)
                        {
                            // If supports are the same shape, MT/OT has to go to the SE tower
                            if (pair.isSupport)
                            {
                                seSoakers.Set(slotPlayer1);
                                swSoakers.Set(slotPlayer2);
                            }

                            // If dps are the same shape, M1/M2 goes to the SW tower
                            if (!pair.isSupport)
                            {
                                swSoakers.Set(slotPlayer1);
                                seSoakers.Set(slotPlayer2);
                            }
                        }
                        else
                        { // Otherwise people just go to their default side
                            if (pair.isSupport)
                            {
                                if (shapeA == Shape.Stack)
                                {
                                    swSoakers.Set(slotPlayer1);
                                }

                                if (shapeB == Shape.Stack)
                                {
                                    swSoakers.Set(slotPlayer2);
                                }
                            }

                            if (!pair.isSupport)
                            {
                                if (shapeA == Shape.Stack)
                                {
                                    seSoakers.Set(slotPlayer1);
                                }

                                if (shapeB == Shape.Stack)
                                {
                                    seSoakers.Set(slotPlayer2);
                                }
                            }
                        }
                    }
                }

                // Second set of towers (tower set even)
                if ((currentTowerSet & 1) == 0)
                {
                    if (pair.isSupport)
                    {
                        // They have different shapes - both go to the same tower which is west tower
                        if (shapeA != shapeB)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }
                        else
                        { // healer goes to SW tower, tank goes to SE tower - player2 is healer, player1 is tank
                            swSoakers.Set(slotPlayer2);
                            seSoakers.Set(slotPlayer1);
                        }
                    }

                    if (!pair.isSupport)
                    {
                        if (shapeA != shapeB)
                        {
                            // They have different shapes - both go to the same tower which is east tower
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                        else
                        {
                            // range goes to SE tower, melee goes to SE tower - player2 is range, player1 is melee
                            seSoakers.Set(slotPlayer2);
                            swSoakers.Set(slotPlayer1);
                        }
                    }
                }
            }

            if (pair.role == TowerRole.Taker && dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                // First set of towers (tower set odd)
                if ((currentTowerSet & 1) != 0)
                {
                    // Cones and spreads are forced, where cone is always SW and spread is always SE
                    if (shapeA == Shape.Cone)
                    {
                        swSoakers.Set(slotPlayer1);
                    }

                    if (shapeB == Shape.Cone)
                    {
                        swSoakers.Set(slotPlayer2);
                    }

                    if (shapeA == Shape.Spread)
                    {
                        seSoakers.Set(slotPlayer1);
                    }

                    if (shapeB == Shape.Spread)
                    {
                        seSoakers.Set(slotPlayer2);
                    }

                    // If the pairs have the same shape, an adjustment is needed
                    if (shapeA == shapeB)
                    {
                        // If supports are the same shape, MT/OT has to go to the SE tower
                        if (pair.isSupport)
                        {
                            seSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }

                        // If dps are the same shape, M1/M2 goes to the SW tower
                        if (!pair.isSupport)
                        {
                            swSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                    }
                    else
                    {
                        if (pair.isSupport)
                        {
                            if (shapeA == Shape.Stack)
                            {
                                swSoakers.Set(slotPlayer1);
                            }

                            if (shapeB == Shape.Stack)
                            {
                                swSoakers.Set(slotPlayer2);
                            }
                        }

                        if (!pair.isSupport)
                        {
                            if (shapeA == Shape.Stack)
                            {
                                seSoakers.Set(slotPlayer1);
                            }

                            if (shapeB == Shape.Stack)
                            {
                                seSoakers.Set(slotPlayer2);
                            }
                        }
                    }
                }

                // Second set of towers (tower set even)
                if ((currentTowerSet & 1) == 0)
                {
                    if (pair.isSupport)
                    {
                        // They have different shapes - both go to the same tower which is west tower
                        if (shapeA != shapeB)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }
                        else
                        { // healer goes to SW tower, tank goes to SE tower - player2 is healer, player1 is tank
                            swSoakers.Set(slotPlayer2);
                            seSoakers.Set(slotPlayer1);
                        }
                    }

                    if (!pair.isSupport)
                    {
                        if (shapeA != shapeB)
                        {
                            // They have different shapes - both go to the same tower which is east tower
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                        else
                        {
                            // range goes to SE tower, melee goes to SW tower - player2 is range, player1 is melee
                            seSoakers.Set(slotPlayer2);
                            swSoakers.Set(slotPlayer1);
                        }
                    }
                }
            }
        }
    }

    private void SetupPairs(int[] slots)
    {
        if (pairsLocked)
        {
            return;
        }

        pairsLocked = true;

        foreach (var pair in pairs)
        {
            if (pair.role != TowerRole.Unknown)
            {
                continue;
            }

            var shapeA = shapes[slots[(int)pair.player1Assignment]];
            var shapeB = shapes[slots[(int)pair.player2Assignment]];

            if (shapeA == Shape.None || shapeB == Shape.None)
            {
                pairsLocked = false;
                continue;
            }

            pair.role = shapeA == shapeB ? TowerRole.Helper : TowerRole.Taker;
        }

        foreach (var pair in pairs)
        {
            if (pair.role == TowerRole.Unknown)
            {
                pairsLocked = false;
                break;
            }
        }
    }
}

sealed class ForsakenBaitsSpreadStacks(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 3, 3)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();

    public override void Update()
    {
        if (towers == null || shapes == null)
        {
            return;
        }

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot(true, true, true))
        {
            if (DMUHelpers.InAnyTower(towers.Towers, player.Position, 4f))
            {
                var shape = shapes.shapes[i];
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    AddStack(player);
                }
                else if (shape == ForsakenShapes.Shape.Spread)
                {
                    AddSpread(player);
                }
            }
        }
    }
}

sealed class ForsakenBaitsCone(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Spellwave)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly AOEShapeCone cone = new(40f, 45f.Degrees());

    public override void Update()
    {
        if (towers == null || shapes == null)
        {
            return;
        }

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot(true, true, true))
        {
            if (DMUHelpers.InAnyTower(towers.Towers, player.Position, 4f))
            {
                if (shapes.shapes[i] == ForsakenShapes.Shape.Cone)
                {
                    Actor? closestPlayer = null;
                    var bestDistSq = float.MaxValue;
                    var party = Raid.WithoutSlot(false, true, true);
                    for (var j = 0; j < party.Length; ++j)
                    {
                        var p = party[j];
                        if (p == player)
                            continue;
                        var distSq = (p.Position - player.Position).LengthSq();
                        if (distSq < bestDistSq)
                        {
                            bestDistSq = distSq;
                            closestPlayer = p;
                        }
                    }
                    if (closestPlayer != null)
                    {
                        CurrentBaits.Add(new(player, closestPlayer, cone));
                    }
                }
            }
        }
    }
}

sealed class ForsakenBaitsBossClones(DMU module) : Components.UniformStackSpread(module, 5f, 5f)
{
    private readonly List<Actor> clones = []; // Also includes the boss since he will cast the same spell
    private readonly List<Actor> baiters = []; // List of players currently baiting - prevents dupes
    private readonly List<Actor> _clones = module.Enemies((uint)OID.P2KefkaHelpers);
    private readonly Actor bossP2 = module.BossP2()!;
    private int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.FuturesEndCast) or ((uint)AID.PastsEndCast))
        {
            var count = _clones.Count;
            for (var i = 0; i < count; ++i)
            {
                clones.Add(_clones[i]);
            }
            clones.Add(bossP2);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.PastsEndSpread) or ((uint)AID.PastsEndSpread1) or ((uint)AID.FuturesEndSpread) or ((uint)AID.FuturesEndSpread1))
        {
            ++NumCasts;

            if (NumCasts == 4)
            {
                clones.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void Update()
    {
        Spreads.Clear();
        baiters.Clear();

        if (clones.Count == 0)
        {
            return;
        }

        foreach (var clone in clones)
        {
            Actor? baiter = null;
            var bestDistSq = float.MaxValue;
            var party = Raid.WithoutSlot();
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                if (baiters.Contains(p))
                    continue;
                var d = (p.Position - clone.Position).LengthSq();
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    baiter = p;
                }
            }
            if (baiter == null)
            {
                continue;
            }

            baiters.Add(baiter);
            AddSpread(baiter);
        }
    }
}

// Used for odd tower sets in figuring out what each player is responsible for
sealed class ForsakenSolverSet1(BossModule module) : BossComponent(module)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    public uint colourCircle = Colors.Safe;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || shapes.swSoakers.None() || shapes.seSoakers.None())
        {
            return;
        }

        if ((shapes.currentTowerSet & 1) == 0)
        {
            return;
        }
        var towersSpan = CollectionsMarshal.AsSpan(towers.Towers);
        ref var towerSW = ref towersSpan[towers.CurrentSW];
        ref var towerSE = ref towersSpan[towers.CurrentSE];
        var posSW = towerSW.Position;
        var posSE = towerSE.Position;
        var midpoint = new WPos((posSW.X + posSE.X) * 0.5f, (posSW.Z + posSE.Z) * 0.5f);
        var newSouth = (midpoint - Arena.Center).Normalized();

        var towardSW = (posSW - midpoint).Normalized();
        var towardSE = (posSE - midpoint).Normalized();
        var center = Arena.Center;

        // Case: SW players with different debuffs
        if (shapes.swSoakers[pcSlot])
        {
            var shape = shapes.shapes[pcSlot];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(posSW + -towardSW * 0.5f + -newSouth * 1.0f, 1.0f, colourCircle, 2.0f);
                }
                else if (shape == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(posSW + newSouth * 3.0f, 1.0f, colourCircle, 2.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (center - posSW).Normalized();
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(posSW + toCenter + 0.5f * toCenter.OrthoL(), 1.0f, colourCircle, 2.0f);
                }
                else if (shape == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(posSW - 3.0f * toCenter, 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SW players with same debuffs
        else if (shapes.supportHelpers[pcSlot])
        {
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(posSW + newSouth * 4.5f, 1.0f, colourCircle, 2.0f);
                }
                else if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(posSW - towardSW * 3.0f - newSouth * 4.0f, 1.0f, colourCircle, 2.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (center - posSW).Normalized();
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(posSW - 4.5f * toCenter, 1.0f, colourCircle, 2.0f);
                }
                else if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(posSW + 4.5f * toCenter + 0.5f * toCenter.OrthoL(), 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SE players with different debuffs
        else if (shapes.seSoakers[pcSlot])
        {
            var shape = shapes.shapes[pcSlot];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(posSE - towardSE * 2.5f + newSouth * 2.5f, 1.0f, colourCircle, 2.0f);
                }
                else if (shape == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(posSE + towardSE * 2.0f - newSouth * 3.0f, 1.0f, colourCircle, 2.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (center - posSE).Normalized();
                var orthoL = toCenter.OrthoL();
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(posSE + 3.0f * toCenter - 2.0f * orthoL, 1.0f, colourCircle, 2.0f);
                }
                else if (shape == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(posSE - 2.5f * toCenter + 2.5f * orthoL, 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SE players with same debuffs
        else if (shapes.dpsHelpers[pcSlot])
        {
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(posSE - towardSE * 4.0f + newSouth * 3.0f, 1.0f, colourCircle, 2.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (center - posSE).Normalized();
                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(posSE + 4.5f * toCenter + toCenter.OrthoL(), 1.0f, colourCircle, 2.0f);
                }
            }
        }
    }

    public override void Update()
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2)
        {
            return;
        }

        if ((shapes.currentTowerSet & 1) == 0)
        {
            return;
        }

        var party = new BitMask(0xFF);
        var towersSpan = CollectionsMarshal.AsSpan(towers.Towers);
        var tSE = towers.CurrentSE;
        var tSW = towers.CurrentSW;
        for (var i = 0; i < 2; ++i)
        {
            ref var t = ref towersSpan[i];
            if (i == tSW)
            {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }
            else if (i == tSE)
            {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }
        }
    }
}

// Used for even tower sets in figuring out what each player is responsible for
sealed class ForsakenSolverSet2(BossModule module) : BossComponent(module)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || shapes.swSoakers.None() || shapes.seSoakers.None())
        {
            return;
        }

        if ((shapes.currentTowerSet & 1) != 0)
        {
            return;
        }
        var towersSpan = CollectionsMarshal.AsSpan(towers.Towers);
        var towerSW = towersSpan[towers.CurrentSW].Position;
        var towerSE = towersSpan[towers.CurrentSE].Position;
        var center = Arena.Center;
        // Case: SW players with different debuffs (soakers)
        if (shapes.swSoakers[pcSlot])
        {
            var toCenter = (center - towerSW).Normalized();
            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(towerSW + toCenter * 3.5f, 0.75f, Colors.Safe, 1.0f);
                }
                else if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(towerSW - toCenter * 3.5f, 0.75f, Colors.Safe, 1.0f);
                    }
                    else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(towerSW + (-toCenter).Rotate(34f.Degrees()) * 3.57f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var offset = 2.0f * toCenter.OrthoL();
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(towerSW + 3.0f * toCenter + offset, 0.75f, Colors.Safe, 1.0f);
                }
                else if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(towerSW - 3.0f * toCenter - offset, 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SW players with same debuffs (helpers)
        else if (shapes.supportHelpers[pcSlot])
        {
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (center - towerSW).Normalized();

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(towerSW + toCenter.OrthoL() * 4.5f, 0.75f, Colors.Safe, 1.0f);
                    }
                    else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(towerSW + toCenter.Rotate(82f.Degrees()) * 7.07f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
                else if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(towerSW + toCenter.Rotate(35.0f.Degrees()) * 11.5f, 0.75f, Colors.Safe, 1.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var offset = toCenter.OrthoL();
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(towerSW + toCenter + 7.0f * offset, 0.75f, Colors.Safe, 1.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(towerSW + 9.0f * toCenter + 6.0f * offset, 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SE players with different debuffs (soakers)
        else if (shapes.seSoakers[pcSlot])
        {
            var toCenter = (Arena.Center - towerSE).Normalized();

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(towerSE + toCenter * 3.5f, 0.75f, Colors.Safe, 1.0f);
                }
                else if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(towerSE - toCenter * 3.5f, 0.75f, Colors.Safe, 1.0f);
                    }
                    else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(towerSE + (-toCenter).Rotate(-26f.Degrees()) * 3.6f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var offset = 2.0f * toCenter.OrthoL();
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(towerSE + 3.0f * toCenter - offset, 0.75f, Colors.Safe, 1.0f);
                }
                else if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(towerSE - 3.0f * toCenter + offset, 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SE players with same debuffs (helpers)
        else if (shapes.dpsHelpers[pcSlot])
        {
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];
            var toCenter = (center - towerSE).Normalized();

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(towerSE + toCenter.OrthoR() * 4.5f, 0.75f, Colors.Safe, 1.0f);
                    }
                    else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(towerSE + toCenter.Rotate(-82f.Degrees()) * 7.07f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
                else if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(towerSE + toCenter.Rotate(-35.0f.Degrees()) * 11.5f, 0.75f, Colors.Safe, 1.0f);
                }
            }
            else if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var offset = toCenter.OrthoL();
                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(towerSE + toCenter - 7.0f * offset, 0.75f, Colors.Safe, 1.0f);
                }
                else if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(towerSE + 9.0f * toCenter - 6.0f * offset, 0.75f, Colors.Safe, 1.0f);
                }
            }
        }
    }

    public override void Update()
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2)
        {
            return;
        }

        if ((shapes.currentTowerSet & 1) != 0)
        {
            return;
        }

        var party = new BitMask(0xFF);
        var towersSpan = CollectionsMarshal.AsSpan(towers.Towers);
        var tSE = towers.CurrentSE;
        var tSW = towers.CurrentSW;
        for (var i = 0; i < 2; ++i)
        {
            ref var t = ref towersSpan[i];
            if (i == tSW)
            {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }
            else if (i == tSE)
            {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }
        }
    }
}

sealed class WingsOfDestructionLeftRight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WingsOfDestructionLeft, (uint)AID.WingsOfDestructionRight], new AOEShapeRect(80f, 20f));

sealed class WingsOfDestructionTB(BossModule module) : Components.GenericBaitAway(module, (uint)AID.WingsOfDestructionTB, true, true)
{
    private Actor? casterPosition;
    private readonly AOEShapeCircle circle = new(7f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingsOfDestructionTB)
        {
            casterPosition = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WingsOfDestructionTB1)
        {
            casterPosition = null;
            NumCasts++;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (casterPosition == null)
        {
            return;
        }

        var party = Raid.WithoutSlot();
        if (party.Length > 1)
        {
            Actor closest = party[0], farthest = party[0];
            float minD = float.MaxValue, maxD = float.MinValue;
            for (var i = 0; i < party.Length; ++i)
            {
                var d = (party[i].Position - casterPosition.Position).LengthSq();
                if (d < minD)
                {
                    minD = d;
                    closest = party[i];
                }
                if (d >= maxD)
                {
                    maxD = d;
                    farthest = party[i];
                }
            }
            CurrentBaits.Add(new(casterPosition, closest, circle));
            CurrentBaits.Add(new(casterPosition, farthest, circle));
        }
    }
}

sealed class Trine(DMU module) : Components.GenericAOEs(module, (uint)AID.Trine)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> triangles = [];
    private const float radius = 5.77350269189626f; // 10f * MathF.Sqrt(3f) / 3f;
    private const float halfradius = 5.77350269189626f * 0.5f;
    private readonly AOEShapeCircle circle = new(6f);
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly Actor bossP2 = module.BossP2()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is var oid && oid is not (uint)OID.YellowTriangle and not (uint)OID.YellowTriangle1)
        {
            return;
        }

        triangles.Add(actor);

        var direction = oid == (uint)OID.YellowTriangle ? 1f : -1f;
        var pos = actor.Position;

        aoes.Add(new(circle, pos + new WDir(direction * radius, 0f)));
        aoes.Add(new(circle, pos + new WDir(direction * -halfradius, 5f)));
        aoes.Add(new(circle, pos + new WDir(direction * -halfradius, -5f)));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is ((uint)OID.YellowTriangle) or ((uint)OID.YellowTriangle1))
        {
            if (state == (uint)Animations.TriangleExplosion)
            {
                aoes.RemoveRange(0, 3);
                NumCasts += 3;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return CollectionsMarshal.AsSpan(aoes);
        }

        (int currentWave, int nextWave)[] wave = [(9, 3), (3, 9), (9, 0)];
        var (currentSize, nextSize) = wave[NumCasts < 9 ? 0 : NumCasts < 12 ? 1 : 2];
        var count = Math.Min(currentSize + nextSize, aoes.Count);
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = aoes[i] with
            {
                Color = i < currentSize ? Colors.Danger : default,
                Risky = i < currentSize
            };
        }

        return CollectionsMarshal.AsSpan(aoes)[..count];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts < 9)
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var waymarkA = WorldState.Waymarks.GetFieldMark((int)Waymark.A);
        var waymark1 = WorldState.Waymarks.GetFieldMark((int)Waymark.N1);

        if (waymarkA is not Vector3 wayA || waymark1 is not Vector3 way1)
        {
            return;
        }

        var center = Arena.Center;
        var waymarkAAngle = (new WPos(wayA) - center).ToAngle();
        var waymark1Angle = (new WPos(way1) - center).ToAngle();
        var firstWave = new WPos[Math.Min(3, triangles.Count)];
        for (var i = 0; i < firstWave.Length; ++i)
        {
            firstWave[i] = triangles[i].Position;
        }

        Array.Sort(firstWave, delegate (WPos x, WPos y)
        {
            var xAngle = (x - center).ToAngle();
            var yAngle = (y - center).ToAngle();

            var xDeg = xAngle.AlmostEqual(waymarkAAngle, 0.01f) ? 180f : (xAngle - waymarkAAngle + 180f.Degrees()).Normalized().Deg;
            var yDeg = yAngle.AlmostEqual(waymarkAAngle, 0.01f) ? 180f : (yAngle - waymarkAAngle + 180f.Degrees()).Normalized().Deg;

            return xDeg < yDeg ? 1 : -1;
        });

        if (assignment is not PartyRolesConfig.Assignment.MT and not PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(firstWave[0], 1f, Colors.Safe, 2f);
            return;
        }

        var ccwSpot = firstWave[0];
        var bestCCW = float.MaxValue;
        for (var i = 0; i < firstWave.Length; ++i)
        {
            var deg = (waymark1Angle - (firstWave[i] - center).ToAngle()).Normalized().Deg;
            if (deg < bestCCW)
            {
                bestCCW = deg;
                ccwSpot = firstWave[i];
            }
        }
        if (assignment == PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(center + (ccwSpot - center).Normalized() * 20f, 1f, Colors.Safe, 2f);
        }

        var closestSpot = ccwSpot;
        var bossP = bossP2.Position;
        var counter = (ccwSpot - bossP).Length();
        var angleCCW = (ccwSpot - bossP).ToAngle();
        for (var r = 0.5f; r <= counter; r += 0.5f)
        {
            var found = false;
            var bestDeg = float.MaxValue;
            for (var degree = -60f; degree <= 60f; degree += 5f)
            {
                var spot = bossP + r * (angleCCW + degree.Degrees()).ToDirection();

                if (!DMUHelpers.AnyContains(aoes, spot))
                {
                    var deg = ((spot - bossP).ToAngle() - angleCCW).Abs().Deg;
                    if (deg < bestDeg)
                    {
                        bestDeg = deg;
                        closestSpot = spot;
                        found = true;
                    }
                }
            }

            if (found)
            {
                break;
            }
        }

        if (assignment == PartyRolesConfig.Assignment.MT)
        {
            Arena.ZoneCircleOutline(closestSpot, 1f, Colors.Safe, 2f);
        }
    }
}

static class DMUHelpers
{
    public static bool InAnyTower(List<Components.GenericTowers.Tower> towers, WPos pos, float radius)
    {
        var count = towers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (pos.InCircle(towers[i].Position, radius))
                return true;
        }
        return false;
    }

    public static bool AnyContains(List<Components.GenericAOEs.AOEInstance> aoes, WPos pos, int start = 0, int count = int.MaxValue)
    {
        var end = Math.Min(aoes.Count, start + Math.Min(count, aoes.Count));
        for (var i = start; i < end; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return false;
    }

    public static List<Actor> ClosestPlayers(Actor[] party, WPos origin, List<Actor> result, int n = int.MaxValue)
    {
        party.SortedByRange(origin, result);
        if (result.Count > n)
        {
            result.RemoveRange(n, result.Count - n);
        }
        return result;
    }
}

static class DMUShapes
{
    public static readonly AOEShapeCircle Circle15 = new(15.0f);
    public static readonly AOEShapeCircle Circle5 = new(5.0f);
    public static readonly AOEShapeCircle Circle6 = new(6.0f);
    public static readonly AOEShapeCircle Circle8 = new(8.0f);
    public static readonly AOEShapeDonut Donut4_10 = new(4.0f, 10.0f);
    public static readonly AOEShapeDonut Donut6_40 = new(6.0f, 40.0f);
}
