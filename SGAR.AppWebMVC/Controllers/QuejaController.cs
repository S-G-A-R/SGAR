using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Ciudadano, Supervisor")]
    public class QuejaController : Controller
    {
        private readonly SgarDbContext _context;

        public QuejaController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Queja
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> Index(Queja queja, int topRegistro = 10)
        {
            var quejas = await _context.Quejas.ToListAsync();
            var quejasQ1 = quejas
                .Where(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value));

            var query = quejasQ1.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queja.Titulo))
                query = query.Where(s => s.Titulo.Contains(queja.Titulo));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdCiudadanoNavigation);
            query = query.OrderByDescending(s => s.Id);

            return View(query.OrderByDescending(s => s.Id).ToList());
        }
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> List(Queja queja, int topRegistro = 10)
        {
            var quejas = await _context.Quejas.Include(s => s.IdCiudadanoNavigation.Zona).ToListAsync();
            var quejasQ1 = quejas
                .Where(s => s.IdCiudadanoNavigation.Zona.IdAlcaldia == Convert.ToInt32(User.FindFirst("Alcaldia").Value));

            var query = quejasQ1.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queja.Titulo))
                query = query.Where(s => s.Titulo.Contains(queja.Titulo));
            if (!string.IsNullOrWhiteSpace(queja.TipoSituacion))
                query = query.Where(s => s.TipoSituacion == queja.TipoSituacion);
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdCiudadanoNavigation);
            query = query.OrderByDescending(s => s.Id);

            return View(query.OrderByDescending(s => s.Id).ToList());
        }

        // GET: Queja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var queja = await _context.Quejas
                .Include(q => q.IdCiudadanoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (queja == null)
            {
                return NotFound();
            }

            return View(queja);
        }

        // GET: Queja/Create
        [Authorize(Roles = "Ciudadano")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Queja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> Create(Queja queja)
        {
            try
            {
                queja.Estado = 1;
                queja.Motivo = "";
                queja.IdCiudadano = Convert.ToInt32(User.FindFirst("Id").Value);
                if (queja.File != null)
                    queja.Archivo = await GenerarByteImage(queja.File);


                _context.Add(queja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(queja);
            }

            
        }

        // GET: Queja/Edit/5
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var queja = await _context.Quejas.FindAsync(id);
            if (queja == null)
            {
                return NotFound();
            }
            return View(queja);
        }

        // POST: Queja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int id, Queja queja)
        {
            if (id != queja.Id)
            {
                return NotFound();
            }


            try
            {

                var quejaUpdate = await _context.Quejas.FirstOrDefaultAsync(s => s.Id == queja.Id);
                quejaUpdate.Estado = queja.Estado;
                quejaUpdate.Motivo = queja.Motivo;

                _context.Update(quejaUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuejaExists(queja.Id))
                {
                    return NotFound();
                }
                else
                {
                    return View(queja);
                }
            }

        }

       

        // POST: Queja/Delete/5
        [Authorize(Roles = "Ciudadano")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var queja = await _context.Quejas.FindAsync(id);
            if (queja != null)
            {
                _context.Quejas.Remove(queja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuejaExists(int id)
        {
            return _context.Quejas.Any(e => e.Id == id);
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
