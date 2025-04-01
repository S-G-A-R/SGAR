using System;
using System.Collections;
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

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Roles = "Alcaldia, Operador")]
    public class OperadorController : Controller
    {
        private readonly SgarDbContext _context;

        public OperadorController(SgarDbContext context)
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
        public async Task<IActionResult> Login(Operador operador)
        {
            try
            {
                operador.Password = GenerarHash256(operador.Password);
                var operadorAuth = await _context.Operadores.FirstOrDefaultAsync(s => s.CorreoLaboral == operador.CorreoLaboral && s.Password == operador.Password);

                if (operadorAuth != null && operadorAuth.Id > 0 && operadorAuth.CorreoLaboral == operador.CorreoLaboral)
                {
                    var claims = new[] { 
                        new Claim("Id", operadorAuth.Id.ToString()), 
                        new Claim("Alcaldia", operadorAuth.IdAlcaldia.ToString()), 
                        new Claim("Nombre", operadorAuth.Nombre + " " + operadorAuth.Apellido), 
                        new Claim(ClaimTypes.Role, operadorAuth.GetType().Name)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    return RedirectToAction("Menu", "Operador");
                }
                else
                {
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    return View();
                }
            }
            catch
            {
                return View(operador);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Operador
        [Authorize(Roles = "Alcaldia")]
        public async Task<IActionResult> Index(Operador operador, int topRegistro = 10)
        {
            var query = _context.Operadores.AsQueryable();
            if (!string.IsNullOrWhiteSpace(operador.Nombre))
                query = query.Where(s => s.Nombre.Contains(operador.Nombre));
            if (!string.IsNullOrWhiteSpace(operador.Dui))
                query = query.Where(s => s.Dui.Contains(operador.Dui));
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.OrderByDescending(s => s.Id);
            if (!(User.FindFirst("Id").Value == "1"))
                query = query.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value));
            return View(await query.ToListAsync());
        }
        // GET: Operador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores
                .Include(o => o.IdAlcaldiaNavigation)
                .Include(o => o.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            var referentes =  _context.ReferentesOperadores
                .Where(s => s.IdOperador == operador.Id).ToList();

            operador.ReferentesOperador = referentes;

            var tipos = new SortedList<int, string>();
            if (operador == null)
            {
                return NotFound();
            }
            if (operador.ReferentesOperador != null)
            {
                int numItem = 1;
                foreach (var item in operador.ReferentesOperador)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            return View(operador);
        }

        // GET: Operador/Create
        public IActionResult Create()
        {
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id");
            return View(new Operador());
        }

        // POST: Operador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Operador operador)
        {
            var tipos = new SortedList<int, string>();
            try
            {
                if (operador.SolvenciaFile != null)
                {
                    operador.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile);
                }

                if (operador.LicenciaFile != null)
                {
                    operador.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile);
                }

                if (operador.AntecedentesFile != null)
                {
                    operador.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile);
                }

                if (operador.FotoFile != null)
                {
                    operador.Foto = await GenerarByteImage(operador.FotoFile);
                }
                operador.IdAlcaldia = Convert.ToInt32(User.FindFirst("Id").Value);
                operador.Password = GenerarHash256(operador.Password);

                _context.Add(operador);
                await _context.SaveChangesAsync();

                var operadorId = _context.Operadores.FirstOrDefault(s => s.CodigoOperador == operador.CodigoOperador).Id;

                foreach (var item in operador.ReferentesOperador)
                {
                    item.IdOperador = operadorId;
                    _context.ReferentesOperadores.Add(item);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));

            }
            catch
            {
                ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
                
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
                return View(operador);
            }



        }
        // GET: Operador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores.FindAsync(id);

            var referentes = _context.ReferentesOperadores
                .Where(s => s.IdOperador == operador.Id).ToList();

            operador.ReferentesOperador = referentes;

            var tipos = new SortedList<int, string>();
            if (operador == null)
            {
                return NotFound();
            }
            if (operador.ReferentesOperador != null)
            {
                int numItem = 1;
                foreach (var item in operador.ReferentesOperador)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Id", operador.VehiculoId);
            return View(operador);
        }

        // POST: Operador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Operador operador)
        {
            if (id != operador.Id)
            {
                return NotFound();
            }

            var operadorUpdate = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == operador.Id);

            try
            {
                operadorUpdate.Nombre = operador.Nombre;
                operadorUpdate.Apellido = operador.Apellido;
                operadorUpdate.TelefonoPersonal = operador.TelefonoPersonal;
                operadorUpdate.CorreoPersonal = operador.CorreoPersonal;
                operadorUpdate.Dui = operador.Dui;
                operadorUpdate.Ayudantes = operador.Ayudantes;
                operadorUpdate.CodigoOperador = operador.CodigoOperador;
                operadorUpdate.CorreoLaboral = operador.CorreoLaboral;
                operadorUpdate.TelefonoLaboral = operador.TelefonoLaboral;
                operadorUpdate.VehiculoId = operador.VehiculoId;

                var solvenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.SolvenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.SolvenciaDoc = await GenerarByteImage(operador.SolvenciaFile, solvenciaAnterior);

                var licenciaAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.LicenciaDoc).FirstOrDefaultAsync();
                operadorUpdate.LicenciaDoc = await GenerarByteImage(operador.LicenciaFile, licenciaAnterior);

                var antecendeAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.AntecedentesDoc).FirstOrDefaultAsync(); // Se corrige la propiedad
                operadorUpdate.AntecedentesDoc = await GenerarByteImage(operador.AntecedentesFile, antecendeAnterior);

                var fotoAnterior = await _context.Operadores
                    .Where(s => s.Id == operador.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                operadorUpdate.Foto = await GenerarByteImage(operador.FotoFile, fotoAnterior);

                var listaIds = operador.ReferentesOperador.Select(s => s.Id).ToList();
                var referentes = await _context.ReferentesOperadores.Where(s=>s.IdOperador == operador.Id).Select(s=>s.Id).ToListAsync();
                _context.Update(operadorUpdate);
                await _context.SaveChangesAsync();

                var referentDel = new List<ReferentesOperador>();
                foreach(var referente in referentes)
                {
                    var existe = listaIds.FirstOrDefault(s => s == referente);
                    if (!(existe > 0))
                    {
                        var find = await _context.ReferentesOperadores.FirstOrDefaultAsync(s => s.Id == referente);
                        if (find != null)
                            referentDel.Add(find);
                    }
                    else
                    {
                        var fetch = await _context.ReferentesOperadores.FirstOrDefaultAsync(s => s.Id == referente);
                        var refe = operador.ReferentesOperador.FirstOrDefault(s => s.Id == referente);

                        fetch.Parentesco = refe.Parentesco;
                        fetch.Tipo = refe.Tipo;
                        fetch.Nombre = refe.Nombre;
                        _context.ReferentesOperadores.Update(fetch);
                        await _context.SaveChangesAsync();
                    }
                }

                if(referentDel.Count > 0)
                {
                    foreach(var item in referentDel) 
                        _context.ReferentesOperadores.Remove(item);
                    await _context.SaveChangesAsync();
                }


                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperadorExists(operador.Id))
                {
                    return NotFound();
                }
                else
                {
                    return View(operador);
                }
            }
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

        // GET: Operador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operador = await _context.Operadores
                .Include(o => o.IdAlcaldiaNavigation)
                .Include(o => o.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            var referentes = _context.ReferentesOperadores
                .Where(s => s.IdOperador == operador.Id).ToList();

            operador.ReferentesOperador = referentes;

            var tipos = new SortedList<int, string>();
            if (operador == null)
            {
                return NotFound();
            }
            if (operador.ReferentesOperador != null)
            {
                int numItem = 1;
                foreach (var item in operador.ReferentesOperador)
                {
                    item.NumItem = numItem;
                    numItem++;
                }
                tipos.Add(1, "Personal");
                tipos.Add(2, "Laboral");
                ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            }

            return View(operador);
        }

        // POST: Operador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var operador = await _context.Operadores.FindAsync(id);
            if (operador != null)
            {
                var horarios = await _context.Horarios.Where(s=>s.IdOperador == operador.Id).ToListAsync();
                foreach (var horario in horarios)
                {
                    _context.Horarios.Remove(horario);
                    await _context.SaveChangesAsync();
                }
                _context.Operadores.Remove(operador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private bool OperadorExists(int id)
        {
            return _context.Operadores.Any(e => e.Id == id);
        }

        public IActionResult AddReferenteOp(Operador operador)
        {
            var tipos = new SortedList<int, string>();
            
            if (operador.ReferentesOperador == null)
                operador.ReferentesOperador = new List<ReferentesOperador>();
            operador.ReferentesOperador.Add(new ReferentesOperador
            {
                Nombre = "",
                NumItem = operador.ReferentesOperador.Count + 1,
                Parentesco = "",
                Tipo = 1
            });

            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            
            return PartialView("_ReferentesOperador", operador.ReferentesOperador);
        }

        public IActionResult DeleteReferenteOp(int id, Operador operador)
        {
            var tipos = new SortedList<int, string>();
            int num = id;
            if (operador.ReferentesOperador.Count == 0)
                operador.ReferentesOperador = new List<ReferentesOperador>();
            var referenteDel = operador.ReferentesOperador.FirstOrDefault(s => s.NumItem == num);
            if (referenteDel != null)
            {
                operador.ReferentesOperador.Remove(referenteDel);
                int numItemNew = 1;
                foreach (var item in operador.ReferentesOperador)
                {
                    item.NumItem = numItemNew;
                    numItemNew++;
                }
            }
            tipos.Add(1, "Personal");
            tipos.Add(2, "Laboral");
            ViewBag.Tipos = new SelectList(tipos, "Key", "Value", 1);
            return PartialView("_ReferentesOperador", operador.ReferentesOperador);
        }
    }
}
