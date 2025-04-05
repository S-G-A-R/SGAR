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

        public async Task<IActionResult> Index(string operador, string zona, int topRegistro = 10)
        {
            // Declara un método asíncrono llamado Index que devuelve un IActionResult.
            // Este método acepta tres parámetros:
            // - operador: una cadena opcional para filtrar horarios por nombre de operador.
            // - zona: una cadena opcional para filtrar horarios por nombre de zona.
            // - topRegistro: un entero opcional que indica el número máximo de registros a devolver (por defecto es 10).

            var horarios = _context.Horarios
                // Obtiene una colección de todos los registros de la tabla "Horarios" del contexto de la base de datos (_context).
                .Include(h => h.IdOperadorNavigation)
                // Incluye los datos de la entidad relacionada "IdOperadorNavigation" (probablemente una tabla de Operadores) para cada horario.
                // Esto permite acceder a las propiedades del operador directamente desde el objeto horario.
                .Include(h => h.IdZonaNavigation)
                // Incluye los datos de la entidad relacionada "IdZonaNavigation" (probablemente una tabla de Zonas) para cada horario.
                // Esto permite acceder a las propiedades de la zona directamente desde el objeto horario.
                .Where(h => h.IdZonaNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value))
                // Filtra la colección de horarios para incluir solo aquellos cuya propiedad "IdAlcaldia" de la entidad relacionada "IdZonaNavigation"
                // coincida con el valor del claim "Id" del usuario actual. Se asume que este claim contiene el ID de la alcaldía del usuario.
                // User.FindFirst("Id").Value obtiene el valor del primer claim con el tipo "Id" del usuario autenticado.
                // Convert.ToInt32() convierte este valor a un entero para la comparación.
                .AsQueryable();
            // Convierte la colección resultante a un IQueryable<Horario>. Esto es importante para permitir la adición de más cláusulas Where dinámicamente.


            if (!string.IsNullOrWhiteSpace(operador))
            {
                // Verifica si el parámetro "operador" no es nulo, vacío o contiene solo espacios en blanco.
                horarios = horarios.Where(h => h.IdOperadorNavigation.Nombre.ToLower().Contains(operador.ToLower()));
                // Si "operador" tiene un valor, filtra la colección "horarios" para incluir solo aquellos donde el nombre del operador (obtenido a través de la navegación "IdOperadorNavigation")
                // contenga la cadena proporcionada en "operador" (ignorando mayúsculas y minúsculas con ToLower()).
            }

            if (!string.IsNullOrWhiteSpace(zona))
            {
                // Verifica si el parámetro "zona" no es nulo, vacío o contiene solo espacios en blanco.
                horarios = horarios.Where(h => h.IdZonaNavigation.Nombre.ToLower().Contains(zona.ToLower()));
                // Si "zona" tiene un valor, filtra la colección "horarios" para incluir solo aquellos donde el nombre de la zona (obtenido a través de la navegación "IdZonaNavigation")
                // contenga la cadena proporcionada en "zona" (ignorando mayúsculas y minúsculas con ToLower()).
            }

            if (topRegistro > 0)
            {
                // Verifica si el parámetro "topRegistro" es mayor que 0.
                horarios = horarios.Take(topRegistro);
                // Si "topRegistro" es mayor que 0, limita el número de resultados en la colección "horarios" a la cantidad especificada en "topRegistro".
            }

            return View(await horarios.ToListAsync());
            // Devuelve una vista (probablemente una vista de Razor) y le pasa la lista de horarios resultante.
            // await horarios.ToListAsync() ejecuta la consulta a la base de datos de forma asíncrona y convierte el resultado en una lista antes de pasársela a la vista.
        }

        public IActionResult Create()
        {
            // Declara un método síncrono llamado Create que devuelve un IActionResult.
            // Este método probablemente muestra un formulario para crear un nuevo horario.

            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            // Obtiene la alcaldía del usuario actual desde la base de datos.
            // _context.Alcaldias accede a la tabla de Alcaldías.
            // FirstOrDefault busca el primer registro donde el Id coincida con el valor del claim "Id" del usuario (convertido a entero).
            // Se asume que el claim "Id" contiene el ID de la alcaldía del usuario autenticado.

            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            // Obtiene el municipio asociado a la alcaldía del usuario.
            // _context.Municipios accede a la tabla de Municipios.
            // FirstOrDefault busca el primer registro donde el Id coincida con el IdMunicipio de la alcaldía obtenida anteriormente.

            var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
            // Obtiene una lista de todos los distritos que pertenecen al municipio del usuario.
            // _context.Distritos accede a la tabla de Distritos.
            // Where filtra los distritos donde el IdMunicipio coincide con el Id del municipio del usuario.
            // ToList() ejecuta la consulta y convierte el resultado en una lista.

            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
            // Agrega un elemento predeterminado a la lista de distritos.
            // Este elemento tiene un Id de 0 y el nombre "Seleccione un distrito", lo que se usa típicamente en un dropdown list como opción inicial.
            // Se asigna IdMunicipio = 0 para indicar que no está asociado a ningún municipio real.

            var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona" }]);
            // Crea una nueva lista de objetos Zona y le agrega un elemento predeterminado.
            // Este elemento tiene un Id de 0 y el nombre "Seleccione una zona", para ser usado como opción inicial en un dropdown list de zonas.
            // Inicialmente, esta lista de zonas está vacía o contiene solo la opción de selección. Probablemente se llenará dinámicamente en el cliente o en otro método.

            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            // Obtiene una lista de todos los operadores que pertenecen a la alcaldía del usuario actual.
            // _context.Operadores accede a la tabla de Operadores.
            // Where filtra los operadores donde el IdAlcaldia coincide con el ID de la alcaldía del usuario.
            // ToList() ejecuta la consulta y convierte el resultado en una lista.

            operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });
            // Agrega un elemento predeterminado a la lista de operadores.
            // Este elemento tiene un Id de 0 y el nombre "Seleccione un operador", para ser usado como opción inicial en un dropdown list de operadores.

            // Agregar la lista de distritos a ViewData
            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
            // Agrega la lista de distritos a ViewData con la clave "Distritos".
            // SelectList se utiliza para crear una lista de elementos que se pueden usar en un dropdown list en la vista.
            // "Id" se especifica como el valor de cada opción, "Nombre" como el texto visible de cada opción, y 0 como el valor preseleccionado (ninguno en este caso).

            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", 0);
            // Agrega la lista de zonas a ViewData con la clave "Zonas".
            // Similar a los distritos, prepara una SelectList para un dropdown list de zonas.

            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", 0);
            // Agrega la lista de operadores a ViewData con la clave "Operadores".
            // Similar a los distritos y zonas, prepara una SelectList para un dropdown list de operadores.

            // También puedes agregar el título de la vista
            ViewData["Title"] = "Crear Horario";
            // Agrega un título a ViewData con la clave "Title", que se puede usar para mostrar el título de la página en la vista.

            return View();
            // Devuelve la vista predeterminada asociada a esta acción "Create".
            // La vista utilizará los datos almacenados en ViewData (listas de distritos, zonas, operadores y el título) para renderizar el formulario de creación de horarios.
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Horario horario, List<int> DiasSeleccionados)
        {
            // Declara un método síncrono llamado Create que recibe un objeto Horario y una lista de enteros (DiasSeleccionados) como parámetros.
            // Este método se encarga de procesar el envío del formulario de creación de un nuevo horario.

            try
            {
                // Bloque try para envolver el código que puede generar excepciones.

                // Convertir los días seleccionados en un formato adecuado
                horario.Dia = string.Join(",", DiasSeleccionados); // O bien usar un formato binario, etc.
                                                                   // Toma la lista de enteros DiasSeleccionados (que probablemente representan los IDs de los días de la semana seleccionados por el usuario)
                                                                   // y los convierte en una cadena separada por comas. Esta cadena se asigna a la propiedad "Dia" del objeto "horario".
                                                                   // El comentario indica que se podría usar otro formato como binario para representar los días.

                // Verifica que el IdZona esté correcto
                Debug.WriteLine($"Zona seleccionada: {horario.IdZona}");
                // Escribe en la ventana de depuración el valor de la propiedad IdZona del objeto horario.
                // Esto es útil para verificar que el valor recibido del formulario para la zona es el esperado durante el desarrollo.

                _context.Horarios.Add(horario);
                // Agrega el objeto "horario" a la colección de entidades "Horarios" en el contexto de la base de datos (_context).
                // Esto marca la entidad para ser insertada en la base de datos cuando se llamen los cambios.

                _context.SaveChanges();
                // Guarda todos los cambios realizados en el contexto de la base de datos.
                // En este caso, inserta el nuevo registro de horario en la tabla correspondiente.

                return RedirectToAction(nameof(Index));
                // Si la operación de guardado es exitosa, redirige al usuario a la acción "Index" del mismo controlador.
                // nameof(Index) se utiliza para obtener el nombre de la acción como una cadena, lo que evita errores de escritura.
            }
            catch
            {
                // Bloque catch que se ejecuta si ocurre alguna excepción dentro del bloque try.
                // Esto indica que hubo un error al intentar guardar el nuevo horario.

                // Recargar las listas de operadores y zonas en caso de error

                var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
                // Vuelve a obtener la alcaldía del usuario desde la base de datos (similar a la acción Create sin parámetros).

                var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
                // Vuelve a obtener el municipio asociado a la alcaldía del usuario.

                var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
                // Vuelve a obtener la lista de distritos para el municipio del usuario.

                distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
                // Vuelve a agregar la opción predeterminada a la lista de distritos.

                var zonas = new List<Zona>([new Zona { Id = 0, Nombre = "Seleccione una zona" }]);
                // Vuelve a crear la lista de zonas con la opción predeterminada.
                // Nota: Aquí probablemente se debería volver a cargar las zonas correspondientes al distrito seleccionado (si la lógica lo requiere).

                var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
                // Vuelve a obtener la lista de operadores para la alcaldía del usuario.

                operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });
                // Vuelve a agregar la opción predeterminada a la lista de operadores.

                ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", 0);
                // Vuelve a cargar la lista de distritos en ViewData para que esté disponible en la vista.

                ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", 0);
                // Vuelve a cargar la lista de zonas en ViewData para que esté disponible en la vista.

                ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", 0);
                // Vuelve a cargar la lista de operadores en ViewData para que esté disponible en la vista.

                return View(horario);
                // Devuelve la misma vista "Create" pero ahora con el objeto "horario" que se intentó guardar y las listas de selección recargadas en ViewData.
                // Esto permite al usuario ver los datos que ingresó y los mensajes de validación (si los hay) para corregir el error e intentar guardar de nuevo.
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Declara un método síncrono llamado Edit que recibe un entero 'id' como parámetro.
            // Este método se encarga de mostrar el formulario para editar un horario existente.

            var horario = _context.Horarios
                // Obtiene un registro de la tabla "Horarios" del contexto de la base de datos (_context).
                .Include(h => h.IdOperadorNavigation)
                // Incluye los datos de la entidad relacionada "IdOperadorNavigation" (Operador) para el horario.
                .Include(h => h.IdZonaNavigation)
                // Incluye los datos de la entidad relacionada "IdZonaNavigation" (Zona) para el horario.
                .FirstOrDefault(h => h.Id == id);
            // Busca el primer horario cuyo Id coincida con el 'id' proporcionado.
            // FirstOrDefault devuelve null si no se encuentra ningún horario con ese Id.

            if (horario == null)
            {
                // Verifica si el horario con el Id especificado no fue encontrado en la base de datos.
                return NotFound();
                // Devuelve un resultado NotFound (código de estado HTTP 404), indicando que el recurso solicitado no existe.
            }

            ViewBag.DiasSeleccionados = string.IsNullOrEmpty(horario.Dia)
                ? new List<int>()
                : horario.Dia.Split(',').Select(int.Parse).ToList();
            // Prepara los días seleccionados para la vista.
            // Si la propiedad 'Dia' del horario es nula o vacía, crea una nueva lista vacía de enteros.
            // De lo contrario, divide la cadena de 'Dia' por comas, convierte cada parte a un entero y crea una lista de enteros.
            // Esta lista se almacena en ViewBag para ser utilizada en la vista para marcar los días previamente seleccionados.

            var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
            // Obtiene la alcaldía del usuario actual desde la base de datos (similar a la acción Create).

            var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
            // Obtiene el municipio asociado a la alcaldía del usuario.

            var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
            // Obtiene la lista de distritos para el municipio del usuario.

            distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
            // Agrega la opción predeterminada a la lista de distritos.

            var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
            // Obtiene la lista de operadores para la alcaldía del usuario.

            operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });
            // Agrega la opción predeterminada a la lista de operadores.

            var distritoId = horario.IdZonaNavigation != null ? horario.IdZonaNavigation.IdDistrito : 0;
            // Obtiene el Id del distrito asociado a la zona del horario que se va a editar.
            // Si la navegación a IdZonaNavigation es nula, se establece distritoId en 0.

            var zonas = _context.Zonas.Where(z => z.IdDistrito == distritoId).ToList();
            // Obtiene la lista de zonas que pertenecen al distrito del horario que se va a editar.

            zonas.Add(new Zona { Id = 0, Nombre = "Seleccione una zona" });
            // Agrega la opción predeterminada a la lista de zonas.

            // Convertir los días guardados en la BD a una lista
            ViewBag.DiasSeleccionados = horario.Dia.Split(',').Select(int.Parse).ToList();
            // Vuelve a asignar la lista de días seleccionados al ViewBag. Esto asegura que esté disponible para la vista.
            // Aunque se calculó antes, se repite para claridad o en caso de modificaciones posteriores.

            ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", horario.IdZonaNavigation?.IdDistrito);
            // Prepara la lista de distritos para el dropdown list en la vista.
            // El tercer parámetro (horario.IdZonaNavigation?.IdDistrito) establece el distrito actualmente asociado al horario como la opción preseleccionada.
            // Se usa el operador condicional null (?) para evitar errores si IdZonaNavigation es null.

            ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
            // Prepara la lista de zonas para el dropdown list en la vista.
            // El tercer parámetro (horario.IdZona) establece la zona actualmente asociada al horario como la opción preseleccionada.

            ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);
            // Prepara la lista de operadores para el dropdown list en la vista.
            // El tercer parámetro (horario.IdOperador) establece el operador actualmente asociado al horario como la opción preseleccionada.

            return View(horario);
            // Devuelve la vista predeterminada asociada a la acción "Edit", pasando el objeto "horario" encontrado como modelo.
            // La vista utilizará este modelo y los datos en ViewData/ViewBag para mostrar el formulario de edición con los valores actuales del horario.
        }


        [HttpPost]
        public IActionResult Edit(Horario horario, List<int> DiasSeleccionados)
        {
            // Declara un método síncrono llamado Edit que recibe un objeto Horario y una lista de enteros (DiasSeleccionados) como parámetros.
            // Este método se encarga de procesar el envío del formulario de edición de un horario existente.

            try
            {
                // Bloque try para envolver el código que puede generar excepciones.

                var horarioExistente = _context.Horarios.Find(horario.Id);
                // Busca un horario existente en la base de datos utilizando el Id proporcionado en el objeto 'horario'.
                // _context.Horarios.Find() es una forma eficiente de buscar una entidad por su clave primaria.

                if (horarioExistente == null)
                {
                    // Verifica si no se encontró ningún horario con el Id proporcionado.
                    ModelState.AddModelError("", "El horario no existe.");
                    // Agrega un error al ModelState, indicando que el horario no existe.
                    // ModelState se utiliza para gestionar los errores de validación y otros errores relacionados con el modelo.
                    return View(horario);
                    // Devuelve la vista "Edit" con el objeto 'horario' recibido. Esto mostrará el error al usuario.
                }


                // Guardar días seleccionados en formato adecuado
                horarioExistente.Dia = string.Join(",", DiasSeleccionados);
                // Actualiza la propiedad 'Dia' del horario existente con la cadena de días seleccionados (separados por comas).

                horarioExistente.IdOperador = horario.IdOperador;
                // Actualiza la propiedad 'IdOperador' del horario existente con el valor del objeto 'horario' recibido.

                horarioExistente.IdZona = horario.IdZona;
                // Actualiza la propiedad 'IdZona' del horario existente con el valor del objeto 'horario' recibido.

                horarioExistente.HoraEntrada = horario.HoraEntrada;
                // Actualiza la propiedad 'HoraEntrada' del horario existente con el valor del objeto 'horario' recibido.

                horarioExistente.HoraSalida = horario.HoraSalida;
                // Actualiza la propiedad 'HoraSalida' del horario existente con el valor del objeto 'horario' recibido.

                horarioExistente.Turno = horario.Turno;
                // Actualiza la propiedad 'Turno' del horario existente con el valor del objeto 'horario' recibido.

                _context.Update(horarioExistente);
                // Marca la entidad 'horarioExistente' como modificada en el contexto de la base de datos.
                // Aunque se actualizan las propiedades individualmente, _context.Update() asegura que Entity Framework sepa que esta entidad ha cambiado.

                _context.SaveChanges();
                // Guarda todos los cambios realizados en el contexto de la base de datos.
                // En este caso, actualiza el registro del horario en la tabla correspondiente.

                return RedirectToAction(nameof(Index));
                // Si la operación de guardado es exitosa, redirige al usuario a la acción "Index" del mismo controlador.
            }
            catch
            {
                // Bloque catch que se ejecuta si ocurre alguna excepción dentro del bloque try.
                // Esto indica que hubo un error al intentar guardar las modificaciones del horario.

                // Recargar datos en caso de error
                var id = horario.Id;
                // Obtiene el Id del horario que se intentó editar.

                horario = _context.Horarios
                    // Vuelve a obtener el horario de la base de datos, incluyendo las navegaciones.
                    .Include(h => h.IdOperadorNavigation)
                    .Include(h => h.IdZonaNavigation)
                    .FirstOrDefault(h => h.Id == id);
                // Esto asegura que la vista reciba un objeto horario con las propiedades de navegación cargadas.

                var yourAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
                // Vuelve a obtener la alcaldía del usuario.

                var yourMunicipio = _context.Municipios.FirstOrDefault(s => s.Id == yourAlcaldia.IdMunicipio);
                // Vuelve a obtener el municipio del usuario.

                var distritos = _context.Distritos.Where(s => s.IdMunicipio == yourMunicipio.Id).ToList();
                // Vuelve a obtener la lista de distritos para el municipio del usuario.

                distritos.Add(new Distrito { Id = 0, Nombre = "Seleccione un distrito", IdMunicipio = 0 });
                // Vuelve a agregar la opción predeterminada a la lista de distritos.

                var zonas = _context.Zonas.Where(z => z.IdDistrito == horario.IdZonaNavigation.IdDistrito).ToList();
                // Vuelve a obtener la lista de zonas para el distrito del horario (si la navegación es válida).

                zonas.Add(new Zona { Id = 0, Nombre = "Seleccione una zona" });
                // Vuelve a agregar la opción predeterminada a la lista de zonas.

                var operadores = _context.Operadores.Where(s => s.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value)).ToList();
                // Vuelve a obtener la lista de operadores para la alcaldía del usuario.

                operadores.Add(new Operador { Id = 0, Nombre = "Seleccione un operador" });
                // Vuelve a agregar la opción predeterminada a la lista de operadores.

                ViewBag.DiasSeleccionados = DiasSeleccionados;
                // Vuelve a pasar la lista de días seleccionados al ViewBag para que la vista pueda mantener la selección del usuario.

                ViewData["Distritos"] = new SelectList(distritos, "Id", "Nombre", horario.IdZonaNavigation?.IdDistrito);
                // Vuelve a cargar la lista de distritos en ViewData, manteniendo la selección actual.

                ViewData["Zonas"] = new SelectList(zonas, "Id", "Nombre", horario.IdZona);
                // Vuelve a cargar la lista de zonas en ViewData, manteniendo la selección actual.

                ViewData["Operadores"] = new SelectList(operadores, "Id", "Nombre", horario.IdOperador);
                // Vuelve a cargar la lista de operadores en ViewData, manteniendo la selección actual.

                return View(horario);
                // Devuelve la vista "Edit" con el objeto 'horario' recargado y los datos de las listas de selección actualizados.
                // Esto permite al usuario ver los datos que intentó guardar y cualquier mensaje de error para corregirlos.
            }
        }


        public IActionResult Details(int id)
        {
            // Declara un método síncrono llamado Details que recibe un entero 'id' como parámetro.
            // Este método se encarga de mostrar los detalles de un horario específico.

            var horario = _context.Horarios
                // Obtiene un registro de la tabla "Horarios" del contexto de la base de datos (_context).
                .Include(h => h.IdOperadorNavigation)
                // Incluye los datos de la entidad relacionada "IdOperadorNavigation" (Operador) para el horario.
                .Include(h => h.IdZonaNavigation)
                // Incluye los datos de la entidad relacionada "IdZonaNavigation" (Zona) para el horario.
                .FirstOrDefault(h => h.Id == id);
            // Busca el primer horario cuyo Id coincida con el 'id' proporcionado.
            // FirstOrDefault devuelve null si no se encuentra ningún horario con ese Id.

            if (horario == null)
            {
                // Verifica si el horario con el Id especificado no fue encontrado en la base de datos.
                return NotFound();
                // Devuelve un resultado NotFound (código de estado HTTP 404), indicando que el recurso solicitado no existe.
            }

            // Extraer los días seleccionados del atributo Dia
            var diasSeleccionados = string.IsNullOrEmpty(horario.Dia)
                ? new List<int>()
                : horario.Dia.Split(',').Select(int.Parse).ToList();
            // Extrae los días seleccionados que están almacenados como una cadena separada por comas en la propiedad 'Dia' del horario.
            // Si 'horario.Dia' es nulo o vacío, se crea una nueva lista vacía de enteros.
            // De lo contrario, la cadena se divide por comas, cada parte se convierte a un entero y se crea una lista de enteros.

            ViewBag.DiasSeleccionados = diasSeleccionados;
            // Almacena la lista de días seleccionados en ViewBag para que pueda ser utilizada en la vista para mostrar los días en los que el horario está activo.

            return View(horario);
            // Devuelve la vista predeterminada asociada a la acción "Details", pasando el objeto "horario" encontrado como modelo.
            // La vista utilizará este modelo y los datos en ViewBag para mostrar los detalles del horario, incluyendo la información del operador, la zona y los días seleccionados.
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Declara un método síncrono llamado Delete que recibe un entero 'id' como parámetro.
            // Este método se encarga de mostrar una confirmación antes de eliminar un horario específico.

            var horario = _context.Horarios
                // Obtiene un registro de la tabla "Horarios" del contexto de la base de datos (_context).
                .Include(h => h.IdOperadorNavigation)
                // Incluye los datos de la entidad relacionada "IdOperadorNavigation" (Operador) para el horario.
                .Include(h => h.IdZonaNavigation)
                // Incluye los datos de la entidad relacionada "IdZonaNavigation" (Zona) para el horario.
                .FirstOrDefault(h => h.Id == id);
            // Busca el primer horario cuyo Id coincida con el 'id' proporcionado.
            // FirstOrDefault devuelve null si no se encuentra ningún horario con ese Id.

            if (horario == null)
            {
                // Verifica si el horario con el Id especificado no fue encontrado en la base de datos.
                return NotFound();
                // Devuelve un resultado NotFound (código de estado HTTP 404), indicando que el recurso solicitado no existe.
            }

            // Extraer los días seleccionados del atributo Dia
            var diasSeleccionados = string.IsNullOrEmpty(horario.Dia)
                ? new List<int>()
                : horario.Dia.Split(',').Select(int.Parse).ToList();
            // Extrae los días seleccionados que están almacenados como una cadena separada por comas en la propiedad 'Dia' del horario.
            // Si 'horario.Dia' es nulo o vacío, se crea una nueva lista vacía de enteros.
            // De lo contrario, la cadena se divide por comas, cada parte se convierte a un entero y se crea una lista de enteros.

            ViewBag.DiasSeleccionados = diasSeleccionados;
            // Almacena la lista de días seleccionados en ViewBag para que pueda ser utilizada en la vista para mostrar los días en los que el horario está activo.
            // Esto permite al usuario ver todos los detalles del horario antes de confirmar la eliminación.

            return View(horario);
            // Devuelve la vista predeterminada asociada a la acción "Delete", pasando el objeto "horario" encontrado como modelo.
            // Esta vista probablemente mostrará los detalles del horario y un botón para confirmar la eliminación.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Declara un método síncrono llamado DeleteConfirmed que recibe un entero 'id' como parámetro.
            // Este método se encarga de eliminar el horario de la base de datos después de que el usuario ha confirmado la eliminación.

            var horario = _context.Horarios.Find(id);
            // Busca el horario con el Id especificado en la base de datos utilizando el método Find.
            // Find es eficiente para buscar por la clave primaria.

            if (horario != null)
            {
                // Verifica si se encontró un horario con el Id proporcionado.
                _context.Horarios.Remove(horario);
                // Marca el objeto 'horario' para ser eliminado de la base de datos.
                // La entidad se rastrea por el contexto y se prepara para la eliminación.

                _context.SaveChanges();
                // Guarda todos los cambios realizados en el contexto de la base de datos.
                // En este caso, elimina el registro del horario de la tabla correspondiente.
            }
            // Si el horario con el Id especificado no se encuentra (horario es null), no se realiza ninguna acción de eliminación.
            // Esto podría ocurrir si el horario ya fue eliminado por otro usuario o proceso.

            return RedirectToAction(nameof(Index));
            // Redirige al usuario a la acción "Index" del mismo controlador, que probablemente muestra la lista de horarios actualizada.
            // Esto indica al usuario que la operación de eliminación (si se realizó) ha finalizado.
        }

        public IActionResult GetZonas(int idDistrito)
        {
            // Declara un método síncrono llamado GetZonas que recibe un entero 'idDistrito' como parámetro.
            // Este método probablemente se utiliza para obtener dinámicamente las zonas correspondientes a un distrito seleccionado,
            // y devuelve los resultados en formato JSON.

            var zonas = _context.Zonas
                // Accede a la tabla "Zonas" a través del contexto de la base de datos (_context).
                .Where(z => z.IdDistrito == idDistrito) // Filtrar zonas por distrito
                                                        // Filtra la colección de zonas para incluir solo aquellas cuya propiedad 'IdDistrito' coincida con el 'idDistrito' proporcionado.
                .Select(z => new { z.Id, z.Nombre })
                // Proyecta los resultados en una nueva colección anónima que contiene solo las propiedades 'Id' y 'Nombre' de cada zona.
                // Esto es útil para enviar solo la información necesaria al cliente.
                .ToList();
            // Ejecuta la consulta a la base de datos y convierte los resultados en una lista.

            return Json(zonas);
            // Devuelve la lista de zonas en formato JSON.
            // Esto permite que una solicitud AJAX desde el cliente (probablemente una página web con un dropdown list de distritos)
            // reciba los datos de las zonas correspondientes y actualice dinámicamente otro dropdown list de zonas.
        }


        private DateTime GetDateTimeForDay(string dia, TimeOnly hora)
        {
            // Declara un método privado llamado GetDateTimeForDay que recibe una cadena 'dia' (nombre del día) y un objeto TimeOnly 'hora' como parámetros.
            // Este método tiene como objetivo combinar el día de la semana especificado con la hora proporcionada,
            // y devolver un objeto DateTime que represente esa fecha y hora en la semana actual.

            var today = DateTime.Today;
            // Obtiene la fecha actual (sin la hora) y la almacena en la variable 'today'.

            var dayOfWeek = Enum.Parse<DayOfWeek>(dia, true); // Convertimos el nombre del día a DayOfWeek
                                                              // Convierte la cadena 'dia' (por ejemplo, "Monday", "Tuesday") a su correspondiente valor de la enumeración DayOfWeek.
                                                              // El segundo argumento 'true' indica que la conversión no debe distinguir entre mayúsculas y minúsculas.

            // Calculamos la fecha correcta del día de la semana (de acuerdo con el día actual)
            int daysToAdd = (int)dayOfWeek - (int)today.DayOfWeek;
            // Calcula la diferencia en días entre el día de la semana deseado ('dayOfWeek') y el día de la semana actual ('today.DayOfWeek').
            // Los valores de la enumeración DayOfWeek se convierten a enteros para realizar la resta.

            if (daysToAdd < 0)
                daysToAdd += 7;
            // Si la diferencia en días es negativa, significa que el día de la semana deseado ya pasó en la semana actual.
            // En este caso, se le suma 7 para obtener la fecha del mismo día de la semana en la *próxima* semana.
            // Esto asegura que el método siempre devuelva una fecha en la semana actual o la siguiente.

            // Convertimos TimeOnly a TimeSpan
            TimeSpan horaEntrada = hora.ToTimeSpan();
            // Convierte el objeto TimeOnly 'hora' a un objeto TimeSpan, que representa un intervalo de tiempo.
            // Esto es necesario para poder sumar la hora a un objeto DateTime.

            // Fecha final con la hora combinada
            return today.AddDays(daysToAdd).Add(horaEntrada);
            // Crea un nuevo objeto DateTime combinando la fecha actual ('today') con los días calculados para llegar al día de la semana deseado ('daysToAdd')
            // y la hora proporcionada ('horaEntrada').
            // El método devuelve esta fecha y hora combinadas.
        }

        public IActionResult FullCalendar()
        {
            // Declara un método síncrono llamado FullCalendar que devuelve un IActionResult.
            // Este método probablemente se utiliza para mostrar una vista que integra la librería FullCalendar.

            return View();
            // Devuelve la vista predeterminada asociada a la acción "FullCalendar".
            // Se espera que esta vista contenga el código HTML y JavaScript necesarios para inicializar y mostrar un calendario utilizando la librería FullCalendar.
            // La lógica para cargar los eventos en el calendario probablemente se realizará a través de una llamada AJAX a otra acción del controlador.
        }

        public IActionResult GetHorarios()
        {
            // Declara un método síncrono llamado GetHorarios que devuelve un IActionResult.
            // Este método se encarga de obtener los horarios de la base de datos,
            // transformarlos en un formato adecuado para FullCalendar y devolverlos como JSON.

            List<Horario> horarios = new List<Horario>();
            // Inicializa una lista vacía para almacenar los horarios obtenidos de la base de datos.

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
            // Define un diccionario que mapea los números de día de la semana (probablemente almacenados en la base de datos)
            // a sus nombres correspondientes en español.

            if (User.IsInRole("Alcaldia"))
            {
                // Verifica si el usuario actual tiene el rol "Alcaldia".
                horarios = _context.Horarios
                    .Include(h => h.IdOperadorNavigation)
                    .Include(h => h.IdZonaNavigation)
                    .Where(s => s.IdOperadorNavigation.IdAlcaldia == Convert.ToInt32(User.FindFirst("Id").Value))
                    .ToList();
                // Si el usuario es una Alcaldía, obtiene todos los horarios cuyos operadores pertenecen a la misma alcaldía del usuario.
                // Se incluyen las entidades relacionadas Operador y Zona para acceder a sus propiedades.
                // Se utiliza el claim "Id" del usuario para identificar la alcaldía.
            }
            if (User.IsInRole("Operador"))
            {
                // Verifica si el usuario actual tiene el rol "Operador".
                horarios = _context.Horarios
                    .Include(h => h.IdOperadorNavigation)
                    .Include(h => h.IdZonaNavigation)
                    .Where(s => s.IdOperador == Convert.ToInt32(User.FindFirst("Id").Value))
                    .ToList();
                // Si el usuario es un Operador, obtiene todos los horarios que están asignados a ese operador.
                // Se incluye las entidades relacionadas Operador y Zona.
                // Se utiliza el claim "Id" del usuario para identificar al operador.
            }
            if (User.IsInRole("Ciudadano"))
            {
                // Verifica si el usuario actual tiene el rol "Usuario" (se asume que este rol representa un usuario final).
                horarios = _context.Horarios
                    .Include(h => h.IdOperadorNavigation)
                    .Include(h => h.IdZonaNavigation)
                    .Where(s => s.IdZonaNavigation.Nombre == User.FindFirst("Zona").Value)
                    .ToList();
                // Si el usuario tiene el rol "Usuario", obtiene todos los horarios que corresponden a la zona del usuario.
                // Se incluye las entidades relacionadas Operador y Zona.
                // Se utiliza el claim "Zona" del usuario para identificar la zona.
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
            // Transforma la lista de horarios en una lista de eventos en un formato esperado por FullCalendar.
            // - SelectMany itera sobre cada horario y luego sobre cada día de la semana en que ese horario está activo (obtenido al dividir la cadena 'Dia').
            // - Para cada día, crea un nuevo objeto anónimo con las siguientes propiedades:
            //   - id: El ID del horario.
            //   - title: Una cadena que combina el nombre del operador y la zona.
            //   - start: La fecha y hora de inicio del horario para ese día específico (utilizando el método GetDateTimeForDay).
            //   - end: La fecha y hora de fin del horario para ese día específico (utilizando el método GetDateTimeForDay).
            //   - operador: El nombre del operador.
            //   - zona: El nombre de la zona.
            //   - turno: "Matutino" o "Vespertino" basado en el valor de la propiedad 'Turno'.
            //   - horaEntrada: La hora de entrada formateada con AM/PM.
            //   - horaSalida: La hora de salida formateada con AM/PM.
            //   - dia: Una cadena con los nombres de los días de la semana para este horario (convirtiendo los números a nombres usando el diccionario diasSemana).
            // - ToList() convierte la secuencia resultante en una lista.

            return Json(events);
            // Devuelve la lista de eventos en formato JSON.
            // Esta respuesta JSON será consumida por la librería FullCalendar en la vista para mostrar los horarios en el calendario.
        }


        private string ObtenerTurno(int turno)
        {
            // Declara un método privado llamado ObtenerTurno que recibe un entero 'turno' como parámetro.
            // Este método tiene como objetivo devolver una cadena que represente el turno (Matutino o Vespertino)
            // basado en el valor del entero proporcionado.

            return turno == 1 ? "Matutino" : "Vespertino";
            // Utiliza el operador condicional ternario para determinar el turno.
            // Si el valor de 'turno' es igual a 1, devuelve la cadena "Matutino".
            // De lo contrario (si 'turno' no es 1), devuelve la cadena "Vespertino".
            // Se asume que el valor 1 representa el turno matutino y cualquier otro valor representa el turno vespertino.
        }

        [HttpPost]
        public IActionResult CreateHorario([FromBody] Horario horario)
        {
            // Declara un método síncrono llamado CreateHorario que recibe un objeto Horario como parámetro.
            // El atributo [FromBody] indica que el objeto horario se espera que venga en el cuerpo de la solicitud HTTP (probablemente como JSON).
            // Este método se utiliza para crear un nuevo horario a través de una solicitud AJAX.

            if (ModelState.IsValid)
            {
                // Verifica si el objeto 'horario' recibido es válido según las reglas de validación definidas en el modelo (DataAnnotations).
                _context.Horarios.Add(horario);
                // Agrega el objeto 'horario' a la colección de entidades Horarios en el contexto de la base de datos.
                // Esto marca la entidad para ser insertada en la base de datos.
                _context.SaveChanges();
                // Guarda los cambios en la base de datos, lo que resulta en la inserción del nuevo horario.
                return Json(new { success = true });
                // Si la creación del horario es exitosa y la validación es correcta, devuelve una respuesta JSON con la propiedad 'success' establecida en true.
                // Esto indica al cliente que la operación se realizó con éxito.
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
            // Si el ModelState no es válido (hay errores de validación), devuelve una respuesta JSON con:
            // - 'success' establecido en false, indicando que la operación falló.
            // - 'errors' que contiene una colección de todos los mensajes de error de validación presentes en el ModelState.
            //   - ModelState.Values obtiene todos los estados de las propiedades del modelo.
            //   - SelectMany(v => v.Errors) flattening la lista de errores de cada propiedad en una única colección.
            // Esto permite al cliente recibir información sobre los errores de validación y mostrarlos al usuario.
        }


        [HttpPost]
        public IActionResult UpdateHorario([FromBody] Horario horario)
        {
            // Declara un método síncrono llamado UpdateHorario que recibe un objeto Horario como parámetro.
            // El atributo [FromBody] indica que el objeto horario se espera que venga en el cuerpo de la solicitud HTTP (probablemente como JSON).
            // Este método se utiliza para actualizar un horario existente a través de una solicitud AJAX.

            if (ModelState.IsValid)
            {
                // Verifica si el objeto 'horario' recibido es válido según las reglas de validación definidas en el modelo (DataAnnotations).
                _context.Horarios.Update(horario);
                // Marca el objeto 'horario' como modificado en el contexto de la base de datos.
                // Entity Framework rastreará los cambios realizados en las propiedades de esta entidad.
                _context.SaveChanges();
                // Guarda los cambios en la base de datos, lo que resulta en la actualización del registro del horario.
                return Json(new { success = true });
                // Si la actualización del horario es exitosa y la validación es correcta, devuelve una respuesta JSON con la propiedad 'success' establecida en true.
                // Esto indica al cliente que la operación se realizó con éxito.
            }
            return Json(new { success = false });
            // Si el ModelState no es válido (hay errores de validación), devuelve una respuesta JSON con la propiedad 'success' establecida en false.
            // Esto indica al cliente que la operación falló debido a errores de validación en los datos proporcionados.
            // A diferencia del método CreateHorario, aquí no se envían los detalles de los errores de validación.
        }


        [HttpPost]
        public IActionResult DeleteHorario(int id)
        {
            // Declara un método síncrono llamado DeleteHorario que recibe un entero 'id' como parámetro.
            // Este método se utiliza para eliminar un horario de la base de datos a través de una solicitud AJAX.

            var horario = _context.Horarios.Find(id);
            // Busca el horario con el Id especificado en la base de datos utilizando el método Find.
            // Find es eficiente para buscar por la clave primaria.

            if (horario != null)
            {
                // Verifica si se encontró un horario con el Id proporcionado.
                _context.Horarios.Remove(horario);
                // Marca el objeto 'horario' para ser eliminado de la base de datos.
                // La entidad se rastrea por el contexto y se prepara para la eliminación.
                _context.SaveChanges();
                // Guarda los cambios en la base de datos, lo que resulta en la eliminación del registro del horario.
                return Json(new { success = true });
                // Si la eliminación del horario es exitosa, devuelve una respuesta JSON con la propiedad 'success' establecida en true.
                // Esto indica al cliente que la operación se realizó con éxito.
            }
            return Json(new { success = false });
            // Si no se encontró ningún horario con el Id especificado, devuelve una respuesta JSON con la propiedad 'success' establecida en false.
            // Esto indica al cliente que la operación de eliminación no se pudo completar (probablemente porque el horario no existía).
        }

    }
}
