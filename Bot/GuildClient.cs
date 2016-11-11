using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GuildDiscordBot.Bot
{
    public class GuildClient : DiscordClient
    {
        public GuildClient() : base()
        {
            RuntimeStatics.Client = this;
            this.UsingCommands(cs =>
            {
                cs.AllowMentionPrefix = false;
                cs.PrefixChar = '!';
                cs.HelpMode = HelpMode.Public;
            });
            ImplementCommands();

            ExecuteAndWait(async () => { await Connect("", TokenType.Bot); });
        }

        public void ImplementCommands()
        {
            GetService<CommandService>().CreateGroup("application", cb =>
             {
                 //This entire command group requires Admin
                 cb.AddCheck((cm, u, ch) => u.ServerPermissions.Administrator, "command requires permission: Administrator");

                 //Add Question
                 cb.CreateCommand("add")
                 .Description("Add a question to the end of the interview pool")
                 .Parameter("Question", ParameterType.Unparsed)
                 .Do(async e => { await AppCommands.Add(e); });

                 //Remove question
                 cb.CreateCommand("remove")
                 .Description("Removes the question with specified ID")
                 .Parameter("ID")
                 .Do(async e => { await AppCommands.Remove(e); });

                 //List Questions
                 cb.CreateCommand("list")
                 .Description("Lists the active application questions")
                 .Do(async e => { await AppCommands.List(e); });

                 //Reorder Questions
                 cb.CreateCommand("moveto")
                 .Description("Reorders the specified question to the desired ID Usage: moveto 1 4")
                 .Parameter("FirstID")
                 .Parameter("SecondID")
                 .Do(async e => { await AppCommands.MoveTo(e); });
             });

            GetService<CommandService>().CreateGroup("Guild", cb =>
            {
                //This entire command group requires Admin
                cb.AddCheck((cm, u, ch) => u.ServerPermissions.Administrator, "command requires permission: Administrator");

                //Set guild name
                cb.CreateCommand("setname")
                .Description($"Sets the guild Name. Current Value: `{Program.Guild.Name}`")
                .Parameter("Name")
                .Do(e => { Program.Guild.Name = e.GetArg("Name"); Program.SaveGuildInfo(); });

                //Set guild realm
                cb.CreateCommand("setrealm")
                .Description($"Sets the guild Realm. Current Value: `{Program.Guild.Realm}`")
                .Parameter("Realm")
                .Do(e => { Program.Guild.Realm = e.GetArg("Realm"); Program.SaveGuildInfo(); });

                //Set Raid Days
                cb.CreateCommand("setraiddays")
                .Description($"Sets the raid days. Current Value: `{Program.Guild.RaidDays}`")
                .Parameter("Days")
                .Do(e => { Program.Guild.RaidDays = e.GetArg("Days"); Program.SaveGuildInfo(); });

                //set Raid Times
                cb.CreateCommand("setraidtimes")
                .Description($"Sets the raid times. Current Value: `{Program.Guild.RaidTimes}`")
                .Parameter("Times")
                .Do(e => { Program.Guild.RaidTimes = e.GetArg("Times"); Program.SaveGuildInfo(); });

                //set Officer ID
                cb.CreateCommand("setofficerid")
                .Description($"Sets the officer role ID. Current Value: `{Program.Guild.OfficerID}`")
                .Parameter("ID")
                .Do(e =>
                {
                    ulong ID = 0;
                    if (ulong.TryParse(e.GetArg("ID"), out ID))
                    {
                        Program.Guild.OfficerID = ID; Program.SaveGuildInfo();
                    }
                });

                //set @everyone ID
                cb.CreateCommand("seteveryoneid")
                .Description($"Sets the @everyone role ID. Current Value: `{Program.Guild.EveryoneID}`")
                .Parameter("ID")
                .Do(e =>
                {
                    ulong ID = 0;
                    if (ulong.TryParse(e.GetArg("ID"), out ID))
                    {
                        Program.Guild.EveryoneID = ID; Program.SaveGuildInfo();
                    }
                });
            });

            //Begins the application process
            GetService<CommandService>().CreateCommand("apply")
                .Description("Begins or restarts the application process")
                //.AddCheck((cm, u, ch) => u.Roles.Count() <= 1)
                .Do(async e => { await AppCommands.Apply(e); });

            //Lists the server roles as well as their ID
            GetService<CommandService>().CreateCommand("roles")
                .Description("Lists the available roles as well as their internal IDs")
                .Do(async e => 
                {
                    string roleStr = "```";
                    int num = 1;
                    foreach(Role r in e.Server.Roles)
                    {
                        roleStr += $"{num}. {r.Name} | {r.Id}\n"; 
                    }
                    roleStr += "```";
                    await e.Channel.SendMessage(roleStr);
                });

            //Clears all messages in a channel.
            GetService<CommandService>().CreateCommand("clear")
                .Description("Clear all messages in a channel")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async e => { await AppCommands.ClearChannel(e); });

            GetService<CommandService>().CreateCommand("eatmyass")
                .Description(":^)")
                .Do(async e => { await e.Channel.SendMessage(":ok_hand::yum::sweat_drops:"); });

            // Message received event.
            MessageReceived += (async (s,e) => 
            {
                if (e.User.IsBot || e.Message.Text.Contains("!apply")) 
                    return;
                if(RuntimeStatics.ActiveChannels.ContainsKey(e.Channel))
                    await AppCommands.HandleApplication(e);
            });
        }
    }
}
