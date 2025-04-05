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
    // Este atributo restringe el acceso a la clase CiudadanoController a usuarios que tengan el rol "Ciudadano" o "Alcaldia".
    [Authorize(Roles = "Ciudadano, Alcaldia")]
    // Esta línea define una clase pública llamada CiudadanoController que hereda de la clase Controller.
    public class CiudadanoController : Controller
    {
        // Declara un campo privado de solo lectura llamado _context de tipo SgarDbContext.
        private readonly SgarDbContext _context;

        // Constructor de la clase CiudadanoController.
        public CiudadanoController(SgarDbContext context)
        {
            // Inicializa el campo _context con la instancia de SgarDbContext proporcionada como argumento.
            _context = context;
        }

        // Definición de la acción Menu del controlador.
        public IActionResult Menu()
        {
            // Devuelve la vista predeterminada asociada con la acción Menu.
            return View();
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
        // Definición de la acción Login que recibe un objeto Ciudadano como parámetro.
        public async Task<IActionResult> Login(Ciudadano ciudadano)
        {
            // Bloque try para manejar excepciones.
            try
            {
                // Genera un hash SHA-256 de la contraseña proporcionada por el usuario.
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                // Busca en la base de datos un ciudadano que coincida con el correo y la contraseña hash.
                var ciudadanoAuth = await _context.Ciudadanos.FirstOrDefaultAsync(s => s.Correo == ciudadano.Correo && s.Password == ciudadano.Password);
                // Busca la zona asociada al ciudadano autenticado.
                var zona = await _context.Zonas.FirstOrDefaultAsync(s => s.Id == ciudadanoAuth.ZonaId);
                // Busca la alcaldía asociada a la zona del ciudadano autenticado.
                var alcaldia = await _context.Alcaldias.FirstOrDefaultAsync(s => s.Id == zona.IdAlcaldia);
                // Verifica si la autenticación fue exitosa.
                if (ciudadanoAuth != null && ciudadanoAuth.Id > 0 && ciudadanoAuth.Correo == ciudadano.Correo)
                {
                    // Crea una lista de claims (afirmaciones) que representan la identidad del usuario autenticado.
                    var claims = new[] { 
                        // Claim que almacena el ID del ciudadano.
                        new Claim("Id", ciudadanoAuth.Id.ToString()), 
                        // Claim que almacena el nombre de la zona asociada al ciudadano.
                        new Claim("Zona", zona.Nombre), 
                        // Claim que almacena el ID de la alcaldía asociada a la zona del ciudadano.
                        new Claim("Alcaldia", alcaldia.Id.ToString()), 
                        // Claim que almacena el nombre completo del ciudadano.
                        new Claim("Nombre", ciudadanoAuth.Nombre + " " + ciudadanoAuth.Apellido), 
                        // Claim que almacena el rol del usuario (en este caso, el nombre de la clase Ciudadano).
                        new Claim(ClaimTypes.Role, ciudadanoAuth.GetType().Name)
                    };
                    // Crea una identidad basada en los claims y el esquema de autenticación de cookies.
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // Inicia sesión del usuario utilizando la autenticación de cookies.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    // Redirige al usuario a la acción Menu del controlador Ciudadano después de iniciar sesión.
                    return RedirectToAction("Menu", "Ciudadano");
                }
                // Si la autenticación falla.
                else
                {
                    // Agrega un error al modelo si la autenticación falla.
                    ModelState.AddModelError("", "El email o contraseña estan incorrectos");
                    // Devuelve la vista Login con el mensaje de error.
                    return View();
                }
            }
            // Bloque catch para manejar excepciones generales.
            catch
            {
                // Devuelve la vista Login con el objeto ciudadano proporcionado.
                return View(ciudadano);
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

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra los detalles de un ciudadano específico.
        // GET: Ciudadano/Details/5 
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

            // Busca en la base de datos el ciudadano con el id proporcionado.
            var ciudadano = await _context.Ciudadanos
                .Include(c => c.Zona) // Incluye la navegación a la entidad Zona relacionada con el ciudadano.
                .FirstOrDefaultAsync(m => m.Id == id); // Obtiene el primer ciudadano que coincide con el id proporcionado, o null si no se encuentra ninguno.

            // Verifica si el ciudadano encontrado es nulo.
            if (ciudadano == null)
            {
                // Si el ciudadano es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Devuelve la vista Details con el ciudadano encontrado como modelo.
            return View(ciudadano);
        }

        // Definición de la acción GetMunicipiosFromDepartamentoId, que devuelve un JsonResult.
        // Esta línea define un método público llamado GetMunicipiosFromDepartamentoId.
        // El método toma un entero llamado departamentoId como parámetro.
        // El método devuelve un objeto JsonResult.
        // Atributo que permite el acceso a esta acción sin requerir autenticación
        [AllowAnonymous]
        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            // Esta línea construye y retorna un objeto JsonResult.
            // _context.Municipios accede a la tabla Municipios en la base de datos a través del contexto de la base de datos.
            // .Where(m => m.IdDepartamento == departamentoId) filtra los municipios para seleccionar solo aquellos cuyo IdDepartamento coincide con el valor proporcionado en el parámetro departamentoId.
            // .ToList() convierte el resultado filtrado en una lista de objetos Municipio.
            // Json(...) convierte la lista de municipios en un objeto JSON, que es el formato esperado por JsonResult.
            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        // Definición de la acción GetDistritosFromMunicipioId, que devuelve un JsonResult.
        // Esta línea define un método público llamado GetDistritosFromMunicipioId.
        // El método toma un entero llamado municipioId como parámetro.
        // El método devuelve un objeto JsonResult.
        // Atributo que permite el acceso a esta acción sin requerir autenticación
        [AllowAnonymous]
        public JsonResult GetDistritosFromMunicipioId(int municipioId) 
        {
            // Esta línea construye y retorna un objeto JsonResult.
            // _context.Distritos accede a la tabla Distritos en la base de datos a través del contexto de la base de datos.
            // .Where(m => m.IdMunicipio == municipioId) filtra los distritos para seleccionar solo aquellos cuyo IdMunicipio coincide con el valor proporcionado en el parámetro municipioId.
            // .ToList() convierte el resultado filtrado en una lista de objetos Distrito.
            // Json(...) convierte la lista de distritos en un objeto JSON, que es el formato esperado por JsonResult.
            return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());
        }

        // Definición de la acción GetZonasFromDistritoId, que devuelve un JsonResult.
        // Esta línea define un método público llamado GetZonasFromDistritoId.
        // El método toma un entero llamado distritoId como parámetro.
        // El método devuelve un objeto JsonResult.
        // Atributo que permite el acceso a esta acción sin requerir autenticación
        [AllowAnonymous]
        public JsonResult GetZonasFromDistritoId(int distritoId) 
        {
            // Esta línea construye y retorna un objeto JsonResult.
            // _context.Zonas accede a la tabla Zonas en la base de datos a través del contexto de la base de datos.
            // .Where(m => m.IdDistrito == distritoId) filtra las zonas para seleccionar solo aquellas cuyo IdDistrito coincide con el valor proporcionado en el parámetro distritoId.
            // .ToList() convierte el resultado filtrado en una lista de objetos Zona.
            // Json(...) convierte la lista de zonas en un objeto JSON, que es el formato esperado por JsonResult.
            return Json(_context.Zonas.Where(m => m.IdDistrito == distritoId).ToList());
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra el formulario para crear un nuevo ciudadano.
        // GET: Ciudadano/Create 
        // Atributo que permite el acceso a esta acción sin requerir autenticación.
        [AllowAnonymous]
        // Definición de la acción Create, que devuelve un IActionResult.
        public IActionResult Create() 
        {
            // Crea una lista de zonas con una opción predeterminada "SELECCIONAR".
            List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
            // Crea una lista de distritos con una opción predeterminada "SELECCIONAR".
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            // Crea una lista de municipios con una opción predeterminada "SELECCIONAR".
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            // Obtiene una lista de departamentos de la base de datos, excluyendo el departamento con Id 1.
            var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
            // Agrega una opción predeterminada "SELECCIONAR" a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            // Crea un SelectList para los municipios y lo asigna a ViewData.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // Crea un SelectList para los distritos y lo asigna a ViewData.
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            // Crea un SelectList para los departamentos y lo asigna a ViewData.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // Crea un SelectList para las zonas y lo asigna a ViewData.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", 0);
            // Devuelve la vista Create.
            return View(new Ciudadano()); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y se utiliza para crear un nuevo ciudadano.
        // POST: Ciudadano/Create 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // Atributo que permite el acceso a esta acción sin requerir autenticación.
        [AllowAnonymous]
        // Definición de la acción Create, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Dui,Correo,Password,ConfirmarPassword,ZonaId,Notificacion")] Ciudadano ciudadano) 
        {
            // Bloque try para manejar excepciones.
            try
            {
                if (ciudadano.Notificacion.Latitud == null ||
                   ciudadano.Notificacion.Latitud == 0 ||
                   ciudadano.Notificacion.Longitud == null ||
                   ciudadano.Notificacion.Longitud == 0)
                    throw new Exception("Debe seleccionar su ubicación");
                // Genera un hash SHA-256 de la contraseña proporcionada por el usuario.
                ciudadano.Password = GenerarHash256(ciudadano.Password);
                // Agrega el objeto ciudadano al contexto de la base de datos.
                _context.Ciudadanos.Add(ciudadano);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();

                var ciudadanoId = _context.Ciudadanos.FirstOrDefault(s => s.Dui == ciudadano.Dui).Id;

                ciudadano.Notificacion.IdCiudadano = ciudadanoId;
                _context.NotificacionesUbicaciones.Add(ciudadano.Notificacion);
                await _context.SaveChangesAsync();

                // Redirige al usuario a la acción Login después de crear el ciudadano.
                return RedirectToAction(nameof(Login)); 
            }
            // Bloque catch para manejar excepciones generales.
            catch
            {
                // Crea una lista de zonas con una opción predeterminada "SELECCIONAR".
                List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
                // Crea una lista de distritos con una opción predeterminada "SELECCIONAR".
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                // Crea una lista de municipios con una opción predeterminada "SELECCIONAR".
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                // Obtiene una lista de departamentos de la base de datos, excluyendo el departamento con Id 1.
                var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
                // Agrega una opción predeterminada "SELECCIONAR" a la lista de departamentos.
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                // Crea un SelectList para los municipios y lo asigna a ViewData.
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                // Crea un SelectList para los distritos y lo asigna a ViewData.
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                // Crea un SelectList para los departamentos y lo asigna a ViewData.
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                // Crea un SelectList para las zonas y lo asigna a ViewData, seleccionando la zona del ciudadano creado.
                ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);
                // Devuelve la vista Create con el objeto ciudadano proporcionado.
                return View(ciudadano); 
            }
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP GET y muestra el formulario para editar el perfil de un ciudadano específico.
        // GET: Ciudadano/Edit/5 
        // Este atributo restringe el acceso a la acción a usuarios que tengan el rol "Ciudadano".
        [Authorize(Roles = "Ciudadano")]
        // Definición de la acción Perfil, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Perfil(int? id) 
        {
            // Verifica si el parámetro id es nulo.
            if (id == null) 
            {
                // Si id es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca en la base de datos el ciudadano con el id proporcionado.
            var datosCiudadano = await _context.Ciudadanos.FindAsync(id);
            // Verifica si el ciudadano encontrado es nulo.
            if (datosCiudadano == null) 
            {
                // Si el ciudadano es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Obtiene una lista de todas las zonas de la base de datos.
            var zonas = _context.Zonas.ToList();
            // Crea un SelectList para las zonas y lo asigna a ViewData.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre");

            // Devuelve la vista Perfil con el ciudadano encontrado como modelo.s
            return View(datosCiudadano); 
        }

        // Este atributo restringe el acceso a la acción a usuarios que tengan el rol "Ciudadano".
        [Authorize(Roles = "Ciudadano")]
        // Definición de la acción Edit, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Edit(int? id) 
        {
            // Verifica si el parámetro id es nulo.
            if (id == null) 
            {
                // Si id es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca en la base de datos el ciudadano con el id proporcionado.
            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            // Verifica si el ciudadano encontrado es nulo.
            if (ciudadano == null) 
            {
                // Si el ciudadano es nulo, devuelve un resultado NotFound (código de estado 404).
                return NotFound();
            }
            // Crea una lista de zonas con una opción predeterminada "SELECCIONAR".
            List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
            // Crea una lista de distritos con una opción predeterminada "SELECCIONAR".
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            // Crea una lista de municipios con una opción predeterminada "SELECCIONAR".
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            // Obtiene una lista de departamentos de la base de datos, excluyendo el departamento con Id 1.
            var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
            // Agrega una opción predeterminada "SELECCIONAR" a la lista de departamentos.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // Busca la zona asociada al ciudadano.
            var zona = _context.Zonas.FirstOrDefault(s => s.Id == ciudadano.ZonaId);
            // Agrega la zona actual del ciudadano a la lista de zonas.
            zonas.Add(new Zona { Nombre = zona.Nombre, Id = zona.Id, IdAlcaldia = zona.IdAlcaldia, IdDistrito = zona.IdDistrito, Descripcion = zona.Descripcion });

            // Crea un SelectList para los municipios y lo asigna a ViewData.
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            // Crea un SelectList para los distritos y lo asigna a ViewData.
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            // Crea un SelectList para los departamentos y lo asigna a ViewData.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            // Crea un SelectList para las zonas y lo asigna a ViewData, seleccionando la zona del ciudadano.
            ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);
            // Devuelve la vista Edit con el ciudadano encontrado como modelo.
            return View(ciudadano); 
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y se utiliza para editar un ciudadano específico.
        // POST: Ciudadano/Edit/5 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // Este atributo restringe el acceso a la acción a usuarios que tengan el rol "Ciudadano".
        [Authorize(Roles = "Ciudadano")]
        // Definición de la acción Edit, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Correo,ZonaId")] Ciudadano ciudadano) 
        {
            // Verifica si el id proporcionado en la URL no coincide con el id del ciudadano en el objeto Ciudadano.
            if (id != ciudadano.Id) 
            {
                // Si los ids no coinciden, devuelve un resultado NotFound (código de estado 404).
                return NotFound(); 
            }

            // Busca el ciudadano existente en la base de datos por su id.
            var ciudadanoUpdate = _context.Ciudadanos.FirstOrDefault(s => s.Id == ciudadano.Id);
            // Bloque try para manejar excepciones.
            try
            {
                ciudadanoUpdate.Nombre = ciudadano.Nombre; // Actualiza el nombre del ciudadano existente.
                ciudadanoUpdate.Apellido = ciudadano.Apellido; // Actualiza el apellido del ciudadano existente.
                ciudadanoUpdate.Correo = ciudadano.Correo; // Actualiza el correo del ciudadano existente.
                ciudadanoUpdate.ZonaId = ciudadano.ZonaId; // Actualiza la ZonaId del ciudadano existente.

                // Marca el ciudadano existente como modificado en el contexto de la base de datos.
                _context.Update(ciudadanoUpdate);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();
                // Redirige al usuario a la acción Menu del controlador Ciudadano después de editar el ciudadano.
                return RedirectToAction("Menu", "Ciudadano"); 
            }
            // Bloque catch para manejar excepciones de concurrencia de la base de datos.
            catch (DbUpdateConcurrencyException) 
            {
                // Crea una lista de zonas con una opción predeterminada "SELECCIONAR".
                List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
                // Crea una lista de distritos con una opción predeterminada "SELECCIONAR".
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                // Crea una lista de municipios con una opción predeterminada "SELECCIONAR".
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                // Obtiene una lista de departamentos de la base de datos, excluyendo el departamento con Id 1.
                var departamentos = _context.Departamentos.Where(s => s.Id != 1).ToList();
                // Agrega una opción predeterminada "SELECCIONAR" a la lista de departamentos.
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

                // Busca la zona asociada al ciudadano.
                var zona = _context.Zonas.FirstOrDefault(s => s.Id == ciudadano.ZonaId);
                // Agrega la zona actual del ciudadano a la lista de zonas.
                zonas.Add(new Zona { Nombre = zona.Nombre, Id = zona.Id, IdAlcaldia = zona.IdAlcaldia, IdDistrito = zona.IdDistrito, Descripcion = zona.Descripcion });

                // Crea un SelectList para los municipios y lo asigna a ViewData.
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                // Crea un SelectList para los distritos y lo asigna a ViewData.
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                // Crea un SelectList para los departamentos y lo asigna a ViewData.
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
                // Crea un SelectList para las zonas y lo asigna a ViewData, seleccionando la zona del ciudadano.
                ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", ciudadano.ZonaId);

                // Verifica si el ciudadano con el id proporcionado existe en la base de datos.
                if (!CiudadanoExists(ciudadano.Id)) 
                {
                    // Si el ciudadano no existe, devuelve un resultado NotFound (código de estado 404).
                    return NotFound(); 
                }
                else
                {
                    // Si el ciudadano existe, devuelve la vista Edit con el objeto ciudadano proporcionado.
                    return View(ciudadano); 
                }
            }
        }

        // Comentario que indica que esta acción responde a solicitudes HTTP POST y se utiliza para eliminar un ciudadano específico.
        // POST: Ciudadano/Delete/5 
        // Atributo que indica que esta acción solo responde a solicitudes HTTP POST.
        [HttpPost]
        // Atributo que protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
        // Este atributo restringe el acceso a la acción a usuarios que tengan el rol "Ciudadano".
        [Authorize(Roles = "Ciudadano")]
        // Definición de la acción Delete, que es asíncrona y devuelve un IActionResult.
        public async Task<IActionResult> Delete(int id) 
        {
            // Busca en la base de datos el ciudadano con el id proporcionado.
            var ciudadano = await _context.Ciudadanos.FindAsync(id);
            // Verifica si el ciudadano encontrado no es nulo.
            if (ciudadano != null) 
            {
                // Elimina el ciudadano del contexto de la base de datos.
                _context.Ciudadanos.Remove(ciudadano);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();
                // Redirige al usuario a la acción Index del controlador Home después de eliminar el ciudadano.
                return Json(new { redirectToUrl = Url.Action("Index", "Home") });
            }
            // Cierra la sesión del usuario eliminando la cookie de autenticación.
            return RedirectToAction("Menu", "Ciudadano");
        }

        // Definición de un método privado que devuelve un valor booleano (true o false).
        private bool CiudadanoExists(int id) 
        {
            // Devuelve true si existe algún ciudadano con el id proporcionado en la base de datos, false en caso contrario.
            return _context.Ciudadanos.Any(e => e.Id == id); 
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

        [AllowAnonymous]
        public IActionResult SaveLocation(Ciudadano ciudadano)
        {

            if (ciudadano.Notificacion == null)
                ciudadano.Notificacion = new NotificacionesUbicacion();

            ciudadano.Notificacion.Titulo = "Ubicacion";
            ciudadano.Notificacion.Estado = 1;
            ciudadano.Notificacion.DistanciaMetros = 0;
            ciudadano.Notificacion.Longitud = 0;
            ciudadano.Notificacion.Latitud = 0;

            return PartialView("_SelectUbication", ciudadano.Notificacion);
        }
    }
}
