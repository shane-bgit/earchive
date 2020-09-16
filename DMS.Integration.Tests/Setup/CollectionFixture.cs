using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DMS.Integration.Tests.Setup
{
    [CollectionDefinition("api")]
    public class CollectionFixture : ICollectionFixture<TestContext>
    {

    }
}
