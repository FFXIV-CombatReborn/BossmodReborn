namespace BossMod.Global.MaskedCarnivale.Stage14.Act1;

public enum OID : uint
{
    Boss = 0x271D //R=2.0
}

public enum AID : uint
{
    TheLastSong = 14756 // Boss->self, 6.0s cast, range 60 circle
}

sealed class LastSong(BossModule module) : Components.CastLineOfSightAOEComplex(module, (uint)AID.TheLastSong, Layouts.Layout2CornersBlockers);

sealed class LastSongHint(BossModule module) : BossComponent(module)
{
    public bool Casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheLastSong)
        {
            Casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheLastSong)
        {
            Casting = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casting)
        {
            hints.Add("Take cover behind a barricade!");
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("These slimes start casting Final Song after death.\nWhile Final Song is not deadly, it does heavy damage and applies silence\nto you. Take cover! For act 2 the spell Loom is strongly recommended.\nThe slimes are strong against blunt melee damage such as J Kick.");
    }
}

sealed class Stage14Act1States : StateMachineBuilder
{
    public Stage14Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<LastSong>()
            .ActivateOnEnter<LastSongHint>()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss) && !module.FindComponent<LastSongHint>()!.Casting;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 624, NameID = 8108, SortOrder = 1)]
public sealed class Stage14Act1 : BossModule
{
    public Stage14Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout2Corners)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);
}
