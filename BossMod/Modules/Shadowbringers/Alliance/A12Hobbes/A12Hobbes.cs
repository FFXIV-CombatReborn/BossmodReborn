namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class LaserResistanceTest(BossModule module) : Components.RaidwideCast(module, (uint)AID.LaserResistanceTest);

sealed class LaserResistanceTestRepeat(BossModule module) : Components.RaidwideInstant(module, (uint)AID.LaserResistanceTestRepeat, 1.1d)
{
    private int _sets;
    private int _remaining;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.LaserResistanceTest)
        {
            _remaining = 2 + _sets++;
            Activation = WorldState.FutureTime(Delay);
        }
        else if (id == WatchedAction)
        {
            ++NumCasts;
            Activation = --_remaining > 0 ? WorldState.FutureTime(Delay) : default;
        }
    }
}

sealed class ShockingDischarge(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.ShockingDischarge, GetVoidzones, 1.03d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ShockingDischargeVoidzone);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class VariableCombatTestCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatConeSlow, (uint)AID.VariableCombatConeFast], new AOEShapeCone(20f, 30f.Degrees()));
sealed class VariableCombatTestCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatCircleSlow, (uint)AID.VariableCombatCircleFast], 2f);
sealed class VariableCombatTestDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatDonutSlow, (uint)AID.VariableCombatDonutFast], new AOEShapeDonut(7f, 19f));

sealed class RingLaser(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(15f, 20f), new AOEShapeDonut(10f, 15f), new AOEShapeDonut(5f, 10f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RingLaser1)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.RingLaser1 => 0,
            (uint)AID.RingLaser2 => 1,
            (uint)AID.RingLaser3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.1d));
    }
}

sealed class LaserSight(BossModule module) : Components.GenericWildCharge(module, 4f, fixedLength: 60f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LaserSightTarget:
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                {
                    Source = Module.PrimaryActor;
                    PlayerRoles[slot] = PlayerRole.Target;
                    var len = PlayerRoles.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        if (PlayerRoles[i] == PlayerRole.Ignore)
                        {
                            PlayerRoles[i] = PlayerRole.Share;
                        }
                    }
                    Activation = WorldState.FutureTime(8.1d);
                }
                break;
            case (uint)AID.LaserSightStack:
                ++NumCasts;
                Source = null;
                Activation = default;
                Array.Fill(PlayerRoles, PlayerRole.Ignore);
                break;
        }
    }
}

sealed class ShortRangeMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShortRangeMissile, 8f)
{
    private readonly UnwillingCargo _cargo = module.FindComponent<UnwillingCargo>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // players on the knockback platform have more important things to worry about
        if (!_cargo.IsAffected(actor))
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_cargo.IsAffected(actor))
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9143, SortOrder = 3)] // 9144 and 9145 also listed as Hobbes
public sealed class A12Hobbes(WorldState ws, Actor primary) : BossModule(ws, primary, PlatformCenterFor(ws.Party.Player()), platformBounds)
{
    public static readonly WPos ArenaCenter = new(-805f, -240f);
    public static readonly WPos[] PlatformCenters = [ArenaCenter + new WDir(-26f, 15f), ArenaCenter + new WDir(default, -30f), ArenaCenter + new WDir(26f, 15f)]; // W, N, E platforms
    public const float PlatformRadius = 20f;
    private static readonly ArenaBoundsCircle platformBounds = new(PlatformRadius);

    private static WPos PlatformCenterFor(Actor? actor) => actor != null ? PlatformCenters[PlatformIndex(actor.Position)] : PlatformCenters[1];

    private void UpdatePlatformCenter()
    {
        if (Raid.Player() is Actor player)
        {
            var center = PlatformCenters[PlatformIndex(player.Position)];
            if (Arena.Center != center)
            {
                Arena.Center = center;
            }
        }
    }

    protected override void UpdatePreModuleActivation() => UpdatePlatformCenter();
    protected override void UpdateModule() => UpdatePlatformCenter();

    public static int PlatformIndex(WPos pos)
    {
        var best = 0;
        var bestDistSq = float.MaxValue;
        for (var i = 0; i < 3; ++i)
        {
            var distSq = (PlatformCenters[i] - pos).LengthSq();
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = i;
            }
        }
        return best;
    }
}
