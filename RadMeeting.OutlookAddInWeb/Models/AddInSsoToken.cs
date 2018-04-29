﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE in the project root for license information.
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace RedMeeting.OutlookAddInWeb.Models
{
    /// <summary>
    /// Represents an add-in SSO token.
    /// </summary>
    public class AddInSsoToken : JwtSecurityToken
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encodedToken">The serialized JWT Token</param>
        public AddInSsoToken(string encodedToken) : base(encodedToken) { }

        /// <summary>
        /// Validates the token
        /// </summary>
        /// <param name="expectedAudience">The valid audience value to check</param>
        /// <returns></returns>
        public async Task<SsoTokenValidationResult> Validate(string expectedAudience)
        {
            SsoTokenValidationResult result = new SsoTokenValidationResult();

            // Since add-in SSO tokens are issued by Azure, we can use the
            // well-known OpenID config to get signing keys
            string openIdConfig = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            ConfigurationManager<OpenIdConnectConfiguration> configManager =
                new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfig, new OpenIdConnectConfigurationRetriever());

            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();

            // Issuer will always be Azure, but it will contain the tenant ID of the
            // Office 365 organization the user belongs to. We can get that from the "tid" claim
            var tenantIdClaim = Claims.FirstOrDefault(claim => claim.Type == "tid");
            if (tenantIdClaim == null)
            {
                result.Message = "Token is malformed, missing tid claim.";
                return result;
            }

            var oidClaim = Claims.FirstOrDefault(claim => claim.Type == "oid");
            if (oidClaim == null)
            {
                result.Message = "Token is malformed, missing oid claim.";
                return result;
            }

            string expectedIssuer = string.Format("https://login.microsoftonline.com/{0}/v2.0", tenantIdClaim.Value);

            // Use System.IdentityModel.Tokens.Jwt library to validate the token
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters tvp = new TokenValidationParameters();

            tvp.ValidateIssuer = true;
            tvp.ValidIssuer = expectedIssuer;
            tvp.ValidateAudience = true;
            tvp.ValidAudience = expectedAudience;
            tvp.ValidateIssuerSigningKey = true;
            tvp.IssuerSigningKeys = config.SigningKeys as IEnumerable<SecurityKey>;
            tvp.ValidateLifetime = true;

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(RawData, tvp, out SecurityToken validatedToken);

                // If no exception, all standard checks passed
                result.IsValid = true;
                result.LifetimeResult = result.SignatureResult = result.AudienceResult = result.IssuerResult = "passed";

                result.ComputedUserId = GenerateUserId(oidClaim.Value, tenantIdClaim.Value);
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                result.AudienceResult = "failed";
                result.Message = ex.Message;
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                result.IssuerResult = "failed";
                result.Message = ex.Message;
            }
            catch (SecurityTokenInvalidLifetimeException ex)
            {
                result.LifetimeResult = "failed";
                result.Message = ex.Message;
            }
            catch (SecurityTokenExpiredException ex)
            {
                result.LifetimeResult = "failed";
                result.Message = ex.Message;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                result.SignatureResult = "failed";
                result.Message = ex.Message;
            }
            catch (SecurityTokenValidationException ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private string GenerateUserId(string userObjectId, string tenantId)
        {
            // Generate a user ID just from the concatenation of the user's object ID
            // and the tenant ID.

            // In a real world scenario if this user ID was going to be used for any reason
            // other than just to correlate Exchange tokens with a backend service, it would
            // be a good idea to secure this with crypto
            return string.Format("{0}@{1}", userObjectId, tenantId);
        }
    }
}