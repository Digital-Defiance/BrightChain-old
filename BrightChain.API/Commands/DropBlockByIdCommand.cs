using BrightChain.EntityFrameworkCore.Contexts;
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
            private readonly BrightChainDbContext _context;
            public DropBlockByIdCommandHandler(BrightChainDbContext context) => this._context = context;
            public async Task<BlockHash> Handle(DropBlockByIdCommand command, CancellationToken cancellationToken)
            {
                var block = await this._context.Blocks.Where(a => a.Id == command.Id.ToString()).FirstOrDefaultAsync();
                if (block == null)
                {
                    return default;
                }

                var id = block.ToBlock().Id;
                this._context.Blocks.Remove(block);
                await this._context.SaveChanges();
                return id;
            }
        }
    }
}
