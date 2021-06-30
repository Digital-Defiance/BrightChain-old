using BrightChain.API.Commands;
using BrightChain.API.Queries;
using BrightChain.Models.Blocks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BrightChain.API.Controllers
{
    [ApiVersion("1.0")]
    public class BlockController : BaseApiController
    {
        /// <summary>
        /// Creates a New Product.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(StoreBlockCommand command) => this.Ok(await this.Mediator.Send(command));
        /// <summary>
        /// Gets Product Entity by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(BlockHash id) => this.Ok(await this.Mediator.Send(new GetBlockByIdQuery { Id = id }));
        /// <summary>
        /// Deletes Product Entity based on Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(BlockHash id) => this.Ok(await this.Mediator.Send(new DropBlockByIdCommand { Id = id }));
        /// <summary>
        /// Updates the Product Entity based on Id.   
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(Block block) => this.Ok(await this.Mediator.Send(new UpdateBlockCommand { Block = block } ));
    }
}
