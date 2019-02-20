using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChinaAreas.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var provider = new ChinaAreaProviderFactory().Provider;
            var data = provider.Provide();
            System.IO.File.WriteAllText("data.json", data);
        }
    }
}
