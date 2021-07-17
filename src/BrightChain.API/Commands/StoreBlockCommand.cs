namespace BrightChain.API.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.EntityFrameworkCore.Data;
    using BrightChain.EntityFrameworkCore.Data.Entities;
    using MediatR;

    public class StoreBlockCommand : IRequest<BlockHash>
    {
        public Block Block { get; set; }
        public class StoreBlockCommandHandler : IRequestHandler<StoreBlockCommand, BlockHash>
        {
            private readonly BrightChainBlockDbContext _context;
            public StoreBlockCommandHandler(BrightChainBlockDbContext context)
            {
                _context = context;
            }

            public async Task<BlockHash> Handle(StoreBlockCommand command, CancellationToken cancellationToken)
            {
                _context.Blocks.Add(BrightChainEntityBlock.FromBrightChainBlock(command.Block));
                _context.SaveChanges();
                return command.Block.Id;
            }
        }
    }
}
