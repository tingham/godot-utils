using System;

namespace URBANFORT.Utilities.Interfaces
{
    public interface IUsable
    {
        void Use(object user);
        bool CanUse(object user);
    }
}