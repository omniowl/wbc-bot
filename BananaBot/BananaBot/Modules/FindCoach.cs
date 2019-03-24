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
    public class FindCoach : ModuleBase
    {
        [Command("find"), Summary("Used to find a Coach.")]
        public async Task Find(string commandText)
        {
            List<IMessage> message = new List<IMessage>() { this.Context.Message };
            await Context.Channel.DeleteMessagesAsync(message);

            SocketGuildUser caller = this.Context.User as SocketGuildUser;
            JToken channels = Utility.GetConfigRootProperty("channels");

            if (Context.Channel.Name.Equals(channels.Children()["ow"]))
            {
                if (commandText.Split(' ').Length < 2)
                {
                    await ListHeroes(caller);
                }
                else
                {
                    await FindOW(commandText.Split(' ')[1]);
                }
            }
            else if (Context.Channel.Name.Equals(channels.Children()["pubg"]))
            {
                await FindFN();
            }
            else
            {
                await caller.SendMessageAsync("Either type ```!find <hero>``` or ```!find``` in the request coaching channel.");
                await ListHeroes(caller);
            }
        }

        private async Task FindOW(string role)
        {
            List<SocketGuildUser> activeCoaches = await Utility.FindAllUsersWithRoleExclude("Coach", this.Context, "Inactive");
            activeCoaches = Utility.FindAllUsersWithRoles($"~{role}", activeCoaches);
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append($"--- Active Coaches ({role}) ---\n");
            if (activeCoaches.Count > 5)
            {
                List<SocketGuildUser> randomCoaches = new List<SocketGuildUser>();
                Random rnd = new Random();
                do
                {
                    int randomIndex = rnd.Next(0, activeCoaches.Count);
                    if (!randomCoaches.Contains(activeCoaches[randomIndex]))
                    {
                        randomCoaches.Add(activeCoaches[randomIndex]);
                    }
                } while (randomCoaches.Count < 5);

                foreach (SocketGuildUser user in randomCoaches)
                {
                    sBuilder.Append($"{user.Mention} ({user.Username}#{user.AvatarId})\n");
                }
            }
            else
            {
                foreach (SocketGuildUser user in activeCoaches)
                {
                    sBuilder.Append($"{user.Mention} ({user.Username}#{user.AvatarId})\n");
                }
            }

            await Context.User.SendMessageAsync(sBuilder.ToString());
        }

        private async Task FindFN()
        {

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
