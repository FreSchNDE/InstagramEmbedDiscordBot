﻿using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Instagram_Reels_Bot.Helpers;

namespace Instagram_Reels_Bot.Modules
{
    public class TextCommands : ModuleBase
    {
        /// <summary>
        /// Parse reel URL:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("reel", RunMode = RunMode.Async)]
        public async Task ReelParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/reel/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse an instagram post:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("p", RunMode = RunMode.Async)]
        public async Task PostParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/p/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse an instagram TV link:
        /// https://www.instagram.com/tv/
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("tv", RunMode = RunMode.Async)]
        public async Task TVParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/tv/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse Story Link
        /// Ex: https://instagram.com/stories/wevolverapp/2718330735469161935
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("stories", RunMode = RunMode.Async)]
        public async Task StoryParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/stories/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Centralized method to handle all Instagram links and respond to text based messages (No slash commands).
        /// </summary>
        /// <param name="url">The Instagram URL of the content</param>
        /// <param name="context">The discord context of the message</param>
        /// <returns></returns>
        private static async Task Responder(string url, ICommandContext context)
        {
            //Process Post:
            InstagramProcessorResponse response = await InstagramProcessor.PostRouter(url, (int)context.Guild.PremiumTier, 1);

            //Embeds:
            //Account Name:
            var account = new EmbedAuthorBuilder();
            account.IconUrl = response.iconURL.ToString();
            account.Name = response.accountName;
            account.Url = response.accountUrl.ToString();

            //Instagram Footer:
            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.IconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png";
            footer.Text = "Instagram";

            var embed = new EmbedBuilder();
            embed.Author = account;
            embed.Title = "Content from " + context.Message.Author.Username + "'s linked post.";
            embed.Footer = footer;
            embed.Timestamp = new DateTimeOffset(response.postDate);
            embed.Url = response.postURL.ToString();
            embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");
            embed.WithColor(new Color(131, 58, 180));

            if (!response.success)
            {
                await context.Message.ReplyAsync(response.error);
                return;
            }
            if (response.isVideo)
            {
                if (response.stream != null)
                {
                    //Response with stream:
                    using (Stream stream = new MemoryStream(response.stream))
                    {
                        FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");
                        await context.Message.Channel.SendFileAsync(attachment, embed: embed.Build());
                    }
                    return;
                }
                else
                {
                    //Response without stream:
                    await context.Message.ReplyAsync(response.contentURL.ToString(), embed: embed.Build(), allowedMentions: AllowedMentions.None);
                    return;
                }

            }
            else
            {
                embed.ImageUrl = "attachment://IGMedia.jpg";
                if (response.stream != null)
                {
                    using (Stream stream = new MemoryStream(response.stream))
                    {
                        FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
                        await context.Channel.SendFileAsync(attachment, embed: embed.Build());
                    }
                }
                else
                {
                    embed.ImageUrl = response.contentURL.ToString();
                    await context.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
                }
            }
        }
    }
}