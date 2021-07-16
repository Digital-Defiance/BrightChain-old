using System.Threading.Tasks;
using BrightChain.API.Commands;
using BrightChain.API.Queries;
using BrightChain.Engine.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create(StoreBlockCommand command)
        {
            return Ok(await Mediator.Send(command).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets Product Entity by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(BlockHash id)
        {
            return Ok(await Mediator.Send(new GetBlockByIdQuery { Id = id }).ConfigureAwait(false));
        }

        /// <summary>
        /// Deletes Product Entity based on Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(BlockHash id)
        {
            return Ok(await Mediator.Send(new DropBlockByIdCommand { Id = id }).ConfigureAwait(false));
        }

        /// <summary>
        /// Updates the Product Entity based on Id.   
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public async Task<IActionResult> Update(Block block)
        {
            return Ok(await Mediator.Send(new UpdateBlockCommand { Block = block }).ConfigureAwait(false));
        }
    }
}
