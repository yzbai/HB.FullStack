﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Base;
using HB.FullStack.Common.Files;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Context;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

//TODO: 这里的ConfigureAWait需不需要去掉
namespace HB.FullStack.Client.Components.Users
{
    internal class UserProfileService : IUserProfileService
    {
        private readonly ClientOptions _options;
        private readonly IApiClient _apiClient;
        private readonly ITransaction _transaction;
        private readonly IFileManager _fileManager;
        private readonly ITokenPreferences _tokenPreferences;
        private readonly UserProfileRepo _userProfileRepo;

        public UserProfileService(
            IOptions<ClientOptions> options,
            IApiClient apiClient,
            ITransaction transaction,
            IFileManager fileManager,
            ITokenPreferences tokenPreferences,
            UserProfileRepo userProfileRepo)
        {
            _options = options.Value;
            _apiClient = apiClient;
            _transaction = transaction;
            _fileManager = fileManager;
            _tokenPreferences = tokenPreferences;
            _userProfileRepo = userProfileRepo;
        }

        public async Task<string?> GetNickNameAsync(GetSetMode getMode)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(_tokenPreferences.UserId!.Value, null, getMode).ConfigureAwait(false);

            return userProfile?.NickName;
        }

        public async Task<UserProfile?> UpdateUserProfileAsync(string? nickName, Gender? gender, DateOnly? birthDay, string? tmpAvatarFileFullPath)
        {
            string? newAvatarFileName = null;

            if (tmpAvatarFileFullPath.IsNotNullOrEmpty())
            {
                newAvatarFileName = FileUtil.GetRandomFileName(Path.GetExtension(tmpAvatarFileFullPath));
                _ = await _fileManager.SetAsync(tmpAvatarFileFullPath, _options.AvatarDirectory, newAvatarFileName).ConfigureAwait(false);
            }

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserProfile>().ConfigureAwait(false);

            try
            {
                Guid userId = _tokenPreferences.UserId!.Value;
                UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

                if (userProfile == null)
                {
                    userProfile = new UserProfile
                    {
                        Id = userId,
                        Level = null,
                        NickName = Conventions.GetRandomNickName(),
                        Gender = gender,
                        BirthDay = birthDay,
                        AvatarFileName = newAvatarFileName
                    };

                    await _userProfileRepo.AddAsync(new UserProfile[] { userProfile }, trans).ConfigureAwait(false);

                    return userProfile;
                }

                if (nickName.IsNotNullOrEmpty()) userProfile.NickName = nickName;
                if (gender != null) userProfile.Gender = gender;
                if (birthDay != null) userProfile.BirthDay = birthDay;
                if (newAvatarFileName.IsNotNullOrEmpty()) userProfile.AvatarFileName = newAvatarFileName;

                await _userProfileRepo.UpdateAsync(new UserProfile[] { userProfile }, trans).ConfigureAwait(false);

                await _transaction.CommitAsync(trans).ConfigureAwait(false);

                return userProfile;
            }
            catch
            {
                await _transaction.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<(Directory2, string?)> GetAvatarFileAsync(GetSetMode getMode = GetSetMode.Mixed)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(_tokenPreferences.UserId!.Value, null, getMode).ConfigureAwait(false);

            string? fileName = userProfile?.AvatarFileName;

            return (_options.AvatarDirectory, fileName);

            //return await _fileManager.GetAsync(ApiClientOptions.AvatarDirectory, fileName).ConfigureAwait(false);
        }

        public async Task<string?> SaveAvatarFileAsync(string avatarFullPath, GetSetMode setMode = GetSetMode.Mixed)
        {
            //TODO: Security Check File Extension

            string newFileName = FileUtil.GetRandomFileName(Path.GetExtension(avatarFullPath));
            string localFullPath = await _fileManager.SetAsync(avatarFullPath, _options.AvatarDirectory, newFileName).ConfigureAwait(false);

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserProfile>().ConfigureAwait(false);

            try
            {
                UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(_tokenPreferences.UserId!.Value, trans, setMode).ConfigureAwait(false);

                if (userProfile == null)
                {
                    return null;
                }

                userProfile.AvatarFileName = newFileName;

                await _userProfileRepo.UpdateAsync(new UserProfile[] { userProfile }, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);

                return localFullPath;
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}