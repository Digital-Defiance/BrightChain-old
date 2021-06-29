using BrightChain.Contexts;
using BrightChain.Models.Blocks;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightChain.API.Queries
{
    public class GetBlockByIdQuery : IRequest<Block>
    {
        public BlockHash Id { get; set; }
        public class GetBlockByIdQueryHandler : IRequestHandler<GetBlockByIdQuery, Block>
        {
            private readonly ApplicationDbContext _context;
            public GetBlockByIdQueryHandler(ApplicationDbContext context) => this._context = context;
            public async Task<Block> Handle(GetBlockByIdQuery query, CancellationToken cancellationToken)
            {
                var block = this._context.Blocks.Where(a => a.Id == query.Id).FirstOrDefault();
                if (block == null)
                {
                    return null;
                }

                return block;
            }
        }
    }
}
