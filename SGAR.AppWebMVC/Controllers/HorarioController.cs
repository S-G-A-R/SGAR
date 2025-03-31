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

namespace SGAR.AppWebMVC.Controllers
{
    public class HorarioController : Controller
    {
        private readonly SgarDbContext _context;

        public HorarioController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Horario
        public async Task<IActionResult> Index()
        {
            // 1. Inicia una operación asincrónica y devuelve un Task<IActionResult>.
            //  Esto significa que la acción Index puede realizar operaciones que no bloquean el hilo principal.

            return View(await _context.Horarios
                // 2. Devuelve una vista (IActionResult) con los datos obtenidos de la base de datos.
                //    La palabra clave 'await' espera a que la operación asincrónica dentro de los paréntesis se complete.

                .Include(h => h.IdOperadorNavigation)
                // 3. Incluye los datos relacionados de la tabla 'Operadores' a través de la propiedad de navegación 'IdOperadorNavigation'.
                //    Esto evita consultas adicionales a la base de datos para obtener la información del operador.

                .Include(h => h.IdZonaNavigation)
                // 4. Incluye los datos relacionados de la tabla 'Zonas' a través de la propiedad de navegación 'IdZonaNavigation'.
                //    Similar al paso anterior, esto evita consultas adicionales para la información de la zona.

                .ToListAsync());
            // 5. Convierte los resultados de la consulta en una lista asincrónica.
            //    'ToListAsync()' es un método de Entity Framework Core que realiza la consulta a la base de datos y devuelve una lista de objetos 'Horario'.
        }

        // GET: Horario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // 1. Declara un método asíncrono llamado 'Details' que acepta un parámetro entero opcional 'id' y devuelve un 'IActionResult'.
            //    Este método maneja la solicitud para mostrar los detalles de un horario específico.

            if (id == null)
            {
                // 2. Verifica si el parámetro 'id' es nulo.
                //    Si 'id' es nulo, significa que no se proporcionó un ID válido.

                return NotFound();
                // 3. Devuelve un resultado 'NotFound' (código de estado 404) si el 'id' es nulo, indicando que el recurso no se encontró.
            }

            var horario = await _context.Horarios
                // 4. Inicia una consulta asíncrona a la base de datos para obtener un horario específico.
                //    La palabra clave 'await' espera a que la consulta se complete.

                .Include(h => h.IdOperadorNavigation)
                // 5. Incluye los datos relacionados de la tabla 'Operadores' a través de la propiedad de navegación 'IdOperadorNavigation'.
                //    Esto evita una consulta adicional a la base de datos para obtener la información del operador.

                .Include(h => h.IdZonaNavigation)
                // 6. Incluye los datos relacionados de la tabla 'Zonas' a través de la propiedad de navegación 'IdZonaNavigation'.
                //    Esto evita una consulta adicional a la base de datos para obtener la información de la zona.

                .FirstOrDefaultAsync(m => m.Id == id);
            // 7. Ejecuta la consulta para encontrar el primer horario cuyo 'Id' coincida con el 'id' proporcionado.
            //    'FirstOrDefaultAsync' devuelve el primer elemento que coincide con la condición o 'null' si no se encuentra ninguno.

            if (horario == null)
            {
                // 8. Verifica si el horario encontrado es nulo.
                //    Si 'horario' es nulo, significa que no se encontró ningún horario con el 'id' proporcionado.

                return NotFound();
                // 9. Devuelve un resultado 'NotFound' (código de estado 404) si el horario es nulo, indicando que el recurso no se encontró.
            }

            return View(horario);
            // 10. Devuelve una vista con el objeto 'horario' encontrado.
            //     La vista mostrará los detalles del horario específico.
        }


        // GET: Horario/Create
       public IActionResult Create()
{
    // 1. Declara un método llamado 'Create' que devuelve un 'IActionResult'.
    //    Este método maneja la solicitud para mostrar el formulario de creación de un nuevo horario.

    List<Zona> zonas = [new Zona { Nombre = "SELECCIONAR", Id = 0, IdDistrito = 0, IdAlcaldia = 0 }];
    // 2. Crea una lista de objetos 'Zona' e inicializa con un elemento predeterminado.
    //    Este elemento se utiliza como opción "SELECCIONAR" en el desplegable de zonas.

    var thisAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.Id == Convert.ToInt32(User.FindFirst("Id").Value));
    // 3. Obtiene la alcaldía del usuario actual desde la base de datos.
    //    'User.FindFirst("Id").Value' recupera el ID de la alcaldía del usuario autenticado.
    //    'Convert.ToInt32()' convierte el valor del ID a un entero.
    //    'FirstOrDefault()' recupera el primer elemento que coincide con la condición o 'null' si no se encuentra ninguno.

