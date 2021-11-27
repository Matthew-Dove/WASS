using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Wass.Code.Persistence.Aws;

namespace Tests.Wass.Code.Persistence.Aws
{
    [TestClass]
    public class S3Tests
    {
        [TestMethod]
        public async Task MyTest()
        {
            var result = await S3.Upload(null, null, null, null);
        }
    }
}
