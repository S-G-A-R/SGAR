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

        public IActionResult Index()
        {
            var horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .ToList();
            return View(horarios);
        }

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
            ViewBag.DiasSeleccionados = string.IsNullOrEmpty(horario.Dia)
            ? new List<int>()
            : horario.Dia.Split(',').Select(int.Parse).ToList();

            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });



            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });

            var distritoId = horario.IdZonaNavigation != null ? horario.IdZonaNavigation.IdDistrito : 0;

            var zonas = _context.Zonas.Where(z => z.IdDistrito == distritoId).ToList();
            zonas.Add(new Zona { Id = 0, Nombre = "Seleccione una zona" });

            // Convertir los días guardados en la BD a una lista
            ViewBag.DiasSeleccionados = horario.Dia.Split(',').Select(int.Parse).ToList();

            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", horario.IdZonaNavigation.IdDistrito);
            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);

            return View(horario);
        }


        [HttpPost]
        public IActionResult Edit(Horario horario, List<int> DiasSeleccionados)
        {
            try
            {
                var horarioExistente = _context.Horarios.Find(horario.Id);
                if (horarioExistente == null)
                {
                    ModelState.AddModelError("", "El horario no existe.");
                    return View(horario);
                }


                // Guardar días seleccionados en formato adecuado
                horarioExistente.Dia = string.Join(",", DiasSeleccionados);
                horarioExistente.IdOperador = horario.IdOperador;
                horarioExistente.IdZona = horario.IdZona;
                horarioExistente.HoraEntrada = horario.HoraEntrada;
                horarioExistente.HoraSalida = horario.HoraSalida;
                horarioExistente.Turno = horario.Turno;

                _context.Update(horarioExistente);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Recargar datos en caso de error
                var id = horario.Id;
                horario = _context.Horarios
                   .Include(h => h.IdOperadorNavigation)
                   .Include(h => h.IdZonaNavigation)
                   .FirstOrDefault(h => h.Id == id);

                var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
                var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
                var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
                distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });

                var zonas = _context.Zonas.Where(z => z.IdDistrito == horario.IdZonaNavigation.IdDistrito).ToList();
                zonas.Add(new Zona { Id = 0, Nombre = "Seleccione una zona" });

                var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
                operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });

                ViewBag.DiasSeleccionados = DiasSeleccionados;
                ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", horario.IdZonaNavigation.IdDistrito);
                ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
                ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);

                return View(horario);
            }


           
        }




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

            // Extraer los días seleccionados del atributo Dia
            var diasSeleccionados = string.IsNullOrEmpty(horario.Dia)
                ? new List<int>()
                : horario.Dia.Split(',').Select(int.Parse).ToList();

            ViewBag.DiasSeleccionados = diasSeleccionados;
            return View(horario);
        }

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

            // Extraer los días seleccionados del atributo Dia
            var diasSeleccionados = string.IsNullOrEmpty(horario.Dia)
                ? new List<int>()
                : horario.Dia.Split(',').Select(int.Parse).ToList();

            ViewBag.DiasSeleccionados = diasSeleccionados;

            return View(horario);
        }

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


        public IActionResult GetHorarios()
        {
            var horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation) // Cargar el operador
                .Include(h => h.IdZonaNavigation) // Cargar la zona
                .Select(h => new
                {
                    id = h.Id,
                    title = (h.IdOperadorNavigation != null ? h.IdOperadorNavigation.Nombre : "Sin operador") +
                            " - " +
                            (h.IdZonaNavigation != null ? h.IdZonaNavigation.Nombre : "Sin zona"),
                    start = DateTime.Today.Add(h.HoraEntrada.ToTimeSpan()),
                    end = DateTime.Today.Add(h.HoraSalida.ToTimeSpan()),
                    operadorId = h.IdOperador,
                    zonaId = h.IdZona
                })
                .ToList();

            return Json(horarios);
        }


        public IActionResult FullCalendar()
        {
            return View();
        }

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

    }
}
