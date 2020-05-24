using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client;
using BackendApi;

namespace FrontendClient.Controllers
{
    public class TaskDetailsController : Controller
    {
        private readonly ILogger<Controller> _logger;

        public TaskDetailsController(ILogger<Controller> logger)
        {
            _logger = logger;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public async Task<IActionResult> Index(string JobId)
        {
            using var channel = GrpcChannel.ForAddress("http://" + Environment.GetEnvironmentVariable("BACKEND_API_HOST") + ":" + Environment.GetEnvironmentVariable("BACKEND_API_PORT"));

            var client = new Job.JobClient(channel);
            var reply = await client.GetProcessingResultAsync(new GetProcessingResultRequest { Id = JobId });

            ViewBag.Id = JobId;
            ViewBag.Text = reply.Text;
            ViewBag.Description = reply.Description;
            ViewBag.Rank = reply.Rank;
            ViewBag.Status = reply.Status;

            return View("Task");
        }
    }
}