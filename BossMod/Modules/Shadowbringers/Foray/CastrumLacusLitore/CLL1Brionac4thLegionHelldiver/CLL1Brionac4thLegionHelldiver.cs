using static BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver.CLL1Brionac4thLegionHelldiver;

namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class ElectricAnvil(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ElectricAnvil)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_arena.IsBrionacArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_arena.IsBrionacArena)
            base.AddGlobalHints(hints);
    }
}

sealed class MagitekMissiles(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MagitekMissiles)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_arena.IsBrionacArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_arena.IsBrionacArena)
        {
            base.AddGlobalHints(hints);
        }
    }
}

sealed class MRVMissile(BossModule module) : Components.RaidwideCast(module, (uint)AID.MRVMissile)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_arena.IsBrionacArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_arena.IsBrionacArena)
            base.AddGlobalHints(hints);
    }
}

sealed class LightningShower(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightningShower)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_arena.IsBrionacArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_arena.IsBrionacArena)
            base.AddGlobalHints(hints);
    }
}

sealed class FalseThunder(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.FalseThunder1, (uint)AID.FalseThunder2], new AOEShapeCone(47f, 65f.Degrees()))
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsBrionacArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class Voltstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voltstream, new AOEShapeRect(40f, 5f), 3)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsBrionacArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsBrionacArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class CommandSuppressiveFormation(BossModule module) : Components.ChargeAOEs(module, (uint)AID.CommandSuppressiveFormation, 3f)
{
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsBrionacArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class DetermineArena(BossModule module) : BossComponent(module)
{
    public bool IsBrionacArena;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (IsBrionacArena && ArenaBottom.Contains(pc.Position - ArenaCenterBottom))
        {
            IsBrionacArena = false;
            Arena.Center = ArenaCenterBottom;
            Arena.Bounds = ArenaBottom;
        }
        else if (!IsBrionacArena && ArenaTop.Contains(pc.Position - ArenaCenterTop))
        {
            IsBrionacArena = true;
            Arena.Center = ArenaCenterTop;
            Arena.Bounds = ArenaTop;
        }
    }
}

sealed class BossHealths(BossModule module) : BossComponent(module)
{
    private readonly Actor? _bossHellDiver = module.Enemies((uint)OID.FourthLegionHelldiver1)[0];

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Top: {Module.PrimaryActor.HPRatio * 100f:f1}%, Bottom: {_bossHellDiver?.HPRatio * 100f:f1}%");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735, NameID = 9436)]
public sealed class CLL1Brionac4thLegionHelldiver : BossModule
{
    public CLL1Brionac4thLegionHelldiver(WorldState ws, Actor primary) : base(ws, primary, ArenaCenterBottom, ArenaBottom)
    {
        ActivateComponent<DetermineArena>();
    }

    public Actor? BossHellDiver;

    protected override void UpdateModule()
    {
        BossHellDiver ??= GetActor((uint)OID.FourthLegionHelldiver1);
    }

    protected override bool CheckPull() => base.CheckPull() || (BossHellDiver?.InCombat ?? false);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (Arena.Center == ArenaCenterTop)
        {
            Arena.Actor(PrimaryActor);
        }
        else
        {
            Arena.Actors(Enemies((uint)OID.FourthLegionHelldiver3));
            Arena.Actor(BossHellDiver);
        }
        var skyarmors = Enemies((uint)OID.FourthLegionSkyArmor);
        var count = skyarmors.Count;
        for (var i = 0; i < count; ++i)
        {
            var skyarmor = skyarmors[i];
            if (Arena.InBounds(skyarmor.Position))
            {
                Arena.Actor(skyarmor);
            }
        }
    }

    public static readonly WPos ArenaCenterBottom = new(80f, -179.41f);
    public static readonly ArenaBoundsRect ArenaBottom = new(29.58f, 24.59f);
    public static readonly WPos ArenaCenterTop = new(new(80f, -222f));
    public static readonly ArenaBoundsRect ArenaTop = new(29.5f, 14.5f);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var potHints = CollectionsMarshal.AsSpan(hints.PotentialTargets);
        var center = Arena.Center;
        for (var i = 0; i < count; ++i)
        {
            var h = potHints[i];
            var e = h.Actor;
            ref var enemyPrio = ref potHints[i].Priority;
            var oid = e.OID;
            if (center == ArenaCenterTop)
            {
                if (oid == (uint)OID.MagitekCore)
                    enemyPrio = 1;
                else if (e == PrimaryActor && e.HPRatio - BossHellDiver?.HPRatio < -0.1f)
                    enemyPrio = AIHints.Enemy.PriorityForbidden;
                else if (oid == (uint)OID.FourthLegionSkyArmor && Arena.InBounds(e.Position))
                    enemyPrio = 0;
                else if (oid != (uint)OID.Boss)
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
            }
            else
            {
                if (oid == (uint)OID.FourthLegionHelldiver3)
                    enemyPrio = 1;
                else if (e == BossHellDiver && e.HPRatio - PrimaryActor.HPRatio < -0.1f)
                    enemyPrio = AIHints.Enemy.PriorityForbidden;
                else if (oid == (uint)OID.FourthLegionSkyArmor && Arena.InBounds(e.Position))
                    enemyPrio = 0;
                else if (oid != (uint)OID.FourthLegionHelldiver1)
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
            }
        }
    }
}
