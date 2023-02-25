namespace Wass.Code.Recipes
{
    public sealed class Recipe
    {
        private readonly InstructionModel[] _instructions;

        public Recipe(InstructionModel[] instructions)
        {
            if (instructions == null || instructions.Length < 1) throw new ArgumentException("Length must be greater than 0.", nameof(instructions));
            foreach (var instruction in instructions) if (instruction == null) throw new ArgumentException("An instruction cannot be null.", nameof(instruction));

            _instructions = instructions;
        }

        // TODO: 
        // I think we'll need a internal step to do a few things before saving data to external sources:
        // 1) Convert FileModel into a metadata object + hash.
        // 2) Ask the storage service if the hash already exists, and if so; skip the upload.
        // Interface: bool DoesFileExist(fileMetaData), bool Upload(file);
    }
}
