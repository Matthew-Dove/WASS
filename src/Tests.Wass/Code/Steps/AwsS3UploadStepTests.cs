using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using Wass.Code.Recipes;
using Wass.Code.Recipes.Steps;

namespace Tests.Wass.Code.Steps
{
    [TestClass]
    public class AwsS3UploadStepTests
    {
        // Credit: Billy Jo Catbagan (https://unsplash.com/photos/PbS9rXhsYIU).
        private readonly string _path = @"Data/billy-jo-catbagan-PbS9rXhsYIU-unsplash.jpg";
        private readonly string _bucket = Path.GetRandomFileName().Replace(".", "").ToLower();

        private FileModel _file;
        private IngredientModel _ingredients;

        [TestInitialize]
        public void TestInitialize()
        {
            var data = File.ReadAllBytes($"./{_path}");
            _file = new FileModel(
                data: data,
                path: _path
            );

            _ingredients = new IngredientModel
            {
                ["storage"] = "standard",
                ["bucket"] = _bucket
            };
        }

        [Ignore]
        [TestMethod]
        public async Task Test()
        {
            var step = new AwsS3UploadStep();
            var result = await step.MethodAsync(_file, _ingredients);
            Assert.IsTrue(result);
        }
    }
}
