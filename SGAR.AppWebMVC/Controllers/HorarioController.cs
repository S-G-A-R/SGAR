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
using System.Security.Policy;

namespace SGAR.AppWebMVC.Controllers
{
    public class HorarioController : Controller
    {
        private readonly SgarDbContext _context;

        public HorarioController(SgarDbContext context)
        {
            _context = context;
        }


        // Acción para mostrar la lista de horarios en la vista Index
        public IActionResult Index()
        {
            var horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .ToList();
            return View(horarios);
        }

        // Acción para mostrar el formulario de crear horario
        public IActionResult Create()
        {
            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s=>s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            var distritos = _context.Distritos.Where(s=>s.IdMunicipio == yourMunicipio.Id).ToList();
            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
            var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona"}]);

            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            operadores.Add(new Operador { Id = 0 , Nombre = "Seleccione un operador"});

            // Agregar la lista de distritos a ViewData
            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", 0);
            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", 0);
            // También puedes agregar el título de la vista
            ViewData["Title"] = "Crear Horario";

            return View();
        }


        // Acción POST para crear un nuevo horario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Horario horario, List<int> DiasSeleccionados)
        {
            try
            {
                // Convertir los días seleccionados en un formato adecuado
                horario.Dia = string.Join(",", DiasSeleccionados); // O bien usar un formato binario, etc.

                // Verifica que el IdZona esté correcto
                Debug.WriteLine($"Zona seleccionada: {horario.IdZona}");

                _context.Horarios.Add(horario);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Recargar las listas de operadores y zonas en caso de error

                var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
                var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
                var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
                distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
                var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona" }]);

                var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
                operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });
                ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", 0);
                ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", 0);
                return View(horario);
            }  
            

            
        }


        // Acción Editar
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var horario = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .FirstOrDefault(h => h.Id == id);

            if (horario == null)
            {
                return NotFound();
            }

            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
            var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona" }]);

            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });

            // Agregar la lista de distritos a ViewData
            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);
            return View(horario);
        }

        [HttpPost]
        public IActionResult Edit(Horario horario)
        {
            if (ModelState.IsValid)
            {
                _context.Update(horario);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
            var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona" }]);

            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });

            // Agregar la lista de distritos a ViewData
            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);
            return View(horario);
        }

        // Acción Detalles
        public IActionResult Details(int id)
        {
            var horario = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .FirstOrDefault(h => h.Id == id);

            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // Acción Eliminar (GET) - muestra la vista de confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var horario = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .FirstOrDefault(h => h.Id == id);

            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // Acción Eliminar (POST) - ejecuta la eliminación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var horario = _context.Horarios.Find(id);
            if (horario != null)
            {
                _context.Horarios.Remove(horario);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // 1️⃣ Obtener horarios para FullCalendar
        public IActionResult GetHorarios()
        {
            var horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .Select(h => new
                {
                    id = h.Id,
                    title = h.IdOperadorNavigation.Nombre + " - " + h.IdZonaNavigation.Nombre,
                    start = DateTime.Today.Add(h.HoraEntrada.ToTimeSpan()),
                    end = DateTime.Today.Add(h.HoraSalida.ToTimeSpan())
                })
                .ToList();

            return Json(horarios);
        }

        public IActionResult FullCalendar()
        {
            return View();
        }

        // 2️⃣ Crear un nuevo horario
        [HttpPost]
        public IActionResult CreateHorario([FromBody] Horario horario)
        {
            if (ModelState.IsValid)
            {
                _context.Horarios.Add(horario);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // 3️⃣ Actualizar un horario
        [HttpPost]
        public IActionResult UpdateHorario([FromBody] Horario horario)
        {
            if (ModelState.IsValid)
            {
                _context.Horarios.Update(horario);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // 4️⃣ Eliminar un horario (separado para POST)
        [HttpPost]
        public IActionResult DeleteHorario(int id)
        {
            var horario = _context.Horarios.Find(id);
            if (horario != null)
            {
                _context.Horarios.Remove(horario);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // Acción para obtener zonas por distrito
        public IActionResult GetZonas(int idDistrito)
        {
            var zonas = _context.Zonas
                                .Where(z => z.IdDistrito == idDistrito) // Filtrar zonas por distrito
                                .ToList();
            return Json(zonas);
        }


    }
}
