namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class MultiMissileSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MultiMissileSmall, 6f, riskyWithSecondsLeft: 1.5d);
sealed class MultiMissileBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MultiMissileBig, 10f, riskyWithSecondsLeft: 2.5d);

sealed class CitadelSiege(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect rect = new(48f, 5f);
    private ArenaBoundsRect currentArena1Bounds;
    private WPos currentArena1Center;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CitadelSiege)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x19:
                switch (state)
                {
                    case 0x00020001u:
                        GenerateArenaBounds(0);
                        break;
                    case 0x00080004u:
                        Arena.Bounds = new ArenaBoundsRect(20f, 23.5f);
                        Arena.Center = A22UltimaOmega.ArenaCenter2;
                        break;
                }
                break;
            case 0x18:
                switch (state)
                {
                    case 0x00020001u:
                        GenerateArenaBounds(1);
                        break;
                    case 0x00200010u:
                        GenerateArenaBounds(2);
                        break;
                    case 0x00800040u:
                        GenerateArenaBounds(3);
                        break;
                    case 0x02000100u: // last part of the arena starts burning, ship explodes shortly after
                        _aoes.Clear();
                        break;
                }
                break;
        }
        void GenerateArenaBounds(int stage)
        {
            Arena.Bounds = currentArena1Bounds = new ArenaBoundsRect(20f - 5f * stage, 23.5f);
            Arena.Center = currentArena1Center = A22UltimaOmega.ArenaCenter1 - new WDir(5f * stage, default);
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (currentArena1Bounds == default)
        {
            return;
        }
        var posX = pc.PosRot.X;
        var curBounds = Arena.Bounds == currentArena1Bounds;
        if (posX > 780f && !curBounds)
        {
            Arena.Bounds = currentArena1Bounds;
            Arena.Center = currentArena1Center;
        }
        else if (posX < 780f && curBounds)
        {
            Arena.Bounds = new ArenaBoundsRect(20f, 23.5f);
            Arena.Center = A22UltimaOmega.ArenaCenter2;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

sealed class CitadelSiegeHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _hint = [];
    private readonly AOEShapeRect rect = new(47f, 2f);
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => active && actor.PosRot.X > 780f ? _hint : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x19)
        {
            switch (state)
            {
                case 0x00020001u:
                    _hint = [new(rect, new(782f, 776.5f), default, WorldState.FutureTime(20.5d), Colors.SafeFromAOE)];
                    active = true;
                    break;
                case 0x00080004u:
                    _hint = [];
                    active = false;
                    break;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && actor.PosRot.X > 780f)
        {
            hints.Add("Go towards edge to escape platform!", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active && actor.PosRot.X > 780f)
        {
            hints.GoalZones.Add(AIHints.GoalRectangle(new(782f, 800f), new(default, 1f), 2f, 23.5f, 99f));
        }
    }
}
