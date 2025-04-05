using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    public class NotificacionesUbicacionController : Controller
    {
        private readonly SgarDbContext _context;

        public NotificacionesUbicacionController(SgarDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Ciudadano")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificacionesUbicacion notificacionesUbicacion)
        {
            try
            {
                _context.NotificacionesUbicaciones.Add(notificacionesUbicacion);
                await _context.SaveChangesAsync();
                return Json(notificacionesUbicacion);
            }
            catch
            {
                return Json(notificacionesUbicacion);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NotificacionesUbicacion notificacionesUbicacion)
        {
            try
            {

                var notiUbicacionUpdate = await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(u => u.Id == notificacionesUbicacion.Id);
                notiUbicacionUpdate.Titulo = notificacionesUbicacion.Titulo;
                notiUbicacionUpdate.Latitud = notificacionesUbicacion.Latitud;
                notiUbicacionUpdate.Estado = notificacionesUbicacion.Estado;
                notiUbicacionUpdate.DistanciaMetros = notificacionesUbicacion.DistanciaMetros;

                _context.NotificacionesUbicaciones.Update(notiUbicacionUpdate);
                await _context.SaveChangesAsync();
                return Json(notificacionesUbicacion);
            }
            catch
            {
                return Json(notificacionesUbicacion);
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var notificacionesUbicacion = await _context.NotificacionesUbicaciones.FindAsync(id);
            if (notificacionesUbicacion != null)
            {
                _context.NotificacionesUbicaciones.Remove(notificacionesUbicacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificacionesUbicacionExists(int id)
        {
            return _context.NotificacionesUbicaciones.Any(e => e.Id == id);
        }

        public async Task<JsonResult> GetUbicationFromCdId()
        {
            return Json(await _context.NotificacionesUbicaciones.FirstOrDefaultAsync(s => s.IdCiudadano == Convert.ToInt32(User.FindFirst("Id").Value)));

        }

        public async Task<JsonResult> GetOpUbication()
        {
            var zona = await _context.Zonas.FirstOrDefaultAsync(x=>x.Nombre == User.FindFirst("Zona").Value);
            var horario = await _context.Horarios.FirstOrDefaultAsync(x => x.IdZona == zona.Id);
            var ubicacion = await _context.Ubicaciones.FirstOrDefaultAsync(x => x.IdOperador == horario.IdOperador);

            return Json(ubicacion);

        }
    }
}
