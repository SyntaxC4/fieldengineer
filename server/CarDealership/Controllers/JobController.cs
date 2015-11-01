using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using FieldEngineerLiteService.DataObjects;
using FieldEngineerLiteService.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System;
using System.Diagnostics;

namespace FieldEngineerLiteService.Controllers
{
    public class JobController : TableController<Job>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            JobDbContext context = new JobDbContext();
            DomainManager = new EntityDomainManager<Job>(context, Request, enableSoftDelete: true);
        }

        // GET tables/Job
        public IQueryable<Job> GetAllJobs()
        {
            return Query();
        }

        // GET tables/Job/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Job> GetJob(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Job/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<Job> PatchJob(string id, Delta<Job> patch)
        {
            var updatedJob = await UpdateAsync(id, patch);

            if (updatedJob.Status == "Completed") {
                await SendNotifications(updatedJob);
            }

            return updatedJob;
        }

        // POST tables/Job
        public async Task<IHttpActionResult> PostJob(Job item)
        {
            Job current = await InsertAsync(item);

            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }


        private async Task SendJsonAsync(string requestUri, string key, string value)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var contentBody = new JObject();
            contentBody[key] = value;

            await client.PostAsJsonAsync(requestUri, contentBody);
        }

        private async Task SendNotifications(Job job)
        {
            string slackUri = ConfigurationManager.AppSettings["SLACK_CustomerNotificationsHook"];
            string notifyUri = ConfigurationManager.AppSettings["SendNotificationUri"];

            try {
                var slackString = String.Format("Job for customer {0} was completed: {1}", job.CustomerName, job.Title);
                await SendJsonAsync(slackUri, "text", slackString);

                await SendJsonAsync(notifyUri, "toast", "Your repair request has been closed. Thank you.");
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }

        // DELETE tables/Job/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteJob(string id)
        {
            return DeleteAsync(id);
        }
    }
}