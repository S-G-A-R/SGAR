using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ZonaController : Controller
    {
        private readonly SgarDbContext _context;

        public ZonaController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Zona
        public async Task<IActionResult> Index(Zona zona, int topRegistro = 10)
        {
            var query = _context.Zonas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(zona.Nombre))
                query = query.Where(s => s.Nombre.Contains(zona.Nombre));
            if (zona.IdDistrito > 0)
                query = query.Where(s => s.IdDistrito == zona.IdDistrito);
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdAlcaldiaNavigation).Include(p => p.IdDistritoNavigation);
            query = query.OrderByDescending(s => s.Id);

            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            var alcaldias = _context.Municipios.ToList();
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        public JsonResult GetDistritosFromMunicipioId(int municipioId)
        {
            return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());
        }

        // GET: Zona/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zona = await _context.Zonas
                .Include(z => z.IdAlcaldiaNavigation)
                .Include(z => z.IdDistritoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            var alcaldias = _context.Municipios.ToList();
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");
            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }

        // GET: Zona/Create
        public IActionResult Create()
        {
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            return View();
        }

        // POST: Zona/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,IdDistrito,IdAlcaldia,Descripcion")] Zona zona)
        {
            try
            {
                zona.IdAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.IdMunicipio == zona.IdAlcaldia).Id;

                _context.Add(zona);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch
            {
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                var departamentos = _context.Departamentos.ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                return View(zona);
            }
        }

        // GET: Zona/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zona = await _context.Zonas.FindAsync(id);
            if (zona == null)
            {
                return NotFound();
            }

            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            var miAlcaldia = _context.Alcaldias.FirstOrDefault(a => a.Id == zona.IdAlcaldia);
            var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == miAlcaldia.IdMunicipio);
            municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });

            var miDistrito = _context.Distritos.FirstOrDefault(a => a.Id == zona.IdDistrito);
            distritos.Add(new Distrito { Nombre = miDistrito.Nombre, Id = miDistrito.Id, IdMunicipio = miDistrito.IdMunicipio });

            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", zona.IdDistrito);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            return View(zona);
        }

        // POST: Zona/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,IdDistrito,IdAlcaldia,Descripcion")] Zona zona)
        {
            if (id != zona.Id)
            {
                return NotFound();
            }
            try
            {

                zona.IdAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.IdMunicipio == zona.IdAlcaldia).Id;
                _context.Update(zona);
                await _context.SaveChangesAsync();
            }
            catch
            {

                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                var departamentos = _context.Departamentos.ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                return View(zona);

            }
            return View(zona);
        }

        // GET: Zona/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zona = await _context.Zonas
                .Include(z => z.IdAlcaldiaNavigation)
                .Include(z => z.IdDistritoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            var alcaldias = _context.Municipios.ToList();
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");
            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }

        // POST: Zona/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zona = await _context.Zonas.FindAsync(id);
            if (zona != null)
            {
                _context.Zonas.Remove(zona);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZonaExists(int id)
        {
            return _context.Zonas.Any(e => e.Id == id);
        }

        // Obtener la lista de zonas
        [HttpGet]
        //Metodo para retornan datos en formato JSON, listos para ser usados en los select de la vista index
        public IActionResult GetZonas()
        {
            var zonas = _context.Zonas
                .Select(z => new { id = z.Id, nombre = z.Nombre })
                .ToList();

            return Json(zonas);
        }

    }
}
