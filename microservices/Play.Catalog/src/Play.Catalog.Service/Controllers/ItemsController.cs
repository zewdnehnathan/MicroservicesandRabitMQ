using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Play.Common;
using Play.Catalog.Service.Entities;

using Play.Catalog.Contracts;
using MassTransit;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
       private readonly IRepository<Item> itemsRepository ;
       private readonly IPublishEndpoint _publishEndPoint;
       
        public ItemsController(IRepository<Item> itemsRepository,IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this._publishEndPoint = publishEndpoint;
        }

        [HttpGet]
       public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
       {

        var items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());
          
         return Ok(items);
       }     

       [HttpGet("{id}")]
       public async Task<ActionResult<Item>> GetByIdAsync(Guid id)
       {
         /*var item =*/ return (await itemsRepository.GetAsync(id));
         /*if(item == null){
             return NotFound();
         }
         return item.;*/
       }

       [HttpPost]
       public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
       {
         var item = new Item
         {
           Name = createItemDto.Name,
           Description=createItemDto.Description,
           Price = createItemDto.Price,
           CreatedDate = DateTimeOffset.UtcNow
         };

         await itemsRepository.CreateAsync(item);

         await _publishEndPoint.Publish(new CatalogItemCreated(item.Id,item.Name,item.Description));
         
         return CreatedAtAction(nameof(GetByIdAsync),new {id=item.Id},item);
       }

       [HttpPut("{id}")]
       public async Task<IActionResult> PutAsync(Guid id,UpdateItemDto updateItemDto)
       {
         var existingItem = await itemsRepository.GetAsync(id);


         if(existingItem == null){
             return NotFound();
         }

         existingItem.Name = updateItemDto.Name;
         existingItem.Description = updateItemDto.Description;
         existingItem.Price = updateItemDto.Price;
        
        await itemsRepository.UpdateAsync(existingItem);

        await _publishEndPoint.Publish(new CatalogItemUpdated(existingItem.Id,existingItem.Name,existingItem.Description));

         return NoContent();
 
       }

       [HttpDelete("{id}")]
       public async Task<IActionResult> DeleteAsync(Guid id)
       {
            var item = await itemsRepository.GetAsync(id);
            if(item == null){
             return NotFound();
            }
            await itemsRepository.RemoveAsync(item.Id);

            await _publishEndPoint.Publish(new CatalogItemDeleted(id));
            
            return NoContent();
       } 

    }

}