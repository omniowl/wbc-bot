using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace BananaBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
            if (msg == null) return;
            if (msg.Author == _discord.CurrentUser) return;     // Ignore self when checking commands

            var context = new SocketCommandContext(_discord, msg);     // Create the command context

            int argPos = 0;     // Check if the message has a valid command prefix
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);     // Execute the command

                if (!result.IsSuccess && msg.Channel.Name.Contains("bot-test-server"))     // If not successful, reply with the error.
                    await context.User.SendMessageAsync(result.ToString());
            }
        }
    }
}