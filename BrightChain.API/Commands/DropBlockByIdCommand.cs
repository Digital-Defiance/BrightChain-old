﻿using BrightChain.Contexts;
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
            private readonly ApplicationDbContext _context;
            public DropBlockByIdCommandHandler(ApplicationDbContext context) => this._context = context;
            public async Task<BlockHash> Handle(DropBlockByIdCommand command, CancellationToken cancellationToken)
            {
                var block = await this._context.Blocks.Where(a => a.Id == command.Id).FirstOrDefaultAsync();
                if (block == null)
                {
                    return default;
                }

                this._context.Blocks.Remove(block);
                await this._context.SaveChanges();
                return block.Id;
            }
        }
    }
}