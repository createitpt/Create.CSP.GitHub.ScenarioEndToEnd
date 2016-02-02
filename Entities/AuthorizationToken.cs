/********************************************************
*                                                        *
*   Copyright (C) Microsoft. All rights reserved.        *
*                                                        *
*********************************************************/

namespace Create.CSP.GitHub.ScenarioEndToEnd.Entities
{
    using System;

    public class AuthorizationToken
    {
        /// <summary>
        /// Captures when the token expires
        /// </summary>
        private DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Constructor for getting an authorization token
        /// </summary>
        /// <param name="access_Token">access token</param>
        /// <param name="expires_in">number of seconds the token is valid for</param>
        public AuthorizationToken(string access_Token, long expires_in)
        {
            this.AccessToken = access_Token;
            this.ExpiresOn = DateTime.UtcNow.AddSeconds(expires_in);
        }

        public AuthorizationToken(string accessToken, DateTime expiresOn)
        {
            this.AccessToken = accessToken;
            this.ExpiresOn = expiresOn;
        }

        /// <summary>
        /// Returns true if the authorization token is near expiry
        /// </summary>
        /// <returns>true if the authorization token is near expiry</returns>
        public bool IsNearExpiry()
        {
            //// if token is expiring in the next minute or expired, return true
            return DateTime.UtcNow > this.ExpiresOn.AddMinutes(-1);
        }
    }
}
