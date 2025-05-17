using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lending_skills_backend.Models;
using lending_skills_backend.DataAccess;
using lending_skills_backend.Dtos.Requests;
using lending_skills_backend.Mappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace lending_skills_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlocksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BlocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DbBlock>>> GetBlocks()
        {
            return await _context.Blocks
                .Select(b => new DbBlock
                {
                    Id = b.Id,
                    Type = b.Type,
                    Title = b.Title,
                    Content = b.Content,
                    Visible = b.Visible,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Date,
                    IsExample = b.IsExample,
                    NextBlockId = b.NextBlockId,
                    PreviousBlockId = b.PreviousBlockId,
                    FormId = b.FormId,
                    PageId = b.PageId
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DbBlock>> GetBlock(Guid id)
        {
            var block = await _context.Blocks
                .Select(b => new DbBlock
                {
                    Id = b.Id,
                    Type = b.Type,
                    Title = b.Title,
                    Content = b.Content,
                    Visible = b.Visible,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Date,
                    IsExample = b.IsExample,
                    NextBlockId = b.NextBlockId,
                    PreviousBlockId = b.PreviousBlockId,
                    FormId = b.FormId,
                    PageId = b.PageId
                })
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        [HttpPost]
        public async Task<ActionResult<DbBlock>> CreateBlock(CreateBlockRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Block data is required" });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(request.Type))
                {
                    return BadRequest(new { error = "Type is required" });
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { error = "Title is required" });
                }

                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest(new { error = "Content is required" });
                }

                if (string.IsNullOrEmpty(request.Date))
                {
                    return BadRequest(new { error = "Date is required" });
                }

                if (string.IsNullOrEmpty(request.IsExample))
                {
                    return BadRequest(new { error = "IsExample is required" });
                }

                var newBlock = request.ToDbBlock();
                _context.Blocks.Add(newBlock);
                
                try 
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    var innerMessage = dbEx.InnerException?.Message ?? "Unknown database error";
                    return BadRequest(new { error = $"Database error: {innerMessage}" });
                }

                return CreatedAtAction(nameof(GetBlock), new { id = newBlock.Id }, newBlock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating block: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }

                return BadRequest(new { error = $"Server error: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DbBlock>> UpdateBlock(Guid id, EditBlockRequest request)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null)
            {
                return NotFound();
            }

            block.UpdateFromRequest(request);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlockExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(block);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBlocks(IEnumerable<DbBlock> blocks)
        {
            foreach (var block in blocks)
            {
                block.UpdatedAt = DateTime.UtcNow;
                _context.Entry(block).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlock(Guid id)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null)
            {
                return NotFound();
            }

            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlockExists(Guid id)
        {
            return _context.Blocks.Any(e => e.Id == id);
        }
    }
} 