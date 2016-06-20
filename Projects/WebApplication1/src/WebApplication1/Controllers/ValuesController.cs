using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            DateTime startTime = DateTime.Now;
            //GetRemote2(id).Wait();
            TestAsync(id).Wait();
            TimeSpan elapsedTime = DateTime.Now.Subtract(startTime);
            return "Remote Execution Time" + elapsedTime.ToString();
        }

        public async Task TestAsync(int numRequests)
        {
            object syncLock = new object();
            int successfulCalls = 0;
            int failedCalls = 0;
            string URL = "http://localhost:6286/api/values";
            HttpClient httpClient = new HttpClient();
            Task[] tasks = new Task[numRequests];

            // kick off URL Requests in parallel
            for (int i = 0; i < numRequests; i++)
            {
                tasks[i] = ProcessUrlAsync(httpClient, syncLock, successfulCalls, failedCalls, URL, i);
            }

            for (int i = 0; i < numRequests; i++)
            {
                tasks[i].Wait();
                Debug.WriteLine(DateTime.Now+"Task Completed " +i);
            }
        }

        private async Task ProcessUrlAsync(HttpClient httpClient, object syncLock, int successfulCalls, int failedCalls, string URL, int id)
        {
            Debug.WriteLine(DateTime.Now+"ProcessUrlAsync"+id);
            HttpResponseMessage httpResponse = null;

            try
            {
                Task<HttpResponseMessage> getTask = httpClient.GetAsync(URL);
                httpResponse = await getTask;
                Debug.WriteLine("Success " + id);
                Interlocked.Increment(ref successfulCalls);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed " + id);
                Interlocked.Increment(ref failedCalls);
            }
            finally
            {
                if (httpResponse != null) httpResponse.Dispose();
            }
            /*
            lock (syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
            */
        }

        public async Task GetRemote2(int id)
        {
            for (int i = 1; i <= id; i++)
            {
                string method = "http://localhost:6286/api/values";
                using (var client = new System.Net.Http.HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(method);
                    Console.WriteLine("Get Remote HTTP Response = " + response);
                }
            }
        }


        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
