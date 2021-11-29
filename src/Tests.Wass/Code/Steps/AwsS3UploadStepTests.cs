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

        [Ignore]
        [TestMethod]
        public async Task Test()
        {
            var data = File.ReadAllBytes($"./{_path}");
            var file = new FileModel(
                data: data,
                path: _path
            );

            var ingredients = new IngredientModel
            {
                ["storage"] = "standard",
                ["bucket"] = _bucket
            };

            var step = new AwsS3UploadStep();
            var result = await step.MethodAsync(file, ingredients);

            Assert.IsTrue(result);
        }
    }
}
