namespace BossMod.Dawntrail.Criterion.C01AMT.C012LoneSwordmaster;

sealed class C012LoneSwordmasterStates : StateMachineBuilder
{
    private static readonly AID[] _steelsbreath = [AID.SteelsbreathReleaseCast, AID.SteelsbreathReleaseLavaCast];
    private static readonly AID[] _fromHeaven = [AID.NearToHeavenOneCast, AID.FarFromHeavenOneCast, AID.NearToHeavenTwoCast, AID.FarFromHeavenTwoCast];
    private static readonly AID[] _echoes = [AID.EchoingEightCast, AID.EchoingOrbitCast];

    public C012LoneSwordmasterStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Malefic1(id, 9.2f);
        Echoes(id + 0x10000u, 5.5f);
        Malefic2(id + 0x20000u, 5.2f);
        Plummet(id + 0x30000u, 11f);
        Malefic3(id + 0x40000u, 10f);

        Cast(id + 0x50000u, AID.MaleficQuartering, 8.8f, 3f);
        Cast(id + 0x50010u, AID.MortalFateCast, 5.2f, 12f);
        Timeout(id + 0x50020u, 1f, "Enrage");
    }

    private void Malefic1(uint id, float delay)
    {
        SteelsbreathRelease(id, delay);
        FromHeaven(id + 0x100u, 7.2f);

        Cast(id + 0x200u, AID.MaleficQuartering, 2f, 3f)
            .ActivateOnEnter<Malefic>();

        Cast(id + 0x210u, AID.ShiftingHorizon, 3.5f, 4f, "Diagonals")
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<ForceOfWill>()
            .ActivateOnEnter<MaleficPortent>();

        ComponentCondition<MaleficPortent>(id + 0x212u, 3f, static p => p.Active, "Tethers appear")
            .DeactivateOnExit<LashOfLight>();
        Cast(id + 0x220u, AID.MaleficPortentCast, 0.1f, 6f);
        ComponentCondition<MaleficPortent>(id + 0x222u, 1f, static m => m.NumCasts > 0, "Tethers resolve")
            .DeactivateOnExit<MaleficPortent>();
        ComponentCondition<ForceOfWill>(id + 0x230u, 1.3f, static w => w.NumCasts > 0, "Charges 1");
        ComponentCondition<ForceOfWill>(id + 0x231u, 0.5f, static w => w.NumCasts > 2, "Charges 2")
            .DeactivateOnExit<ForceOfWill>()
            .DeactivateOnExit<Malefic>();
    }

    private void Echoes(uint id, float delay)
    {
        CastMulti(id, _echoes, delay, 4f)
            .ActivateOnEnter<EchoesBait>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoPredict>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<EchoingOrbit>();

        ComponentCondition<EchoingHush>(id + 0x10u, 0.1f, static h => h.Casters.Count > 0, "Puddle bait")
            .DeactivateOnExit<EchoesBait>();
        ComponentCondition<EchoingHush>(id + 0x11u, 4f, static h => h.NumCasts > 0, "Puddles")
            .DeactivateOnExit<EchoingHush>();
        ComponentCondition<EchoPredict>(id + 0x12u, 3f, static p => p.NumCasts > 0, "Stored AOEs")
            .DeactivateOnExit<EchoPredict>()
            .DeactivateOnExit<EchoingEight>()
            .DeactivateOnExit<EchoingOrbit>();

        ComponentCondition<WaitingWounds>(id + 0x100u, 4.4f, static w => w.NumCasts > 0, "Puddles 1")
            .ActivateOnEnter<WaitingWounds>();
        ComponentCondition<WaitingWounds>(id + 0x101u, 1f, static w => w.NumCasts > 3, "Puddles 2");

        FromHeaven(id + 0x200u, 1f, true);

        Cast(id + 0x300u, AID.SilentEightCast, 1.1f, 4f)
            .ActivateOnEnter<ResoundingSilence>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<MawOfTheWolf>();
        ComponentCondition<ResoundingSilence>(id + 0x310u, 5f, static s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<ResoundingSilence>();
        ComponentCondition<EchoingEight>(id + 0x311u, 3.2f, static e => e.NumCasts > 0, "Crosses")
            .DeactivateOnExit<EchoingEight>();
        ComponentCondition<MawOfTheWolf>(id + 0x312u, 3.2f, static m => m.NumCasts > 0, "Half-room")
            .DeactivateOnExit<MawOfTheWolf>();

        Fangs(id + 0x400u, 3.2f);
    }

    private void Malefic2(uint id, float delay)
    {
        SteelsbreathRelease(id, delay, "Raidwide + arena change")
            .ActivateOnEnter<LavaRect>();

        Cast(id + 0x10u, AID.MaleficQuartering, 3.3f, 3f)
            .ActivateOnEnter<Malefic>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<ForceOfWill>()
            .ActivateOnEnter<WillOfTheUnderworldSmallSlow>()
            .ActivateOnEnter<ResoundingSilence>()
            .ActivateOnEnter<ResoundingSilencePuddle>();

        ComponentCondition<LashOfLight>(id + 0x20u, 8.4f, static l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x21u, 2.1f, static l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>();

        ComponentCondition<WillOfTheUnderworldSmallSlow>(id + 0x30u, 11.4f, static w => w.NumCasts > 0, "Direction AOEs")
            .DeactivateOnExit<WillOfTheUnderworldSmallSlow>();
        ComponentCondition<ResoundingSilence>(id + 0x31u, 0.7f, static s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<ResoundingSilence>()
            .DeactivateOnExit<ForceOfWill>();

        FromHeaven(id + 0x100u, 2.1f);

        Cast(id + 0x200u, AID.MaleficQuartering, 3f, 3f);
        Cast(id + 0x210u, AID.CardinalHorizons, 3.6f, 4f)
            .ActivateOnEnter<LashOfLight>();
        ComponentCondition<LashOfLight>(id + 0x220u, default, static l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x221u, 2.1f, static l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>();

        Cast(id + 0x230u, AID.MawOfTheWolfCast, 0.2f, 3.4f)
            .ActivateOnEnter<MawOfTheWolf>()
            .ActivateOnEnter<WillOfTheUnderworldSmallFast>()
            .ActivateOnEnter<MaleficPortent>()
            .ActivateOnEnter<MaleficPortentCounter>()
            .ExecOnEnter<WillOfTheUnderworldSmallFast>(static n => n.Risky = false);
        ComponentCondition<MawOfTheWolf>(id + 0x232u, 1.5f, static m => m.NumCasts > 0, "Half-room")
            .DeactivateOnExit<MawOfTheWolf>()
            .ExecOnExit<WillOfTheUnderworldSmallFast>(static n => n.Risky = true);

        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x240u, 2.3f, static w => w.NumCasts > 0, "Directions 1");
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x241u, 5.6f, static w => w.NumCasts > 4, "Directions 2");
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x242u, 5.6f, static w => w.NumCasts > 8, "Directions 3");
        ComponentCondition<MaleficPortentCounter>(id + 0x243u, 3.2f, static m => m.NumCasts > 0, "Tethers")
            .DeactivateOnExit<MaleficPortent>()
            .DeactivateOnExit<MaleficPortentCounter>();
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x244u, 2.4f, static w => w.NumCasts > 12, "Directions 4")
            .DeactivateOnExit<ResoundingSilencePuddle>()
            .DeactivateOnExit<WillOfTheUnderworldSmallFast>();

        Cast(id + 0x300u, AID.MaleficAlignmentCast, 2.3f, 3f)
            .ActivateOnEnter<MaleficAlignment>();
        ComponentCondition<MaleficAlignment>(id + 0x302u, 1f, static m => m.NumCasts > 0, "Directions 5")
            .DeactivateOnExit<MaleficAlignment>();

        Fangs(id + 0x400u, 2.2f);

        SteelsbreathRelease(id + 0x500u, 5.1f, "Raidwide + restore arena")
            .DeactivateOnExit<Malefic>();
    }

    private void Plummet(uint id, float delay)
    {
        ComponentCondition<PlummetProximity>(id, delay, static p => p.NumCasts > 0, "Proximity")
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<PlummetProximity>()
            .DeactivateOnExit<PlummetProximity>()
            .DeactivateOnExit<LavaRect>();

        ComponentCondition<SilentEightRock>(id + 0x10u, 2.2f, static r => r.NumRocks > 0, "Rocks appear")
            .ActivateOnEnter<SilentEightRock>()
            .ActivateOnEnter<RockPuddle>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<SteelsbreathBonds>();

        ComponentCondition<SteelsbreathBonds>(id + 0x20u, 3.5f, static s => s.TethersAssigned, "Chains appear");
        ComponentCondition<EchoingHush>(id + 0x21u, 5.7f, static e => e.Casters.Count > 0, "Puddles baited")
            .DeactivateOnExit<SilentEightRock>();
        ComponentCondition<EchoingHush>(id + 0x22u, 4f, static e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<EchoingHush>()
            .DeactivateOnExit<SteelsbreathBonds>()
            .DeactivateOnExit<Plummet>();

        ComponentCondition<EchoingEight>(id + 0x30u, 3f, static e => e.NumCasts > 0, "Crosses")
            .DeactivateOnExit<EchoingEight>();
    }

    private void Malefic3(uint id, float delay)
    {
        Cast(id, AID.MaleficQuartering, delay, 3f)
            .ActivateOnEnter<Malefic>()
            .ActivateOnEnter<WillOfTheUnderworldLarge>();

        Cast(id + 0x10u, AID.CardinalHorizons, 5.6f, 4f)
            .ActivateOnEnter<LashOfLight>();
        ComponentCondition<LashOfLight>(id + 0x20u, default, static l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x21u, 2.1f, static l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>()
            .ExecOnExit<WillOfTheUnderworldLarge>(static w => w.Risky = true);

        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x100u, 8f, static w => w.NumCasts > 0, "Rocks 1");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x101u, 5.6f, static w => w.NumCasts > 4, "Rocks 2");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x102u, 5.6f, static w => w.NumCasts > 8, "Rocks 3");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x103u, 5.6f, static w => w.NumCasts > 12, "Rocks 4")
            .DeactivateOnExit<WillOfTheUnderworldLarge>();

        SteelsbreathRelease(id + 0x200u, 0.6f)
            .DeactivateOnExit<RockPuddle>()
            .DeactivateOnExit<Malefic>();

        Fangs(id + 0x300u, 3.2f);
    }

    private State SteelsbreathRelease(uint id, float delay, string hint = "Raidwide")
    {
        return CastMulti(id, _steelsbreath, delay, 5f, hint)
           .ActivateOnEnter<SteelsbreathRelease>()
           .DeactivateOnExit<SteelsbreathRelease>();
    }

    private void FromHeaven(uint id, float delay, bool waitingWounds = false)
    {
        CastStartMulti(id, _fromHeaven, delay)
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<FromHeaven>();
        if (waitingWounds)
        {
            ComponentCondition<WaitingWounds>(id + 1u, default, static w => w.NumCasts > 6, "Puddles 3")
                .DeactivateOnExit<WaitingWounds>();
        }
        CastEnd(id + 2u, 5f);

        ComponentCondition<HeavensConfluence>(id + 0x10u, 0.3f, static c => c.NumCasts > 0, "In/out 1");
        ComponentCondition<HeavensConfluence>(id + 0x11u, 2.1f, static c => c.NumCasts > 1, "In/out 2")
            .DeactivateOnExit<HeavensConfluence>()
            .DeactivateOnExit<FromHeaven>();
    }

    private void Fangs(uint id, float delay)
    {
        CastStart(id, AID.FangsOfTheUnderworldCast, delay)
            .ActivateOnEnter<FangsOfTheUnderworld>();
        ComponentCondition<FangsOfTheUnderworld>(id + 1u, 5.2f, static f => f.NumCasts > 0, "Stack 1");
        ComponentCondition<FangsOfTheUnderworld>(id + 2u, 2f, static f => f.NumCasts > 2, "Stack 3")
            .DeactivateOnExit<FangsOfTheUnderworld>();
    }
}
