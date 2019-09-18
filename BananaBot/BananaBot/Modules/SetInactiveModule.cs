using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BananaBot.Modules
{
    public class SetInactiveModule : ModuleBase
    {
        [Command("setinactive"), Summary("Will set the inactive role if it's absent, otherwise remove it")]
        public async Task SetInactiveRole()
        {
            List<IMessage> message = new List<IMessage>() { this.Context.Message };
            await Context.Channel.DeleteMessagesAsync(message);
            string coachRoleString = (string)Utility.GetConfigRootProperty("ow-coach-role");

            IReadOnlyCollection<SocketRole> userRoles = ((SocketGuildUser)Context.User).Roles;

            bool HasRole = Utility.UserHasRole(coachRoleString, userRoles);
            if (HasRole == true)
            {
                string inactiveRoleString = (string)Utility.GetConfigRootProperty("ow-coach-inactive-role");
                HasRole = Utility.UserHasRole(inactiveRoleString, userRoles);
                SocketRole inactiveRole = Utility.GetRoleObject(Context, inactiveRoleString);
                if (inactiveRole != null)
                {
                    string roleStatusString = "";
                    if (HasRole == false)
                    {
                        await ((SocketGuildUser)Context.User).AddRoleAsync(inactiveRole);
                        roleStatusString = "was added to";
                    }
                    else
                    {
                        await ((SocketGuildUser)Context.User).RemoveRoleAsync(inactiveRole);
                        roleStatusString = "was removed from";
                    }

                    await Context.Channel.SendMessageAsync($"Inactive role {roleStatusString} {Context.User.Mention}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"The Inactive role wasn't found {Context.User.Mention}. Please contact a mod or supervisor.");
                }
            }
        }
    }
}
