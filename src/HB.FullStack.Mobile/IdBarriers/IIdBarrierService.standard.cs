using System;
namespace HB.FullStack.Mobile.IdBarriers
{
    public interface IIdBarrierService
    {
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="MobileException"></exception>
        void Initialize();
    }
}