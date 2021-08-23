namespace BrightChain.API.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BrightChain.Engine.Models.Blocks;
    using BrightChain.Engine.Services;
    using MediatR;

    public class UpdateBlockCommand : IRequest<Block>
    {
        public Block Block { get; set; }
        public class UpdateBlockCommandHandler : IRequestHandler<UpdateBlockCommand, Block>
        {
            private readonly BrightBlockService _brightChain;
            public UpdateBlockCommandHandler(BrightBlockService brightChain)
            {
                this._brightChain = brightChain;
            }

            public async Task<Block> Handle(UpdateBlockCommand command, CancellationToken cancellationToken)
            {
                // there will be very limited contexts where we can update blocks-
                // apart from some metadata updates perhaps?
                throw new NotImplementedException();
            }
        }
    }
}
