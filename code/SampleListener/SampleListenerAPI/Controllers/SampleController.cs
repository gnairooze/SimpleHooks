﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SampleListenerAPI.Data;
using SampleListenerAPI.Models;
using System.Configuration;

namespace SampleListenerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController(SampleDbContext contextDb, IConfiguration configuration) : ControllerBase
    {
        private readonly SampleDbContext _context = contextDb;
        private readonly IConfiguration _configuration = configuration;


        [HttpPost]
        public IActionResult Post(JObject jEventData)
        {
            #region add api name to the event data
            string? apiName = _configuration.GetSection("ApiName").Value;

            jEventData.Add("ApiName", apiName);
            #endregion

            SampleModel sampleModel = new()
            {
                CorrelationId = (string) jEventData.SelectToken("simpleHooksMetadata.eventBusinessId")!,
                EventName = (string) jEventData.SelectToken("simpleHooksMetadata.eventDefinitionName")!,
                EventData = jEventData.ToString()
            };

            _context.SampleModels.Add(sampleModel);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("api is running successfully");
        }

    }
}
