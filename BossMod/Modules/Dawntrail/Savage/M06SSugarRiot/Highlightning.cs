namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Highlightning(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(21f);
    public AOEInstance[] AOE = [];
    private WPos lastPosition;
    private bool active;
    private DateTime nextActivation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TempestPiece)
        {
            active = true;
            AOE = [new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(6.7d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Highlightning)
        {
            AOE = [];
            if (++NumCasts == 5)
            {
                active = false;
            }
            nextActivation = WorldState.FutureTime(10.6d);
            lastPosition = caster.Position;
        }
    }

    public override void Update()
    {
        if (!active || AOE.Length != 0)
        {
            return;
        }
        var tempests = Module.Enemies((uint)OID.TempestPiece);
        if (tempests.Count == 0)
        {
            return;
        }
        var tempest = tempests[0];
        var angle = (int)Angle.FromDirection(tempest.Position - lastPosition).Deg;
        if (angle == 0)
        {
            return; // cloud didn't start moving yet
        }

        WPos next = angle switch
        {
            -149 or -150 or -90 => new(87f, 92f),
            90 or 146 or 147 => new(115f, 92f),
            >= -35 and <= -32 or 28 or 29 => new(100f, 115f),
            _ => default
        };
        if (next != default)
        {
            AOE = [new(circle, next.Quantized(), default, nextActivation)];
        }

        Module.FindComponent<LightningStormHint>()?.UpdateAOE();
    }
}
