using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    public class TiposVehiculoController : Controller
    {
        private readonly SgarDbContext _context;

        public TiposVehiculoController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: TiposVehiculo
        public async Task<IActionResult> Index(TiposVehiculo tiposVehiculo, int topRegistro = 10)
        {
            var query = _context.TiposVehiculos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tiposVehiculo.Descripcion))
                query = query.Where(s => s.Descripcion.ToLower().Contains(tiposVehiculo.Descripcion.ToLower()));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.OrderByDescending(s => s.Id);
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // GET: TiposVehiculo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tiposVehiculo == null)
            {
                return NotFound();
            }

            return View(tiposVehiculo);
        }

        // GET: TiposVehiculo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TiposVehiculo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tiposVehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tiposVehiculo);
        }

        // GET: TiposVehiculo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);
            if (tiposVehiculo == null)
            {
                return NotFound();
            }
            return View(tiposVehiculo);
        }

        // POST: TiposVehiculo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tipo,Descripcion")] TiposVehiculo tiposVehiculo)
        {
            if (id != tiposVehiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tiposVehiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TiposVehiculoExists(tiposVehiculo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tiposVehiculo);
        }

        // GET: TiposVehiculo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tiposVehiculo == null)
            {
                return NotFound();
            }

            return View(tiposVehiculo);
        }

        // POST: TiposVehiculo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tiposVehiculo = await _context.TiposVehiculos.FindAsync(id);
            if (tiposVehiculo != null)
            {
                _context.TiposVehiculos.Remove(tiposVehiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TiposVehiculoExists(int id)
        {
            return _context.TiposVehiculos.Any(e => e.Id == id);
        }
    }
}
