namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class WindsPeak(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindsPeak, 5f)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class WindsPeakKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WindsPeak, 10f)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return base.ActiveKnockbacks(slot, actor);
        else
            return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_arena.IsDawonArena && Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 10f), c.Activation);
        }
    }
}

sealed class HeartOfNature(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeartOfNature)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_arena.IsDawonArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_arena.IsDawonArena)
            base.AddGlobalHints(hints);
    }
}

sealed class TheKingsNotice(BossModule module) : Components.CastGaze(module, (uint)AID.TheKingsNotice)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return base.ActiveEyes(slot, actor);
        else
            return [];
    }
}

sealed class TasteOfBlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TasteOfBlood, new AOEShapeCone(40f, 90f.Degrees()))
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class Pentagust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pentagust, new AOEShapeCone(50f, 10f.Degrees()))
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class FervidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FervidPulse, Obey.Cross)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class FrigidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidPulse, Obey.Donut);

sealed class SwoopingFrenzy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzy, 12f)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class MoltingPlumage(BossModule module) : Components.RaidwideCast(module, (uint)AID.MoltingPlumage);

sealed class Scratch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Scratch)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_arena.IsDawonArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_arena.IsDawonArena)
        {
            base.AddGlobalHints(hints);
        }
    }
}

sealed class TwinAgonies(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TwinAgonies)
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_arena.IsDawonArena)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_arena.IsDawonArena)
        {
            base.AddGlobalHints(hints);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735, NameID = 9452)]
public sealed class CLL4Dawon(WorldState ws, Actor primary) : BossModule(ws, primary, DawonStartingArena.Center, DawonStartingArena)
{
    public static readonly WPos LyonCenter = new(80f, -874f);
    public static readonly ArenaBoundsCustom LyonStartingArena = new([new Polygon(LyonCenter, 24.5f, 48)]);
    public static readonly ArenaBoundsCustom LyonDefaultArena = new([new Polygon(LyonCenter, 20f, 48)]);
    public static readonly WPos DawonCenter = new(80f, -813f);
    public static readonly ArenaBoundsCustom DawonStartingArena = new([new Polygon(DawonCenter, 34.5f, 96)], [new Rectangle(new(80f, -778f), 20f, 1.25f)]);
    public static readonly ArenaBoundsCustom DawonDefaultArena = new([new Polygon(DawonCenter, 30f, 96)]);
    private static readonly uint[] adds = [(uint)OID.TamedBeetle, (uint)OID.TamedCoeurl, (uint)OID.TamedManticore];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var potHints = CollectionsMarshal.AsSpan(hints.PotentialTargets);
        var center = Arena.Center;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var e = ref potHints[i].Actor;
            ref var enemyPrio = ref potHints[i].Priority;
            ref readonly var oid = ref e.OID;
            if (center == LyonCenter)
            {
                if (oid != (uint)OID.LyonTheBeastKing)
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
            }
            else
            {
                if (oid is not (uint)OID.Boss and not (uint)OID.LyonTheBeastKing)
                    enemyPrio = 1;
                else if (oid == (uint)OID.Boss)
                    enemyPrio = 0;
                else if (oid == (uint)OID.LyonTheBeastKing)
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (Arena.Center == LyonCenter)
        {
            Arena.Actors(Enemies((uint)OID.LyonTheBeastKing));
        }
        else
        {
            Arena.Actors(this, adds);
            Arena.Actor(PrimaryActor);
        }
    }
}
