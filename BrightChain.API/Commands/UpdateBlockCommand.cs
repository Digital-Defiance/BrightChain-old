using BrightChain.Contexts;
using BrightChain.Models.Blocks;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.API.Commands
{
    public class UpdateBlockCommand : IRequest<Block>
    {
        public Block Block { get; set; }
        public class UpdateBlockCommandHandler : IRequestHandler<UpdateBlockCommand, Block>
        {
            private readonly ApplicationDbContext _context;
            public UpdateBlockCommandHandler(ApplicationDbContext context) => this._context = context;
            public async Task<Block> Handle(UpdateBlockCommand command, CancellationToken cancellationToken)
            {
                var block = this._context.Blocks.Where(a => a.Id == command.Block.Id).FirstOrDefault();
                if (block == null)
                {
                    return default;
                }
                else
                {
                    // block.prop = value
                    await this._context.SaveChanges();
                    return block;
                }
            }
        }
    }
}
