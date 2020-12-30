using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stexchange.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    static class ControllerContextFactory
    {
        private static HttpContext CreateHttpContext(string value, bool isCookieCreated = true)
        {
            //mock the response
            var responseMock = new Mock<HttpResponse>();
            var resCookies = new Mock<IResponseCookies>();
            responseMock.Setup(res => res.Cookies).Returns(resCookies.Object);
            var response = responseMock.Object;

            //mock the request
            var requestMock = new Mock<HttpRequest>();
            var reqCookies = new Mock<IRequestCookieCollection>();
            if (isCookieCreated)
            {
                response.Cookies.Append(StexChangeController.Cookies.SessionToken, value);
                reqCookies.Setup(cookies => cookies.TryGetValue(StexChangeController.Cookies.SessionToken, out value)).Returns(true);
            }
            requestMock.Setup(req => req.Cookies).Returns(reqCookies.Object);
            var request = requestMock.Object;

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response).Returns(response);
            httpContextMock.Setup(c => c.Request).Returns(request);
            return httpContextMock.Object;
        }
        public static ControllerContext CreateControllerContext(string cookieVal, bool isCookieCreated = true)
        {
            return new ControllerContext()
            {
                HttpContext = CreateHttpContext(cookieVal, isCookieCreated)
            };
        }
    }
}
