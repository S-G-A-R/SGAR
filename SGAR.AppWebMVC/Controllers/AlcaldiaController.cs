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
    // Este atributo restringe el acceso a este controlador a usuarios que tengan el rol "Alcaldia".
    [Authorize(Roles = "Alcaldia")] 
    // Esta línea define una clase llamada AlcaldiaController que hereda de la clase Controller, lo que la convierte en un controlador MVC.
    public class AlcaldiaController : Controller 
    {
        // Declara un campo privado de solo lectura llamado _context de tipo SgarDbContext.
        private readonly SgarDbContext _context;

        // Constructor de la clase AlcaldiaController.
        public AlcaldiaController(SgarDbContext context) 
        {
            // Inicializa el campo _context con la instancia de SgarDbContext proporcionada como argumento.
            _context = context; 
        }

        // Atributo que permite el acceso a esta acción sin requerir autenticación.
        [AllowAnonymous]
        // Definición de la acción Login del controlador.
        public IActionResult Login() 
        {
            // Devuelve la vista predeterminada asociada con la acción Login.
            return View(); 
        }

        // Permite el acceso a esta acción sin requerir autenticación.
        [AllowAnonymous]
        // Indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Definición de la acción Login que recibe un objeto Alcaldia como parámetro.
        public async Task<IActionResult> Login(Alcaldia alcaldia) 
        {
            // Genera un hash SHA-256 de la contraseña proporcionada por el usuario.
            alcaldia.Password = GenerarHash256(alcaldia.Password);
            // Busca en la base de datos una alcaldía que coincida con el correo y la contraseña hash.
            var alcaldiaAuth = await _context.Alcaldias .FirstOrDefaultAsync(s => s.Correo == alcaldia.Correo && s.Password == alcaldia.Password);
            // Busca el municipio asociado a la alcaldía autenticada.
            var municipio = await _context.Municipios.FirstOrDefaultAsync(s => s.Id == alcaldiaAuth.IdMunicipio);
            // Verifica si la autenticación fue exitosa.
            if (alcaldiaAuth != null && alcaldiaAuth.Id > 0 && alcaldiaAuth.Correo == alcaldia.Correo) 
            {
                // Crea una lista de claims (afirmaciones) que representan la identidad del usuario autenticado.
                var claims = new[] { 
                    // Claim que almacena el ID de la alcaldía.
                    new Claim("Id", alcaldiaAuth.Id.ToString()), 
                    // Claim que almacena el nombre del municipio asociado a la alcaldía.
                    new Claim("Municipio", municipio.Nombre), 
                    // Claim que almacena el rol del usuario (en este caso, el nombre de la clase Alcaldia).
                    new Claim(ClaimTypes.Role, alcaldiaAuth.GetType().Name) 
                    };
                // Crea una identidad basada en los claims y el esquema de autenticación de cookies.
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                // Inicia sesión del usuario utilizando la autenticación de cookies.
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                // Redirige al usuario a la acción Index del controlador Alcaldia después de iniciar sesión.
                return RedirectToAction("Index", "Alcaldia"); 
            }
            else
            {
                // Agrega un error al modelo si la autenticación falla.
                ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                // Devuelve la vista Login con el mensaje de error.
                return View(); 
            }
        }

        // Permite el acceso a esta acción sin requerir autenticación.
        [AllowAnonymous]
        // Definición de la acción CerrarSesion, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> CerrarSesion() 
        {
            // Cierra la sesión del usuario eliminando la cookie de autenticación.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirige al usuario a la acción Index del controlador Home después de cerrar la sesión.
            return RedirectToAction("Index", "Home"); 
        }

        // Comentario que indica que la siguiente acción responde a solicitudes HTTP GET.
        // GET: Alcaldia 
        // Definición de la acción Index, que devuelve un IActionResult.
        public IActionResult Index() 
        {
            // Devuelve la vista predeterminada asociada con la acción Index.
            return View(); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y lista las alcaldías.
        // GET: ListAlcaldias 
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción List, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> List(Alcaldia alcaldia, int topRegistro = 10) 
        {
            // Crea una consulta IQueryable para la tabla Alcaldias en la base de datos.
            var query = _context.Alcaldias.AsQueryable();

            // Si se proporciona un correo electrónico en el objeto alcaldia.
            if (!string.IsNullOrWhiteSpace(alcaldia.Correo))
                // Filtra la consulta para incluir solo las alcaldías cuyo correo electrónico contiene el valor proporcionado.
                query = query.Where(s => s.Correo.Contains(alcaldia.Correo));

            // Si se proporciona un IdMunicipio válido en el objeto alcaldia.
            if (alcaldia.IdMunicipio > 0)
                // Filtra la consulta para incluir solo las alcaldías que pertenecen al municipio especificado.
                query = query.Where(s => s.IdMunicipio == alcaldia.IdMunicipio);

            // Si se proporciona un valor válido para topRegistro.
            if (topRegistro > 0)
                // Limita el número de resultados devueltos por la consulta al valor especificado.
                query = query.Take(topRegistro);

            // Incluye la navegación a la entidad Municipio relacionada con cada alcaldía en los resultados.
            query = query.Include(p => p.IdMunicipioNavigation);
            // Ordena los resultados de la consulta por Id de alcaldía en orden descendente.
            query = query.OrderByDescending(s => s.Id);

            // Crea una lista de municipios con un elemento predeterminado "SELECCIONAR".
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            // Obtiene todos los departamentos de la base de datos.
            var departamentos = _context.Departamentos.ToList();
            // Agrega un elemento predeterminado "SELECCIONAR" a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            // Obtiene todos los municipios de la base de datos.
            var alcaldias = _context.Municipios.ToList();

            // Crea un SelectList para los municipios y lo asigna a ViewData["MunicipioId"].
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // Crea un SelectList para los departamentos y lo asigna a ViewData["DepartamentoId"].
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // Crea un SelectList para las alcaldías y lo asigna a ViewData["AlcaldiaId"].
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            // Devuelve la vista List con los resultados de la consulta como modelo en orden descendente.
            return View(await query.OrderByDescending(s => s.Id).ToListAsync()); 
        }

        // Esta línea define un método público llamado GetMunicipiosFromDepartamentoId.
        // El método toma un entero llamado departamentoId como parámetro.
        // El método devuelve un objeto JsonResult.
        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            // Esta línea construye y retorna un objeto JsonResult.
            // _context.Municipios accede a la tabla Municipios en la base de datos a través del contexto de la base de datos.
            // .Where(m => m.IdDepartamento == departamentoId) filtra los municipios para seleccionar solo aquellos cuyo IdDepartamento coincide con el valor proporcionado en el parámetro departamentoId.
            // .ToList() convierte el resultado filtrado en una lista de objetos Municipio.
            // Json(...) convierte la lista de municipios en un objeto JSON, que es el formato esperado por JsonResult.
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
            
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra los detalles de una alcaldía específica.
        // GET: Alcaldia/Details/5 
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Details, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Details(int? id) 
        {
            // Verifica si el parámetro id es nulo.
            if (id == null) 
            {
                // Si id es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca en la base de datos la alcaldía con el id proporcionado.
            var alcaldia = await _context.Alcaldias 
                .Include(a => a.IdMunicipioNavigation) // Incluye la navegación a la entidad Municipio relacionada con la alcaldía.
                .FirstOrDefaultAsync(m => m.Id == id); // Obtiene la primera alcaldía que coincide con el id proporcionado, o null si no se encuentra ninguna.

            // Verifica si la alcaldía encontrada es nula.
            if (alcaldia == null) 
            {
                // Si la alcaldía es nula, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Devuelve la vista Details con la alcaldía encontrada como modelo.
            return View(alcaldia); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra el formulario para crear una nueva alcaldía.
        // GET: Alcaldia/Create 
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Create, que devuelve un IActionResult.
        public IActionResult Create() 
        {
            // Crea una lista de municipios con un elemento predeterminado "SELECCIONAR".
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            // Obtiene todos los departamentos de la base de datos.
            var departamentos = _context.Departamentos.ToList();
            // Agrega un elemento predeterminado "SELECCIONAR" a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // Crea un SelectList para los municipios y lo asigna a ViewData["MunicipioId"].
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // Crea un SelectList para los departamentos y lo asigna a ViewData["DepartamentoId"].
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // Devuelve la vista Create.
            return View(); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y crea una nueva alcaldía.
        // POST: Alcaldia/Create 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Create, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Create([Bind("Id,IdMunicipio,Correo,Password")] Alcaldia alcaldia) 
        {
            // Bloque try para manejar excepciones.
            try
            {
                // Genera un hash SHA-256 de la contraseña proporcionada por el usuario.
                alcaldia.Password = GenerarHash256(alcaldia.Password);
                // Agrega la nueva alcaldía al contexto de la base de datos.
                _context.Add(alcaldia);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();
                // Redirige al usuario a la acción List después de crear la alcaldía.
                return RedirectToAction(nameof(List)); 
            }
            // Bloque catch para manejar excepciones.
            catch
            {
                // Crea una lista de municipios con un elemento predeterminado "SELECCIONAR".
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                // Obtiene todos los departamentos de la base de datos.
                var departamentos = _context.Departamentos.ToList();
                // Agrega un elemento predeterminado "SELECCIONAR" a la lista de departamentos.
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                // Crea un SelectList para los municipios y lo asigna a ViewData["MunicipioId"].
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                // Crea un SelectList para los departamentos y lo asigna a ViewData["DepartamentoId"].
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                // Devuelve la vista Create con la alcaldía proporcionada como modelo.
                return View(alcaldia); 
            }
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra el formulario para editar una alcaldía específica.
        // GET: Alcaldia/Edit/5 
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Edit, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Edit(int? id) 
        {
            // Verifica si el parámetro id es nulo.
            if (id == null) 
            {
                // Si id es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca en la base de datos la alcaldía con el id proporcionado.
            var alcaldia = await _context.Alcaldias.FindAsync(id);
            // Verifica si la alcaldía encontrada es nula.
            if (alcaldia == null) 
            {
                // Si la alcaldía es nula, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Crea una lista de municipios con un elemento predeterminado "SELECCIONAR".
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            // Obtiene todos los departamentos de la base de datos.
            var departamentos = _context.Departamentos.ToList();
            // Agrega un elemento predeterminado "SELECCIONAR" a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // Busca el municipio asociado a la alcaldía que se está editando.
            var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == alcaldia.IdMunicipio);
            // Agrega el municipio asociado a la alcaldía a la lista de municipios.
            municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });

            // Crea un SelectList para los municipios y lo asigna a ViewData["MunicipioId"].
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            // Crea un SelectList para los departamentos y lo asigna a ViewData["DepartamentoId"].
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // Devuelve la vista Edit con la alcaldía encontrada como modelo.
            return View(alcaldia); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y actualiza una alcaldía específica.
        // POST: Alcaldia/Edit/5 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Edit, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdMunicipio,Correo")] Alcaldia alcaldia) 
        {
            // Verifica si el id proporcionado en la URL coincide con el id de la alcaldía proporcionada en el cuerpo de la solicitud.
            if (id != alcaldia.Id) 
            {
                // Si los ids no coinciden, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Bloque try para manejar excepciones.
            try
            {
                // Actualiza la alcaldía en el contexto de la base de datos.
                _context.Update(alcaldia);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();
                // Redirige al usuario a la acción Index después de actualizar la alcaldía.
                return RedirectToAction(nameof(Index)); 
            }
            // Bloque catch para manejar excepciones de concurrencia de la base de datos.
            catch (DbUpdateConcurrencyException) 
            {
                // Verifica si la alcaldía existe en la base de datos.
                if (!AlcaldiaExists(alcaldia.Id)) 
                {
                    // Si la alcaldía no existe, devuelve un resultado NotFound (código de estado 404).
                    return NotFound(); 
                }
                // Si la alcaldía existe, pero ocurrió una excepción de concurrencia.
                else
                {
                    // Crea una lista de municipios con un elemento predeterminado "SELECCIONAR".
                    List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                    // Obtiene todos los departamentos de la base de datos.
                    var departamentos = _context.Departamentos.ToList();
                    // Agrega un elemento predeterminado "SELECCIONAR" a la lista de departamentos.
                    departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                    // Crea un SelectList para los municipios y lo asigna a ViewData["MunicipioId"].
                    ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                    // Crea un SelectList para los departamentos y lo asigna a ViewData["DepartamentoId"].
                    ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                    // Devuelve la vista Edit con la alcaldía proporcionada como modelo.
                    return View(alcaldia); 
                }
            }
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra la vista de confirmación para eliminar una alcaldía específica.
        // GET: Alcaldia/Delete/5 
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción Delete, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Delete(int? id) 
        {
            // Verifica si el parámetro id es nulo.
            if (id == null) 
            {
                // Si id es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca en la base de datos la alcaldía con el id proporcionado.
            var alcaldia = await _context.Alcaldias 
                .Include(a => a.IdMunicipioNavigation) // Incluye la navegación a la entidad Municipio relacionada con la alcaldía.
                .FirstOrDefaultAsync(m => m.Id == id); // Obtiene la primera alcaldía que coincide con el id proporcionado, o null si no se encuentra ninguna.

            // Verifica si la alcaldía encontrada es nula.
            if (alcaldia == null) 
            {
                // Si la alcaldía es nula, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Devuelve la vista Delete con la alcaldía encontrada como modelo.
            return View(alcaldia); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y confirma la eliminación de una alcaldía específica.
        // POST: Alcaldia/Delete/5 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST y que el nombre de la acción es "Delete".
        [HttpPost, ActionName("Delete")]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // Atributo que restringe el acceso a esta acción a usuarios con la política de autorización "Admin".
        [Authorize(Policy = "Admin")]
        // Definición de la acción DeleteConfirmed, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> DeleteConfirmed(int id) 
        {
            // Busca en la base de datos la alcaldía con el id proporcionado.
            var alcaldia = await _context.Alcaldias.FindAsync(id);
            // Verifica si la alcaldía encontrada no es nula.
            if (alcaldia != null)
            {
                // Elimina la alcaldía del contexto de la base de datos.
                _context.Alcaldias.Remove(alcaldia); 
            }

            // Guarda los cambios en la base de datos de forma asíncrona.
            await _context.SaveChangesAsync();
            // Redirige al usuario a la acción Index después de eliminar la alcaldía.
            return RedirectToAction(nameof(Index)); 
        }

        // Definición de un método privado que verifica si una alcaldía existe en la base de datos.
        private bool AlcaldiaExists(int id) 
        {
            // Devuelve verdadero si existe alguna alcaldía con el id proporcionado, falso en caso contrario.
            return _context.Alcaldias.Any(e => e.Id == id); 
        }

        // Definición de un método público estático que genera un hash SHA-256 de una cadena de entrada.
        public static string GenerarHash256(string input) 
        {
            // Crea una instancia de SHA256 para generar el hash. El bloque using asegura que el objeto se elimine correctamente después de su uso.
            using (SHA256 sha256Hash = SHA256.Create()) 
            {
                // Convierte la cadena de entrada a bytes utilizando la codificación UTF-8 y genera el hash SHA-256 de los bytes.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Crea un StringBuilder para construir la representación hexadecimal del hash.
                StringBuilder builder = new StringBuilder();
                // Itera a través de los bytes del hash.
                for (int i = 0; i < bytes.Length; i++) 
                {
                    // Convierte cada byte a su representación hexadecimal de dos dígitos y lo agrega al StringBuilder.
                    builder.Append(bytes[i].ToString("x2")); 
                }
                // Devuelve la representación hexadecimal del hash como una cadena.
                return builder.ToString(); 
            }
        }
    }
}
