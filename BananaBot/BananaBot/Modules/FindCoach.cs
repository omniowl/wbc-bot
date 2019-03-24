using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BananaBot.Modules
{
    public class FindCoach : ModuleBase
    {
        [Command("find"), Summary("Used to find a Coach.")]
        public async Task Find(string commandText = "")
        {
            JToken channels = Utility.GetConfigRootProperty("channels");
            Dictionary<string, string> channelDict = new Dictionary<string, string>();
            foreach (JProperty property in channels.Children())
            {
                channelDict.Add((string)property.Name, (string)property.Value);
            }

            if (Context.Channel.Name.Equals(channelDict["ow"]))
            {
                List<IMessage> message = new List<IMessage>() { this.Context.Message };
                await Context.Channel.DeleteMessagesAsync(message);
                SocketGuildUser caller = this.Context.User as SocketGuildUser;
                if (string.IsNullOrEmpty(commandText))
                {
                    await ListHeroes(caller);
                }
                else
                {
                    string[] heroes = Utility.GetConfigRootProperty("ow-heroes").ToObject<string[]>();
                    List<string> heroList = new List<string>(heroes);
                    for (int i = 0; i < heroList.Count; i++)
                    {
                        heroList[i] = heroList[i].ToLower();
                    }

                    commandText = commandText.ToLower();
                    if (heroList.Contains(commandText))
                    {
                        await FindOW(commandText);
                    }
                    else
                    {
                        await ListHeroes(caller);
                    }
                }
            }
            else if (Context.Channel.Name.Equals(channelDict["fn"]))
            {
                List<IMessage> message = new List<IMessage>() { this.Context.Message };
                await Context.Channel.DeleteMessagesAsync(message);
                await FindFN();
            }
        }

        private async Task FindOW(string role)
        {
            List<SocketGuildUser> activeCoaches = await Utility.FindAllUsersWithRoleExclude("Coach", this.Context, "Inactive");
            activeCoaches = Utility.FindAllUsersWithRoles($"~{role}", activeCoaches);

            await SendCoachList(activeCoaches, role);
        }

        private async Task FindFN()
        {
            List<SocketGuildUser> activeCoaches = await Utility.FindAllUsersWithRoleExclude("Coach", this.Context, "Inactive");
            activeCoaches = Utility.FindAllUsersWithRoles("Fortnite", activeCoaches);

            await SendCoachList(activeCoaches);
        }

        private async Task SendCoachList(List<SocketGuildUser> coaches, string role = "")
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append($"--- Active Coaches {(string.IsNullOrEmpty(role) ? "" : "(" + role + ")")} ---\n");
            if (coaches.Count > 5)
            {
                List<SocketGuildUser> randomCoaches = new List<SocketGuildUser>();
                Random rnd = new Random();
                do
                {
                    int randomIndex = rnd.Next(0, coaches.Count);
                    if (!randomCoaches.Contains(coaches[randomIndex]))
                    {
                        randomCoaches.Add(coaches[randomIndex]);
                    }
                } while (randomCoaches.Count < 5);

                foreach (SocketGuildUser user in randomCoaches)
                {
                    sBuilder.Append($"{user.Mention} ({user.Username}#{user.Discriminator})\n");
                }
            }
            else
            {
                foreach (SocketGuildUser user in coaches)
                {
                    sBuilder.Append($"{user.Mention} ({user.Username}#{user.Discriminator})\n");
                }
            }
            await Context.User.SendMessageAsync(sBuilder.ToString());
        }

        private async Task ListHeroes(SocketGuildUser caller)
        {
            JToken heroes = Utility.GetConfigRootProperty("ow-heroes");
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append("Below you can find the different hero roles for Overwatch that we do coaching for:");
            sBuilder.Append("\n");
            foreach (string hero in heroes.Children())
            {
                sBuilder.Append(hero).Append(",");
            }
            await caller.SendMessageAsync(sBuilder.ToString());
        }
    }
}
