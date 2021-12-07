namespace Wass.Code.Recipes
{
    /**
     * One or more steps make up a recipe.
     * A recipe controls what happens to the files presented to WASS.
     * You may have more than one recipe running at a time.
     * 
     * For example, you might have a recipe that moves files < 1GB to S3 Standard.
     * Another recipe that moves files >= 1GB to S3 Glacier Deep Archive.
     * A recipe that moves movie files to your media server.
     * 
     * Steps can do more things too, such as: compression, encryption, time delay, watch for new files, etc.
    **/
    public abstract class Step
    {
        internal bool IsAsync { get; }

        internal abstract string Version { get; }

        /// <summary>When true the asynchronous method is called, otherwise the synchronous method will be invoked.</summary>
        protected Step(bool isAsync) => IsAsync = isAsync;

        /// <summary>Returns true if the step completed successfully.</summary>
        internal abstract bool Method(FileModel file, IngredientModel ingredients);

        /// <summary>Returns true if the step completed successfully.</summary>
        internal abstract Task<bool> MethodAsync(FileModel file, IngredientModel ingredients);
    }

    public abstract class StorageStep : Step
    {
        protected StorageStep(bool isAsync) : base(isAsync) { }

        internal abstract Task<bool> DoesFileExist(string filepath, IngredientModel ingredients);
    }
}
