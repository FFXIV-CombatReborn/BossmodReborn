// CONTRIB: made by legendoficeman
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Heavensword.DeepDungeons.PalaceoftheDead.Floors51to60.D60TheBlackRider
{
    public enum OID : uint
    {
        Boss = 0x1814, // R3.920, x1
        Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type // Used to spawn the lingering puddles
        Actor1e858e = 0x1E858E, // R0.500, EventObj type, spawn during fight
        VoidsentDiscarnate = 0x18E6, // R1.000, spawn during fight
        Helper = 0x233C, // R0.500, x12, 523 type
    };
    public enum AID : uint
    {
        CleaveAuto = 7179, // Boss->player, no cast, range 8+R ?-degree cone // Need to add this
        Geirrothr = 7087, // Boss->self, no cast, range 6+R ?-degree cone
        HallOfSorrow = 7088, // Boss->location, no cast, range 9 circle
        Infaturation = 7157, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
        Valfodr = 7089, // Boss->player, 4.0s cast, width 6 rect charge
    };

    class CleaveAuto : Components.BaitAwayCast // Don't particularly like how this shows up all the time but. It'll be fine. Need to look having it dissapear if another thing is being casted by him
    {
        public CleaveAuto() : base(ActionID.MakeSpell(AID.CleaveAuto), new AOEShapeCone(9, 45.Degrees())) { }
    }

    class Infaturation : Components.SelfTargetedAOEs
    {
        public Infaturation() : base(ActionID.MakeSpell(AID.Infaturation), new AOEShapeCircle(7)) { }
    }

    class HallOfSorrow : Components.PersistentVoidzoneAtCastTarget
    {
        public HallOfSorrow() : base(9, ActionID.MakeSpell(AID.HallOfSorrow), m => m.Enemies(OID.Actor1e858e).Where(z => z.EventState != 7), 0.8f) { }
    }

    class Valfodr : Components.BaitAwayChargeCast
    {
        public Valfodr() : base(ActionID.MakeSpell(AID.Valfodr), 3) { }
    }

    class D60TheBlackRiderStates : StateMachineBuilder
    {
        public D60TheBlackRiderStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Infaturation>()
                .ActivateOnEnter<HallOfSorrow>()
                .ActivateOnEnter<Valfodr>()
                .ActivateOnEnter<CleaveAuto>()
                ;
        }
    }

    [ModuleInfo(CFCID = 204, NameID = 5309)]
    public class D60TheBlackRider : BossModule
    {
        public D60TheBlackRider(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300, -220), 25)) { }

    }
}