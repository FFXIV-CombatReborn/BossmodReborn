namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class Aetherblight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(20f);
    private static readonly AOEShapeDonut donut = new(16f, 60f);
    public List<string> Hints = new(4);
    public bool Show = true;
    private bool relentlessReaping;
    private bool rotated;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || !Show)
        {
            return [];
        }
        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        (var shape, var hint) = iconID switch
        {
            (uint)IconID.AetherblightCircle1 or (uint)IconID.AetherblightCircle2 => ((AOEShape)circle, "Circle"),
            (uint)IconID.AetherblightDonut1 or (uint)IconID.AetherblightDonut2 => (donut, "Donut"),
            _ => default
        };
        if (shape != null)
        {
            Hints.Add(hint);
            if (!relentlessReaping)
            {
                AddAOE(actor.Position);
                void AddAOE(WPos pos) => _aoes.Add(new(shape, pos.Quantized(), actor.Rotation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AetherblightCircle or (uint)AID.AetherblightDonut)
        {
            ++NumCasts;
            relentlessReaping = false;
            if (_aoes.Count != 0)
            {
                Hints.RemoveAt(0);
                _aoes.RemoveAt(0);
                if (Hints.Count == 0)
                {
                    rotated = false;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SoulReaping:
                Show = false;
                break;
            case (uint)AID.Aetherblight:
                Show = true;
                if (_aoes.Count != 0)
                {
                    _aoes.Ref(0).Activation = Module.CastFinishAt(spell, 1.2d);
                }
                break;
            case (uint)AID.RelentlessReaping:
                relentlessReaping = true;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = Hints.Count;
        if (count > 0)
        {
            var sb = new StringBuilder("Stored: ", 8 + 4 * (count - 1) + count * 5);
            var ord = CollectionsMarshal.AsSpan(Hints);
            for (var i = 0; i < count; ++i)
            {
                sb.Append(ord[i]);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (!rotated && relentlessReaping && Hints.Count == 4) // if hints < 4 the player was probably in prison and data could not be collected
        {
            var index = modelState switch
            {
                21 => 0,
                147 => 1,
                65 => 2,
                22 => 3,
                _ => -1
            };

            if (index != -1)
            {
                Utils.RotateList(Hints, index);
                var rot = actor.Rotation;
                var loc = actor.Position;
                for (var i = 0; i < 4; ++i)
                {
                    switch (Hints[i])
                    {
                        case "Circle":
                            AddAOE(circle, loc, i);
                            break;
                        case "Donut":
                            AddAOE(donut, loc, i);
                            break;
                    }
                }
                rotated = true;
                void AddAOE(AOEShape shape, WPos pos, int i) => _aoes.Add(new(shape, pos.Quantized(), rot, WorldState.FutureTime(14.4d + i * 2.8d)));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Show && _aoes.Count > 1)
        {
            ref var aoe2 = ref _aoes.Ref(1);
            if (aoe2.Shape == donut)
            {
                // make ai stay close to donut to ensure successfully dodging the combo
                hints.AddForbiddenZone(new SDInvertedCircle(aoe2.Origin, 21f), _aoes.Ref(0).Activation);
            }
        }
    }
}
