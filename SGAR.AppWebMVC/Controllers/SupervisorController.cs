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
    [Authorize(Roles = "Alcaldia, Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly SgarDbContext _context;

        public SupervisorController(SgarDbContext context)
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
        public async Task<IActionResult> Login(Supervisor supervisor)
        {
            try
            {
                supervisor.Password = GenerarHash256(supervisor.Password);
                var supervisorAuth = await _context.Supervisores.FirstOrDefaultAsync(s => s.CorreoLaboral == supervisor.CorreoLaboral && s.Password == supervisor.Password);

                if (supervisorAuth != null && supervisorAuth.Id > 0 && supervisorAuth.CorreoLaboral == supervisor.CorreoLaboral)
                {
                    var claims = new[] {
                        new Claim("Id", supervisorAuth.Id.ToString()),
                        new Claim("Alcaldia", supervisorAuth.IdAlcaldia.ToString()),
                        new Claim("Nombre", supervisorAuth.Nombre + " " + supervisorAuth.Apellido),
                        new Claim(ClaimTypes.Role, supervisorAuth.GetType().Name)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    return RedirectToAction("Menu", "Supervisor");
                }
                else
                {
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    return View();
                }
            }
            catch
            {
                return View(supervisor);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Supervisor
        public async Task<IActionResult> Index(Supervisor supervisor, int topRegistro = 10)
        {
            var query = _context.Supervisores.AsQueryable();
            if (!string.IsNullOrWhiteSpace(supervisor.Codigo))
                query = query.Where(s => s.Codigo.Contains(supervisor.Codigo));
            if (!string.IsNullOrWhiteSpace(supervisor.Dui))
                query = query.Where(s => s.Dui.Contains(supervisor.Dui));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.OrderByDescending(s => s.Id);
            if (!(User.FindFirst("Id").Value == "1"))
                query = query.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value));
            return View(await query.ToListAsync());
        }

        // GET: Supervisor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisor = await _context.Supervisores
                .Include(o => o.IdAlcaldiaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            supervisor.ReferentesSupervisores = referentes;

            var tipos = new SortedList<int, string>();
            if (supervisor == null)
            {
                return NotFound();
            }
            if (supervisor.ReferentesSupervisores != null)
            {
                int numItem = 1;
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            return View(supervisor);
        }

        // GET: Supervisor/Create
        public IActionResult Create()
        {
            return View(new Supervisor());
        }

        // POST: Supervisor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supervisor supervisor)
        {
            var tipos = new SortedList<int, string>();
            try
            {
                
                if (supervisor.FotoFile != null)
                {
                    supervisor.Foto = await GenerarByteImage(supervisor.FotoFile);
                }
                supervisor.IdAlcaldia = Convert.ToInt32(User.FindFirst("Id").Value);
                supervisor.Password = GenerarHash256(supervisor.Password);

                _context.Add(supervisor);
                await _context.SaveChangesAsync();

                var supervisorId = _context.Supervisores.FirstOrDefault(s => s.Codigo == supervisor.Codigo).Id;

                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    item.IdSupervisor = supervisorId;
                    _context.ReferentesSupervisores.Add(item);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));

            }
            catch
            {

                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                return View(supervisor);
            }
        }

        // GET: Supervisor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisor = await _context.Supervisores.FindAsync(id);

            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            supervisor.ReferentesSupervisores = referentes;

            var tipos = new SortedList<int, string>();
            if (supervisor == null)
            {
                return NotFound();
            }
            if (supervisor.ReferentesSupervisores != null)
            {
                int numItem = 1;
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }
            return View(supervisor);
        }

        // POST: Supervisor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supervisor supervisor)
        {
            if (id != supervisor.Id)
            {
                return NotFound();
            }

            var supervisorUpdate = await _context.Supervisores
                .FirstOrDefaultAsync(o => o.Id == supervisor.Id);

            try
            {
                supervisorUpdate.Nombre = supervisor.Nombre;
                supervisorUpdate.Apellido = supervisor.Apellido;
                supervisorUpdate.Telefono = supervisor.Telefono;
                supervisorUpdate.CorreoPersonal = supervisor.CorreoPersonal;
                supervisorUpdate.Dui = supervisor.Dui;
                supervisorUpdate.Codigo = supervisor.Codigo;
                supervisorUpdate.CorreoLaboral = supervisor.CorreoLaboral;
                supervisorUpdate.TelefonoLaboral = supervisor.TelefonoLaboral;


                var fotoAnterior = await _context.Supervisores
                    .Where(s => s.Id == supervisor.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                supervisorUpdate.Foto = await GenerarByteImage(supervisor.FotoFile, fotoAnterior);

                var listaIds = supervisor.ReferentesSupervisores.Select(s => s.Id).ToList();
                var referentes = await _context.ReferentesSupervisores.Where(s => s.IdSupervisor == supervisor.Id).Select(s => s.Id).ToListAsync();
                _context.Update(supervisorUpdate);
                await _context.SaveChangesAsync();

                var referentDel = new List<ReferentesSupervisor>();
                foreach (var referente in referentes)
                {
                    var existe = listaIds.FirstOrDefault(s => s == referente);
                    if (!(existe > 0))
                    {
                        var find = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        if (find != null)
                            referentDel.Add(find);
                    }
                    else
                    {
                        var fetch = await _context.ReferentesSupervisores.FirstOrDefaultAsync(s => s.Id == referente);
                        var refe = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.Id == referente);

                        fetch.Parentesco = refe.Parentesco;
                        fetch.Tipo = refe.Tipo;
                        fetch.Nombre = refe.Nombre;
                        _context.ReferentesSupervisores.Update(fetch);
                        await _context.SaveChangesAsync();
                    }
                }

                if (referentDel.Count > 0)
                {
                    foreach (var item in referentDel)
                        _context.ReferentesSupervisores.Remove(item);
                    await _context.SaveChangesAsync();
                }


                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupervisorExists(supervisor.Id))
                {
                    return NotFound();
                }
                else
                {
                    return View(supervisor);
                }
            }
        }

        // GET: Supervisor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisor = await _context.Supervisores
                .Include(o => o.IdAlcaldiaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            var referentes = _context.ReferentesSupervisores
                .Where(s => s.IdSupervisor == supervisor.Id).ToList();

            supervisor.ReferentesSupervisores = referentes;

            var tipos = new SortedList<int, string>();
            if (supervisor == null)
            {
                return NotFound();
            }
            if (supervisor.ReferentesSupervisores != null)
            {
                int numItem = 1;
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            return View(supervisor);
        }

        // POST: Supervisor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supervisor = await _context.Supervisores.FindAsync(id);
            if (supervisor != null)
            {
                _context.Supervisores.Remove(supervisor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupervisorExists(int id)
        {
            return _context.Supervisores.Any(e => e.Id == id);
        }

        public static string GenerarHash256(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
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

        public IActionResult AddReferenteSu(Supervisor supervisor)
        {
            var tipos = new SortedList<int, string>();

            if (supervisor.ReferentesSupervisores == null)
                supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
            supervisor.ReferentesSupervisores.Add(new ReferentesSupervisor
            {
                Nombre = "",
                NumItem = supervisor.ReferentesSupervisores.Count + 1,
                Parentesco = "",
                Tipo = 1
            });

            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);

            return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
        }

        public IActionResult DeleteReferenteSu(int id, Supervisor supervisor)
        {
            var tipos = new SortedList<int, string>();
            int num = id;
            if (supervisor.ReferentesSupervisores.Count == 0)
                supervisor.ReferentesSupervisores = new List<ReferentesSupervisor>();
            var referenteDel = supervisor.ReferentesSupervisores.FirstOrDefault(s => s.NumItem == num);
            if (referenteDel != null)
            {
                supervisor.ReferentesSupervisores.Remove(referenteDel);
                int numItemNew = 1;
                foreach (var item in supervisor.ReferentesSupervisores)
                {
                    item.NumItem = numItemNew;
                    numItemNew++;
                }
            }
            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            return PartialView("_ReferentesSupervisor", supervisor.ReferentesSupervisores);
        }
    }
}

