namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class Crystallize : BossComponent
{
    public enum Element { None, Water, Earth, Ice }
    public Element CurElement;

    private const float _waterRadius = 6f;
    private const float _earthRadius = 6f;
    private const float _iceRadius = 5f;

    public Crystallize(BossModule module) : base(module)
    {
        CurElement = (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) switch
        {
            (uint)AID.CrystallizeSwordStaffWater or (uint)AID.CrystallizeChakramWater => Element.Water,
            (uint)AID.CrystallizeStaffEarth or (uint)AID.CrystallizeChakramEarth => Element.Earth,
            (uint)AID.CrystallizeStaffIce or (uint)AID.CrystallizeChakramIce => Element.Ice,
            _ => Element.None
        };
        if (CurElement == Element.None)
            ReportError($"Unexpected boss cast {Module.PrimaryActor.CastInfo?.Action.ID ?? 0u}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var party = Raid.WithoutSlot(false, true, true);
        var pos = actor.Position;
        switch (CurElement)
        {
            case Element.Water:
                var healersInRange = 0;
                for (var i = 0; i < party.Length; ++i)
                {
                    var p = party[i];
                    if (p.Role == Role.Healer && p.Position.InCircle(pos, _waterRadius))
                        ++healersInRange;
                }
                if (healersInRange > 1)
                    hints.Add("Hit by two aoes!");
                else if (healersInRange == 0)
                    hints.Add("Stack with healer!");
                break;
            case Element.Earth:
                for (var i = 0; i < party.Length; ++i)
                {
                    if (!party[i].Position.InCircle(pos, _earthRadius))
                    {
                        hints.Add("Stack!");
                        break;
                    }
                }
                break;
            case Element.Ice:
                for (var i = 0; i < party.Length; ++i)
                {
                    var p = party[i];
                    if (p != actor && p.Position.InCircle(pos, _iceRadius))
                    {
                        hints.Add("Spread!");
                        break;
                    }
                }
                break;
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var hint = CurElement switch
        {
            Element.Water => "Stack in fours",
            Element.Earth => "Stack all",
            Element.Ice => "Spread",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        switch (CurElement)
        {
            case Element.Water:
                foreach (var player in Raid.WithoutSlot(false, true, true))
                {
                    if (player.Role == Role.Healer)
                    {
                        Arena.Actor(player, Colors.Danger, drawWorld: true);
                        Arena.ZoneCircleOutline(player.Position, _waterRadius, Colors.Safe);
                    }
                    else
                    {
                        Arena.Actor(player, Colors.PlayerGeneric);
                    }
                }
                break;
            case Element.Earth:
                Arena.ZoneCircleOutline(pc.Position, _earthRadius, Colors.Safe);
                foreach (var player in Raid.WithoutSlot(false, true, true))
                {
                    if (player == pc)
                    {
                        continue;
                    }
                    var inaoe = player.Position.InCircle(pc.Position, _earthRadius);
                    Arena.Actor(player, inaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: inaoe ? true : null);
                }
                break;
            case Element.Ice:
                Arena.ZoneCircleOutline(pc.Position, _iceRadius, Colors.Danger);
                foreach (var player in Raid.WithoutSlot(false, true, true))
                {
                    if (player == pc)
                    {
                        continue;
                    }
                    var inaoe = player.Position.InCircle(pc.Position, _iceRadius);
                    Arena.Actor(player, inaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: inaoe ? true : null);
                }
                break;
        }
    }

    // note: this is pure validation, we currently rely on crystallize cast id to determine element...
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor != Module.PrimaryActor || status.ID != (uint)SID.CrystallizeElement)
            return;

        var element = status.Extra switch
        {
            0x151 => Element.Water,
            0x152 => Element.Earth,
            0x153 => Element.Ice,
            _ => Element.None
        };
        if (element == Element.None || element != CurElement)
            ReportError($"Unexpected extra of element buff: {status.Extra:X4}, cur element {CurElement}");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurElement == Element.None)
            return;

        var element = spell.Action.ID switch
        {
            (uint)AID.CrystallineWater => Element.Water,
            (uint)AID.CrystallineStone => Element.Earth,
            (uint)AID.CrystallineBlizzard => Element.Ice,
            _ => Element.None
        };

        if (element == Element.None)
            return;

        if (element != CurElement)
            ReportError($"Unexpected element cast: got {spell.Action}, expected {CurElement}");
        CurElement = Element.None;
    }
}
