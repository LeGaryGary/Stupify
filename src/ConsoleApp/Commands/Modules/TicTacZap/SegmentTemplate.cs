using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class SegmentTemplate : StupifyModuleBase
    {
        [Command("SaveTemplate")]
        public async Task SaveTemplateCommand()
        {
            var user = await this.GetUserAsync();
            var userSelection = TicTacZapController.GetUserSelection(user.UserId);
            if (userSelection == null)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            var templateId = await Db.NewTemplateAsync(user);
            var segment = await TicTacZapManagement.Segments.GetAsync((int)userSelection);
            await SegmentTemplates.SaveAsync(templateId, segment);
        }

        [Command("Templates")]
        public async Task ShowTemplatesCommand()
        {
            var templates = await GetTemplatesAsync();
            var message = string.Empty;
            foreach (var template in templates)
            {
                message += template.Name == null
                    ? $"Id: {template.SegmentTemplateId}"
                    : $"Id: {template.SegmentTemplateId}, {template.Name}";
                message += Environment.NewLine;
            }

            await ReplyAsync("```" + message + "```");
        }

        [Command("Template")]
        public async Task ShowTemplateCommand(int templateId)
        {
            var user = await this.GetUserAsync();
            var dbTemplate = await Db.SegmentTemplates.FirstOrDefaultAsync(st => st.SegmentTemplateId == templateId && st.User.UserId == user.UserId);
            if (dbTemplate == null)
            {
                await ReplyAsync(Responses.TemplateOwnershipProblem);
            }

            var template = await SegmentTemplates.GetAsync(templateId);
            await ReplyAsync("```" + template.TextRender() + "```");
        }

        private async Task<List<DataModels.SegmentTemplate>> GetTemplatesAsync()
        {
            var user = await this.GetUserAsync();
            return await Db.SegmentTemplates.Where(st => st.User.UserId == user.UserId).ToListAsync();
        }
    }
}
