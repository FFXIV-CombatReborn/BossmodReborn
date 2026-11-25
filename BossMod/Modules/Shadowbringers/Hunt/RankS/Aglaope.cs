namespace BossMod.Shadowbringers.Hunt.RankS.Aglaope;

public enum OID : uint
{
    Boss = 0x281E // R=2.4
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    FourfoldSuffering = 16819, // Boss->self, 5.0s cast, range 5-50 donut
    SeductiveSonata = 16824, // Boss->self, 3.0s cast, range 40 circle, applies Seduced for 6s (forced march towards boss at 1.7y/s)
    DeathlyVerse = 17074, // Boss->self, 5.0s cast, range 6 circle (right after Seductive Sonata, instant kill), 6*1.7 = 10.2 + 6 = 16.2y minimum distance to survive
    Tornado = 18040, // Boss->location, 3.0s cast, range 6 circle
    AncientAero = 16823, // Boss->self, 3.0s cast, range 40+R width 6 rect
    SongOfTorment = 16825, // Boss->self, 5.0s cast, range 50 circle, interruptible raidwide with bleed
    AncientAeroIII = 18056 // Boss->self, 3.0s cast, range 30 circle, knockback 10, away from source
}

public enum SID : uint
{
    Seduced = 991 // Boss->player, extra=0x11
}

sealed class SongOfTorment(BossModule module) : Components.CastInterruptHint(module, (uint)AID.SongOfTorment, hintExtra: "Raidwide + Bleed");

sealed class SeductiveSonata(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    bool done;
    private static readonly AOEShapeCircle circle = new(16.2f); // circle + minimum distance to survive seducing status

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SeductiveSonata)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SeductiveSonata)
        {
            done = true;
        }
    }

    public override void Update()
    {
        // this AOE should wait for Effect Results, since they can be delayed by over 2.1s, which would cause unknowning players and AI to run back into the death zone
        if (done)
        {
            if (Module.Raid.Player() is not Actor player)
                return;
            var statuses = CollectionsMarshal.AsSpan(player.PendingStatuses);
            var len = statuses.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var s = ref statuses[i];
                if (s.StatusId == (uint)SID.Seduced)
                {
                    return;
                }
            }
            _aoe = [];
            done = false;
        }
    }
}

sealed class DeathlyVerse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyVerse, 6f);
sealed class Tornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tornado, 6f);
sealed class FourfoldSuffering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FourfoldSuffering, new AOEShapeDonut(5f, 50f));
sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(42.4f, 3f));
sealed class AncientAeroIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientAeroIII);
sealed class AncientAeroIIIKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AncientAeroIII, 10f, shape: new AOEShapeCircle(30f));

sealed class AglaopeStates : StateMachineBuilder
{
    public AglaopeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<SeductiveSonata>()
            .ActivateOnEnter<DeathlyVerse>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<FourfoldSuffering>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<AncientAeroIIIKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8653)]
public sealed class Aglaope(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
