using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Client.MobileApp.Services
{
    internal interface IUserDomainService
    {
        Task<bool> NeedRegisterProfileAsync();
    }
}
