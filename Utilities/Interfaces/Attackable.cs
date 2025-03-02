using Godot;
using System;
using URBANFORT.Utilities.Data;

namespace URBANFORT.Utilities.Interfaces
{
    public interface IAttackable
    {
        Godot.Collections.Array<string> AttackableBy { get; set; }

        bool CanAttack(object attacker);

        void Attack(object attacker, Damage damage);
    }
}