// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.UnitTest.Infrastructure;

using RichardSzalay.MockHttp;

internal class MockHttpClientFactory : IHttpClientFactory
{
    private readonly MockHttpMessageHandler mockHttpMessageHandler;

    public MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler)
    {
        this.mockHttpMessageHandler = mockHttpMessageHandler;
    }

    public HttpClient CreateClient(string name)
    {
        return this.mockHttpMessageHandler.ToHttpClient();
    }
}
