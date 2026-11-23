namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class AzureAuspice(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(6f, 40f));
sealed class NAzureAuspice(BossModule module) : AzureAuspice(module, (uint)AID.NAzureAuspice);
sealed class SAzureAuspice(BossModule module) : AzureAuspice(module, (uint)AID.SAzureAuspice);

abstract class BoundlessAzure(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 5f));
sealed class NBoundlessAzure(BossModule module) : BoundlessAzure(module, (uint)AID.NBoundlessAzureAOE);
sealed class SBoundlessAzure(BossModule module) : BoundlessAzure(module, (uint)AID.SBoundlessAzureAOE);

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 32 'rest' casts
sealed class Upwell(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class LineSequence
    {
        public WPos NextOrigin;
        public WDir Advance;
        public Angle Rotation;
        public DateTime NextActivation;
        public AOEShapeRect? NextShape; // wide for first line, null for first line mirror, narrow for remaining lines
    }

    private readonly List<LineSequence> _lines = [];

    private readonly AOEShapeRect _shapeWide = new(30f, 5f, 30f);
    private readonly AOEShapeRect _shapeNarrow = new(30f, 2.5f, 30f);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: think about imminent/future color/risk, esp for overlapping lines
        var imminentDeadline = WorldState.FutureTime(5d);
        var count = _lines.Count;
        var aoes = _aoesBuffer;
        aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var l = _lines[i];
            if (l.NextShape != null && l.NextActivation <= imminentDeadline)
                aoes.Add(new(l.NextShape, l.NextOrigin, l.Rotation, l.NextActivation));

        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NUpwellFirst or (uint)AID.SUpwellFirst)
        {
            var advance = spell.Rotation.ToDirection().OrthoR() * 5f;
            var pos = caster.Position;
            var activation = Module.CastFinishAt(spell);
            _lines.Add(new() { NextOrigin = pos, Advance = advance, Rotation = spell.Rotation, NextActivation = activation, NextShape = _shapeWide });
            _lines.Add(new() { NextOrigin = pos, Advance = -advance, Rotation = (spell.Rotation + 180f.Degrees()).Normalized(), NextActivation = activation });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NUpwellFirst or (uint)AID.SUpwellFirst)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1f) && l.NextShape == _shapeWide && l.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index < 0 || index + 1 >= _lines.Count)
            {
                ReportError($"Unexpected exaline end");
            }
            else
            {
                Advance(_lines[index]);
                Advance(_lines[index + 1]);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NUpwellRest or (uint)AID.SUpwellRest)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1f) && l.NextShape == _shapeNarrow && l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (index < 0)
            {
                ReportError($"Unexpected exaline @ {caster.Position} / {caster.Rotation}");
            }
            else
            {
                Advance(_lines[index]);
            }
        }
    }

    private void Advance(LineSequence line)
    {
        line.NextOrigin += line.Advance;
        line.NextActivation = WorldState.FutureTime(2d);
        var offset = (line.NextOrigin - Arena.Center).Abs();
        line.NextShape = offset.X < 19f && offset.Z < 19f ? _shapeNarrow : null;
    }
}
