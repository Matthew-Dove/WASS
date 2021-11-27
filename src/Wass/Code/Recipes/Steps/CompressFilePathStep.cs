﻿using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        internal CompressFilePathStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => CompressFilePath(file);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFilePath(FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;

            try
            {
                var directoryBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Directory));
                var nameBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Name));
                var extensionBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Extension));

                var directory = Encoding.UTF8.GetString(directoryBytes);
                var name = Encoding.UTF8.GetString(nameBytes);
                var extension = Encoding.UTF8.GetString(extensionBytes);

                file = file
                    .WithDirectory(directory)
                    .WithName(name)
                    .WithExtension(extension);

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
