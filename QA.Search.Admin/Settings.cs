using System;

namespace QA.Search.Admin
{
    public class Settings
    {
        /// <summary>
        /// Semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public string ElasticSearchUrl { get; set; }

        /// <summary>
        /// Prefix for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexPrefix { get; set; } = "";

        /// <summary>
        /// Wildcard mask for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexMask => IndexPrefix + "*";

        /// <summary>
        /// Prefix for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasPrefix { get; set; } = "";

        /// <summary>
        /// Wildcard mask for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasMask => AliasPrefix + "*";

        /// <summary>
        /// Prefix for all ElasticSearch mapping templates that used by Search App
        /// </summary>
        public string TemplatePrefix { get; set; } = "";

        /// <summary>
        /// Wildcard mask for all ElasticSearch mapping templates that used by Search App
        /// </summary>
        public string TemplateMask => TemplatePrefix + "*";

        /// <summary>
        /// Host for Search.Admin application (used in email)
        /// </summary>
        public string AdminAppUrl { get; set; }

        /// <summary>
        /// Invite user message subject
        /// </summary>
        public string InviteUserMessageSubject { get; set; }

        /// <summary>
        /// Invite user message body template
        /// </summary>
        public string InviteUserMessageBodyTemplate { get; set; }

        /// <summary>
        /// Reset password message subject
        /// </summary>
        public string ResetPasswordMessageSubject { get; set; }

        /// <summary>
        /// Reset password message body template
        /// </summary>
        public string ResetPasswordMessageBodyTemplate { get; set; }

        /// <summary>
        /// Indexes, which can't be managed from amdin UI
        /// </summary>
        public string[] ReadonlyPrefixes { get; set; }
    }
}
