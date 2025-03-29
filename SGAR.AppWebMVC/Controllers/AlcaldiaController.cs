using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using System.Security.Policy;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Alcaldia")]
    public class AlcaldiaController : Controller
    {
        private readonly SgarDbContext _context;

        public AlcaldiaController(SgarDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(Alcaldia alcaldia)
        {
            alcaldia.Password = GenerarHash256(alcaldia.Password);
            var alcaldiaAuth = await _context.Alcaldias
                .FirstOrDefaultAsync(s => s.Correo == alcaldia.Correo && s.Password == alcaldia.Password);
            var municipio = await _context.Municipios.FirstOrDefaultAsync(s => s.Id == alcaldiaAuth.IdMunicipio);
            if (alcaldiaAuth != null && alcaldiaAuth.Id > 0 && alcaldiaAuth.Correo == alcaldia.Correo)
            {
                var claims = new[] {
                    new Claim("Id", alcaldiaAuth.Id.ToString()),
                    new Claim("Municipio", municipio.Nombre),
                    new Claim(ClaimTypes.Role, alcaldiaAuth.GetType().Name)
                    };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Index", "Alcaldia");
            }
            else
            {
                ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                return View();
            }
        }
        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Alcaldia
        public IActionResult Index()
        {
            return View();
        }

        // GET: ListAlcaldias
        public async Task<IActionResult> List(Alcaldia alcaldia, int topRegistro = 10)
        {

            var query = _context.Alcaldias.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(alcaldia.Correo))
                query = query.Where(s => s.Correo.Contains(alcaldia.Correo));
            if (alcaldia.IdMunicipio > 0)
                query = query.Where(s => s.IdMunicipio == alcaldia.IdMunicipio);
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.IdMunicipioNavigation);
            query = query.OrderByDescending(s => s.Id);

            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            var alcaldias = _context.Municipios.ToList();
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        // GET: Alcaldia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alcaldia = await _context.Alcaldias
                .Include(a => a.IdMunicipioNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alcaldia == null)
            {
                return NotFound();
            }

            return View(alcaldia);
        }

        // GET: Alcaldia/Create
        public IActionResult Create()
        {
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            return View();
        }

        // POST: Alcaldia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdMunicipio,Correo,Password")] Alcaldia alcaldia)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    alcaldia.Password = GenerarHash256(alcaldia.Password);
                    _context.Add(alcaldia);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List));
                }
            }
            catch
            {
                return RedirectToAction(nameof(List));
            }

            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            return View(alcaldia);
        }

        // GET: Alcaldia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alcaldia = await _context.Alcaldias.FindAsync(id);
            if (alcaldia == null)
            {
                return NotFound();
            }

            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == alcaldia.IdMunicipio);
            municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });

            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            return View(alcaldia);
        }

        // POST: Alcaldia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMunicipio,Correo")] Alcaldia alcaldia)
        {
            if (id != alcaldia.Id)
            {
                return NotFound();
            }

           
                try
                {
                    _context.Update(alcaldia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlcaldiaExists(alcaldia.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return RedirectToAction(nameof(List));
                    }
                }

            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            return View(alcaldia);
        }

        // GET: Alcaldia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alcaldia = await _context.Alcaldias
                .Include(a => a.IdMunicipioNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alcaldia == null)
            {
                return NotFound();
            }

            return View(alcaldia);
        }

        // POST: Alcaldia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alcaldia = await _context.Alcaldias.FindAsync(id);
            if (alcaldia != null)
            {
                _context.Alcaldias.Remove(alcaldia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlcaldiaExists(int id)
        {
            return _context.Alcaldias.Any(e => e.Id == id);
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
