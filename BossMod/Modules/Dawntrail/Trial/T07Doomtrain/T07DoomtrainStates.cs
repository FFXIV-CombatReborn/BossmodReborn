namespace BossMod.Dawntrail.Trial.T07Doomtrain;

sealed class T07DoomtrainStates : StateMachineBuilder
{
    public T07DoomtrainStates(T07Doomtrain module) : base(module)
    {
        SimplePhase(0u, P1, "P1")
            .ActivateOnEnter<CarGeometry>()
            .Raw.Update = () => module.AetherIntermission?.IsDead ?? false;
        DeathPhase(1u, P2);
    }

    void P1(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000u, 40000);
        Car3(id + 0x20000u, 50000f);
        Car4_1(id + 0x30000u, 60000f);
        Intermission(id + 0x40000u, 70000);
    }

    void P2(uint id)
    {
        ComponentCondition<RunawayTrain>(id, 4.3f, static comp => comp.WatchedAction != default)
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<RunawayTrain>(id + 1u, 15.2f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<RunawayTrain>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Car4_2(id + 0x50000u, 80000f);
        Car5(id + 0x60000u, 90000f);
    }

    void Car1(uint id, float delay)
    {
        CastStart(id + 0x100u, AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<LightningBurstTankBuster>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<WindpipeDrawIn>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110u, false, 0.2f, "Car 2 start");

    }

    void Car2(uint id, float delay)
    {
        Timeout(id, 0f).ExecOnEnter<CarGeometry>(static comp => ++comp.Car)
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<WindpipeDrawIn>();
        CastStart(id + 0x100u, AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110u, false, 0.2f, "Car 3 start");
    }

    void Car3(uint id, float delay)
    {
        Timeout(id, 0f).ExecOnEnter<CarGeometry>(static comp => ++comp.Car)
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<HeadOnEmission>();

        CastStart(id + 0x100u, AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110u, false, 0.2f, "Car 4 start");
    }

    void Car4_1(uint id, float delay)
    {
        Timeout(id, 0f).ExecOnEnter<CarGeometry>(static comp => ++comp.Car)
            .ActivateOnEnter<HeadOnEmission>();
        CastStart(id + 0x100u, AID.RunawayTrain, 1.9f)
            .ActivateOnEnter<RunawayTrain>();
        ComponentCondition<RunawayTrain>(id + 0x101, 5.9f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<RunawayTrain>();
        Targetable(id + 0x110u, false, 0.2f, "Intermission start");
    }

    // move to circle arena with ghost train
    void Intermission(uint id, float delay)
    {
        Timeout(id, delay, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .OnEnter(() =>
            {
                // Added the outer donut for railroad tracks and to improve how
                // aetherial ray cone aoe presents on the radar.
                var center = new WPos(-400, -400);
                Module.Arena.Center = center;
                Shape[] intermissionCircles = [new Circle(center, 14.5f), new Donut(center, 22.4f, 27.5f)];
                ArenaBoundsCustom intermissionArena = new(intermissionCircles);
                Module.Arena.Bounds = intermissionArena;
            })
            .ActivateOnEnter<AetherSurge>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RunawayTrainRaidwide>()
            .ActivateOnEnter<RunawayTrain>();

        CastStart(id + 0x100, AID.RunawayTrain, 1.9f)
            .ActivateOnEnter<RunawayTrain>();
        ComponentCondition<RunawayTrain>(id + 0x101, 5.9f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<RunawayTrain>();
        Targetable(id + 0x110, false, 0.2f, "Return to car 4");
    }

    // Return to car 4 after intermission with ghost train
    void Car4_2(uint id, float delay)
    {
        //Reset the arena back to car 4
        Timeout(id, 0).ExecOnEnter<CarGeometry>(static comp => comp.Car = 4)
            .ActivateOnEnter<ArenaChangeCar4>()
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<ArcaneRevelation>()
            .ActivateOnEnter<DerailmentSiegeCircle>()
            .ActivateOnEnter<Derail>();

        Targetable(id + 0x260, false, 0.2f, "Moves to car 5");
    }

    void Car5(uint id, float delay)
    {
        Targetable(id + 0x110, true, 0.2f, "On car 5")
            .ActivateOnEnter<HeadOnEmission>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayUpper>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<Electray3>()
            .ActivateOnEnter<WindpipeDrawIn>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<LightningBurstTankBuster>();
    }
}
