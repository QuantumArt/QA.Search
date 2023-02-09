namespace QA.Search.Admin
{
    public class Settings
    {
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
