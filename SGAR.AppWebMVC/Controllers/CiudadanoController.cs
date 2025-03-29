using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using System.Text;
using System.Security.Cryptography;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Ciudadano, Alcaldia")]
    public class CiudadanoController : Controller
    {
        private readonly SgarDbContext _context;

        public CiudadanoController(SgarDbContext context)
        {
            _context = context;
        }
        public IActionResult Menu()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(Ciudadano ciudadano)
        {
            try
            {
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                var ciudadanoAuth = await _context.Ciudadanos
                    .FirstOrDefaultAsync(s => s.Correo == ciudadano.Correo && s.Password == ciudadano.Password);
                var zona = await _context.Zonas.FirstOrDefaultAsync(s => s.Id == ciudadanoAuth.ZonaId);
                var alcaldia = await _context.Alcaldias.FirstOrDefaultAsync(s => s.Id == zona.IdAlcaldia);
                if (ciudadanoAuth != null && ciudadanoAuth.Id > 0 && ciudadanoAuth.Correo == ciudadano.Correo)
                {
                    var claims = new[] {
                    new Claim("Id", ciudadanoAuth.Id.ToString()),
                    new Claim("Zona", zona.Nombre),
                    new Claim("Alcaldia", alcaldia.Id.ToString()),
                    new Claim("Nombre", ciudadanoAuth.Nombre + " " + ciudadanoAuth.Apellido),
                    new Claim(ClaimTypes.Role, ciudadanoAuth.GetType().Name) };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    return RedirectToAction("Menu", "Ciudadano");
                }
                else
                {
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    return View();
                }
            }
            catch
            {
                return View(ciudadano);
            }
            
        }
        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        // GET: Ciudadano
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Index()
        {
            var sgarDbContext = _context.Ciudadanos.Include(c => c.Zona);
            return View(await sgarDbContext.ToListAsync());
        }

        // GET: Ciudadano/Details/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudadano = await _context.Ciudadanos
                .Include(c => c.Zona)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ciudadano == null)
            {
                return NotFound();
            }

            return View(ciudadano);
        }
        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        public JsonResult GetDistritosFromMunicipioId(int municipioId)
        {
            return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());
        }
        public JsonResult GetZonasFromDistritoId(int distritoId)
        {
            return Json(_context.Zonas.Where(m => m.IdDistrito == distritoId).ToList());
        }

        // GET: Ciudadano/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", 0);
            return View();
        }

        // POST: Ciudadano/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Dui,Correo,Password,ZonaId")] Ciudadano ciudadano)
        {
            try
            {
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                _context.Add(ciudadano);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }

            catch
            {
                List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);
                return View(ciudadano);
            }
            
        }

        // GET: Ciudadano/Edit/5

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            if (ciudadano == null)
            {
                return NotFound();
            }
            ViewData["ZonaId"] = new SelectList(_context.Zonas, "Id", "Nombre", ciudadano.ZonaId);
            return View(ciudadano);
        }

        // POST: Ciudadano/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Dui,Correo,Password,ZonaId")] Ciudadano ciudadano)
        {
            if (id != ciudadano.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ciudadano);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CiudadanoExists(ciudadano.Id))
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
            ViewData["ZonaId"] = new SelectList(_context.Zonas, "Id", "Nombre", ciudadano.ZonaId);
            return View(ciudadano);
        }

        // GET: Ciudadano/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudadano = await _context.Ciudadanos
                .Include(c => c.Zona)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ciudadano == null)
            {
                return NotFound();
            }

            return View(ciudadano);
        }

        // POST: Ciudadano/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            if (ciudadano != null)
            {
                _context.Ciudadanos.Remove(ciudadano);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CiudadanoExists(int id)
        {
            return _context.Ciudadanos.Any(e => e.Id == id);
        }

        public static string GenerarHash256(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convierte la contraseña a un arreglo de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convierte el arreglo de bytes a una cadena hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
