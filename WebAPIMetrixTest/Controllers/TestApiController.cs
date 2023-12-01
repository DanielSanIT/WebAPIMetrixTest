using Microsoft.AspNetCore.Mvc;
using WebAPIMetrixTest.Services;

namespace WebAPIMetrixTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestApiController : ControllerBase
    {
        private readonly ILogger<TestApiController> _log;
        private readonly MetricsService _metrics;

        public TestApiController(ILogger<TestApiController> logger, MetricsService metricsService)
        {
            _log = logger;
            _metrics = metricsService;
        }

        [HttpGet]
        [Route("hello/{id}/get")]
        public string GetHello(string id)
        {
            //Usual counter with tag
            _metrics.ApiCallCounter.Add(1, new KeyValuePair<string, object?>("name", "hello"));
            //_log.LogInformation("Send error");
            return $"Hello Guid: {id}";
        }

        [HttpGet]
        [Route("bye")]
        public string Getbye()
        {
            //Usual counter with other tag
            _metrics.ApiCallCounter.Add(1, new KeyValuePair<string, object?>("name", "bye"));
            _log.LogInformation("Send error");
            return "Bye!";
        }

        [HttpPut]
        [Route("error")]
        public string PutError()
        {
            // error without activity
            try
            {
                throw new Exception("Oops!");
            }
            catch (Exception ex)
            {
                //throw;
            }
            return $"throw new Exception(\"Oops!\")";
        }

        [HttpPut]
        [Route("error2")]
        public string PutError2()
        {
            // error with activity
            try
            {
                throw new Exception("Oops!");
            }
            catch (Exception ex)
            {
                //_metricsService.activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                //throw;
            }
            return $"throw new Exception(\"Oops!\")";
        }

        [HttpPut]
        [Route("random")]
        public string PutRandom()
        {
            // record random number
            int random = Random.Shared.Next(0, 100);
            _metrics.SetRandomIntGauge(random);
            _log.LogInformation("Say hello done");
            return $"Random will be {random}!";
        }

        [HttpPut]
        [Route("randomNumber")]
        public string PutRandomNumber()
        {
            // record random number
            int random = Random.Shared.Next(0, 100);
            _metrics.RndNumberHistogram.Record(random);
            return $"Random will be {random}!";
        }

        [HttpPut]
        [Route("randomTimer")]
        public string PutRandomTimer()
        {
            int random = Random.Shared.Next(0, 20000);

            using(MetricsService.BeginTimedOperation(_metrics.RndTimeHistogram, MetricsService.TimeMetricType.Seconds))
            {
                Thread.Sleep(random);
            }
            return $"Random will be {random}!";
        }

        [HttpPut]
        [Route("logs")]
        public string PutLogs()
        {
            _log.LogInformation("Say hello done");
            _log.LogInformation("Say hello done");
            _log.LogInformation("Say hello done");
            _log.LogError("Say hello done");
            _log.LogError("Say hello done");
            _log.LogWarning("Say hello done");

            return $"Did put logs!";
        }
    }
}