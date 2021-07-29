namespace BrightChain.API.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Services;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DropBlockByIdCommand : IRequest<BlockHash>
    {
        public BlockHash Id { get; set; }

        public class DropBlockByIdCommandHandler : IRequestHandler<DropBlockByIdCommand, BlockHash>
        {
            private readonly BrightBlockService _brightChain;
            private readonly DbContext _context;

            public DropBlockByIdCommandHandler(BrightBlockService brightBlockService)
            {
                this._brightChain = brightBlockService;
            }

            public async Task<BlockHash> Handle(DropBlockByIdCommand command, CancellationToken cancellationToken)
            {
                var chainBlock = await this._brightChain
                    .TryDropBlockAsync(command.Id)
                        .ConfigureAwait(false);

                return chainBlock is Block ? chainBlock.Id : command.Id;
            }
        }
    }
}
