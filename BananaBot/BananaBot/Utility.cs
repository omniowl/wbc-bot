/* * * * * * * * * * * * * * * * * * * * * * * * * *
 *  © Mads Falkenberg Sønderstrup, Denmark, 2017   *
 *  All Rights Reserved.                           *
 * * * * * * * * * * * * * * * * * * * * * * * * * */

using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using Discord;

namespace BananaBot
{
    public static class Utility
    {
        private static Dictionary<String, ulong> _serverids;

        /// <summary>
        /// Used to initialize any values this static class might need.
        /// Should only be run once, from the main method.
        /// </summary>
        public static void Init()
        {
            JObject jsonObj = JObject.Parse(File.ReadAllText(AppContext.BaseDirectory + "_configuration.json"));
            JToken jToken = jsonObj.SelectToken("server-ids");
            _serverids = new Dictionary<String, ulong>();
            foreach (JProperty obj in jToken.Children())
            {
                _serverids.Add(obj.Name, ulong.Parse((String)obj.Value));
            }
        }

        public static JToken GetConfigRootProperty(string key)
        {
            JObject jsonObj = JObject.Parse(File.ReadAllText(AppContext.BaseDirectory + "_configuration.json"));
            JToken jToken = jsonObj.SelectToken(key);
            return jToken;
        }

        /// <summary>
        /// Checks to see if the serverID provided matches a server from our configuration.
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="serverName"></param>
        /// <returns>True if the server ID matches the ID of the server requested, else False in all other cases.</returns>
        public static bool CompareServerIds(ulong serverId, String serverName)
        {
            if (_serverids.ContainsKey(serverName))
            {
                if (_serverids[serverName] == serverId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Used to find all users with a single role as search query.
        /// </summary>
        /// <param name="role">The role to look for.</param>
        /// <param name="context">The command context currently in use</param>
        /// <param name="includeOffline">Optional parameter to include offline users. Defaults to false.</param>
        /// <returns>A list of users that have the mentioned role.</returns>
        public async static Task<List<SocketGuildUser>> FindAllUsersWithRole(String role, ICommandContext context, bool includeOffline = false)
        {
            List<SocketGuildUser> data = new List<SocketGuildUser>();
            IReadOnlyCollection<SocketGuildUser> result = await context.Guild.GetUsersAsync() as IReadOnlyCollection<SocketGuildUser>;
            if (includeOffline == true)
            {
                foreach (SocketGuildUser user in result)
                {
                    foreach (SocketRole sRole in user.Roles)
                    {
                        if (sRole.Name.Equals(role))
                        {
                            data.Add(user);
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (SocketGuildUser user in result)
                {
                    if (user.Status == Discord.UserStatus.Online || user.Status == Discord.UserStatus.Idle || user.Status == Discord.UserStatus.AFK)
                    {
                        foreach (SocketRole sRole in user.Roles)
                        {
                            if (sRole.Name.Equals(role))
                            {
                                data.Add(user);
                                break;
                            }
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Used to find all users with a single role as search query and exludes a role from the result.
        /// </summary>
        /// <param name="role">The role to look for.</param>
        /// <param name="context">The command context currently in use</param>
        /// <param name="exclude">The role to exclude.</returns>
        public async static Task<List<SocketGuildUser>> FindAllUsersWithRoleExclude(String role, ICommandContext context, String exclude)
        {
            List<SocketGuildUser> data = new List<SocketGuildUser>();
            IReadOnlyCollection<SocketGuildUser> result = await context.Guild.GetUsersAsync() as IReadOnlyCollection<SocketGuildUser>;
            List<SocketGuildUser> filteredUsers = result.ToList();

            for (int i = 0; i < filteredUsers.Count; i++)
            {
                foreach (SocketRole sRole in filteredUsers[i].Roles)
                {
                    if (sRole.Name.Equals(exclude))
                    {
                        filteredUsers.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            foreach (SocketGuildUser user in filteredUsers)
            {
                if (!user.IsBot)
                {
                    foreach (SocketRole sRole in user.Roles)
                    {
                        if (sRole.Name.Equals(role))
                        {
                            data.Add(user);
                            break;
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Used to find all users with a single role as search query.
        /// </summary>
        /// <param name="role">The role to look for.</param>
        /// <param name="users">The List of Users to look through for the mentioned role.</param>
        /// <returns>A list of users that have the mentioned role.</returns>
        public static List<SocketGuildUser> FindAllUsersWithRole(String role, List<SocketGuildUser> users)
        {
            List<SocketGuildUser> data = new List<SocketGuildUser>();
            foreach (SocketGuildUser user in users)
            {
                if (!user.IsBot)
                {
                    foreach (SocketRole sRole in user.Roles)
                    {
                        if (role.Equals(sRole.Name.ToLower()))
                        {
                            data.Add(user);
                            break;
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Used to find all users with multiple roles as search query.
        /// </summary>
        /// <param name="roles">The roles to look for in comma seperated string.</param>
        /// <param name="users">The List of Users to look through for the mentioned roles.</param>
        /// <param name="isExplicitSearch">If this is set to false, then anyone with one of the roles is returned. If True only those with all the roles are returned.</param>
        /// <returns>A list of users that have one or more of the mentioned roles.</returns>
        public static List<SocketGuildUser> FindAllUsersWithRoles(String roles, List<SocketGuildUser> users, bool isExplicitSearch = true)
        {
            List<SocketGuildUser> data = new List<SocketGuildUser>();
            roles = roles.ToLower();
            // Split the list of roles to look for using the comma as the delimiter.
            String[] rolesToCheck = roles.Split(",");
            if (isExplicitSearch == true)
            {
                foreach (SocketGuildUser user in users)
                {
                    List<String> roleList = new List<String>();
                    int checkSum = 0;
                    // Get all the roles the user got in string format
                    // so we can compare them to the roles we are looking for
                    foreach (SocketRole role in user.Roles)
                    {
                        roleList.Add(role.Name);
                    }

                    for (int i = 0; i < roleList.Count; i++)
                    {
                        roleList[i] = roleList[i].ToLower();
                    }

                    // Check if we can find any matches.
                    foreach (String sRole in rolesToCheck)
                    {
                        if (roleList.Contains(sRole.ToLower()))
                        {
                            checkSum++;
                        }
                    }
                    // If we found as many matching roles as there are roles in
                    // our control list, then we are good and we add them to the
                    // return list of data.
                    if (checkSum == (rolesToCheck.Length))
                    {
                        data.Add(user);
                    }
                }
            }
            else
            {
                foreach (SocketGuildUser user in users)
                {
                    foreach (SocketRole sRole in user.Roles)
                    {
                        if (roles.Contains(sRole.Name.ToLower()))
                        {
                            data.Add(user);
                            break;
                        }
                    }
                }
            }

            return data;
        }
    }
}
