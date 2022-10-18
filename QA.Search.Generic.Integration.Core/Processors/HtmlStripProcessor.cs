using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QA.Search.Generic.Integration.Core.Extensions;
using QA.Search.Generic.Integration.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace QA.Search.Generic.Integration.Core.Processors
{
    /// <summary>
    /// Удаляет из каждого текстового поля JSON-документа:
    /// HTML-разметку, содержимое тегов script и style, HMTL-комментарии
    /// </summary>
    public class HtmlStripProcessor<TMarker> : DocumentProcessor<TMarker>
        where TMarker : IServiceMarker
    {
        private readonly ILogger _logger;

        public HtmlStripProcessor(ILogger<TMarker> logger)
        {
            _logger = logger;
        }

        public override JObject Process(JObject document)
        {
            document.VisitObjects(HandleArticle);

            return document;
        }

        private static RegexOptions RegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private static Regex HtmlTagRegexp = new Regex(@"<[^<>]+>", RegexOptions);
        private static Regex HtmlEntityRegexp = new Regex(@"&(?:[a-z]+|#[0-9]+);", RegexOptions);
        private static Regex LineBreakRegex = new Regex(@"[^\S\r\n]*\r?\n[^\S\r\n]*", RegexOptions);
        private static Regex ParagraphRegex = new Regex(@"\n{2,}", RegexOptions);

        private void HandleArticle(IDictionary<string, JToken> article)
        {
            _logger.LogTrace($"Before HtmlStrip = {article}");

            foreach (var prop in article.ToArray())
            {
                if (prop.Value.Type == JTokenType.String)
                {
                    string input = prop.Value.Value<string>();

                    if (HtmlTagRegexp.IsMatch(input))
                    {
                        article[prop.Key] = StripHtmlTags(WebUtility.HtmlDecode(input));
                    }
                    else if (HtmlEntityRegexp.IsMatch(input))
                    {
                        article[prop.Key] = WebUtility.HtmlDecode(input);
                    }
                }
            }

            _logger.LogTrace($"After HtmlStrip = {article}");
        }

        private string StripHtmlTags(string input)
        {
            try
            {
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(input);

                htmlDoc.DocumentNode
                        .SelectNodes("//comment()")?.ToList()
                        .ForEach(comment => comment.Remove());

                htmlDoc.DocumentNode
                    .SelectNodes("//style")?.ToList()
                    .ForEach(style => style.Remove());

                htmlDoc.DocumentNode
                    .SelectNodes("//script")?.ToList()
                    .ForEach(script => script.Remove());

                htmlDoc.DocumentNode
                    .SelectNodes("//br")?.ToList()
                    .ForEach(br =>
                    {
                        var newLine = htmlDoc.CreateTextNode("\n");
                        br.ParentNode.ReplaceChild(newLine, br);
                        br.Remove();
                    });

                string innerText = htmlDoc.DocumentNode.InnerText;

                return ParagraphRegex.Replace(LineBreakRegex.Replace(innerText, "\n"), "\n\n");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while processing html {Html}", input);

                return input;
            }
        }
    }
}