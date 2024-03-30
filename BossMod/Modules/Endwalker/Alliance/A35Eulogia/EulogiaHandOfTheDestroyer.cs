﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A35Eulogia
{
    class HandOfTheDestroyer : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(90, 20);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
            {
                _aoes.Clear();
                ++NumCasts;
            }
        }
    }
}
