using System;
using URBANFORT.Utilities.Data;

namespace URBANFORT.Utilities.Interfaces
{
    public interface IAttackable
    {
        bool CanAttack(object attacker);

        void Attack(object attacker, Damage damage);
    }
}