using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Operador, Supervisor")]
    public class MantenimientoController : Controller
    {
        private readonly SgarDbContext _context;

        public MantenimientoController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Mantenimiento
        [Authorize(Roles = "Operador")]
        public async Task<IActionResult> Index(Mantenimiento mantenimiento, int topRegistro = 10)
        {
            var mantenimientos = await _context.Mantenimientos.ToListAsync();
            var mantenimientosQ1 = mantenimientos
                .Where(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value));

            var query = mantenimientosQ1.AsQueryable();
            if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
                query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdOperadorNavigation);
            query = query.OrderByDescending(s => s.Id);

            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> List(Mantenimiento mantenimiento, int topRegistro = 10)
        {
            var mantenimientos = await _context.Mantenimientos.Include(s => s.IdOperadorNavigation).ToListAsync();
            var mantenimientosQ1 = mantenimientos
                .Where(s => s.IdOperadorNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));

            var query = mantenimientosQ1.AsQueryable();
            if (!string.IsNullOrWhiteSpace(mantenimiento.Titulo))
                query = query.Where(s => s.Titulo.Contains(mantenimiento.Titulo));
            if (!string.IsNullOrWhiteSpace(mantenimiento.TipoSituacion))
                query = query.Where(s => s.TipoSituacion == mantenimiento.TipoSituacion);
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdOperadorNavigation);
            query = query.OrderByDescending(s => s.Id);

            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // GET: Mantenimiento/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.IdOperadorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            return View(mantenimiento);
        }

        // GET: Mantenimiento/Create
        [Authorize(Roles = "Operador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mantenimiento/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Operador")]
        public async Task<IActionResult> Create(Mantenimiento mantenimiento)
        {
            try
            {
                mantenimiento.Estado = 1;
                mantenimiento.Motivo = "";
                mantenimiento.IdOperador = Convert.ToInt32(User.FindFirst("Id").Value);
                if (mantenimiento.File != null)
                    mantenimiento.Archivo = await GenerarByteImage(mantenimiento.File);


                _context.Add(mantenimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(mantenimiento);
            }
        }

        // GET: Mantenimiento/Edit/5
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null)
            {
                return NotFound();
            }
           
            return View(mantenimiento);
        }

        // POST: Mantenimiento/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int id, Mantenimiento mantenimiento)
        {
            if (id != mantenimiento.Id)
            {
                return NotFound();
            }

                try
                {
                    var mantenimientoUpdate = await _context.Mantenimientos.FirstOrDefaultAsync(s => s.Id == mantenimiento.Id);
                    mantenimientoUpdate.Estado = mantenimiento.Estado;
                    mantenimientoUpdate.Motivo = mantenimiento.Motivo;

                    _context.Update(mantenimientoUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MantenimientoExists(mantenimiento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return View(mantenimiento);
                    }
                }
        }

        // GET: Mantenimiento/Delete/5
        [Authorize(Roles = "Operador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.IdOperadorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            return View(mantenimiento);
        }

        // POST: Mantenimiento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Operador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento != null)
            {
                _context.Mantenimientos.Remove(mantenimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MantenimientoExists(int id)
        {
            return _context.Mantenimientos.Any(e => e.Id == id);
        }

        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            byte[]? bytes = bytesImage;
            if (file != null && file.Length > 0)
            {
                // Construir la ruta del archivo               
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    bytes = memoryStream.ToArray(); // Devuelve los bytes del archivo
                }
            }
            return bytes;
        }
    }
}
