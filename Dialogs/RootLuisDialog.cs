namespace SimpleEchoBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Azure.CognitiveServices.Search.EntitySearch;
    using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
    using Newtonsoft.Json;
    using SimpleEchoBot.BingSearch;

    [LuisModel("f389e196-9a58-4215-b31d-c41f66030c9c", "895b47d9df6049309b967e94a628e68a")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>, IDialog<object>
    {
        //Variables and API keys
        private const string CSSEy1 = "1.Computing Science/Software Engineering 1st Year";
        private const string CSy2 = "2.Computing Science 2rd Year";
        private const string SEy2 = "3.Software Engineering 2nd Year";
        private const string CSfinal = "4.Computing Sience Final Year";
        private const string SEfinal = "5.Software Engineering Final Year";
        private IEnumerable<string> options = new List<string> { CSSEy1, CSy2, SEy2, CSfinal, SEfinal };
        public DateTime closestTime { get; private set; }
        private string searchType = string.Empty;
        private string query = string.Empty;
        private const string BING_KEY = "63ae4a3764aa48a6bb4702a5f0ed8b91";
        private const string searchWeb = "Search Course Information";
        private const string searchImage = "Search Images or Gifs";
        
        
        //Luis intent when a query does not match any other intent
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";//returns this message with query text entered by user

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

       
        //Luis Intent when user greets the bot
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {

            string[] greetings = "Hi. How can I help you?|Hello|Hey there!|Hello. How can I help you?|Hi. Need anything?".Split('|');//Random array for greeting replies
            Random rnd = new Random();
            string rndGreet = greetings[rnd.Next(greetings.Length)];

            await context.PostAsync("" + rndGreet);//message built with empty string and rndGreet variable containing random string

            //  context.Wait(this.MessageReceived);

            string[] links = "bear.gif|jsnow.gif|obiwankenobi.gif|whale.gif|youngkenobi.gif".Split('|');//Random array for gifs
            Random rndLink = new Random();
            string rndGif = links[rnd.Next(links.Length)];


            var HiGif = new HeroCard
            {

                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/gifstorage/" + rndGif) }
            };

            var message = context.MakeMessage();//creates the message
            message.Attachments.Add(HiGif.ToAttachment());//adds attachement to the message
            await context.PostAsync(message);//posts message


        }

        [LuisIntent("Myself")]
        public async Task Myself(IDialogContext context, LuisResult result)
        {

            string[] extras = "I don't bite:)|Don't worry, Im not related to Skynet.|Nothing else to say.|I could be smartest AI in the world.|Now you know everything about me.".Split('|');
            Random rnd = new Random();
            string rndExtra = extras[rnd.Next(extras.Length)];

            await context.PostAsync("I am a chatbot developed as Final Year Project of Computing Science Course. "+rndExtra);

            context.Wait(this.MessageReceived);


        }

        [LuisIntent("BusTimetable")]
        public async Task TrainTimetable(IDialogContext context, LuisResult result)
        {

            var TrainImg = new HeroCard
            {

                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/busTimetable.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"https://www.translink.co.uk/Documents/Services/Students/6234j%20Unilink%20Timetable%20Station%20Poster.pdf") }
            };

            var message = context.MakeMessage();//creates the message
            message.Attachments.Add(TrainImg.ToAttachment());//adds attachement to the message
            await context.PostAsync(message);//posts message


        }

        //Luis Help intent. Returns help message 
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<object> result, LuisResult result1)
        {
            await context.PostAsync("Hi! Try asking me or tell me these: 'When is next bus?', 'Show me lecture timetable' or 'Search'");

            context.Wait(this.MessageReceived);
          

        }
        //Luis intent where user asks for a bus time without specifying which bus.
        [LuisIntent("BusGeneral")]
        public async Task BusGeneral(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            
            await context.PostAsync("Which bus, University or Botanic?");
            context.Wait(this.MessageReceived);


        }


        //Luis Intent for Botanic bus times
        [LuisIntent("BusBotanic")]
        public async Task BusBotanic(IDialogContext context, LuisResult result)
        {
            var busTimesBot124 = new string[]//Array with bus times for Monday,Tuesday and Thursday
            {

            "08:05",
            "08:20",
            "08:30",
            "08:40",
            "08:45",
            "08:50",
            "09:00",
            "09:15",
            "09:20",
            "09:30",
            "09:35",
            "09:45",
            "10:00",
            "10:15",
            "10:30",
            "11:00",
            "11:30",
            "11:45",
            "12:00",
            "12:30",
            "12:45",
            "13:00",
            "13:30",
            "14:00",
            "14:15",
            "14:30",
            "15:15",
            "17:15",
            "17:50",
            "18:40",
           
            }
            .Select(x => DateTime.Parse(x))//Parse as time
            .ToList();

            var busTimesBot35 = new string[]//Array for Wednesday and Friday bus times
           {
            "08:05",
            "08:20",
            "08:30",
            "08:40",
            "08:45",
            "08:50",
            "09:00",
            "09:15",
            "09:20",
            "09:30",
            "09:35",
            "09:45",
            "10:00",
            "10:15",
            "10:30",
            "11:00",
            "11:30",
            "11:45",
            "12:00",
            "12:30",
            "12:45",
            "13:00",
            "14:00",
            "14:15"

           }
           .Select(x => DateTime.Parse(x))//Parse as string as time
           .ToList();

            
            var now = DateTime.Now.TimeOfDay;//get current time
            DayOfWeek today = DateTime.Today.DayOfWeek;//get current week day

            if (today == DayOfWeek.Monday || today == DayOfWeek.Tuesday || today == DayOfWeek.Thursday)//checks if today is any of these day and runs if it is
            {
                DateTime closestTime = (from i in busTimesBot124
                                        where i.TimeOfDay > now
                                        orderby i.TimeOfDay ascending
                                        select i).First();

                await context.PostAsync("Next bus leaves Botanic at: " + closestTime);//displays that time

                context.Wait(this.MessageReceived);
            }

            else
                if (today == DayOfWeek.Wednesday || today == DayOfWeek.Friday)//checks for day
            {
               closestTime = (from i in busTimesBot35
                               where i.TimeOfDay > now
                               orderby i.TimeOfDay ascending
                               select i).First();//sort for closest time

            
    

                await context.PostAsync("Next bus leaves Botanic at: "+closestTime );// returns this message if matched

                context.Wait(this.MessageReceived);
            }
            else//else returns that there is no service
            {
                await context.PostAsync("No Bus Service Saturday or Sunday");

                context.Wait(this.MessageReceived);
            }



          
        }

        [LuisIntent("BusUniversity")]
        public async Task BusJordanstown(IDialogContext context, LuisResult result)
        {
            var busTimesUni124 = new string[]
            {

            "08:50",
            "09:05",
            "09:30",
            "09:45",
            "10:15",
            "10:45",
            "11:15",
            "11:45",
            "12:15",
            "12:45",
            "13:15",
            "13:30",
            "13:45",
            "14:15",
            "14:50",
            "15:00",
            "15:15",
            "16:00",
            "16:20",
            "16:35",
            "16:55",
            "17:10",
            "17:30",
            "18:00",
            "19:10"
            }
            .Select(x => DateTime.Parse(x))
            .ToList();

            var busTimesUni35 = new string[]
           {
            "08:50",
            "09:05",
            "09:30",
            "09:45",
            "10:15",
            "10:45",
            "11:15",
            "11:45",
            "12:15",
            "12:45",
            "13:15",
            "13:30",
            "13:45",
            "14:15",
            "14:50",
            "15:00",
            "16:35"
           
           }
           .Select(x => DateTime.Parse(x))
           .ToList();

            var now = DateTime.Now.TimeOfDay;
            DayOfWeek today = DateTime.Today.DayOfWeek;

            if (today == DayOfWeek.Monday || today == DayOfWeek.Tuesday || today == DayOfWeek.Thursday)
            {
                closestTime = (from i in busTimesUni124
                               where i.TimeOfDay > now
                               orderby i.TimeOfDay ascending
                               select i).First();
              

                await context.PostAsync("Next bus leaves University at: " + closestTime);

                context.Wait(this.MessageReceived);
            }

            else
                if (today == DayOfWeek.Wednesday || today == DayOfWeek.Friday)
            {
                closestTime = (from i in busTimesUni35
                               where i.TimeOfDay > now
                               orderby i.TimeOfDay ascending
                               select i).First();

                await context.PostAsync("Next bus leaves University at: " +closestTime);

                context.Wait(this.MessageReceived);
            }
            else
            {
                await context.PostAsync("No Bus Service Saturday or Sunday ");

                context.Wait(this.MessageReceived);
            }



            
        }

      //Intent and Dialog for search selection
        [LuisIntent("Search")]
        public async Task MessageReceivedAsync(IDialogContext context, LuisResult result)
        {
            PromptDialog.Choice(
                context: context,
                resume: ResumeAfterSearchTypeSelecting,//resumes another function
                prompt: "Select Search Type",//shows this message to the user
                options: new List<string>{

                searchWeb,
               
                searchImage
                //gives user two selections

            });
        }



        public async Task ResumeAfterSearchTypeSelecting(IDialogContext context, IAwaitable<string> result)//starts when selection for search type is made
        {
            searchType = (await result) as string;//gets user query as string
            PromptDialog.Text(
                context: context,
                resume: ResumeAfterEnteringQuery,//resumes at another function
                prompt: "Enter Your Query");// prompts user to enter query
        }


        public async Task ResumeAfterEnteringQuery(IDialogContext context, IAwaitable<string> result)
        {
            query = (await result) as string;//gets the user querry 
            switch (searchType)
            {


                case searchWeb:
                    {
                        await BingSearch.SearchUlsterAsync(context, BING_KEY, query+ " site:ulster.ac.uk");//passes query to BingSearch Class
                        break;
                    }

                case searchImage:
                    {
                        await BingSearch.SearchImageAsync(context, BING_KEY, query);
                        break;
                    }


            }
         
        }

        //Lecture timetable selection
        [LuisIntent("LectureTimetableMenu")]
        public async virtual Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result1, LuisResult result)
        {
            var message = await result1;

            PromptDialog.Choice<string>(
                context,
                this.DisplaySelectedCard,
                this.options,
                "Which timetable would you like to see?",
                "Wrong Selection. Please Try Again.",
                3,
                PromptStyle.PerLine);
        }

        public async Task DisplaySelectedCard(IDialogContext context, IAwaitable<string> result)
        {
            var selectedCard = await result;//selects card based on what user enter

            var message = context.MakeMessage();//makes message

            var attachment = GetSelectedCard(selectedCard);
            message.Attachments.Add(attachment);//adds attachement to message

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        private static Attachment GetSelectedCard(string selectedCard)//case for card selection
        {
            switch (selectedCard)
            {
                case CSSEy1:
                    return GetCSSEy1();

                case CSy2:
                    return GetCSy2();

                case SEy2:
                    return GetSEy2();

                case CSfinal:
                    return GetCSfinal();

                case SEfinal:
                    return GetSEfinal();


                default:
                    return GetCSfinal();
            }
        }

        private static Attachment GetCSSEy1()//card with image and url
        {
            var CSSEy1 = new HeroCard
            {
                Title = "Computing Science/Software Engineering",
                Subtitle = "First Year",
                Text = "Timetable",
                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/CSSEy1.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"http://faccompeng.ulster.ac.uk/timetables/servefile.php?filename=\scm\ft_undergrad\bsc_hons_compsci_beng_hons_seng_yr1.htm") }
            };

            return CSSEy1.ToAttachment();
        }

        private static Attachment GetCSy2()
        {
            var CSy2 = new HeroCard
            {
                Title = "Computing Science",
                Subtitle = "Second Year",
                Text = "Timetable",
                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/CSy2.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"http://faccompeng.ulster.ac.uk/timetables/servefile.php?filename=\scm\ft_undergrad\bsc_hons_compsci_yr2.htm") }
            };

            return CSy2.ToAttachment();
        }

        private static Attachment GetSEy2()
        {
            var SEy2 = new HeroCard
            {
                Title = "Software Engineering",
                Subtitle = "Second Year",
                Text = "Timetable",
                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/SEy2.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"http://faccompeng.ulster.ac.uk/timetables/servefile.php?filename=\scm\ft_undergrad\bsc_hons_seng_yr2.htm") }
            };

            return SEy2.ToAttachment();
        }

        private static Attachment GetCSfinal()
        {
            var CSfinal = new HeroCard
            {
                Title = "Computing Science",
                Subtitle = "Final Year",
                Text = "Timetable",
                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/CSfinal.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"http://faccompeng.ulster.ac.uk/timetables/servefile.php?filename=\scm\ft_undergrad\bsc_hons_compsci_finalyr.htm") }
            };

            return CSfinal.ToAttachment();
        }

        private static Attachment GetSEfinal()
        {
            var SEfinal = new HeroCard
            {
                Title = "Software Engineering",
                Subtitle = "Final Year",
                Text = "Timetable",
                Images = new List<CardImage> { new CardImage(@"https://liarabot819e.blob.core.windows.net/liara-images/SEfinal.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Open", value: @"http://faccompeng.ulster.ac.uk/timetables/servefile.php?filename=\scm\ft_undergrad\bsc_hons_seng_finalyr.htm") }
            };

            return SEfinal.ToAttachment();
        }

       

    }
}
