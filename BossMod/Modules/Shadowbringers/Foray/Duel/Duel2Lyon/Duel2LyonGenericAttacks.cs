namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

sealed class Enaero(BossModule module) : Components.Dispel(module, (uint)SID.Enaero, (uint)AID.RagingWinds1);

sealed class HeartOfNature(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NaturesPulse1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.NaturesPulse1 => 0,
                (uint)AID.NaturesPulse2 => 1,
                (uint)AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class TasteOfBlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TasteOfBlood, new AOEShapeCone(40f, 90f.Degrees()));
sealed class TasteOfBloodHint(BossModule module) : Components.CastHint(module, (uint)AID.TasteOfBlood, "Go behind Lyon!");

sealed class RavenousGale(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(1.5f);
    private readonly List<AOEInstance> _aoes = [];
    private bool casting;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count + (casting ? 1 : 0);
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        if (casting)
            aoes[index++] = new AOEInstance(circle, actor.Position, default);

        for (var i = 0; i < _aoes.Count; ++i)
            aoes[index++] = _aoes[i];

        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.RavenousGaleVoidzone)
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(4.6d)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.RavenousGaleVoidzone)
            _aoes.RemoveAt(0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RavenousGale)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RavenousGale)
            casting = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        base.AddGlobalHints(hints);
        if (casting)
            hints.Add("Move a little to avoid voidzone spawning under you");
    }
}

sealed class TwinAgonies(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TwinAgonies, "Use Manawall or tank mitigations");
sealed class WindsPeak(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindsPeak1, 5f);

sealed class WindsPeakKB(BossModule module) : Components.GenericKnockback(module)
{
    private DateTime Time;
    private bool watched;
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (watched && WorldState.CurrentTime < Time.AddSeconds(4.4d))
            return new Knockback[1] { new(Module.PrimaryActor.Position, 15f, _activation) };
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindsPeak1)
        {
            watched = true;
            Time = WorldState.CurrentTime;
            _activation = Module.CastFinishAt(spell);
        }
    }
}

sealed class TheKingsNotice(BossModule module) : Components.CastGaze(module, (uint)AID.TheKingsNotice);
sealed class SplittingRage(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.SplittingRage);

sealed class NaturesBlood(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NaturesBlood1)
        {
            Lines.Add(new(caster.Position, 6f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.1d, 7, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NaturesBlood1 or (uint)AID.NaturesBlood2)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class SpitefulFlameCircleVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VermillionFlame)
            _aoes.Add(new(circle, actor.Position.Quantized()));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SpitefulFlame1)
        {
            if (++NumCasts == 12)
            {
                NumCasts = 0;
                _aoes.Clear();
            }
        }
    }
}

sealed class SpitefulFlameRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpitefulFlame2, new AOEShapeRect(80f, 2f));

sealed class DynasticFlame(BossModule module) : Components.BaitAwayTethers(module, 10f, (uint)TetherID.fireorbs)
{
    private int orbcount;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 18f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
            hints.Add("Go to the edge and run until 4 orbs are spawned");
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VermillionFlame)
        {
            if (++orbcount == 4)
            {
                CurrentBaits.Clear();
                orbcount = 0;
            }
        }
    }
}

sealed class SkyrendingStrike(BossModule module) : Components.CastHint(module, (uint)AID.SkyrendingStrike, "Enrage!", true);
