﻿using System.Net.Http.Headers;

namespace Ethereal.FAF.API.Client
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenProvider TokenProvider;

        public AuthHeaderHandler(ITokenProvider tenantProvider)
        {
            this.TokenProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await TokenProvider.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
    public class VerifyHeaderHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.Query.StartsWith("verify"))
            {
                var verify = request.RequestUri.Query.Split('=')[0];
                request.Headers.Add("Verify", verify);
                request.RequestUri = new Uri(request.RequestUri.AbsolutePath);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
