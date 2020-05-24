using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Frontend.Models;
using BackendApi;
using Grpc.Net.Client;

namespace Frontend.Controllers
{ 
    public class HomeController : Controller
    {
        private readonly ILogger<Controller> _logger;

        public HomeController(ILogger<Controller> logger)
        {
            _logger = logger;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        async public Task<IActionResult> HandleAddTaskRequest(String description, String text)
        {
            using var channel = GrpcChannel.ForAddress("http://" + Environment.GetEnvironmentVariable("BACKEND_API_HOST") + ":" + Environment.GetEnvironmentVariable("BACKEND_API_PORT"));
           
            var client = new Job.JobClient(channel);
            var reply = await client.RegisterAsync(new RegisterRequest { 
                Description = description,
                Data = text
            });

            return RedirectToAction("Index", "TaskDetails", new { JobId = reply.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
