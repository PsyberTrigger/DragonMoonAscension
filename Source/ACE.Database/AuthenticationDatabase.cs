using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;

using log4net;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using ACE.Database.Models.Auth;
using ACE.Entity.Enum;
using System.Collections.Generic;
using System;
using System.Net;
using System.Security.Principal;

namespace ACE.Database
{
    public class AuthenticationDatabase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(bool retryUntilFound)
        {
            var config = Common.ConfigManager.Config.MySql.Authentication;

            for (; ; )
            {
                using (var context = new AuthDbContext())
                {
                    if (((RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>()).Exists())
                    {
                        log.DebugFormat("Successfully connected to {0} database on {1}:{2}.", config.Database, config.Host, config.Port);
                        return true;
                    }
                }

                log.Error($"Attempting to reconnect to {config.Database} database on {config.Host}:{config.Port} in 5 seconds...");

                if (retryUntilFound)
                    Thread.Sleep(5000);
                else
                    return false;
            }
        }


        public int GetAccountCount()
        {
            using (var context = new AuthDbContext())
                return context.Account.Count();
        }

        /// <exception cref="MySqlException">Account with name already exists.</exception>
        public Account CreateAccount(string name, string password, AccessLevel accessLevel, IPAddress address)
        {
            var account = new Account();

            account.AccountName = name;
            account.SetPassword(password);
            account.SetSaltForBCrypt();
            account.AccessLevel = (uint)accessLevel;

            account.CreateTime = DateTime.UtcNow;
            account.CreateIP = address.GetAddressBytes();

            using (var context = new AuthDbContext())
            {
                context.Account.Add(account);

                context.SaveChanges();
            }

            return account;
        }

