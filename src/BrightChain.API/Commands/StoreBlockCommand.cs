namespace BrightChain.API.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Services;
    using BrightChain.EntityFrameworkCore.Data;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using MediatR;

    public class StoreBlockCommand : IRequest<BlockHash>
    {
        public Block Block { get; set; }

        public class StoreBlockCommandHandler : IRequestHandler<StoreBlockCommand, BlockHash>
        {
            private readonly BrightBlockService _brightChain;

            public StoreBlockCommandHandler(BrightBlockService brightChain)
            {
                this._brightChain = brightChain;
            }

            public async Task<BlockHash> Handle(StoreBlockCommand command, CancellationToken cancellationToken)
            {
                var storedBlock = await this._brightChain.TryStoreBlockAsync(command.Block)
                    .ConfigureAwait(false);

                return storedBlock.Id;
            }
        }
    }
}
