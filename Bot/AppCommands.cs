using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuildDiscordBot;

namespace GuildDiscordBot.Bot
{
    public static class AppCommands
    {
        // Test .Do commands.
        async static public Task Test(CommandEventArgs e)
        {
          await  e.Channel.SendMessage("Test Response");
        }

        //Removes question
        async internal static Task Remove(CommandEventArgs e)
        {
            int ID = 0;

            if (int.TryParse(e.GetArg("ID"), out ID))
            {
                if (ID < RuntimeStatics.Questions.Count || ID > 0)
                {
                    RuntimeStatics.Questions.RemoveAt(ID - 1);
                    await e.Channel.SendMessage($"Removed question with ID {ID}");
                    SaveQuestions();
                }
                else
                    await e.Channel.SendMessage($"ID {ID} was not found in question list.");
            }
            else
                await e.Channel.SendMessage("Remove command requires an Integer for the ID");
        }

        //Adds question
        async internal static Task Add(CommandEventArgs e)
        {
            RuntimeStatics.Questions.Add(e.GetArg("Question"));
            await e.Channel.SendMessage($"Added Question {e.GetArg("Question")} at ID {RuntimeStatics.Questions.Count}");
            SaveQuestions();
        }

        //Lists questions
        async internal static Task List(CommandEventArgs e)
        {
            if (RuntimeStatics.Questions.Count <= 0) { await e.Channel.SendMessage("No Questions set. Use `!application add <Question>` to add a question."); return; }
            int ID = 1;
            string str = "";
            foreach(string q in RuntimeStatics.Questions)
            {
                str += ($"`{ID}. | {q}`\n");
                ID++;
            }
            await e.Channel.SendMessage(str);
        }

        //Moves a question to the specified location
        async internal static Task MoveTo(CommandEventArgs e)
        {
            int FirstID, SecondID;
            var questions = RuntimeStatics.Questions;
            if (int.TryParse(e.GetArg("FirstID"), out FirstID) && int.TryParse(e.GetArg("SecondID"), out SecondID))
            {
                if (FirstID > 0 && FirstID <= questions.Count && SecondID > 0 && SecondID <= questions.Count)
                {
                    var q = questions.ElementAt(FirstID - 1);
                    RuntimeStatics.Questions.RemoveAt(FirstID - 1);
                    RuntimeStatics.Questions.Insert(SecondID - 1, q);
                    SaveQuestions();
                    await e.Channel.SendMessage($"Moved question ID {FirstID} to {SecondID}");
                }
                else
                    await e.Channel.SendMessage("ID was out of range. Please enter numbers within the current range of questions.");
            }
            else
                await e.Channel.SendMessage($"{e.GetArg("FirstID")} or {e.GetArg("SecondID")} is not a valid integers");
        }

        //Begins the application process.
        async internal static Task Apply(CommandEventArgs e)
        {
            Channel userchannel = null;
            var userchannelList = e.Server.AllChannels.Where(ch => ch.Name == e.User.Name);
            if (userchannelList.Count() > 0) userchannel = userchannelList.First();
            if (userchannel != null)
            {
                RuntimeStatics.ActiveChannels[userchannel] = 0;
                await userchannel.DeleteMessages(userchannel.Messages.ToArray());
                await BeginApplication(e, userchannel);
            }
            else
            {
                //why the fuck does this exist
                var userpermission = new ChannelPermissionOverrides(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);
                var everyonepermission = new ChannelPermissionOverrides(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

                userchannel = await e.Server.CreateChannel(e.User.Name, ChannelType.Text);

                await userchannel.AddPermissionsRule(e.Server.GetRole(Program.Guild.EveryoneID), everyonepermission);
                await userchannel.AddPermissionsRule(e.User, userpermission);
                await userchannel.AddPermissionsRule(e.Server.GetRole(Program.Guild.OfficerID), userpermission);

                RuntimeStatics.ActiveChannels.Add(userchannel, 0);
                await Apply(e);
            }
        }

        //Called when a user begins the application process.
        async private static Task BeginApplication(CommandEventArgs e, Channel userchannel)
        {
            Console.WriteLine($"Starting Application for {e.User.Name} at {DateTime.Now}");
            await userchannel.SendMessage(
                $"{e.User.Mention} Thank you for applying to {Program.Guild.Name}-{Program.Guild.Realm}! Our raid times are {Program.Guild.RaidDays} {Program.Guild.RaidTimes}."
                + $"\nPlease answer every question, you may come back and edit your answers at any time."
                + $"\nThere is no time-limit. Officers will not be aware of your application until it is complete. (Although they can see it)"
                + $"\nIf you cannot answer a question, please indicate so with N/A."
                + $"\nIf for any reason this bot stops responding, Restart the application with !apply"
                + $"\nPlease reply to this message to begin the application.");
        }

        // saves the current question list
        private static void SaveQuestions()
        {
            Tools.Tools.SaveToJSONFile(RuntimeStatics.Questions, RuntimeStatics.QuestionPath);
        }

        // handles the application process.
        async public static Task HandleApplication(MessageEventArgs e)
        {
            if (RuntimeStatics.Questions.Count <= 0)
                await FinalizeApplication(e);

            var questionNum = RuntimeStatics.ActiveChannels[e.Channel];
            if(questionNum >= RuntimeStatics.Questions.Count)
            {
                await FinalizeApplication(e);
            }
            else
            {
                Console.WriteLine($"Pushing question to {e.User.Name} at {DateTime.Now}");
                await e.Channel.SendMessage(RuntimeStatics.Questions.ElementAt(questionNum));
                RuntimeStatics.ActiveChannels[e.Channel] += 1;
            }
        }

        //completes the application process.
        private static async Task FinalizeApplication(MessageEventArgs e)
        {
            Console.WriteLine($"Completed Application for {e.User.Name} at {DateTime.Now}");
            await e.Channel.SendMessage($"Thank you for applying to {Program.Guild.Name}-{Program.Guild.Realm}"
                + "\nPlease leave discord open, an officer will be reviewing your application within the next 24 hours."
                + $"\nIf for any reason you need to contact an officer, you may do so here using {e.Server.GetRole(Program.Guild.OfficerID).Mention}");
            RuntimeStatics.ActiveChannels.Remove(e.Channel);
        }

        //Clears all messages in a channel.
        async public static Task ClearChannel(CommandEventArgs e)
        {
            var messages = await e.Channel.DownloadMessages(100);
            await e.Channel.DeleteMessages(messages);
            Console.WriteLine($"{e.Channel.Name} cleared by {e.User.Name} at {DateTime.Now}");
        }
    }
}
