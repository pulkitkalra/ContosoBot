using ContosoBot.Controllers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ContosoBot
{
    [Serializable]
    public class CurrencyCard : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(ActivityReceivedAsync);
        }

        public async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var reply = activity.CreateReply();
            reply.Attachments = new List<Attachment>();
            // Update LUIS to make the difference between convertFrom and convertTo.
            String convertFrom = "";
            String convertTo = "";
            Decimal amount = 0;
            for (int i =0; i <3; i++)
            {
                String typeC = MessagesController.StLUIS.entities[i].type;
                String entity = MessagesController.StLUIS.entities[i].entity;
                if (typeC.Equals("MoneyAmount"))
                {
                    try
                    {
                        amount = Decimal.Parse(Regex.Replace(entity, @"\s+", "")); // removing whitespace from entity to convert to decimal.
                    } catch
                    {
                        employError(reply, context);
                    }
                    
                } else if (typeC.Equals("convertFrom"))
                {
                    convertFrom = entity;
                } else if (typeC.Equals("convertTo"))
                {
                    convertTo = entity;
                }
            }

            List<CardImage> images = new List<CardImage>();
            CardImage ci = new CardImage("https://cdn.pixabay.com/photo/2012/04/11/17/27/money-29047_960_720.png");
            images.Add(ci);
            String display = "";
            ThumbnailCard hc = new ThumbnailCard();
            try
            {
                display = CurrencyConvert(amount, convertFrom, convertTo);
                hc = new ThumbnailCard()
                {
                    Title = "Convert: " + amount + " " + convertFrom + " to " + convertTo + ":\n" + display,
                    Images = images
                };
                reply.Attachments.Add(hc.ToAttachment());

            } catch 
            {
                employError(reply, context);
            }

            await context.PostAsync(reply);
            context.Wait(ActivityReceivedAsync);
        }

        public static string CurrencyConvert(decimal amount, string fromCurrency, string toCurrency)
        {
            //Grab your values and build your Web Request to the API
            string apiURL = String.Format("https://www.google.com/finance/converter?a={0}&from={1}&to={2}&meta={3}", amount, fromCurrency, toCurrency, Guid.NewGuid().ToString());

            //Make your Web Request and grab the results
            var request = WebRequest.Create(apiURL);

            //Get the Response
            var streamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.ASCII);

            //Grab your converted value (ie 2.45 USD)
            var result = Regex.Matches(streamReader.ReadToEnd(), "<span class=\"?bld\"?>([^<]+)</span>")[0].Groups[1].Value;

            //Get the Result
            return result;
        }

        public async void employError(Activity reply, IDialogContext context)
        {
            HeroCard card = new HeroCard()
            {
                Title = "Sorry, I'm not sure I understand.",
                Subtitle = "Pls. ensure you use correct currency symbols and valid numerals."
            };
            reply.Attachments.Add(card.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(ActivityReceivedAsync);
        }
    }

}