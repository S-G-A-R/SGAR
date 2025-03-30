using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SGAR.AppWebMVC.Controllers
{
    public class HorarioController : Controller
    {
        private readonly SgarDbContext _context;

        public HorarioController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Horario
        public async Task<IActionResult> Index()
        {
            return View(await _context.Horarios.Include(h => h.IdOperadorNavigation).Include(h => h.IdZonaNavigation).ToListAsync());
        }

        // GET: Horario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // GET: Horario/Create
        public IActionResult Create()
        {
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id");
            ViewData["IdZona"] = new SelectList(_context.Zonas, "Id", "Nombre");
            return View();
        }

        // POST: Horario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Horario horario, List<string> DiasSeleccionados)
        {
            if (ModelState.IsValid)
            {
                horario.Dia = ConvertirDiasABinario(DiasSeleccionados);
                _context.Add(horario);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Horario creado exitosamente!!!";
                return RedirectToAction("Index");
            }
            return View(horario);
        }

        // GET: Horario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.Horarios.FindAsync(id);
            if (horario == null)
            {
                return NotFound();
            }
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", horario.IdOperador);
            ViewData["IdZona"] = new SelectList(_context.Zonas, "Id", "Nombre", horario.IdZona);
            return View(horario);
        }

        // POST: Horario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Horario horario, List<string> DiasSeleccionados)
        {
            if (ModelState.IsValid)
            {
                var horarioEncontrado = await _context.Horarios.FindAsync(horario.Id);
                if (horarioEncontrado == null)
                {
                    return NotFound();
                }

                horarioEncontrado.HoraEntrada = horario.HoraEntrada;
                horarioEncontrado.HoraSalida = horario.HoraSalida;
                horarioEncontrado.Turno = horario.Turno;
                horarioEncontrado.IdOperador = horario.IdOperador;
                horarioEncontrado.IdZona = horario.IdZona;
                horarioEncontrado.Dia = ConvertirDiasABinario(DiasSeleccionados);

                _context.Update(horarioEncontrado);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Horario editado exitosamente!!!";
                return RedirectToAction("Index");
            }
            return View(horario);
        }

        // GET: Horario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // POST: Horario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == null || _context.Horarios == null)
            {
                return NotFound();
            }

            var horario = await _context.Horarios.FirstOrDefaultAsync(h => h.Id == id);

            if (horario == null)
            {
                return NotFound();
            }

            _context.Horarios.Remove(horario);
            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Horario eliminado exitosamente!!!";
            return RedirectToAction("Index");
        }

        private bool HorarioExists(int id)
        {
            return _context.Horarios.Any(e => e.Id == id);
        }

        private string ConvertirDiasABinario(List<string> dias)
        {
            string[] semana = new string[] { "0", "0", "0", "0", "0", "0", "0" };
            foreach (var dia in dias)
            {
                switch (dia)
                {
                    case "Lunes": semana[0] = "1"; break;
                    case "Martes": semana[1] = "1"; break;
                    case "Miércoles": semana[2] = "1"; break;
                    case "Jueves": semana[3] = "1"; break;
                    case "Viernes": semana[4] = "1"; break;
                    case "Sábado": semana[5] = "1"; break;
                    case "Domingo": semana[6] = "1"; break;
                }
            }
            return string.Join("", semana);
        }

        public IActionResult Calendar()
        {
            List<Horario> horarios = _context.Horarios.ToList();
            List<object> items = new List<object>();

            foreach (Horario horario in horarios)
            {
                var item = new
                {
                    id = horario.Id,
                    title = $"Operador {horario.IdOperador} - Turno {horario.Turno}",
                    start = DateTime.Today.Add(horario.HoraEntrada.ToTimeSpan()).ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = DateTime.Today.Add(horario.HoraSalida.ToTimeSpan()).ToString("yyyy-MM-ddTHH:mm:ss"),
                    dias = horario.Dia // Aquí se almacenan los días en formato "1001011"
                };
                items.Add(item);
            }

            ViewBag.Horarios = JsonConvert.SerializeObject(items);
            return View();
        }



    }
}
