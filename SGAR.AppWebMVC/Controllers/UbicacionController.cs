using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using NuGet.Protocol;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    
    public class UbicacionController : Controller
    {
        private readonly SgarDbContext _context;

        public UbicacionController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Ubicacion
        [Authorize(Roles = "Operador")]
        public IActionResult Index()
        {

            var ubicacionExists = _context.Ubicaciones.FirstOrDefault(s=>s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value));
            var ubicacion = new Ubicacion();
            if (ubicacionExists != null)
            {
                ubicacion = ubicacionExists;
            }

            return View(ubicacion);
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Ubicacion ubicacion)
        {
            try
            {
                _context.Ubicaciones.Add(ubicacion);
                await _context.SaveChangesAsync();
                return Json(ubicacion);
            }
            catch
            {
                return Json(ubicacion);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit([FromBody] Ubicacion ubicacion)
        {

            try
            {

                var ubicacionUpdate = await _context.Ubicaciones.FirstOrDefaultAsync(u => u.Id == ubicacion.Id);
                ubicacionUpdate.Longitud = ubicacion.Longitud;
                ubicacionUpdate.Latitud = ubicacion.Latitud;
                ubicacionUpdate.FechaActualizacion = ubicacion.FechaActualizacion;

                _context.Ubicaciones.Update(ubicacionUpdate);
                await _context.SaveChangesAsync();
                return Json(ubicacion);
            }
            catch 
            {
                return Json(ubicacion);
            }
        }


        [HttpPost]
        public async void Delete(int id)
        {
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion != null)
            {
                _context.Ubicaciones.Remove(ubicacion);
            }

            await _context.SaveChangesAsync();
        }

        private bool UbicacionExists(int id)
        {
            return _context.Ubicaciones.Any(e => e.IdOperador == id);
        }

        public async Task<JsonResult> GetUbicationFromOpId(int id)
        {
            return Json(await _context.Ubicaciones.FirstOrDefaultAsync(s => s.IdOperador == id));

        }
    }
}
