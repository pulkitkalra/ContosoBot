using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using ContosoBot.Controllers;
using Microsoft.Bot.Builder.Dialogs;

namespace ContosoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public static StockLUIS StLUIS;
        public static Boolean favOn;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var userMessage = activity.Text;
                Activity reply;
                string StockRateString;
                StLUIS = await GetEntityFromLUIS(activity.Text);
                if (StLUIS.intents.Count() > 0)
                {
                    switch (StLUIS.intents[0].intent)
                    {
                        // users asks for stock price of particular stock.
                        case "ConvertCurrency":
                            await Conversation.SendAsync(activity, () => new CurrencyCard());
                            break;
                        case "StockPrice":
                            await Conversation.SendAsync(activity, () => new StockCards());
                            break;
                        // user asks for converting particular currency.                        

                        // user wants to set a particular stock as their favourite.
                        case "SetAsFavourite":
                            string favStock = StLUIS.entities[0].entity;
                            userData.SetProperty<string>("FavStock", favStock);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            StockRateString = favStock + " has been set as your favourite stock.\nYou can call it by simply typing 'favourite stock' or similar.\nReset your favourite by typing 'Clear favourite stock' or similar.";
                            reply = activity.CreateReply(StockRateString);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                            favOn = true;
                            break;
                        // users wants to get their favourite stock.
                        case "GetFavourite":
                            string fStock = userData.GetProperty<string>("FavStock");
                            if (fStock == null)
                            {
                                StockRateString = "You have not assigned any stock as favourite.";
                                reply = activity.CreateReply(StockRateString);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else
                            {
                                favOn = true;
                                StockCards.favStock = fStock;
                                await Conversation.SendAsync(activity, () => new StockCards());
                            }
                            break;
                        // users wants to clear favourite stock.
                        case "ClearFavourite":
                            await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                            StockRateString = "Your favourite stock has been cleared.";
                            reply = activity.CreateReply(StockRateString);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                            favOn = false;
                            break;
                        // user wants to do something that is not supported or understood.
                        default:
                            StockRateString = "Sorry, I am not getting you...";
                            reply = activity.CreateReply(StockRateString);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                            break;
                    }
                }
                else
                {
                    StockRateString = "Sorry, I am not getting you...";
                    reply = activity.CreateReply(StockRateString);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private static async Task<StockLUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            StockLUIS Data = new StockLUIS();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v2.0/apps/4795ab7b-b2b9-413a-8e2b-92505c976e3f?subscription-key=f6ea8956f36b4379bcab23f342bfb460&q=" + Query
                    + "&verbose=true";

                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<StockLUIS>(JsonDataResponse);
                }
            }
            return Data;
        }
    }


}