        /// <summary>
        /// Will return null if the accountId was not found.
        /// </summary>
        public Account GetAccountById(uint accountId)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountId == accountId);
            }
        }

        /// <summary>
        /// Will return null if the accountName was not found.
        /// </summary>
        public Account GetAccountByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);
            }
        }

        /// <summary>
        /// id will be 0 if the accountName was not found.
        /// </summary>
        public uint GetAccountIdByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                var result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);

                return (result != null) ? result.AccountId : 0;
            }
        }

        /// <summary>
        /// result will be false if the endPoint was not found.
        /// </summary>
        public bool GetIPIsBanned(IPEndPoint endPoint)
        {
            using (var context = new AuthDbContext())
            {
                var result = context.BlackList
                    .AsNoTracking()
                    .FirstOrDefault(r => r.IP == endPoint.Address.GetAddressBytes());

                return (result != null) ? true : false;
            }
        }

        /// <summary>
        /// Get the Access Limit Per Account.
        /// Result will be 0 or a uint value based on the account
        /// </summary>
        public uint GetALPAccount(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                Account result = null;
                result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);
                return (result != null) ? result.AccessLimitExcess : 0;
            }
        }

        /// <summary>
        /// Get the Access Limit Per Account.
        /// Result will be 0 or a uint value based on the account
        /// </summary>
        public uint GetALPAccount(Account account)
        {
            using (var context = new AuthDbContext())
            {
                Account result = null;
                result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == account.AccountName);
                return (result != null) ? result.AccessLimitExcess : 0;
            }
        }

        /// <summary>
        /// Get the Access Limit Per Account.
        /// Result will be 0 or a uint value based on the account
        /// </summary>
        public uint GetALPAccount(uint accountId)
        {
            using (var context = new AuthDbContext())
            {
                Account result = null;
                result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountId == accountId);
                return (result != null) ? result.AccessLimitExcess : 0;
            }
        }

        /// <summary>
        /// Get the Access Limit Per IP.
        /// Result will be false if the endPoint was not found.
        /// </summary>
        public byte GetALPIP(IPEndPoint endPoint)
        {
            using (var context = new AuthDbContext())
            {
                var result = context.AccessLimits
                    .AsNoTracking()
                    .FirstOrDefault(r => r.IP == endPoint.Address.GetAddressBytes());

                return (result != null) ? result.AccessLimit : (byte)0;
            }
        }

        /// <summary>
        /// Get the Access Limit Per IP.
        /// Result will be false if the accountId was not found.
        /// </summary>
        public byte GetALPIP(uint accountId)
        {
            using (var context = new AuthDbContext())
            {
                AccessLimits result = null;
                try
                {
                    result = context.AccessLimits
                        .AsNoTracking()
                        .FirstOrDefault(r => r.AccountId == accountId);
                }
                catch
                {
                    return (byte)0; // redundant
                }

                return (result != null) ? result.AccessLimit : (byte)0;
            }
        }

        /// <summary>
        /// Set the Access Limit Per Account.
        /// Result will be 0 or greater uint value
        /// </summary>
        public bool SetALPAccount(string accountName, uint limit = 0)
        {
            using (var context = new AuthDbContext())
            {
                try
                {
                    var listing = context.Account
                        .First(r => r.AccountName == accountName);

                    // Update the existing
                    listing.AccessLimitExcess = limit;
                }
                catch
                {
                    return false;
                }
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Set the Access Limit Per IP.
        /// Result will be false if the endPoint was not found.
        /// </summary>
        public bool SetALPIP(IPEndPoint endPoint, string accountName = "", byte Limit = 3)
        {
            using (var context = new AuthDbContext())
            {

                // Check if the account/IP is already listed
                try
                {
                    var listing = context.AccessLimits
                        .First(r => r.IP == endPoint.Address.GetAddressBytes());

                    // Update the existing
                    listing.AccountId = GetAccountIdByName(accountName);
                    listing.AccessLimit = Limit;
                }
                catch
                {
                    // Create a new AccessLimits item
                    var newLimit = new AccessLimits
                    {
                        AccountId = GetAccountIdByName(accountName),
                        IP = endPoint.Address.GetAddressBytes(),
                        AccessLimit = Limit
                    };

                    // Add the new item to the AccessLimits DbSet
                    context.AccessLimits.Add(newLimit);
                }

                // Save the changes to the database
                return context.SaveChanges() > 0;

            }
        }

        /// <summary>
        /// result will be false if the endPoint was not found.
        /// </summary>
        public bool UpdateIPIsBanned(IPEndPoint endPoint)
        {
            using (var context = new AuthDbContext())
            {

                // Create a new BlackList item
                var newBlackListItem = new BlackList
                {
                    IP = endPoint.Address.GetAddressBytes()
                };

                // Add the new item to the BlackList DbSet
                context.BlackList.Add(newBlackListItem);

                // Save the changes to the database
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// result will be false if the endPoint was not found.
        /// </summary>
        public bool RemoveIPIsBanned(IPEndPoint endPoint)
        {
            using (var context = new AuthDbContext())
            {
                // Convert the IP address to a byte array
                byte[] ipAddressBytes = endPoint.Address.GetAddressBytes();

                // Find the existing BlackList item
                var blackListItem = context.BlackList
                                           .FirstOrDefault(b => b.IP.SequenceEqual(ipAddressBytes));

                if (blackListItem != null)
                {
                    // Remove the found item
                    context.BlackList.Remove(blackListItem);

                    // Save the changes to the database
                    return context.SaveChanges() > 0;
                }

                // Return false if the item was not found
                return false;
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var context = new AuthDbContext())
            {
                context.Entry(account).State = EntityState.Modified;

                context.SaveChanges();
            }
        }

        public bool UpdateAccountAccessLevel(uint accountId, AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var account = context.Account
                    .First(r => r.AccountId == accountId);

                if (account == null)
                    return false;

                account.AccessLevel = (uint)accessLevel;

                context.SaveChanges();
            }

            return true;
        }

        public List<string> GetListofAccountsByAccessLevel(AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.AccessLevel == Convert.ToUInt32(accessLevel)).ToList();

                var result = new List<string>();
                foreach (var account in results)
                    result.Add(account.AccountName);

                return result;
            }
        }

        public List<string> GetListofBannedAccounts()
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.BanExpireTime > DateTime.UtcNow).ToList();

                var result = new List<string>();
                foreach (var account in results)
                {
                    var bannedbyAccount = account.BannedByAccountId.Value > 0 ? $"account {GetAccountById(account.BannedByAccountId.Value).AccountName}" : "CONSOLE";
                    result.Add($"{account.AccountName} -- banned by {bannedbyAccount} until server time {account.BanExpireTime.Value.ToLocalTime():MMM dd yyyy  h:mmtt}{(!string.IsNullOrWhiteSpace(account.BanReason) ? $" -- Reason: {account.BanReason}" : "")}");
                }

                return result;
            }
        }
    }
}
