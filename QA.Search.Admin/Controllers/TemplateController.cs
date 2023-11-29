using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class TemplateController : Controller
    {
        private readonly IElasticLowLevelClient _elastic;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public TemplateController(IElasticLowLevelClient elastic, IElasticSettingsProvider elasticSettingsProvider)
        {
            _elastic = elastic;
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(TemplateFile[]), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetTemplates()
        {
            IDictionary<string, JRaw> newTemplatesByName = await ReadTemplatesAsync();

            var response = await _elastic.Indices.GetTemplateV2ForAllAsync<StringResponse>(_elasticSettingsProvider.GetTemplateMask());

            if (!response.Success)
            {
                if (response.HttpStatusCode == 404)
                    return Ok(Array.Empty<TemplateFile>());

                return StatusCode(500, response.OriginalException.Message);
            }

            IDictionary<string, JToken> oldTempaltesByName = JObject.Parse(response.Body);

            TemplateFile[] templates = newTemplatesByName
                .Select(newPair => new TemplateFile
                {
                    Name = newPair.Key,
                    NewContent = newPair.Value,
                    OldContent = oldTempaltesByName.ContainsKey(newPair.Key)
                        ? oldTempaltesByName[newPair.Key]
                        : null
                })
                .Concat(oldTempaltesByName
                    .Where(oldPair => !newTemplatesByName.ContainsKey(oldPair.Key))
                    .Select(oldPair => new TemplateFile
                    {
                        Name = oldPair.Key,
                        NewContent = null,
                        OldContent = oldPair.Value,
                    }))
                .OrderBy(file => file.Name)
                .ToArray();

            return Ok(templates);
        }

        [HttpPut("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> ApplyTemplate(string templateName)
        {
            templateName = _elasticSettingsProvider.GetAlias(templateName);

            IDictionary<string, JRaw> newTemplatesByName = await ReadTemplatesAsync();

            JRaw templateContent = newTemplatesByName[templateName];

            if (templateContent == null)
            {
                return NotFound();
            }

            var response = await _elastic.Indices.PutTemplateV2ForAllAsync<VoidResponse>(
                templateName, templateContent.ToString(Formatting.None));

            if (!response.Success)
            {
                return StatusCode(500, response.OriginalException.Message);
            }

            return Ok();
        }

        [HttpDelete("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeleteTemplate(string templateName)
        {
            templateName = _elasticSettingsProvider.GetAlias(templateName);

            var response = await _elastic.Indices.DeleteTemplateV2ForAllAsync<VoidResponse>(templateName);

            if (!response.Success)
            {
                return StatusCode(500, response.OriginalException.Message);
            }

            return Ok();
        }

        // TODO: handle "synonyms_index" and "synonyms_search" lists
        private async Task<IDictionary<string, JRaw>> ReadTemplatesAsync()
        {
            string templatesPath = Path.Combine(AppContext.BaseDirectory, "ElasticSearch/_template");

            string[] fileNames = Directory.GetFiles(templatesPath, "*.json");

            var tempalteFiles = await Task.WhenAll(fileNames.Select(async name => new
            {
                Name = Path.GetFileNameWithoutExtension(name),
                Content = new JRaw(await System.IO.File.ReadAllTextAsync(name)),
            }));

            return tempalteFiles.ToDictionary(file => file.Name, file => file.Content);
        }

        public class TemplateFile
        {
            public string Name { get; set; }
            public JToken OldContent { get; set; }
            public JToken NewContent { get; set; }
        }
    }
}