using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wass.Code.Recipes;
using Wass.Code.Recipes.Steps;

namespace Tests.Wass.Code.Steps
{
    [TestClass]
    public class SetFilePathStepTests
    {
        private FileModel _file;
        private IngredientModel _ingredients;

        [TestInitialize]
        public void TestInitialize()
        {
            _file = new FileModel(
                data: new byte[1] { 0 },
                location: @"C:\Backup\Pictures\Family",
                name: @"Christmas",
                extension: "jpg"
            );

            _ingredients = new IngredientModel {
                ["path"] = @"\\NETWORK\User\My Docs\doc001.txt"
            };
        }

        [TestMethod]
        public void Test()
        {
            var step = new SetFilePathStep();
            step.Method(ref _file, _ingredients);
        }
    }
}

