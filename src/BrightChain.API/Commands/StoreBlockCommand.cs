using BrightChain.EntityFrameworkCore.Data;
using BrightChain.Models.Blocks;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.API.Commands
{
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
                _context.Blocks.Add(BrightChainBlock.FromBrightChainBlock(command.Block));
                _context.SaveChanges();
                return command.Block.Id;
            }
        }
    }
}