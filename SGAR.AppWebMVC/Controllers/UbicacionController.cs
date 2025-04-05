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
        // 1. Declara una variable privada de solo lectura llamada _context del tipo SgarDbContext.
        //    SgarDbContext probablemente es la clase que representa la conexión a la base de datos
        //    utilizando Entity Framework Core. La inyección de dependencias se utilizará para
        //    proporcionar una instancia de este contexto.
        private readonly SgarDbContext _context;

        // 2. Constructor de la clase UbicacionController.
        //    Recibe una instancia de SgarDbContext como parámetro.
        //    Esta instancia se inyecta automáticamente por el sistema de inyección de dependencias de ASP.NET Core.
        public UbicacionController(SgarDbContext context)
        {
            // 3. Asigna la instancia de SgarDbContext recibida al campo privado _context.
            //    Esto permite que el controlador acceda a la base de datos a través de este contexto.
            _context = context;
        }

        // 4. Acción (endpoint) que responde a las solicitudes HTTP GET a la ruta "/Ubicacion" (por defecto
        //    si no se especifica un enrutamiento diferente).
        //    El atributo [Authorize(Roles = "Operador")] indica que solo los usuarios que tengan el rol "Operador"
        //    autenticado podrán acceder a esta acción. Cualquier otro usuario será redirigido (normalmente a una
        //    página de inicio de sesión o un error de acceso denegado).
        [Authorize(Roles = "Operador")]
        public IActionResult Index()
        {
            // 5. Busca en la tabla "Ubicaciones" (a través de la propiedad DbSet<Ubicacion> llamada "Ubicaciones"
            //    en el _context) la primera ubicación que cumpla con la condición especificada en la expresión lambda.
            //    La condición es que el valor de la propiedad "IdOperador" de la ubicación sea igual al ID del
            //    usuario actualmente autenticado.
            //    User.FindFirst("Id").Value obtiene el valor de la reclamación (claim) llamada "Id" del usuario.
            //    Se asume que durante la autenticación, el ID del usuario se guarda como una reclamación con el nombre "Id".
            //    Convert.ToInt32() convierte el valor de la reclamación (que es una cadena) a un entero para la comparación.
            //    FirstOrDefault() devuelve el primer elemento que cumple la condición o null si no se encuentra ninguno.
            var ubicacionExists = _context.Ubicaciones.FirstOrDefault(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value));

            // 6. Crea una nueva instancia de la clase Ubicacion.
            //    Esta instancia se utilizará para pasarla a la vista, ya sea la ubicación existente encontrada
            //    o una nueva instancia si no se encontró ninguna.
            var ubicacion = new Ubicacion();

            // 7. Verifica si se encontró una ubicación existente para el operador actual.
            if (ubicacionExists != null)
            {
                // 8. Si se encontró una ubicación existente (ubicacionExists no es null),
                //    asigna esa ubicación existente a la variable "ubicacion".
                //    De esta manera, la vista recibirá la información de la ubicación existente.
                ubicacion = ubicacionExists;
            }

            // 9. Retorna una vista (probablemente llamada "Index.cshtml" por convención) y le pasa el modelo "ubicacion".
            //    La vista utilizará este modelo para mostrar la información de la ubicación del operador actual.
            //    Si no se encontró ninguna ubicación, la vista recibirá una instancia vacía de "Ubicacion".
            return View(ubicacion);
        }


        // 1. [HttpPost] indica que esta acción responde a las solicitudes HTTP POST.
        // 2. public async Task<ActionResult> Create([FromBody] Ubicacion ubicacion) declara una acción asíncrona llamada Create
        //    que devuelve un ActionResult (que permite diferentes tipos de respuestas HTTP).
        // 3. Recibe un parámetro llamado 'ubicacion' del tipo Ubicacion.
        // 4. [FromBody] indica que los datos para este parámetro se deben tomar del cuerpo de la solicitud HTTP.
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Ubicacion ubicacion)
        {
            // 5. Bloque try para manejar posibles excepciones durante la ejecución.
            try
            {
                // 6. _context.Ubicaciones.Add(ubicacion); agrega la instancia de 'ubicacion' al DbSet de 'Ubicaciones'
                //    en el contexto de la base de datos. Esto marca la entidad para ser insertada en la base de datos.
                _context.Ubicaciones.Add(ubicacion);

                // 7. await _context.SaveChangesAsync(); guarda todos los cambios pendientes en el contexto de la base de datos.
                //    Esto incluye la inserción de la nueva 'ubicacion' en la tabla correspondiente.
                //    'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
                await _context.SaveChangesAsync();

                // 8. return Json(ubicacion); devuelve una respuesta JSON que contiene la instancia de 'ubicacion' que se acaba de crear.
                //    Esto es útil para que el cliente reciba la información de la entidad creada, posiblemente incluyendo
                //    cualquier valor generado por la base de datos (como un ID).
                return Json(ubicacion);
            }
            // 9. Bloque catch para capturar cualquier excepción que ocurra dentro del bloque try.
            catch
            {
                // 10. return Json(ubicacion); en caso de error, también se devuelve una respuesta JSON con la instancia de 'ubicacion'
                //     que se intentó crear. Esto puede ayudar al cliente a entender qué datos se enviaron, aunque la creación falló.
                //     Sin embargo, en un escenario de producción, sería más apropiado devolver un código de estado HTTP de error
                //     y posiblemente un mensaje de error más descriptivo.
                return Json(ubicacion);
            }
        }

        // 1. [HttpPost] indica que esta acción responde a las solicitudes HTTP POST.
        // 2. public async Task<ActionResult> Edit([FromBody] Ubicacion ubicacion) declara una acción asíncrona llamada Edit
        //    que devuelve un ActionResult. Recibe un parámetro llamado 'ubicacion' del tipo Ubicacion,
        //    cuyos datos se toman del cuerpo de la solicitud HTTP ([FromBody]).
        [HttpPost]
        public async Task<ActionResult> Edit([FromBody] Ubicacion ubicacion)
        {
            // 3. Bloque try para manejar posibles excepciones durante la ejecución.
            try
            {
                // 4. var ubicacionUpdate = await _context.Ubicaciones.FirstOrDefaultAsync(u => u.Id == ubicacion.Id);
                //    Busca de forma asíncrona la primera entidad 'Ubicacion' en la base de datos cuyo 'Id' coincida
                //    con el 'Id' de la 'ubicacion' recibida en el cuerpo de la solicitud.
                //    'FirstOrDefaultAsync' devuelve null si no se encuentra ninguna entidad con el 'Id' especificado.
                var ubicacionUpdate = await _context.Ubicaciones.FirstOrDefaultAsync(u => u.Id == ubicacion.Id);

                // 5. ubicacionUpdate.Longitud = ubicacion.Longitud;
                //    Actualiza la propiedad 'Longitud' de la entidad 'ubicacionUpdate' con el valor de la propiedad 'Longitud'
                //    de la 'ubicacion' recibida.
                ubicacionUpdate.Longitud = ubicacion.Longitud;

                // 6. ubicacionUpdate.Latitud = ubicacion.Latitud;
                //    Actualiza la propiedad 'Latitud' de la entidad 'ubicacionUpdate' con el valor de la propiedad 'Latitud'
                //    de la 'ubicacion' recibida.
                ubicacionUpdate.Latitud = ubicacion.Latitud;

                // 7. ubicacionUpdate.FechaActualizacion = ubicacion.FechaActualizacion;
                //    Actualiza la propiedad 'FechaActualizacion' de la entidad 'ubicacionUpdate' con el valor de la propiedad
                //    'FechaActualizacion' de la 'ubicacion' recibida.
                ubicacionUpdate.FechaActualizacion = ubicacion.FechaActualizacion;

                // 8. _context.Ubicaciones.Update(ubicacionUpdate); marca la entidad 'ubicacionUpdate' en el contexto de la base de datos
                //    como modificada. Entity Framework Core realizará un seguimiento de los cambios realizados en esta entidad.
                _context.Ubicaciones.Update(ubicacionUpdate);

                // 9. await _context.SaveChangesAsync(); guarda de forma asíncrona todos los cambios pendientes en el contexto de la base de datos.
                //    Esto incluye la actualización de la entidad 'ubicacionUpdate' en la tabla correspondiente.
                //    'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
                await _context.SaveChangesAsync();

                // 10. return Json(ubicacion); devuelve una respuesta JSON que contiene la instancia de 'ubicacion' (la que se envió para editar).
                //     Esto indica al cliente que la operación de edición probablemente fue exitosa y le proporciona los datos actualizados.
                return Json(ubicacion);
            }
            // 11. Bloque catch para capturar cualquier excepción que ocurra dentro del bloque try.
            catch
            {
                // 12. return Json(ubicacion); en caso de error, también se devuelve una respuesta JSON con la instancia de 'ubicacion'
                //     que se intentó editar. Al igual que en la acción 'Create', en un escenario de producción, sería más apropiado
                //     devolver un código de estado HTTP de error y posiblemente un mensaje de error más descriptivo para el cliente.
                return Json(ubicacion);
            }
        }


        // 1. [HttpPost] indica que esta acción responde a las solicitudes HTTP POST.
        // 2. public async void Delete(int id) declara una acción asíncrona llamada Delete que no devuelve ningún valor explícito (void).
        // 3. Recibe un parámetro entero llamado 'id', que probablemente representa el ID de la ubicación a eliminar.
        [HttpPost]
        public async void Delete(int id)
        {
            // 4. var ubicacion = await _context.Ubicaciones.FindAsync(id);
            //    Busca de forma asíncrona en la tabla 'Ubicaciones' la entidad con el ID especificado.
            //    'FindAsync' es eficiente para buscar por la clave primaria. Devuelve la entidad encontrada o null si no existe.
            var ubicacion = await _context.Ubicaciones.FindAsync(id);

            // 5. if (ubicacion != null) verifica si se encontró una ubicación con el ID proporcionado.
            if (ubicacion != null)
            {
                // 6. _context.Ubicaciones.Remove(ubicacion); marca la entidad 'ubicacion' para ser eliminada de la base de datos
                //    cuando se guarden los cambios.
                _context.Ubicaciones.Remove(ubicacion);
            }

            // 7. await _context.SaveChangesAsync(); guarda de forma asíncrona todos los cambios pendientes en el contexto de la base de datos.
            //    En este caso, si se llamó a 'Remove', se ejecutará la eliminación de la ubicación en la base de datos.
            //    'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
            await _context.SaveChangesAsync();
        }

        // 1. private bool UbicacionExists(int id) declara un método privado que devuelve un valor booleano
        // 2. y toma un parámetro entero llamado 'id'.
        private bool UbicacionExists(int id)
        {
            // 3. return _context.Ubicaciones.Any(e => e.IdOperador == id);
            //    Verifica si existe alguna entidad en la tabla 'Ubicaciones' cuyo campo 'IdOperador' coincida con el 'id' proporcionado.
            //    'Any()' devuelve true si al menos un elemento cumple la condición, y false en caso contrario.
            return _context.Ubicaciones.Any(e => e.IdOperador == id);
        }

        // 1. public async Task<JsonResult> GetUbicationFromOpId(int id) declara una acción asíncrona pública
        // 2. que devuelve un 'JsonResult' (un resultado que se formatea como JSON).
        // 3. Recibe un parámetro entero llamado 'id', que probablemente representa el ID del operador.
        public async Task<JsonResult> GetUbicationFromOpId(int id)
        {
            // 4. return Json(await _context.Ubicaciones.FirstOrDefaultAsync(s => s.IdOperador == id));
            //    Busca de forma asíncrona la primera entidad en la tabla 'Ubicaciones' cuyo campo 'IdOperador' coincida con el 'id' proporcionado.
            //    'FirstOrDefaultAsync' devuelve la primera entidad que cumple la condición o null si no se encuentra ninguna.
            // 5. El resultado de la búsqueda se envuelve en un 'JsonResult', lo que significa que se serializará a formato JSON
            //    y se enviará como la respuesta HTTP.
            return Json(await _context.Ubicaciones.FirstOrDefaultAsync(s => s.IdOperador == id));
        }
    }
}
