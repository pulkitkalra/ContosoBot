using ContosoBot.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContosoBot
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Timeline> timelineTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://msatrial2.azurewebsites.net");
            this.timelineTable = this.client.GetTable<Timeline>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddTimeline(Timeline timeline)
        {
            await this.timelineTable.InsertAsync(timeline);
        }

        public async Task RemoveTimeline(Timeline timeline)
        {
            await this.timelineTable.DeleteAsync(timeline);
        }

        public async Task<List<Timeline>> GetTimelines()
        {
            return await this.timelineTable.ToListAsync();
        }

        public async Task UpdateTimeline(JObject jObject)
        {
            await this.timelineTable.UpdateAsync(jObject);
        }
    }
}