    var distritos = _context.Distritos.Where(s=> s.IdMunicipio == thisAlcaldia.IdMunicipio).ToList();
    // 4. Obtiene una lista de distritos que pertenecen al municipio de la alcaldía del usuario.
    //    'Where()' filtra los distritos por 'IdMunicipio'.
    //    'ToList()' convierte los resultados de la consulta en una lista.

    distritos.Add(new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 });
    // 5. Agrega un elemento predeterminado a la lista de distritos.
    //    Este elemento se utiliza como opción "SELECCIONAR" en el desplegable de distritos.

    ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
    // 6. Crea un objeto 'SelectList' para los distritos y lo asigna a 'ViewData["DistritoId"]'.
    //    'SelectList' se utiliza para generar un desplegable en la vista.
    //    El tercer parámetro (0) establece el valor seleccionado por defecto.

    ViewData["ZonaId"] = new SelectList(zonas, "Id", "Nombre", 0);
    // 7. Crea un objeto 'SelectList' para las zonas y lo asigna a 'ViewData["ZonaId"]'.
    //    Similar al paso anterior, esto genera un desplegable para las zonas.
    //    El tercer parámetro (0) establece el valor seleccionado por defecto.

    return View();
    // 8. Devuelve la vista 'Create'.
    //    La vista mostrará el formulario para crear un nuevo horario, incluyendo los desplegables de distritos y zonas.
}

        // POST: Horario/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Horario horario, List<string> DiasSeleccionados)
        {
            // 1. Declara un método asíncrono llamado 'Create' que maneja la solicitud POST para crear un horario.
            //    'HttpPost' indica que este método responde a solicitudes HTTP POST.
            //    'ValidateAntiForgeryToken' protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
            //    'Horario horario' recibe el objeto 'Horario' enviado desde el formulario.
            //    'List<string> DiasSeleccionados' recibe la lista de días seleccionados desde el formulario.

            try
            {
                // 2. Inicia un bloque 'try' para manejar posibles excepciones durante la creación del horario.

                // Convierte los días seleccionados a formato binario
                horario.Dia = ConvertirDiasABinario(DiasSeleccionados);
                // 3. Llama a la función 'ConvertirDiasABinario' para convertir la lista de días seleccionados a un valor binario.
                //    El resultado se asigna a la propiedad 'Dia' del objeto 'horario'.
                //    Se asume que 'ConvertirDiasABinario' es una función definida en otro lugar del código.

                // Agrega el horario al contexto y guarda los cambios
                _context.Add(horario);
                // 4. Agrega el objeto 'horario' al contexto de la base de datos para que Entity Framework Core lo rastree.

                await _context.SaveChangesAsync();
                // 5. Guarda los cambios en la base de datos de forma asincrónica.

                // Muestra un mensaje de éxito y redirige a la vista de Index
                TempData["AlertMessage"] = "Horario creado exitosamente!!!";
                // 6. Almacena un mensaje de éxito en 'TempData' para mostrarlo en la vista 'Index'.

                return RedirectToAction("Index");
                // 7. Redirige al usuario a la vista 'Index', que muestra la lista de horarios.
            }
            catch
            {
                // 8. Inicia un bloque 'catch' para manejar cualquier excepción que ocurra durante la creación del horario.

                return RedirectToAction(nameof(Index)); // Si la validación falla, vuelve a la vista de creación con el modelo.
               // 9. Redirige al usuario a la vista 'Index' en caso de error.
              //    'nameof(Index)' obtiene el nombre de la acción 'Index' como una cadena.
            }

        }

        // GET: Horario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // 1. Declara un método asíncrono llamado 'Edit' que acepta un parámetro entero opcional 'id' y devuelve un 'IActionResult'.
            //    Este método maneja la solicitud para mostrar el formulario de edición de un horario específico.

            if (id == null)
            {
                // 2. Verifica si el parámetro 'id' es nulo.
                //    Si 'id' es nulo, significa que no se proporcionó un ID válido.

                return NotFound();
                // 3. Devuelve un resultado 'NotFound' (código de estado 404) si el 'id' es nulo, indicando que el recurso no se encontró.
            }

            var horario = await _context.Horarios.FindAsync(id);
            // 4. Busca un horario específico en la base de datos de forma asíncrona utilizando su 'id'.
            //    'FindAsync' es un método de Entity Framework Core que busca una entidad por su clave primaria.

            if (horario == null)
            {
                // 5. Verifica si el horario encontrado es nulo.
                //    Si 'horario' es nulo, significa que no se encontró ningún horario con el 'id' proporcionado.

                return NotFound();
                // 6. Devuelve un resultado 'NotFound' (código de estado 404) si el horario es nulo, indicando que el recurso no se encontró.
            }

            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", horario.IdOperador);
            // 7. Crea un objeto 'SelectList' para los operadores y lo asigna a 'ViewData["IdOperador"]'.
            //    'SelectList' se utiliza para generar un desplegable en la vista.
            //    El cuarto parámetro (horario.IdOperador) establece el valor seleccionado por defecto al operador que ya tiene asignado el horario.
            //  "Id","Id" significa que el valor y el texto que se muestran son el ID.

            ViewData["IdZona"] = new SelectList(_context.Zonas, "Id", "Nombre", horario.IdZona);
            // 8. Crea un objeto 'SelectList' para las zonas y lo asigna a 'ViewData["IdZona"]'.
            //    Similar al paso anterior, esto genera un desplegable para las zonas.
            //    El cuarto parámetro (horario.IdZona) establece el valor seleccionado por defecto a la zona que ya tiene asignado el horario.
            //  "Id", "Nombre" significa que el valor es el ID y el texto que se muestra es el nombre.

            return View(horario);
            // 9. Devuelve la vista 'Edit' con el objeto 'horario' encontrado.
            //    La vista mostrará el formulario de edición del horario, incluyendo los desplegables de operadores y zonas.
        }

        // POST: Horario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Horario horario, List<string> DiasSeleccionados)
        {
            // 1. Declara un método asíncrono llamado 'Edit' que maneja la solicitud POST para editar un horario.
            //    'HttpPost' indica que este método responde a solicitudes HTTP POST.
            //    'ValidateAntiForgeryToken' protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
            //    'Horario horario' recibe el objeto 'Horario' enviado desde el formulario.
            //    'List<string> DiasSeleccionados' recibe la lista de días seleccionados desde el formulario.

            if (ModelState.IsValid)
            {
                // 2. Verifica si el modelo 'horario' es válido según las reglas de validación definidas en el modelo.
                //    'ModelState.IsValid' devuelve 'true' si el modelo es válido, 'false' en caso contrario.

                var horarioEncontrado = await _context.Horarios.FindAsync(horario.Id);
                // 3. Busca el horario existente en la base de datos utilizando su 'Id'.
                //    'FindAsync' es un método asíncrono para buscar una entidad por su clave primaria.

                if (horarioEncontrado == null)
                {
                    // 4. Verifica si el horario encontrado es nulo.
                    //    Si 'horarioEncontrado' es nulo, significa que no se encontró ningún horario con el 'Id' proporcionado.

                    return NotFound();
                    // 5. Devuelve un resultado 'NotFound' (código de estado 404) si el horario no se encuentra.
                }

                horarioEncontrado.HoraEntrada = horario.HoraEntrada;
                // 6. Actualiza la propiedad 'HoraEntrada' del horario encontrado con el valor del horario recibido.
                horarioEncontrado.HoraSalida = horario.HoraSalida;
                // 7. Actualiza la propiedad 'HoraSalida' del horario encontrado con el valor del horario recibido.
                horarioEncontrado.Turno = horario.Turno;
                // 8. Actualiza la propiedad 'Turno' del horario encontrado con el valor del horario recibido.
                horarioEncontrado.IdOperador = horario.IdOperador;
                // 9. Actualiza la propiedad 'IdOperador' del horario encontrado con el valor del horario recibido.
                horarioEncontrado.IdZona = horario.IdZona;
                // 10. Actualiza la propiedad 'IdZona' del horario encontrado con el valor del horario recibido.
                horarioEncontrado.Dia = ConvertirDiasABinario(DiasSeleccionados);
                //11. Llama a la funcion ConvertirDiasABinario para convertir la lista de dias seleccionados a binario.

                _context.Update(horarioEncontrado);
                // 12. Marca el horario encontrado como modificado en el contexto de la base de datos.
                //     Esto indica a Entity Framework Core que debe actualizar este registro en la base de datos.

                await _context.SaveChangesAsync();
                // 13. Guarda los cambios en la base de datos de forma asíncrona.

                TempData["AlertMessage"] = "Horario editado exitosamente!!!";
                // 14. Almacena un mensaje de éxito en 'TempData' para mostrarlo en la vista 'Index'.

                return RedirectToAction("Index");
                // 15. Redirige al usuario a la vista 'Index', que muestra la lista de horarios.
            }

            return View(horario);
            // 16. Si el modelo no es válido, vuelve a mostrar el formulario de edición con los errores de validación.
        }


        // GET: Horario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // 1. Declara un método asíncrono llamado 'Delete' que acepta un parámetro entero opcional 'id' y devuelve un 'IActionResult'.
            //    Este método maneja la solicitud para mostrar la confirmación de eliminación de un horario específico.

            if (id == null)
            {
                // 2. Verifica si el parámetro 'id' es nulo.
                //    Si 'id' es nulo, significa que no se proporcionó un ID válido.

                return NotFound();
                // 3. Devuelve un resultado 'NotFound' (código de estado 404) si el 'id' es nulo, indicando que el recurso no se encontró.
            }

            var horario = await _context.Horarios
                // 4. Inicia una consulta asíncrona a la base de datos para obtener un horario específico.
                //    La palabra clave 'await' espera a que la consulta se complete.

                .Include(h => h.IdOperadorNavigation)
                // 5. Incluye los datos relacionados de la tabla 'Operadores' a través de la propiedad de navegación 'IdOperadorNavigation'.
                //    Esto evita una consulta adicional a la base de datos para obtener la información del operador.

                .Include(h => h.IdZonaNavigation)
                // 6. Incluye los datos relacionados de la tabla 'Zonas' a través de la propiedad de navegación 'IdZonaNavigation'.
                //    Esto evita una consulta adicional a la base de datos para obtener la información de la zona.

                .FirstOrDefaultAsync(m => m.Id == id);
            // 7. Ejecuta la consulta para encontrar el primer horario cuyo 'Id' coincida con el 'id' proporcionado.
            //    'FirstOrDefaultAsync' devuelve el primer elemento que coincide con la condición o 'null' si no se encuentra ninguno.

            if (horario == null)
            {
                // 8. Verifica si el horario encontrado es nulo.
                //    Si 'horario' es nulo, significa que no se encontró ningún horario con el 'id' proporcionado.

                return NotFound();
                // 9. Devuelve un resultado 'NotFound' (código de estado 404) si el horario es nulo, indicando que el recurso no se encontró.
            }

            return View(horario);
            // 10. Devuelve la vista 'Delete' con el objeto 'horario' encontrado.
            //     La vista mostrará los detalles del horario y un botón de confirmación para la eliminación.
        }

        // POST: Horario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Declara un método asíncrono llamado 'DeleteConfirmed' que maneja la solicitud POST para confirmar la eliminación de un horario.
            //    '[HttpPost, ActionName("Delete")]' indica que este método responde a solicitudes HTTP POST y que se invoca cuando se envía el formulario de confirmación de eliminación (con el nombre de acción "Delete").
            //    '[ValidateAntiForgeryToken]' protege contra ataques de falsificación de solicitudes entre sitios (CSRF).
            //    'int id' recibe el ID del horario a eliminar desde el formulario.

            if (id == null || _context.Horarios == null)
            {
                // 2. Verifica si el ID es nulo o si el DbSet 'Horarios' es nulo.
                //    Esto asegura que se haya proporcionado un ID válido y que el contexto de la base de datos esté disponible.

                return NotFound();
                // 3. Devuelve un resultado 'NotFound' (código de estado 404) si el ID es nulo o 'Horarios' es nulo, indicando que el recurso no se encontró.
            }

            var horario = await _context.Horarios.FirstOrDefaultAsync(h => h.Id == id);
            // 4. Busca el horario con el ID proporcionado en la base de datos de forma asíncrona.
            //    'FirstOrDefaultAsync' devuelve el primer horario que coincide con la condición o 'null' si no se encuentra ninguno.

            if (horario == null)
            {
                // 5. Verifica si el horario encontrado es nulo.
                //    Si 'horario' es nulo, significa que no se encontró ningún horario con el ID proporcionado.

                return NotFound();
                // 6. Devuelve un resultado 'NotFound' (código de estado 404) si el horario no se encuentra.
            }

            _context.Horarios.Remove(horario);
            // 7. Marca el horario encontrado para su eliminación en el contexto de la base de datos.
            //    'Remove' indica a Entity Framework Core que debe eliminar este registro de la base de datos.

            await _context.SaveChangesAsync();
            // 8. Guarda los cambios en la base de datos de forma asíncrona, eliminando el horario.

            TempData["AlertMessage"] = "Horario eliminado exitosamente!!!";
            // 9. Almacena un mensaje de éxito en 'TempData' para mostrarlo en la vista 'Index'.

            return RedirectToAction("Index");
            // 10. Redirige al usuario a la vista 'Index', que muestra la lista de horarios.
        }

        
        private bool HorarioExists(int id)
        {
            // 1. Declara un método privado llamado 'HorarioExists' que acepta un parámetro entero 'id' y devuelve un valor booleano.
            //    Este método verifica si existe un horario con el ID proporcionado en la base de datos.

            return _context.Horarios.Any(e => e.Id == id);
            // 2. Utiliza el método 'Any' de Entity Framework Core para verificar si existe algún horario en la tabla 'Horarios' que coincida con el ID proporcionado.
            //    'Any' devuelve 'true' si encuentra al menos un elemento que cumple con la condición, y 'false' si no encuentra ninguno.
            //    'e => e.Id == id' es una expresión lambda que define la condición de búsqueda: verifica si el 'Id' de cada horario ('e') es igual al 'id' proporcionado.
        }

        private string ConvertirDiasABinario(List<string> dias)
        {
            // 1. Declara un método privado llamado 'ConvertirDiasABinario' que acepta una lista de cadenas 'dias' y devuelve una cadena.
            //    Este método convierte la lista de días seleccionados en una representación binaria.

            string[] semana = new string[] { "0", "0", "0", "0", "0", "0", "0" };
            // 2. Inicializa un arreglo de cadenas llamado 'semana' con 7 elementos, todos inicializados a "0".
            //    Este arreglo representa los 7 días de la semana, donde "0" significa que el día no está seleccionado.

            foreach (var dia in dias)
            {
                // 3. Inicia un bucle 'foreach' para recorrer la lista de días seleccionados 'dias'.
                //    'dia' representa cada día seleccionado en la lista.

                switch (dia)
                {
                    // 4. Inicia una declaración 'switch' para determinar el día de la semana correspondiente a la cadena 'dia'.

                    case "Lunes": semana[0] = "1"; break;
                    // 5. Si 'dia' es "Lunes", establece el primer elemento de 'semana' (índice 0) a "1", indicando que el lunes está seleccionado.

                    case "Martes": semana[1] = "1"; break;
                    // 6. Si 'dia' es "Martes", establece el segundo elemento de 'semana' (índice 1) a "1".

                    case "Miércoles": semana[2] = "1"; break;
                    // 7. Si 'dia' es "Miércoles", establece el tercer elemento de 'semana' (índice 2) a "1".

                    case "Jueves": semana[3] = "1"; break;
                    // 8. Si 'dia' es "Jueves", establece el cuarto elemento de 'semana' (índice 3) a "1".

                    case "Viernes": semana[4] = "1"; break;
                    // 9. Si 'dia' es "Viernes", establece el quinto elemento de 'semana' (índice 4) a "1".

                    case "Sábado": semana[5] = "1"; break;
                    // 10. Si 'dia' es "Sábado", establece el sexto elemento de 'semana' (índice 5) a "1".

                    case "Domingo": semana[6] = "1"; break;
                        // 11. Si 'dia' es "Domingo", establece el séptimo elemento de 'semana' (índice 6) a "1".
                }
            }

            return string.Join("", semana);
            // 12. Devuelve una cadena que es la concatenación de todos los elementos en el arreglo 'semana'.
            //     'string.Join("", semana)' une todos los elementos de 'semana' sin ningún separador entre ellos, creando una cadena binaria.
        }


        public IActionResult Calendar()
        {
            // 1. Declara un método público llamado 'Calendar' que devuelve un 'IActionResult'.
            //    Este método maneja la solicitud para mostrar un calendario con los horarios.

            List<Horario> horarios = _context.Horarios.ToList();
            // 2. Obtiene una lista de todos los horarios de la base de datos y la almacena en la variable 'horarios'.
            //    'ToList()' ejecuta la consulta y devuelve los resultados como una lista de objetos 'Horario'.

            List<object> items = new List<object>();
            // 3. Crea una lista de objetos llamada 'items' para almacenar los eventos del calendario.
            //    Usamos 'object' porque los eventos tendrán propiedades personalizadas.

            foreach (Horario horario in horarios)
            {
                // 4. Inicia un bucle 'foreach' para recorrer la lista de horarios.
                //    'horario' representa cada horario en la lista.

                var dias = horario.Dia; // Esto es un string como "1001011", donde cada 1 indica un día activo
                                        // 5. Obtiene la cadena 'Dia' del horario actual, que representa los días activos en formato binario (por ejemplo, "1001011").

                for (int i = 0; i < dias.Length; i++)
                {
                    // 6. Inicia un bucle 'for' para recorrer cada carácter de la cadena 'dias'.
                    //    'i' representa el índice del carácter actual.

                    if (dias[i] == '1') // Si el día está activo, lo agregamos como evento
                    {
                        // 7. Verifica si el carácter actual en 'dias' es '1', lo que indica que el día correspondiente está activo.

                        // Calculamos la fecha para cada día activo (agregando los días de la semana al día de hoy)
                        var fechaEvento = DateTime.Today.AddDays(i); // Agrega el índice al día actual
                                                                     // 8. Calcula la fecha del evento para el día activo actual.
                                                                     //    'DateTime.Today' obtiene la fecha actual.
                                                                     //    'AddDays(i)' agrega 'i' días a la fecha actual, donde 'i' representa el día de la semana (0 = Lunes, 1 = Martes, etc.).

                        // Creamos el evento con la fecha y la hora de entrada y salida
                        var item = new
                        {
                            // 9. Crea un objeto anónimo 'item' para representar el evento del calendario.
                            id = horario.Id,
                            // 10. Asigna el ID del horario al evento.
                            title = $"Operador {horario.IdOperador} - Turno {horario.Turno}",
                            // 11. Crea el título del evento, que incluye el ID del operador y el turno.
                            start = fechaEvento.Add(horario.HoraEntrada.ToTimeSpan()).ToString("yyyy-MM-ddTHH:mm:ss"),
                            // 12. Calcula la fecha y hora de inicio del evento y la formatea como una cadena ISO 8601.
                            //     'fechaEvento.Add(horario.HoraEntrada.ToTimeSpan())' agrega la hora de entrada a la fecha del evento.
                            end = fechaEvento.Add(horario.HoraSalida.ToTimeSpan()).ToString("yyyy-MM-ddTHH:mm:ss"),
                            // 13. Calcula la fecha y hora de finalización del evento y la formatea como una cadena ISO 8601.
                            //     'fechaEvento.Add(horario.HoraSalida.ToTimeSpan())' agrega la hora de salida a la fecha del evento.
                            dias = dias[i] // Aquí se almacena el día activo
                                           //14. Almacena el dia activo en el evento.
                        };
                        items.Add(item);
                        // 15. Agrega el objeto 'item' a la lista de eventos 'items'.
                    }
                }
            }

            // Convertimos los datos a formato JSON y lo pasamos a la vista
            ViewBag.Horarios = JsonConvert.SerializeObject(items);
            // 16. Serializa la lista de eventos 'items' a formato JSON y la almacena en 'ViewBag.Horarios'.
            //     'JsonConvert.SerializeObject(items)' convierte la lista de objetos en una cadena JSON.

            return View();
            // 17. Devuelve la vista 'Calendar', que mostrará el calendario con los eventos.
        }


    }
}
