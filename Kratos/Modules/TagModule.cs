using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.EntityFramework;
using Kratos.Results;
using Kratos.Extensions;

namespace Kratos.Modules
{
    [Name("Tag Module")]
    [Summary("Provides commands for interacting with tags and their values.")]
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Summary("View a tag and its value")]
        [Permission("tag.view")]
        public async Task<RuntimeResult> ViewAsync(string tag = null)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                if (tag == null)
                {
                    var tags = context.Tags.Where(t => t.GuildId == Context.Guild.Id)
                                           .Select(t => t.Tag)
                                           .OrderBy(t => t);
                    await ReplyAsync($"Tags: {string.Join(", ", tags)}");
                    return SimpleRuntimeResult.FromSuccess();
                }
                var result = await context.Tags.FirstOrDefaultAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id);
                if (result == null)
                    return SimpleRuntimeResult.FromFailure("Tag not found.");

                await ReplyAsync($"{result.Tag}: {result.Value}");
                result.TimesInvoked++;
                await context.SaveChangesAsync();
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("add"), Alias("+")]
        [Summary("Adds a tag with the given value")]
        [Permission("tag.add")]
        public async Task<RuntimeResult> AddAsync(string tag, [Remainder] string value)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                if (await context.Tags.AnyAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id))
                    return SimpleRuntimeResult.FromFailure("A tag by that name already exists.");
                await context.Tags.AddAsync(new TagValue
                {
                    AuthorId = Context.User.Id,
                    CreatedAt = DateTime.UtcNow,
                    GuildId = Context.Guild.Id,
                    Tag = tag,
                    Value = value
                });
                await context.SaveChangesAsync();
                await ReplyAsync("🆗");
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("remove"), Alias("-", "delete")]
        [Summary("Removes a tag")]
        [Permission("tag.remove")]
        public async Task<RuntimeResult> RemoveAsync(string tag)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var target = await context.Tags.FirstOrDefaultAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id);
                if (target == null)
                    return SimpleRuntimeResult.FromWarning("No tag found.");
                context.Tags.Remove(target);
                await context.SaveChangesAsync();
                await ReplyAsync("🆗");
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("modify"), Alias("edit")]
        [Summary("Modifies a tag's value")]
        [Permission("tag.modify")]
        public async Task<RuntimeResult> ModifyAsync(string tag, [Remainder] string newValue)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var target = await context.Tags.FirstOrDefaultAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id);
                if (target == null)
                    return SimpleRuntimeResult.FromFailure("No tag found.");
                target.Value = newValue;
                await context.SaveChangesAsync();
                await ReplyAsync("🆗");
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("rename")]
        [Summary("Renames a tag")]
        [Permission("tag.modify")]
        public async Task<RuntimeResult> RenameAsync(string tag, string newName)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var target = await context.Tags.FirstOrDefaultAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id);
                if (target == null)
                    return SimpleRuntimeResult.FromFailure("No tag found.");
                target.Tag = newName;
                await context.SaveChangesAsync();
                await ReplyAsync("🆗");
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("info")]
        [Summary("Show information for a tag")]
        [Permission("tag.view")]
        public async Task<RuntimeResult> InfoAsync(string tag)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var target = await context.Tags.FirstOrDefaultAsync(t => t.Tag == tag && t.GuildId == Context.Guild.Id);
                if (target == null)
                    return SimpleRuntimeResult.FromFailure("No tag found.");
                var author = Context.Guild.GetUser(target.AuthorId);
                var authorName = author == null ? $"User Left Guild (ID: {target.AuthorId})"
                                                : author.GetFullName();
                var response = new EmbedBuilder()
                    .WithTitle(target.Tag)
                    .AddField("Owner", authorName)
                    .AddField("Uses", target.TimesInvoked)
                    .AddField("Created At", target.CreatedAt.ToString("dd/MM/yyyy hh:mm:ss UTC"));
                await ReplyAsync("", embed: response);
                return SimpleRuntimeResult.FromSuccess();
            }
        }
    }
}
