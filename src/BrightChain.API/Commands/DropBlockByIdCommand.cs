using BrightChain.EntityFrameworkCore.Data;
using BrightChain.Models.Blocks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.API.Commands
{
    public class DropBlockByIdCommand : IRequest<BlockHash>
    {
        public BlockHash Id { get; set; }
        public class DropBlockByIdCommandHandler : IRequestHandler<DropBlockByIdCommand, BlockHash>
        {
            private readonly BrightChainBlockDbContext _context;
            public DropBlockByIdCommandHandler(BrightChainBlockDbContext context)
            {
                _context = context;
            }

            public async Task<BlockHash> Handle(DropBlockByIdCommand command, CancellationToken cancellationToken)
            {
                var block = await _context.Blocks.Where(a => a.Id == command.Id.ToString()).FirstOrDefaultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (block == null)
                {
                    return default;
                }

                var id = block.ToBlock().Id;
                _context.Blocks.Remove(block);
                _context.SaveChanges();
                return id;
            }
        }
    }
}
