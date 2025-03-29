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
    public class MarcaController : Controller
    {
        private readonly SgarDbContext _context;

        public MarcaController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Marca
        public async Task<IActionResult> Index(Marca marca, int topRegistro = 10)
        {
            var query = _context.Marcas.AsQueryable();
            if(!string.IsNullOrWhiteSpace(marca.Nombre))
                query = query.Where(s => s.Nombre.Contains(marca.Nombre));
            if(!string.IsNullOrWhiteSpace(marca.Modelo))
                query = query.Where(s => s.Modelo.Contains(marca.Modelo));
            if(topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.OrderByDescending(s => s.Id);
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // GET: Marca/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (marca == null)
            {
                return NotFound();
            }

            return View(marca);
        }

        // GET: Marca/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Marca/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }

        // GET: Marca/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return NotFound();
            }
            return View(marca);
        }

        // POST: Marca/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Modelo,YearOfFabrication")] Marca marca)
        {
            if (id != marca.Id)
            {
                return NotFound();
            }

            var marcaUpdate = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == marca.Id);
            try
                {
                    marcaUpdate.Nombre = marca.Nombre;
                    marcaUpdate.Modelo = marca.Modelo;
                    marcaUpdate.YearOfFabrication = marca.YearOfFabrication;
                    _context.Update(marcaUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
            }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MarcaExists(marca.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                    return View(marca);
                     }
                }
           
            
        }

        // GET: Marca/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (marca == null)
            {
                return NotFound();
            }

            return View(marca);
        }

        // POST: Marca/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca != null)
            {
                _context.Marcas.Remove(marca);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MarcaExists(int id)
        {
            return _context.Marcas.Any(e => e.Id == id);
        }
    }
}
