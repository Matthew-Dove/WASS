using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    public class ResourceOptions : SmartEnum
    {
        public static readonly ResourceOptions None = new(1);
        public static readonly ResourceOptions Files = new(2);
        public static readonly ResourceOptions Tags = new(3);

        private ResourceOptions(int value) : base(value) { }
    }
}
