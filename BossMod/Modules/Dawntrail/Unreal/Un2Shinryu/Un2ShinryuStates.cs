namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

// phase 1 is a fixed timeline (two kills on 2026-10-18 matched within 0.3s), taken from the first earthen fury cast.
// two slots are random: levinbolt or hypernova at 60s, hellfire or judgment bolt at 102s. dark matter is an hp push
// (seen at 266s, 302s and 345s) and ends the phase. everything after ~300s comes from a single partial pull.
// phases 2 and 3 are simple enough to run with everything active and no timeline.
class Un2ShinryuStates : StateMachineBuilder
{
    private float _t = -10.2f; // timeline cursor: when the previous state ends, in seconds from the first earthen fury cast, which comes 10.2s after the pull

    public Un2ShinryuStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "Phase 1: tiles")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.CastInfo?.IsSpell(AID.DarkMatter) == true;
        DeathPhase(1, id => AllComponents(SimpleState(id, 10000f, "Phase 2 + 3")));
    }

    private void Phase1(uint id)
    {
        AllComponents(BossCast(id, AID.EarthenFuryVisual, 0f, 10f, "Earthen Fury (raidwide, tiles crack)")).SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x10, 21.0f, "Chains x4");
        BossCast(id + 0x20, AID.TidalWaveVisual, 23.2f, 10f, "Tidal Wave (knockback)").SetHint(StateMachine.StateHint.Knockback);
        At(id + 0x30, 38.2f, "Tail marker");
        WingCast(id + 0x40, OID.LeftWing, AID.SummonIcicle, 46.3f, 4f, "Icicles");
        At(id + 0x50, 56.1f, "Tail Slap + Spikesicle");
        WingCastMulti(id + 0x60, OID.RightWing, [AID.LevinboltVisual, AID.HypernovaVisual], 60.4f, 8f, "Levinbolt (spread) or Hypernova (stack)");
        WingCast(id + 0x70, OID.LeftWing, AID.IceStormVisual, 77.4f, 6f, "Ice Storm (raidwide)").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x80, 83.5f, "Dragonfist (center)");
        BossCast(id + 0x90, AID.AkhMorn, 92.6f, 4f, "Akh Morn (tank stack x3)").SetHint(StateMachine.StateHint.Tankbuster);
        BossCastMulti(id + 0xA0, [AID.HellfireVisual, AID.JudgmentBoltVisual], 101.9f, 10f, "Hellfire (in water) or Judgment Bolt (out), + Spikesicle").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0xB0, 127.0f, "Tail marker");
        WingCast(id + 0xC0, OID.RightWing, AID.LevinboltVisual, 130.1f, 8f, "Levinbolt (spread)");
        At(id + 0xD0, 145.0f, "Tail Slap");
        WingCast(id + 0xE0, OID.LeftWing, AID.IceStormVisual, 147.2f, 6f, "Ice Storm (raidwide)").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0xF0, 154.1f, "Chains x2 (tanks + healers)");
        BossCast(id + 0x100, AID.EarthBreath, 163.3f, 6f, "Earth Breath (2 baits)");
        BossCast(id + 0x110, AID.AkhMorn, 172.7f, 4f, "Akh Morn (tank stack x3)").SetHint(StateMachine.StateHint.Tankbuster);
        WingCast(id + 0x120, OID.LeftWing, AID.IceStormVisual, 176.7f, 6f, "Ice Storm (raidwide)").SetHint(StateMachine.StateHint.Raidwide);
        BossCast(id + 0x130, AID.DiamondDustVisual, 187.0f, 10f, "Diamond Dust (raidwide, thin ice, healer chains)").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x140, 214.3f, "Reiryu spawn (heal them)");
        At(id + 0x150, 222.2f, "Tail marker, Dragonflight starts");
        At(id + 0x160, 240.1f, "Tail Slap");
        At(id + 0x170, 251.5f, "Dragonflight ends");
        WingCast(id + 0x180, OID.LeftWing, AID.SummonIcicle, 253.4f, 4f, "Icicles");
        BossCast(id + 0x190, AID.AkhMorn, 257.4f, 4f, "Akh Morn (tank stack x3)").SetHint(StateMachine.StateHint.Tankbuster);
        BossCast(id + 0x1A0, AID.AerialBlastVisual, 270.7f, 10f, "Aerial Blast (knockback) + Super Cyclones").SetHint(StateMachine.StateHint.Knockback);
        BossCast(id + 0x1B0, AID.EarthBreath, 301.6f, 6f, "Earth Breath (2 baits)");
        At(id + 0x1C0, 324.6f, "Gyre Charge");
        WingCast(id + 0x1D0, OID.RightWing, AID.HypernovaVisual, 331.6f, 8f, "Hypernova (stack)");
        SimpleState(id + 0x1E0, 60f, "Akh Morn, Levinbolt, Tidal Wave, Dark Matter (unverified)");
    }

    private State BossCast(uint id, AID aid, float at, float castTime, string name)
    {
        var s = Cast(id, aid, at - _t, castTime, name);
        _t = at + castTime;
        return s;
    }

    private State BossCastMulti(uint id, AID[] aids, float at, float castTime, string name)
    {
        var s = CastMulti(id, aids, at - _t, castTime, name);
        _t = at + castTime;
        return s;
    }

    private State WingCast(uint id, OID wing, AID aid, float at, float castTime, string name)
    {
        var s = ActorCast(id, () => Module.Enemies((uint)wing).FirstOrDefault(), aid, at - _t, castTime, false, name);
        _t = at + castTime;
        return s;
    }

    private State WingCastMulti(uint id, OID wing, AID[] aids, float at, float castTime, string name)
    {
        var s = ActorCastMulti(id, () => Module.Enemies((uint)wing).FirstOrDefault(), aids, at - _t, castTime, false, name);
        _t = at + castTime;
        return s;
    }

    private State At(uint id, float at, string name)
    {
        var s = Timeout(id, at - _t, name);
        _t = at;
        return s;
    }

    private static State AllComponents(State s) => s
        .ActivateOnEnter<ArenaChanges>()
        .ActivateOnEnter<EarthenFury>()
        .ActivateOnEnter<EarthenFurySmash>()
        .ActivateOnEnter<Hellfire>()
        .ActivateOnEnter<JudgmentBolt>()
        .ActivateOnEnter<DiamondDust>()
        .ActivateOnEnter<IceStorm>()
        .ActivateOnEnter<DarkMatter>()
        .ActivateOnEnter<Protostar>()
        .ActivateOnEnter<TailSpit>()
        .ActivateOnEnter<Enrage>()
        .ActivateOnEnter<TidalWave>()
        .ActivateOnEnter<AerialBlast>()
        .ActivateOnEnter<SuperCyclone>()
        .ActivateOnEnter<Levinbolt>()
        .ActivateOnEnter<LevinboltAOE>()
        .ActivateOnEnter<Hypernova>()
        .ActivateOnEnter<IcicleAdds>()
        .ActivateOnEnter<IcicleImpact>()
        .ActivateOnEnter<Spikesicle>()
        .ActivateOnEnter<SpikesicleKnockback>()
        .ActivateOnEnter<AkhMorn>()
        .ActivateOnEnter<EarthBreath>()
        .ActivateOnEnter<Dragonfist>()
        .ActivateOnEnter<GyreCharge>()
        .ActivateOnEnter<BurningChains>()
        .ActivateOnEnter<TailMarker>()
        .ActivateOnEnter<TailSlap>()
        .ActivateOnEnter<ThinIce>()
        .ActivateOnEnter<Dragonflight>()
        .ActivateOnEnter<Cocoons>()
        .ActivateOnEnter<DragonAdds>()
        .ActivateOnEnter<MeteorImpact>()
        .ActivateOnEnter<MeteorImpact2>()
        .ActivateOnEnter<Touchdown>()
        .ActivateOnEnter<Fireball>()
        .ActivateOnEnter<BlazingTrail>()
        .ActivateOnEnter<SpikedTail>()
        .ActivateOnEnter<DeathSentence>()
        .ActivateOnEnter<CocoonSpread>()
        .ActivateOnEnter<TeraSlash>()
        .ActivateOnEnter<AtomicRay>()
        .ActivateOnEnter<AetherialRay>()
        .ActivateOnEnter<Wormwail>()
        .ActivateOnEnter<BenightingBreath>();
}
