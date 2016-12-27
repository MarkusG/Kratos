using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;

namespace Kratos.Modules
{
    [Name("Usernote Module"), Group("un")]
    [Summary("Keep notes on users.")]
    public class UsernoteModule : ModuleBase
    {
        private UsernoteService _service;

        [Command("add"), Alias("+")]
        [Summary("Add a usernote")]
        [RequireCustomPermission("usernotes.add")]
        public async Task Add([Summary("User on which the note will be added")] IUser user,
                              [Summary("Content of the note"), Remainder] string content)
        {
            await _service.AddNoteAsync(user.Id, Context.User.Id, (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, content);
            await ReplyAsync(":ok:");
        }

        [Command("get"), Alias("<")]
        [Summary("Get all usernotes for a given user")]
        [RequireCustomPermission("usernotes.view")]
        public async Task Get([Summary("User for which to get notes")] IGuildUser user)
        {
            var notes = await _service.GetNotesForUserAsync(user.Id);
            var name = user.Nickname == null
                       ? user.Username
                       : $"{user.Username} (nickname: {user.Nickname})";

            //var response = new StringBuilder($"**Usernotes for {name}:**\n");

            //foreach (var n in notes)
            //{
            //    var author = await Context.Guild.GetUserAsync(n.AuthorId);
            //    var authorName = author.Nickname ?? author.Username;
            //    var timeStamp = new DateTime(1970, 1, 1).AddSeconds(n.UnixTimestamp);
            //    response.AppendLine($"`({n.Key}) {authorName} at {timeStamp} UTC:` {n.Content}");
            //}

            var responseEmbed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = name;
                    x.IconUrl = user.AvatarUrl;
                })
                .WithThumbnailUrl(user.AvatarUrl);

            foreach (var n in notes)
            {
                var author = await Context.Guild.GetUserAsync(n.AuthorId);
                var authorName = author.Nickname ?? author.Username;
                var timeStamp = new DateTime(1970, 1, 1).AddSeconds(n.UnixTimestamp);

                responseEmbed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"({n.Key}) {authorName} at {timeStamp} UTC";
                    x.Value = n.Content;
                });
            }

            _service.DisposeContext();
            await ReplyAsync("", embed: responseEmbed);
            //await ReplyAsync(response.ToString());
        }

        [Command("get"), Alias("<")
        [Summary("Get a single usernote by ID")]
        [RequireCustomPermission("usernotes.view")]
        public async Task Get([Summary("Note ID")] int id)
        {
            var note = await _service.GetNoteAsync(id);
            if (note == null)
            {
                await ReplyAsync(":x: Note not found.");
                return;
            }

            var author = await Context.Guild.GetUserAsync(note.AuthorId);
            var authorName = author.Nickname ?? author.Username;
            var subject = await Context.Guild.GetUserAsync(note.SubjectId);
            var subjectName = subject.Nickname == null
                              ? subject.Username
                              : $"{subject.Username} (nickname: {subject.Nickname})";
            var timeStamp = new DateTime(1970, 1, 1).AddSeconds(note.UnixTimestamp);

            var responseEmbed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = $"Usernote ID {id}";
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Author";
                    x.Value = authorName;
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Subject";
                    x.Value = subjectName;
                })
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Content";
                    x.Value = note.Content;
                })
                .WithFooter(x =>
                {
                    x.Text = $"at {timeStamp.ToString()}";
                });

            //var response = $"`({note.Key}) By {authorName} regarding {subjectName} at {timeStamp}:` {note.Content}";
            await ReplyAsync("", embed: responseEmbed);
        }

        [Command("remove"), Alias("-")]
        [Summary("Remove a usernote")]
        [RequireCustomPermission("usernotes.remove")]
        public async Task Remove([Summary("Note ID")] int id)
        {
            var result = await _service.RemoveNoteAsync(id);
            _service.DisposeContext();
            await ReplyAsync(result
                             ? ":ok:"
                             : ":warning: Note not found.");
        }

        [Command("clear"), Alias("--")]
        [Summary("Clear all usernotes for a given user")]
        [RequireCustomPermission("usernotes.remove")]
        public async Task Clear([Summary("User on which to clear all notes")] IGuildUser user)
        {
            await _service.ClearNotesAsync(user.Id);
            _service.DisposeContext();
            await ReplyAsync(":ok:");
        }

        public UsernoteModule(UsernoteService s)
        {
            _service = s;
        }
    }
}
