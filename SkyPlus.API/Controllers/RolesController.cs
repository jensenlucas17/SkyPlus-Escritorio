using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly SkyPlusDbContext _context;

        public RolesController(SkyPlusDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.IdRol,
                    r.NombreRol
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
}