using System;
using System.Collections;
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
            return View(new Vehiculo());
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
        public async Task<IActionResult> Create(Vehiculo vehiculo)
        {
            try
            {
                if (vehiculo.fotofile != null)
                {
                    vehiculo.Foto = await GenerarByteImage(vehiculo.fotofile);
                }

                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
                ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
                ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
                return View(vehiculo);
            }
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
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);
        }

        // POST: Vehiculo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            var vehiculoUpdate = await _context.Vehiculos
                .FirstOrDefaultAsync(o => o.Id == vehiculo.Id);

            if (vehiculoUpdate == null)
            {
                return NotFound();
            }

            try
            {
                vehiculoUpdate.IdMarca = vehiculo.IdMarca;
                vehiculoUpdate.Mecanico = vehiculo.Mecanico;
                vehiculoUpdate.Taller = vehiculo.Taller;
                vehiculoUpdate.Estado = vehiculo.Estado;
                vehiculoUpdate.Codigo = vehiculo.Codigo;
                vehiculoUpdate.IdOperador = vehiculo.IdOperador;
                vehiculoUpdate.IdTipoVehiculo = vehiculo.IdTipoVehiculo;
                vehiculoUpdate.Placa = vehiculo.Placa;
                vehiculoUpdate.Descripcion = vehiculo.Descripcion;

                var fotoAnterior = await _context.Vehiculos
                   .Where(s => s.Id == vehiculo.Id)
                   .Select(s => s.Foto).FirstOrDefaultAsync();
                vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, fotoAnterior);

                if (vehiculo.fotofile != null)
                {
                    vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, vehiculoUpdate.Foto);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiculoExists(vehiculo.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Ocurrió un error de concurrencia. Por favor, intente de nuevo.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error inesperado, por favor intente de nuevo.");
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);

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
