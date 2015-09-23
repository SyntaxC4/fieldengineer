using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using FieldEngineerLite.Helpers;
using FieldEngineerLite.Models;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Net.Http;
using Xamarin.Forms;
using System.Reflection;

using Microsoft.Azure.AppService;


namespace FieldEngineerLite
{

    public class JobService
    {
        #region Member variables
        
        public bool LoginInProgress = false;
        public bool Online = false;        
        
        #endregion
        
        // 1. add client initializer
        public IMobileServiceClient MobileService = null;
        public static string MobileAppName = "{sitename}";
        public static string GatewayURL = ""; // Not necessary if you're not using Auth
        public static string MobileAppURL = "{siteurl}";
        public static string MobileAppKey = ""; // No longer required by the server
        
        public AppServiceClient AppService = 
            new AppServiceClient(MobileAppURL);
        // 2. add sync table
        private IMobileServiceSyncTable<Job> jobTable;
          
        
        public async Task InitializeAsync()
        {
            this.MobileService = AppService.CreateMobileServiceClient(
                MobileAppURL,
                MobileAppKey);
            // 3. initialize local store

            var store = new MobileServiceSQLiteStore("local-db-" + MobileAppName);
            store.DefineTable<Job>();

            await MobileService.SyncContext.InitializeAsync(store);

            jobTable = MobileService.GetSyncTable<Job>();
        }

        public async Task<IEnumerable<Job>> ReadJobs(string search)
        {
            // 4. read from local db
            
            var query = jobTable.CreateQuery();
            if (string.IsNullOrEmpty(search) == false)
            {
                query = query.Where(job => (job.Title == search));
            }
            
            return await query.ToEnumerableAsync();
        }

        public async Task UpdateJobAsync(Job job)
        {
            job.Status = Job.CompleteStatus;
            
            // 5. update local db
            await jobTable.UpdateAsync(job);
        }

        public async Task SyncAsync()
        {
            //5. add sync
            try
            {
                await this.MobileService.SyncContext.PushAsync();

                var query = jobTable.CreateQuery()
                    .Where(job => job.AgentId == "Carla Davis");

                await jobTable.PullAsync(null, query);
            }
            catch (Exception)
            { 
            }
        }

        public async Task CompleteJobAsync(Job job)
        {
            await UpdateJobAsync(job);

            if (Online)
                await this.SyncAsync();
        }
    }
}
