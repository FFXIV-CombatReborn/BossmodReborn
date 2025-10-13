namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class Aetherblight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private readonly AOEShapeRect rect = new(100f, 6f);
    private readonly AOEShapeCircle circle = new(20f);
    private readonly AOEShapeDonut donut = new(16f, 60f);
    public List<string> Hints = new(4);
    public bool Show = true;
    private bool relentlessReaping;
    private bool rotated;
    private bool rectSidesRemoved;
    private int index;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Show ? CollectionsMarshal.AsSpan(_aoes)[..index] : [];

    public override void Update()
    {
        var count = _aoes.Count;
        if (!Show || count == 0)
        {
            index = 0;
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        var shape = aoe0.Shape;

        if (count > 2)
        {
            ref var aoe1 = ref aoes[1];
            ref var aoe2 = ref aoes[2];
            if (shape == rect)
            {
                if (aoe1.Shape == rect)
                {
                    if (aoe2.Shape == rect)
                    {
                        if (aoe0.Activation == aoe1.Activation)
                        {
                            index = 2;
                            goto end;
                        }
                        index = 1;
                        goto end;
                    }
                    index = 3;
                    goto end;
                }
                index = 2;
                goto end;
            }
            if ((shape == circle || shape == donut) && aoe1.Shape == rect)
            {
                if (aoe2.Shape == rect)
                {
                    if (count > 3)
                    {
                        ref var aoe3 = ref aoes[3];
                        if (aoe3.Activation == aoe2.Activation)
                        {
                            index = 2;
                            goto end;
                        }
                    }
                    index = 3;
                    goto end;
                }
                index = 2;
                goto end;
            }
            index = 1;
            goto end;
        }
        else if (count == 2)
        {
            ref var aoe1 = ref aoes[1];
            var aoe1shape = aoe1.Shape;
            if ((shape == circle || shape == donut) && aoe1shape == rect || (aoe1shape == rect || aoe1shape == circle || aoe1shape == donut) && shape == rect)
            {
                index = 2;
                goto end;
            }
            index = 1;
            goto end;
        }
        index = count;
    end:
        if (index > 1)
        {
            var color = Colors.Danger;
            ref var aoe1 = ref aoes[1];
            var compare = aoe0.Activation != aoe1.Activation;
            if (compare || index > 2)
            {
                aoe0.Color = color;
            }
            if (!compare && index > 2)
            {
                aoe1.Color = color;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        (var shape, var hint) = iconID switch
        {
            (uint)IconID.AetherblightCircle => ((AOEShape)circle, "Out"),
            (uint)IconID.AetherblightDonut => (donut, "In"),
            (uint)IconID.AetherblightRectSingle => (rect, "Sides"),
            (uint)IconID.AetherblightRectDouble => (rect, "Middle"),
            _ => default
        };
        if (shape != null)
        {
            Hints.Add(hint);
            if (!relentlessReaping)
            {
                if (iconID != (uint)IconID.AetherblightRectDouble)
                {
                    AddAOE(actor.Position);
                }
                else
                {
                    AddAOE(new(88f, 85f));
                    AddAOE(new(112f, 85f));
                }
                void AddAOE(WPos pos) => _aoes.Add(new(shape, pos.Quantized(), actor.Rotation, WorldState.FutureTime(12.4d)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.AetherblightRect1 or (uint)AID.AetherblightRect2 or (uint)AID.AetherblightCircle or (uint)AID.AetherblightDonut)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
            var isSides = id == (uint)AID.AetherblightRect1;
            if ((!rectSidesRemoved && isSides || !isSides) && Hints.Count != 0)
            {
                Hints.RemoveAt(0);
                if (isSides)
                {
                    rectSidesRemoved = true;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RelentlessReaping)
        {
            relentlessReaping = true;
        }
    }

    public void UpdateAOEs(double delay)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        var act = WorldState.FutureTime(delay);
        for (var i = 0; i < len; ++i)
        {
            aoes[i].Activation = act;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = Hints.Count;
        if (count > 0)
        {
            var sb = new StringBuilder("Order: ", 7 + 4 * (count - 1) + count * 5);
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
                        case "Middle":
                            AddAOE(rect, new(88f, 85f), i);
                            AddAOE(rect, new(112f, 85f), i);
                            break;
                        case "Out":
                            AddAOE(circle, loc, i);
                            break;
                        case "In":
                            AddAOE(donut, loc, i);
                            break;
                        case "Sides":
                            AddAOE(rect, loc, i);
                            break;
                    }
                }
                rotated = true;
                void AddAOE(AOEShape shape, WPos pos, int i) => _aoes.Add(new(shape, pos.Quantized(), rot, WorldState.FutureTime(12.4d + i * 2.8d)));
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
