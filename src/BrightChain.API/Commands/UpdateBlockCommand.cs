namespace BrightChain.API.Commands
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.EntityFrameworkCore.Data;
    using MediatR;

    public class UpdateBlockCommand : IRequest<Block>
    {
        public Block Block { get; set; }
        public class UpdateBlockCommandHandler : IRequestHandler<UpdateBlockCommand, Block>
        {
            private readonly BrightChainBlockDbContext _context;
            public UpdateBlockCommandHandler(BrightChainBlockDbContext context)
            {
                _context = context;
            }

            public async Task<Block> Handle(UpdateBlockCommand command, CancellationToken cancellationToken)
            {
                var block = _context.Blocks.Where(a => a.Id == command.Block.Id.ToString()).FirstOrDefault();
                if (block == null)
                {
                    return default;
                }
                else
                {
                    // block.prop = value
                    _context.SaveChanges();
                    return block.ToBlock();
                }
            }
        }
    }
}
