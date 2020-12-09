using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PesoXMeta.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PesoXMeta.Controllers
{
    public class DiasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Dias.Include(p => p.IdentityUser);
            return View(await applicationDbContext.ToListAsync());
        }

        private bool PesoDiasExists(int id)
        {
            return _context.Dias.Any(e => e.Id == id);
        }
    }
}
