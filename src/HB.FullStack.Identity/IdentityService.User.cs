using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    internal partial class IdentityService
    {
        public async Task RegisterUserAsync(RegisterContext context, string lastUser)
        {
            //TODO: RegisterContext的ClientVerson, ClientId, DeviceInfo, Ip 都没有使用到，需要考虑是否需要使用

            ThrowIf.NotValid(context, nameof(context));
            EnsureValidAudience(context);

            TransactionContext transContext = await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);
            User? user = null;
            try
            {
                switch (context)
                {
                    case RegisterByEmail registerByEmail:
                        //TODO: 完成EmailCode验证
                        throw new NotImplementedException();

                    case RegisterByLoginName registerByLoginName:

                        if (!_options.SignInSettings.AllowRegisterByLoginName)
                        {
                            throw IdentityExceptions.DisallowRegisterByLoginName();
                        }

                        //TODO: 安全检查，一般不建议使用LoginName和Password进行注册
                        user = await CreateUserAsync(null, null, registerByLoginName.LoginName, registerByLoginName.Password, false, false, lastUser, transContext).ConfigureAwait(false);

                        break;
                    case RegisterBySms registerBySms:
                        await EnsureValidSmsCode(registerBySms, lastUser).ConfigureAwait(false);

                        user = await CreateUserAsync(registerBySms.Mobile, null, null, null, true, false, lastUser, transContext).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }

                await transContext.CommitAsync().ConfigureAwait(false);

                //return user;
            }
            catch
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
