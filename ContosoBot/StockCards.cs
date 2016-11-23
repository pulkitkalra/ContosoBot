﻿using ContosoBot.Controllers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoBot
{
    [Serializable]
    public class StockCards : IDialog
    {
        public static String favStock;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(ActivityReceivedAsync);
        }

        private async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var reply = activity.CreateReply();
            reply.Attachments = new List<Attachment>();
            // code added below due to unexpected behaviour in MessageController.cs. Will need to consider this at a later date.
            if (MessagesController.StLUIS.intents[0].intent.Equals("ConvertCurrency"))
            {
                CurrencyCard c = new CurrencyCard();
                await c.ActivityReceivedAsync(context, result);
            }
            else
            {
                String stock;
                if (MessagesController.favOn == true)
                {
                    if ((stock = await Stock.GetStock(favStock)) == null)
                    {
                        displayInvalidMsg(reply);
                    }
                    else
                    {
                        displayStock(stock, reply);
                    }
                    MessagesController.favOn = false;
                }
                else if (MessagesController.StLUIS.entities[0].entity == null)
                {
                    HeroCard hc = new HeroCard()
                    {
                        Title = "Sorry, I'm not sure I understand.",
                    };

                    reply.Attachments.Add(hc.ToAttachment());
                }
                else if ((stock = await Stock.GetStock(MessagesController.StLUIS.entities[0].entity)) == null)
                {
                    displayInvalidMsg(reply);
                }
                else
                {
                    displayStock(stock, reply);
                }
                await context.PostAsync(reply);
                context.Wait(ActivityReceivedAsync);
            }
        }

        public void displayStock(String stock, Activity reply)
        {
            String name = stock.Split(',')[0];
            String value = stock.Split(',')[1];

            List<CardImage> images = new List<CardImage>();
            List<CardAction> cardAction = new List<CardAction>();

            CardImage ci = new CardImage("http://chart.finance.yahoo.com/z?s=" + name);
            images.Add(ci);
            CardAction ca = new CardAction()
            {
                // card Action is for button to view more info on stock.
                Title = "More Information",
                Type = "openUrl",
                Value = "https://nz.finance.yahoo.com/q?s=" + name
            };
            cardAction.Add(ca);
            HeroCard tc = new HeroCard()
            {
                Buttons = cardAction,
                Title = name,
                Subtitle = value,
                Images = images,

            };
            reply.Attachments.Add(tc.ToAttachment());
        }

        public void displayInvalidMsg(Activity reply)
        {
            HeroCard hc = new HeroCard()
            {
                Title = "Invalid Stock Name!",
                Subtitle = "Please ensure you use the correct Stock Name symbol.\nE.g. use msft for Microsoft Stock"
            };
            // could add invalid image icon.
            /*List<CardImage> images = new List<CardImage>();
            CardImage ci = new CardImage();
            images.Add(ci);
            hc.Images = images;*/
            reply.Attachments.Add(hc.ToAttachment());
        }
    }
}