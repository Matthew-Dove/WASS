﻿using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        internal CompressFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => CompressFilePath(ref file);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFilePath(ref FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;

            try
            {
                var locationBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Location));
                var nameBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Name));
                var extensionBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Extension));

                var location = Encoding.UTF8.GetString(locationBytes);
                var name = Encoding.UTF8.GetString(nameBytes);
                var extension = Encoding.UTF8.GetString(extensionBytes);

                file = file.WithLocation(location);
                file = file.WithName(name);
                file = file.WithExtension(extension);

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFilePathStep));
            }

            return isValid.Trail(x => $"Is {nameof(CompressFilePathStep)} Valid: {x}.");
        }
    }
}
