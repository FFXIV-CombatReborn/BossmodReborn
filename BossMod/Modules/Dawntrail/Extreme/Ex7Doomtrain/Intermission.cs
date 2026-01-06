namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class Intermission(BossModule module) : BossComponent(module)
{
    public bool Started;
    public bool Active;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        // This *seems* to be how we detect intermission starts, it does a bit of setup before it triggers this.
        if (updateID == 0x8000000C && param1 == 0x59 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0)
        {
            Started = true;
        }
    }
}

sealed class AetherocharAethersoteStackSpread(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 3, 3)
{
    public bool Stack;
    public DateTime Activation = DateTime.MaxValue;
    public void Show(DateTime activation)
    {
        Activation = activation;
        ExtraAISpreadThreshold = 0;
        switch (Stack)
        {
            case true:
                // Stack always targets healers
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Role == Role.Healer), activation);
                break;
            case false:
                AddSpreads(Raid.WithoutSlot(true, true, true).Where(p => p.Role != Role.Tank), activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Stack)
            hints.Add("Stack!");
        else
            hints.Add("Spread!");
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID == OID.Doomtrain1)
        {
            if (iconID is (uint)IconID._Gen_Icon_m0969_mrt_sht_c0k2 or (uint)IconID._Gen_Icon_m0969_mrt_sht_c1k2)
            {
                Stack = iconID == (uint)IconID._Gen_Icon_m0969_mrt_sht_c0k2;
                Active = true;
                Show(WorldState.FutureTime(6.5d));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == OID.Doomtrain1)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Aetherosote:
                    Stacks.Clear();
                    Active = false;
                    break;
                case (uint)AID.Aetherochar:
                    Spreads.Clear();
                    Active = false;
                    break;
            }
        }
    }
}
