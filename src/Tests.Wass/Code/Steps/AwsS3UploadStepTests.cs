using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Wass.Code.Persistence.Configuration;
using Wass.Code.Recipes;
using Wass.Code.Recipes.Steps;

namespace Tests.Wass.Code.Steps
{
    [TestClass]
    public class AwsS3UploadStepTests
    {
        private FileModel _file;
        private IngredientModel _ingredients;

        [TestInitialize]
        public void TestInitialize()
        {
            Config.S3.AccessKeyId = "AccessKeyId";
            Config.S3.SecretAccessKey = "SecretAccessKey";

            _file = new FileModel(
                data: new byte[1] { 0 },
                path: @"C:\Backup\Pictures\Family\Christmas.jpg"
            );

            _ingredients = new IngredientModel
            {
                ["storage"] = "glacier"
            };
        }

        [TestMethod]
        public async Task Test()
        {
            var step = new AwsS3UploadStep();
            var result = await step.MethodAsync(_file, _ingredients);
        }
    }
}
