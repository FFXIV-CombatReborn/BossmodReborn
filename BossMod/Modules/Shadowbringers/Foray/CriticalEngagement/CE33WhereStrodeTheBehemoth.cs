namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE33WhereStrodeTheBehemoth;

public enum OID : uint
{
    Boss = 0x2DCB, // R8.7
    BestialCorpse = 0x2DCC, // R4.35
    Voidzone = 0x1E972C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    Thunderbolt = 20197, // BestialCorpse->self, 6.0s cast, range 45 90-degree cone
    WildBoltVisual = 20200, // Boss->self, 4.0s cast, single-target
    WildBolt = 20201, // Helper->self, no cast, range 30 circle
    BlazingMeteor = 20194, // Boss->location, 6.0s cast, range 30 circle
    ZombieBreath = 20198, // Boss->self, 3.0s cast, range 60 width 6 rect
    WildHorn = 20193, // Boss->self/players, 6.0s cast, range 18 120-degree cone
    ZombieBile = 20195, // Boss->self, 4.0s cast, single-target
    ZombieJuiceFirst = 20196, // Helper->location, 6.0s cast, range 6 circle
    ZombieJuiceRepeat = 20199 // Helper->location, no cast, range 6 circle
}

sealed class BlazingMeteor(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.BlazingMeteor, 30f, safeInsideHitbox: false)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.BestialCorpse));
}

sealed class ZombieJuice(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.ZombieJuiceFirst, GetVoidzones, 0.9d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class WildBolt(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.WildBoltVisual, (uint)AID.WildBolt, 0.9d);
sealed class WildHorn(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WildHorn, new AOEShapeCone(18f, 60f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class Thunderbolt(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(45f, 45f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(3)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ZombieJuiceFirst:
            case (uint)AID.ZombieBreath:
                var corpses = Module.Enemies((uint)OID.BestialCorpse);
                var count = corpses.Count;
                var loc = spell.LocXZ;
                if (spell.Action.ID == (uint)AID.ZombieJuiceFirst)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var corpse = corpses[i];
                        var pos = corpse.Position;
                        if (pos.InCircle(loc, 6f))
                        {
                            AddAOE(pos, corpse.Rotation);
                            return; // never more than one corpse per circle
                        }
                    }
                }
                else
                {
                    var rot = spell.Rotation.ToDirection();
                    for (var i = 0; i < count; ++i)
                    {
                        var corpse = corpses[i];
                        var pos = corpse.Position;
                        if (pos.InRect(loc, rot, 60f, default, 7.35f)) // 3 half width + 4.35 hitbox radius
                        {
                            AddAOE(pos, corpse.Rotation);
                            // no early exit, if unlucky boss tanking, rect can hit multiple corpses
                        }
                    }
                }
                break;
            case (uint)AID.Thunderbolt:
                _aoes.Clear();
                break;
        }
        void AddAOE(WPos pos, Angle rot) => _aoes.Add(new(cone, pos.Quantized(), rot, WorldState.FutureTime(12.8d)));
    }
}

sealed class ZombieBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ZombieBreath, new AOEShapeRect(60f, 3f));

sealed class CE33WhereStrodeTheBehemothStates : StateMachineBuilder
{
    public CE33WhereStrodeTheBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlazingMeteor>()
            .ActivateOnEnter<ZombieJuice>()
            .ActivateOnEnter<WildBolt>()
            .ActivateOnEnter<WildHorn>()
            .ActivateOnEnter<ZombieBreath>()
            .ActivateOnEnter<Thunderbolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 15)]
public sealed class CE33WhereStrodeTheBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(231f, 95f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 30f);
}
