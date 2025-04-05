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

        public async Task<IActionResult> Index( string operador, string zona, int topRegistro = 10)
        {
            var horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .Where(h => h.IdZonaNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value))
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(operador))
            {
                horarios = horarios.Where(h => h.IdOperadorNavigation.Nombre.ToLower().Contains(operador.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(zona))
            {
                horarios = horarios.Where(h => h.IdZonaNavigation.Nombre.ToLower().Contains(zona.ToLower()));
            }

            if (topRegistro > 0)
            {
                horarios = horarios.Take(topRegistro);
            }

            return View(await horarios.ToListAsync());
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

      

        public IActionResult GetZonas(int idDistrito)
        {
            var zonas = _context.Zonas
                                .Where(z => z.IdDistrito == idDistrito) // Filtrar zonas por distrito
                                .Select(z => new { z.Id, z.Nombre })
                                .ToList();
            return Json(zonas);
        }

        // Método para obtener el DateTime correcto según el día de la semana
        private DateTime GetDateTimeForDay(string dia, TimeOnly hora)
        {
            var today = DateTime.Today;
            var dayOfWeek = Enum.Parse<DayOfWeek>(dia, true); // Convertimos el nombre del día a DayOfWeek

            // Calculamos la fecha correcta del día de la semana (de acuerdo con el día actual)
            int daysToAdd = (int)dayOfWeek - (int)today.DayOfWeek;
            if (daysToAdd < 0)
                daysToAdd += 7;

            // Convertimos TimeOnly a TimeSpan
            TimeSpan horaEntrada = hora.ToTimeSpan();

            // Fecha final con la hora combinada
            return today.AddDays(daysToAdd).Add(horaEntrada);
        }

        public IActionResult FullCalendar()
        {
            return View();
        }

        public IActionResult GetHorarios()
        {
            List<Horario> horarios = new List<Horario>();

            var diasSemana = new Dictionary<int, string>
                {
                    {1, "Lunes"},
                    {2, "Martes"},
                    {3, "Miércoles"},
                    {4, "Jueves"},
                    {5, "Viernes"},
                    {6, "Sábado"},
                    {7, "Domingo"}
                };


            if (User.IsInRole("Alcaldia"))
            {
                horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .Where(s=>s.IdOperadorNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value))
                .ToList();
            }
            if (User.IsInRole("Operador"))
            {
                horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .Where(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value))
                .ToList();
            }
            if (User.IsInRole("Ciudadano"))
            {
                horarios = _context.Horarios
                .Include(h => h.IdOperadorNavigation)
                .Include(h => h.IdZonaNavigation)
                .Where(s => s.IdZonaNavigation.Nombre == User.FindFirst("Zona").Value)
                .ToList();
            }


            var events = horarios.SelectMany(h => h.Dia.Split(',')
                .Select(dia => new
                {
                    id = h.Id,
                    title = $"{h.IdOperadorNavigation.Nombre} - {h.IdZonaNavigation.Nombre}",
                    start = GetDateTimeForDay(dia, h.HoraEntrada),
                    end = GetDateTimeForDay(dia, h.HoraSalida),
                    operador = h.IdOperadorNavigation.Nombre,
                    zona = h.IdZonaNavigation.Nombre,
                    turno = h.Turno == 1 ? "Matutino" : "Vespertino",
                    horaEntrada = h.HoraEntrada.ToString("hh:mm tt"),  // Formato con AM/PM
                    horaSalida = h.HoraSalida.ToString("hh:mm tt"),
                    dia = string.Join(", ", h.Dia.Split(',').Select(d => diasSemana.ContainsKey(int.Parse(d)) ? diasSemana[int.Parse(d)] : ""))
                })).ToList();

            return Json(events);
        }


        private string ObtenerTurno(int turno)
        {
            return turno == 1 ? "Matutino" : "Vespertino";
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
