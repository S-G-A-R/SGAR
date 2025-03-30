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
    public class OperadorController : Controller
    {
        private readonly SgarDbContext _context;

        public OperadorController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Operador
        public async Task<IActionResult> Index(Operador operador, int topRegistro = 10)
        {
            var query = _context.Operadores.AsQueryable();
            if (!string.IsNullOrWhiteSpace(operador.Nombre))
                query = query.Where(s => s.Nombre.Contains(operador.Nombre));
            if (!string.IsNullOrWhiteSpace(operador.Dui))
                query = query.Where(s => s.Dui.Contains(operador.Dui));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.OrderByDescending(s => s.Id);
            return View(await query.ToListAsync());
        }
        // GET: Operador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores
                .Include(o => o.IdAlcaldiaNavigation)
                .Include(o => o.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (operador == null)
            {
                return NotFound();
            }

            return View(operador);
        }

        // GET: Operador/Create
        public IActionResult Create()
        {
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "IdMunicipio");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id");
            return View();
        }

        // POST: Operador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Operador operador)
        {
            if (ModelState.IsValid)
            {
                if (operador.SolvenciaFile != null)
                {
                    operador.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile);
                }

                if (operador.LicenciaFile != null)
                {
                    operador.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile);
                }

                if (operador.AntecedentesFile != null)
                {
                    operador.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile);
                }

                if (operador.FotoFile != null)
                {
                    operador.Foto = await GenerarByteImage(operador.FotoFile);
                }

                _context.Add(operador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "IdMunicipio", operador.IdAlcaldia);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
            return View(operador);
        }
        // GET: Operador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores.FindAsync(id);
            if (operador == null)
            {
                return NotFound();
            }
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "IdMunicipio", operador.IdAlcaldia);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
            return View(operador);
        }

        // POST: Operador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Operador operador)
        {
            if (id != operador.Id)
            {
                return NotFound();
            }

            var operadorUpdate = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == operador.Id);

            try
            {
                operadorUpdate.Nombre = operador.Nombre;
                operadorUpdate.Apellido = operador.Apellido;
                operadorUpdate.TelefonoPersonal = operador.TelefonoPersonal;
                operadorUpdate.CorreoPersonal = operador.CorreoPersonal;
                operadorUpdate.Dui = operador.Dui;
                operadorUpdate.Ayudantes = operador.Ayudantes;
                operadorUpdate.CodigoOperador = operador.CodigoOperador;
                operadorUpdate.CorreoLaboral = operador.CorreoLaboral;
                operadorUpdate.TelefonoLaboral = operador.CorreoLaboral;
                operadorUpdate.VehiculoId = operador.VehiculoId;
                operadorUpdate.IdAlcaldia = operador.IdAlcaldia;

                var solvenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.SolvenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile, solvenciaAnterior);

                var licenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.LicenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile, licenciaAnterior);

                var antecendeAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.AntecedentesDoc).FirstOrDefaultAsync(); // Se corrige la propiedad
                operadorUpdate.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile, antecendeAnterior);

                var fotoAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                operadorUpdate.Foto = await GenerarByteImage(operador.FotoFile, fotoAnterior);

                _context.Update(operadorUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperadorExists(operador.Id))
                {
                    return NotFound();
                }
                else
                {
                    return View(operador);
                }
            }
        }

        // GET: Operador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores
                .Include(o => o.IdAlcaldiaNavigation)
                .Include(o => o.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (operador == null)
            {
                return NotFound();
            }

            return View(operador);
        }

        // POST: Operador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var operador = await _context.Operadores.FindAsync(id);
            if (operador != null)
            {
                _context.Operadores.Remove(operador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private bool OperadorExists(int id)
        {
            return _context.Operadores.Any(e => e.Id == id);
        }
    }
}
