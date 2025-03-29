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
        public async Task<IActionResult> Index()
        {
            var sgarDbContext = _context.Operadores.Include(o => o.IdAlcaldiaNavigation).Include(o => o.Vehiculo);
            return View(await sgarDbContext.ToListAsync());
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
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "Correo");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id");
            return View();
        }

        // POST: Operador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,TelefonoPersonal,CorreoPersonal,Dui,Foto,Ayudantes,CodigoOperador,TelefonoLaboral,CorreoLaboral,VehiculoId,LicenciaDoc,AntecedentesDoc,SolvenciaDoc,Password,IdAlcaldia")] Operador operador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(operador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "Correo", operador.IdAlcaldia);
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
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "Correo", operador.IdAlcaldia);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
            return View(operador);
        }

        // POST: Operador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,TelefonoPersonal,CorreoPersonal,Dui,Foto,Ayudantes,CodigoOperador,TelefonoLaboral,CorreoLaboral,VehiculoId,LicenciaDoc,AntecedentesDoc,SolvenciaDoc,Password,IdAlcaldia")] Operador operador)
        {
            if (id != operador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(operador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperadorExists(operador.Id))
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
            ViewData["IdAlcaldia"] = new SelectList(_context.Alcaldias, "Id", "Correo", operador.IdAlcaldia);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
            return View(operador);
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

        private bool OperadorExists(int id)
        {
            return _context.Operadores.Any(e => e.Id == id);
        }
    }
}
