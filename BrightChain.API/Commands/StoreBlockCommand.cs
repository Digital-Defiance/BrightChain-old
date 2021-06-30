﻿using BrightChain.Contexts;
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
            private readonly ApplicationDbContext _context;
            public StoreBlockCommandHandler(ApplicationDbContext context) => this._context = context;
            public async Task<BlockHash> Handle(StoreBlockCommand command, CancellationToken cancellationToken)
            {
                this._context.Blocks.Add(command.Block);
                await this._context.SaveChanges();
                return command.Block.Id;
            }
        }
    }
}