namespace BossMod.Shadowbringers.Ultimate.TEA;

// baited flarethrowers
sealed class P3Inception2 : Components.GenericBaitAway
{
    private int _numAetheroplasmsDone, _numTetrashattersDone;
    private BitMask _taken;
    private readonly Actor? bruteJustice;

    private readonly AOEShapeCone _shape = new(100f, 45f.Degrees());

    public P3Inception2(TEA module) : base(module, onlyShowOutlines: true)
    {
        // assume first two are baited by tanks
        ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(a => a.Role != Role.Tank).Mask();
        bruteJustice = module.BruteJustice();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_numAetheroplasmsDone >= 4) // do not show anything until all aetheroplasms (part 1 of the mechanic) are done
        {
            var source = bruteJustice;
            var target = source != null ? Raid.WithoutSlot(false, true, true).ClosestTo(source.Position) : null;
            if (source != null && target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
        if (_numTetrashattersDone == 4)
        {
            OnlyShowOutlines = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Aetheroplasm:
                ++_numAetheroplasmsDone;
                break;
            case (uint)AID.Tetrashatter:
                ++_numTetrashattersDone;
                break;
            case (uint)AID.FlarethrowerP3:
                ++NumCasts;
                foreach (var t in spell.Targets)
                {
                    var slot = Raid.FindSlot(t.ID);
                    _taken.Set(slot);
                    ForbiddenPlayers.Set(slot);
                }
                if (ForbiddenPlayers.Raw == 0xffUL)
                {
                    // assume after both tanks have taken, rest is taken by raid
                    ForbiddenPlayers = _taken;
                }
                break;
        }
    }
}
