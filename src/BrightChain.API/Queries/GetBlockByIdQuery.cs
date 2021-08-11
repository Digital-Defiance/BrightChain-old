using System.Threading;
using System.Threading.Tasks;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Services;
using MediatR;

namespace BrightChain.API.Queries
{
    public class GetBlockByIdQuery : IRequest<Block>
    {
        public BlockHash Id { get; set; }

        public class GetBlockByIdQueryHandler : IRequestHandler<GetBlockByIdQuery, Block>
        {
            private readonly BrightBlockService _brightChain;

            public GetBlockByIdQueryHandler(BrightBlockService brightChain)
            {
                this._brightChain = brightChain;
            }

            public async Task<Block> Handle(GetBlockByIdQuery query, CancellationToken cancellationToken)
            {
                return await this._brightChain.FindBlockByIdAsync(query.Id)
                    .ConfigureAwait(false);
            }
        }
    }
}
