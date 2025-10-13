namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class ProwlingGaleLast(BossModule module) : Components.GenericTowers(module, (uint)AID.ProwlingGaleLast1)
{
    private readonly LamentOfTheCloseDistant _tethers = module.FindComponent<LamentOfTheCloseDistant>()!;
    private readonly M08SHowlingBladeConfig _config = Service.Config.Get<M08SHowlingBladeConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var soakers = spell.Action.ID switch
        {
            (uint)AID.ProwlingGaleLast1 => 1,
            (uint)AID.ProwlingGaleLast2 => 2,
            (uint)AID.ProwlingGaleLast3 => 3,
            _ => default
        };
        if (soakers != default)
        {
            BitMask allowed = default;
            if (_config.LoneWolfsLamentHints)
            {
                var party = Raid.WithSlot(true, true, true);
                var len = party.Length;
                var pos = caster.PosRot;
                for (var i = 0; i < len; ++i)
                {
                    ref var p = ref party[i];
                    var slot = p.Item1;
                    ref var tether = ref _tethers.Partners[slot];
                    var partnerRole = tether.Item1.Role;
                    var playerRole = p.Item2.Role;
                    switch (soakers)
                    {
                        case 2:
                            allowed[slot] = (playerRole == Role.Tank || partnerRole == Role.Tank) && tether.close;
                            break;
                        case 3:
                            allowed[slot] = playerRole == Role.Healer || partnerRole == Role.Healer && tether.close;
                            break;
                        case 1:
                            allowed[slot] = pos.X > 100f ? partnerRole == Role.Tank && !tether.close
                                : pos.Z < 100f ? partnerRole == Role.Healer && !tether.close : playerRole == Role.Tank && !tether.close;
                            break;

                    }
                }
            }
            Towers.Add(new(spell.LocXZ, 2f, soakers, soakers, allowed == default ? allowed : ~allowed, Module.CastFinishAt(spell)));
        }
    }
}

sealed class LamentOfTheCloseDistant(BossModule module) : BossComponent(module)
{
    private readonly M08SHowlingBladeConfig _config = Service.Config.Get<M08SHowlingBladeConfig>();
    public readonly (Actor, bool close)[] Partners = new (Actor, bool)[PartyState.MaxPartySize];
    public bool TethersAssigned;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Partners[slot] != default)
        {
            hints.Add(Partners[slot].close ? "Stay close to partner!" : "Stay away from partner!");
        }
        else if (_config.LoneWolfsLamentHints && !TethersAssigned)
        {
            switch (actor.Role)
            {
                case Role.Tank:
                    hints.Add("Preposition on platform 3 (NW)!");
                    break;
                case Role.Healer:
                    hints.Add("Preposition on platform 1 (S)!");
                    break;
                default:
                    hints.Add("Preposition on platform 5 (E)!");
                    break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Partners[pcSlot].Item1 == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.GreenChains or (uint)TetherID.BlueChains)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            var isClose = tether.ID == (uint)TetherID.GreenChains;
            if (target != null)
            {
                SetPartner(source.InstanceID, (target, isClose));
                SetPartner(target.InstanceID, (source, isClose));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Partners[pcSlot].Item1 is var partner && partner != default)
        {
            Arena.AddLine(pc.Position, partner.Position);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.GreenChains or (uint)TetherID.BlueChains)
        {
            SetPartner(source.InstanceID, default);
            SetPartner(tether.Target, default);
        }
    }

    private void SetPartner(ulong source, (Actor, bool) target)
    {
        var slot = Raid.FindSlot(source);
        if (slot >= 0)
        {
            Partners[slot] = target;
        }
    }
}
