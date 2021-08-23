namespace BrightChain.API.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Models.Hashes;
    using BrightChain.Engine.Services;
    using MediatR;

    public class StoreBlockCommand : IRequest<BlockHash>
    {
        public BrightenedBlock Block { get; set; }

        public class StoreBlockCommandHandler : IRequestHandler<StoreBlockCommand, BlockHash>
        {
            private readonly BrightBlockService _brightChain;

            public StoreBlockCommandHandler(BrightBlockService brightChain)
            {
                this._brightChain = brightChain;
            }

            public async Task<BlockHash> Handle(StoreBlockCommand command, CancellationToken cancellationToken)
            {
                var storedBlock = await this._brightChain.StoreBlockAsync(command.Block)
                    .ConfigureAwait(false);

                return storedBlock.Id;
            }
        }
    }
}
