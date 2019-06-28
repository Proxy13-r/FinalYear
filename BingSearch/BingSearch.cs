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
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;

namespace SimpleEchoBot.BingSearch
{
    public class BingSearch : IDialog<object>
    {
        public async static Task SearchUlsterAsync(IDialogContext context, string key, string query)//gets the query from ResumeAfterEnteringQuery. Uses NuGet packages
        {
            IWebSearchAPI client = new WebSearchAPI(new Microsoft.Azure.CognitiveServices.Search.WebSearch.ApiKeyServiceClientCredentials(key));//buils new api search
            var result = await client.Web.SearchAsync(query:query, count: 5, safeSearch: Microsoft.Azure.CognitiveServices.Search.WebSearch.Models.SafeSearch.Strict);//settings for result amount and safe search
            
            if (result?.WebPages?.Value?.Count > 0)
            {
                await context.PostAsync($"Here are top 5 web search results for **{query}**");//returns this message to user
                foreach (var item in result.WebPages.Value)//runs a loop for 5 items
                {
                    HeroCard card = new HeroCard//creates new hero card
                    {
                        Title = item.Name,//sets title from resultWebPages.Value
                        Text = item.Snippet,//gets text from search results
                        Buttons = new List<CardAction>//creates button
                        {
                            new CardAction(ActionTypes.OpenUrl, "Open Page", value: item.Url)//sets url for button to open
                        }
                    };

                    var message = context.MakeMessage();//creates the message
                    message.Attachments.Add(card.ToAttachment());//adds attachement to the message
                    await context.PostAsync(message);//posts message
                }
            }
        }

      
        
        public async static Task SearchImageAsync(IDialogContext context, string key, string query)
        {
            IImageSearchAPI client = new ImageSearchAPI(new Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials(key));
            var result = await client.Images.SearchAsync(query: query, count: 10, safeSearch: Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models.SafeSearch.Strict);

            if (result?.Value?.Count > 0)
            {
                await context.PostAsync($"Here are Top 10 results that match **{query}**");
                var message = context.MakeMessage();
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                foreach (var item in result.Value)
                {
                    HeroCard card = new HeroCard
                    {
                        Title = item.Name,
                        Images = new List<CardImage>
                        {
                            new CardImage(item.ContentUrl)
                        },
                        Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.OpenUrl, "View Image", value: item.ContentUrl)
                        }
                    };
                    message.Attachments.Add(card.ToAttachment());
                }
                await context.PostAsync(message);
            }
        }


       
 
        Task IDialog<object>.StartAsync(IDialogContext context)
        {
            throw new NotImplementedException();
        }
    }
}