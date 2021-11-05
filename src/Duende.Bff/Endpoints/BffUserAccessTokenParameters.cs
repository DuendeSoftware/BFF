namespace Duende.Bff
{
    /// <summary>
    /// Additional optional parameters for a user access token request
    /// </summary>
    public class BffUserAccessTokenParameters
    {
        public BffUserAccessTokenParameters(string signInScheme = null, string challengeScheme = null,
            bool forceRenewal = false, string resource = null)
        {
            SignInScheme = signInScheme;
            ChallengeScheme = challengeScheme;
            ForceRenewal = forceRenewal;
            Resource = resource;
        }

        /// <summary>
        /// Overrides the default sign-in scheme. This information may be used for state management.
        /// </summary>
        public string SignInScheme { get; }

        /// <summary>
        /// Overrides the default challenge scheme. This information may be used for deriving token service configuration.
        /// </summary>
        public string ChallengeScheme { get; }

        /// <summary>
        /// Force renewal of token.
        /// </summary>
        public bool ForceRenewal { get; }

        /// <summary>
        /// Specifies the resource parameter.
        /// </summary>
        public string Resource { get; set; }
    }
}