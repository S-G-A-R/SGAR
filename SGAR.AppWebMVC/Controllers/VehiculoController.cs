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
    public class VehiculoController : Controller
    {
        private readonly SgarDbContext _context;

        public VehiculoController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Vehiculo
        public async Task<IActionResult> Index()
        {
            var sgarDbContext = _context.Vehiculos.Include(v => v.IdMarcaNavigation).Include(v => v.IdOperadorNavigation).Include(v => v.IdTipoVehiculoNavigation);
            return View(await sgarDbContext.ToListAsync());
        }

        // GET: Vehiculo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.IdMarcaNavigation)
                .Include(v => v.IdOperadorNavigation)
                .Include(v => v.IdTipoVehiculoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // GET: Vehiculo/Create
        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo");
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id");
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion");
            return View();
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

        // POST: Vehiculo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdMarca,Placa,Codigo,IdTipoVehiculo,Mecanico,Taller,IdOperador,Estado,Descripcion,Foto")] Vehiculo vehiculo, IFormFile? file = null)
        {
            if (ModelState.IsValid)
            {
                vehiculo.Foto = await GenerarByteImage(file);
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);
        }

        // GET: Vehiculo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);
        }

        // POST: Vehiculo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMarca,Placa,Codigo,IdTipoVehiculo,Mecanico,Taller,IdOperador,Estado,Descripcion,Foto")] Vehiculo vehiculo, IFormFile? file = null)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    vehiculo.Foto = await GenerarByteImage(file);
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.Id))
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
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);
        }

        // GET: Vehiculo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.IdMarcaNavigation)
                .Include(v => v.IdOperadorNavigation)
                .Include(v => v.IdTipoVehiculoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}
