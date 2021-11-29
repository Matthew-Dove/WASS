namespace Wass.Code.Recipes
{
    public sealed class Recipe
    {
        private readonly InstructionModel[] _instructions;

        public Recipe(InstructionModel[] instructions)
        {
            if (instructions == null || instructions.Length < 1) throw new ArgumentException("Length must be greater than 0.", nameof(instructions));
            foreach (var instruction in instructions) if (instruction == null) throw new ArgumentException("A step cannot be null.", nameof(instruction));

            _instructions = instructions;
        }
    }
}
