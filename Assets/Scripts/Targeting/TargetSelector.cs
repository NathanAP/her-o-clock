using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;

namespace HerOClock.Targeting
{
    /// <summary>
    /// Escolhe o alvo de um personagem seguindo a ordem de prioridade de gameplay.md.
    ///
    /// A cadeia e usada de duas formas:
    /// respeitando o alcance, para decidir quem atacar;
    /// e ignorando o alcance, para decidir em direcao a quem se movimentar.
    ///
    /// Toda comparacao e feita com numeros inteiros de proposito. Sem float no meio,
    /// a escolha e sempre a mesma para o mesmo estado de tabuleiro, o que permite
    /// reproduzir um combate e, mais para frente, simular a progressao offline.
    /// </summary>
    public static class TargetSelector
    {
        public static Character Select(Character self, IReadOnlyList<Character> candidates, bool respectRange)
        {
            if (self == null || candidates == null)
            {
                return null;
            }

            // Regra 1: provocacao ignora todo o resto.
            if (self.TauntedBy != null && self.TauntedBy.IsAlive)
            {
                if (!respectRange || self.CanAttack(self.TauntedBy))
                {
                    return self.TauntedBy;
                }
            }

            Character best = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                Character candidate = candidates[i];

                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                if (respectRange && !self.CanAttack(candidate))
                {
                    continue;
                }

                if (best == null || IsBetterTarget(self, candidate, best))
                {
                    best = candidate;
                }
            }

            return best;
        }

        /// <summary>
        /// Regras 2 a 6. Cada uma so e consultada quando a anterior termina empatada.
        /// A regra 6 e posicional justamente porque ela nunca pode empatar, entao a
        /// cadeia sempre termina com um unico vencedor.
        /// </summary>
        private static bool IsBetterTarget(Character self, Character candidate, Character current)
        {
            // Regra 2: o inimigo mais proximo.
            int candidateDistance = GridPosition.Distance(self.Position, candidate.Position);
            int currentDistance = GridPosition.Distance(self.Position, current.Position);
            if (candidateDistance != currentDistance)
            {
                return candidateDistance < currentDistance;
            }

            // Regra 3: a menor porcentagem de vida atual.
            // Comparada por multiplicacao cruzada para nao precisar de divisao.
            long candidateShare = (long)candidate.CurrentHealth * current.Stats.MaxHealth;
            long currentShare = (long)current.CurrentHealth * candidate.Stats.MaxHealth;
            if (candidateShare != currentShare)
            {
                return candidateShare < currentShare;
            }

            // Regra 4: a menor vida maxima.
            if (candidate.Stats.MaxHealth != current.Stats.MaxHealth)
            {
                return candidate.Stats.MaxHealth < current.Stats.MaxHealth;
            }

            // Regra 5: a menor armadura fisica.
            if (candidate.Stats.PhysicalArmor != current.Stats.PhysicalArmor)
            {
                return candidate.Stats.PhysicalArmor < current.Stats.PhysicalArmor;
            }

            // Regra 6: a menor fileira e, em caso de empate, a menor coluna.
            if (candidate.Position.Row != current.Position.Row)
            {
                return candidate.Position.Row < current.Position.Row;
            }

            return candidate.Position.Column < current.Position.Column;
        }
    }
}
