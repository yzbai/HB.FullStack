/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Client.Base;
using HB.FullStack.Common.Files;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

using MyColorfulTime.Client.Models;

//TODO: 这里的ConfigureAWait需不需要去掉
namespace HB.FullStack.Client.Components.User
{
    internal class UserDomainService : IUserService
    {
        private readonly MyColorfulTimeOptions _options;
        private readonly ITransaction _transaction;
        private readonly IFileManager _fileManager;
        private readonly IPreferenceProvider _preferenceProvider;
        private readonly UserTokenRepo _userTokenRepo;
        private readonly UserProfileRepo _userProfileRepo;

        public UserDomainService(
            IOptions<MyColorfulTimeOptions> options,
            ITransaction transaction,
            IFileManager fileManager,
            IPreferenceProvider preferenceProvider,
            UserTokenRepo userTokenRepo,
            UserProfileRepo userProfileRepo)
        {
            _options = options.Value;
            _transaction = transaction;
            _fileManager = fileManager;
            _preferenceProvider = preferenceProvider;
            _userTokenRepo = userTokenRepo;
            _userProfileRepo = userProfileRepo;
        }

        public async Task LoginBySmsCodeAsync(string mobile, string smsCode, DeviceInfos deviceInfos)
        {
            UserTokenRes tokenRes = await _userTokenRepo.GetBySmsCodeAsync(mobile, smsCode, _options.SignToWhere, deviceInfos).ConfigureAwait(false);

            _preferenceProvider.Login(
                userId: tokenRes.UserId,
                userCreateTime: tokenRes.CreatedTime,
                mobile: tokenRes.Mobile,
                email: tokenRes.Email,
                loginName: tokenRes.LoginName,
                accessToken: tokenRes.AccessToken,
                refreshToken: tokenRes.RefreshToken);
        }

        public Task<bool> NeedRegisterProfileAsync() => IsNickNameExistedAsync();

        public async Task<bool> IsNickNameExistedAsync()
        {
            EnsureLogined();

            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(_preferenceProvider.UserId!.Value, null).ConfigureAwait(false);

            return
                userProfile != null &&
                userProfile.NickName.IsNotNullOrEmpty() &&
                !CommonConventions.IsARandomNickName(userProfile.NickName);
        }

        public async Task<string?> GetNickNameAsync(Guid userId, RepoGetMode getMode)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(userId, null, getMode).ConfigureAwait(false);

            return userProfile?.NickName;
        }

        private void EnsureLogined()
        {
            if (!_preferenceProvider.IsLogined())
            {
                throw ClientExceptions.NotLogined();
            }
        }

        public async Task UpdateUserProfileAsync(string? nickName, Gender? gender, DateTimeOffset? birthDay, string? avatarFileName)
        {
            TransactionContext trans = await _transaction.BeginTransactionAsync<UserProfile>().ConfigureAwait(false);

            try
            {
                await _userProfileRepo.SetAsync(nickName, gender, birthDay, avatarFileName, trans).ConfigureAwait(false);

                await _transaction.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        #region Avatar

        public string DefaultAvatarFileName
        {
            get
            {
                int randomIndex = SecurityUtil.GetRandomInteger(0, _options.DefaultAvatarFileNames.Count - 1);
                return _options.DefaultAvatarFileNames[randomIndex];
            }
        }

        public async Task<string?> GetAvatarFileNameAsync(Guid userId, RepoGetMode getMode)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(userId, null, getMode).ConfigureAwait(false);

            return userProfile?.AvatarFileName;
        }

        ///<summary>
        ///返回本地FullPath
        ///</summary>
        public Task<string> SetAvatarFileAsync(string avatarFullPath)
        {
            //TODO: 修改UserProfile.AvatarFileName
            return _fileManager.SetFileToMixedAsync(avatarFullPath, DirectorySettings.Descriptions.PUBLIC_AVATAR.ToDirectory(null), Path.GetFileName(avatarFullPath));
        }

        #endregion
    }
}