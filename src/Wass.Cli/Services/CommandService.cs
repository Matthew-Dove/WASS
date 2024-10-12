using ContainerExpressions.Containers;

namespace Wass.Cli.Services
{
    public interface ICommandService
    {
        Task<Response<Either<BadRequest, Unit>>> Execute(string[] args);
    }

    public sealed class CommandService(IParser _parser) : ICommandService
    {
        public async Task<Response<Either<BadRequest, Unit>>> Execute(string[] args)
        {
            var response = new Response<Either<BadRequest, Unit>>();
            var command = _parser.GetCommand(args);
            if (!command) return response.With(new BadRequest());
            var cmd = command.Value;



            await Task.Delay(0);
            return response;
        }
    }
}
