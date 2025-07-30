using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Postman.Framework
{
    public abstract class BaseIntegrationTest
    {
        protected ILogger _logger;

        protected virtual void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}
