namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for second lightwave (3 waves, 5 crystals) + hero's glory mechanics
sealed class Lightwave2(BossModule module) : LightwaveCommon(module)
{
    private WPos _safeCrystal;
    private Vector4? _safeCrystalOrigin;

    private readonly WPos _crystalCenter = new(100f, 101f);
    private readonly WPos _crystalTL = new(90f, 92f);
    private readonly WPos _crystalTR = new(110f, 92f);
    private readonly WPos _crystalBL = new(90f, 110f);
    private readonly WPos _crystalBR = new(110f, 110f);
    private readonly AOEShapeCone _gloryAOE = new(40f, 90f.Degrees());

    public override void Update()
    {
        if (NumCasts == 4 && (Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && Module.PrimaryActor.PosRot != _safeCrystalOrigin)
        {
            _safeCrystalOrigin = Module.PrimaryActor.PosRot;
            // first crystal outside the glory cone (heh)
            _safeCrystal = default;
            ReadOnlySpan<WPos> crystals = [_crystalTL, _crystalTR, _crystalBL, _crystalBR];
            for (var i = 0; i < crystals.Length; ++i)
            {
                if (!_gloryAOE.Check(crystals[i], Module.PrimaryActor))
                {
                    _safeCrystal = crystals[i];
                    break;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && _gloryAOE.Check(actor.Position, Module.PrimaryActor))
            hints.Add("GTFO from glory aoe!");

        (var inWave, var inSafeCone) = NumCasts < 4
            ? (WaveAOE.Check(actor.Position, Wave1Pos(), default) || WaveAOE.Check(actor.Position, Wave2Pos(), default), InSafeCone(NextSideCrystal(), _crystalCenter, actor.Position))
            : (WaveAOE.Check(actor.Position, Wave3Pos(), default), _safeCrystal == default || InSafeCone(_crystalCenter, _safeCrystal, actor.Position));

        if (inWave)
            hints.Add("GTFO from wave!");
        if (!inSafeCone)
            hints.Add("Hide behind crystal!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false)
            _gloryAOE.Draw(Arena, Module.PrimaryActor);

        if (NumCasts < 4)
        {
            WaveAOE.Draw(Arena, Wave1Pos(), default);
            WaveAOE.Draw(Arena, Wave2Pos(), default);
            DrawSafeCone(NextSideCrystal(), _crystalCenter);
        }
        else
        {
            WaveAOE.Draw(Arena, Wave3Pos(), default);
            if (_safeCrystal != default)
            {
                DrawSafeCone(_crystalCenter, _safeCrystal);
            }
        }
    }

    private WPos Wave1Pos() => Waves.Count > 0 ? Waves[0].Position : new(86f, 70f);
    private WPos Wave2Pos() => Waves.Count switch
    {
        0 => new(114f, 70f),
        1 => new(Waves[0].PosRot.X < 100f ? 114f : 86f, 70f),
        _ => Waves[1].Position
    };
    private WPos Wave3Pos() => Waves.Count > 2 ? Waves[2].Position : new(100f, 70f);

    private WPos NextSideCrystal()
    {
        var w1Next = (NumCasts & 1) == 0;
        var w1Left = Wave1Pos().X < 100f;
        var nextX = w1Next == w1Left ? _crystalTL.X : _crystalBR.X;
        var nextZ = (NumCasts & 2) == 0 ? _crystalTL.Z : _crystalBR.Z;
        return new(nextX, nextZ);
    }
}
