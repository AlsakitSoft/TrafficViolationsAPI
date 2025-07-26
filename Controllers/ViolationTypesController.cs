using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficViolationsAPI.Data;
using TrafficViolationsAPI.DTOs;

namespace TrafficViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ViolationTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ViolationTypeDto>>> GetViolationTypes()
        {
            var types = await _context.ViolationTypes
                .Where(vt => vt.IsActive)
                .Select(vt => new ViolationTypeDto
                {
                    Violation_Type_ID = vt.Violation_Type_ID,
                    Violation_Description = vt.Violation_Description,
                    DefaultFineAmount = vt.DefaultFineAmount
                })
                .ToListAsync();

            return Ok(types);
        }
    }

}
