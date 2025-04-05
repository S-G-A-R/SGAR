using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;

namespace SGAR.AppWebMVC.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ZonaController : Controller
    {
        private readonly SgarDbContext _context;

        public ZonaController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Zona
        public async Task<IActionResult> Index(Zona zona, int topRegistro = 10)
        {
            // 1. Inicialización de la consulta:
            //    - Se crea una variable 'query' de tipo IQueryable<Zona> a partir del DbSet 'Zonas' del contexto de la base de datos (_context).
            //    - AsQueryable() convierte el DbSet en una consulta LINQ que se puede construir de forma incremental.

            var query = _context.Zonas.AsQueryable();

            // 2. Filtrado por nombre (opcional):
            //    - Se verifica si la propiedad 'Nombre' del objeto 'zona' recibido como parámetro no es nula, vacía o contiene solo espacios en blanco.
            //    - Si tiene un valor, se agrega una cláusula 'Where' a la consulta para filtrar las zonas cuyo nombre contenga el valor de 'zona.Nombre'.

            if (!string.IsNullOrWhiteSpace(zona.Nombre))
                query = query.Where(s => s.Nombre.Contains(zona.Nombre));

            // 3. Filtrado por IdDistrito (opcional):
            //    - Se verifica si la propiedad 'IdDistrito' del objeto 'zona' es mayor que 0.
            //    - Si es mayor que 0, se agrega una cláusula 'Where' a la consulta para filtrar las zonas cuyo 'IdDistrito' coincida con el valor de 'zona.IdDistrito'.

            if (zona.IdDistrito > 0)
                query = query.Where(s => s.IdDistrito == zona.IdDistrito);

            // 4. Limitación de la cantidad de registros (opcional):
            //    - Se verifica si el parámetro 'topRegistro' es mayor que 0. El valor por defecto es 10.
            //    - Si es mayor que 0, se agrega una cláusula 'Take' a la consulta para seleccionar solo los primeros 'topRegistro' elementos.

            if (topRegistro > 0)
                query = query.Take(topRegistro);

            // 5. Inclusión de entidades relacionadas (Eager Loading):
            //    - Se utilizan las cláusulas 'Include' para cargar de forma anticipada las entidades relacionadas 'IdAlcaldiaNavigation' y 'IdDistritoNavigation'.
            //    - Esto evita consultas adicionales a la base de datos cuando se acceden a estas propiedades en la vista.

            query = query
                .Include(p => p.IdAlcaldiaNavigation).Include(p => p.IdDistritoNavigation);

            // 6. Ordenamiento de los resultados:
            //    - Se agrega una cláusula 'OrderByDescending' para ordenar los resultados de la consulta de forma descendente según la propiedad 'Id'.

            query = query.OrderByDescending(s => s.Id);

            // 7. Preparación de listas para DropDownList (SelectList):
            //    - Se crean listas de objetos 'Distrito' y 'Municipio'. Se inicializan con un elemento comodín "SELECCIONAR" con Id 0 para representar una opción no seleccionada en los dropdowns.
            //    - Se obtienen todos los departamentos de la base de datos y se agrega también un elemento "SELECCIONAR".
            //    - Se obtienen todas las alcaldías (municipios) de la base de datos.

            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
            var alcaldias = _context.Municipios.ToList();

            // 8. Pasar datos a la vista a través de ViewData:
            //    - Se utilizan objetos 'SelectList' para crear listas de opciones para los dropdowns en la vista.
            //    - 'ViewData["MunicipioId"]' se llena con una SelectList de 'municipios', utilizando las propiedades "Id" como valor y "Nombre" como texto visible. Se establece 0 como el valor seleccionado por defecto.
            //    - 'ViewData["DistritoId"]' se llena de manera similar con la lista de 'distritos'.
            //    - 'ViewData["DepartamentoId"]' se llena de manera similar con la lista de 'departamentos'.
            //    - 'ViewData["AlcaldiaId"]' se llena con una SelectList de 'alcaldias', utilizando "Id" y "Nombre". No se establece un valor seleccionado por defecto aquí.

            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            // 9. Retorno de la vista:
            //    - Se ejecuta la consulta LINQ construida hasta ahora utilizando 'ToListAsync()' para obtener una lista asíncrona de objetos 'Zona'.
            //    - Se ordena nuevamente la lista de forma descendente por 'Id' (aunque ya se había ordenado en la consulta, se repite aquí).
            //    - Se pasa esta lista como modelo a la vista asociada a esta acción ('Index'). La vista utilizará este modelo para mostrar la información de las zonas.

            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        public JsonResult GetMunicipiosFromDepartamentoId(int departamentoId)
        {
            // 1. Recepción del parámetro:
            //    - La acción recibe un parámetro entero llamado 'departamentoId' a través de la URL o el cuerpo de la solicitud.

            // 2. Consulta a la base de datos:
            //    - Se accede al DbSet 'Municipios' del contexto de la base de datos (_context).
            //    - Se utiliza LINQ (Language Integrated Query) con la cláusula 'Where' para filtrar los municipios cuya propiedad 'IdDepartamento' coincida con el valor del parámetro 'departamentoId'.
            //    - 'ToList()' ejecuta la consulta y devuelve una lista de objetos 'Municipio' que cumplen con el criterio de filtrado.

            // 3. Serialización a JSON y retorno:
            //    - La función 'Json()' toma la lista de municipios obtenida y la serializa a formato JSON (JavaScript Object Notation).
            //    - Este JSON se devuelve como la respuesta de la solicitud HTTP. Esto es comúnmente utilizado en escenarios de AJAX donde se necesita obtener datos del servidor sin recargar la página.

            return Json(_context.Municipios.Where(m => m.IdDepartamento == departamentoId).ToList());
        }

        public JsonResult GetDistritosFromMunicipioId(int municipioId)
        {
            // 1. Recepción del parámetro:
            //    - La acción recibe un parámetro entero llamado 'municipioId'.

            // 2. Consulta a la base de datos:
            //    - Se accede al DbSet 'Distritos' del contexto.
            //    - Se filtran los distritos donde la propiedad 'IdMunicipio' coincida con el valor de 'municipioId'.
            //    - 'ToList()' ejecuta la consulta y devuelve una lista de los distritos correspondientes.

            // 3. Serialización a JSON y retorno:
            //    - La lista de distritos se serializa a formato JSON y se devuelve como respuesta.

            return Json(_context.Distritos.Where(m => m.IdMunicipio == municipioId).ToList());
        }

        // GET: Zona/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // 1. Recepción del parámetro ID:
            //    - La acción recibe un parámetro entero nullable llamado 'id' desde la ruta (por ejemplo, "/Zona/Details/5"). El '?' indica que puede ser nulo.

            // 2. Verificación de ID nulo:
            //    - Se verifica si el valor del parámetro 'id' es nulo.
            //    - Si es nulo, significa que no se proporcionó un ID válido para buscar la zona, por lo que se devuelve un resultado 'NotFound' (código de estado HTTP 404).

            if (id == null)
            {
                return NotFound();
            }

            // 3. Consulta a la base de datos para obtener la zona:
            //    - Se accede al DbSet 'Zonas' del contexto.
            //    - Se utilizan las cláusulas 'Include' para cargar de forma anticipada las entidades relacionadas 'IdAlcaldiaNavigation' y 'IdDistritoNavigation'. Esto significa que cuando se obtenga la 'Zona', también se cargarán los datos de la alcaldía y el distrito asociados.
            //    - 'FirstOrDefaultAsync(m => m.Id == id)' ejecuta una consulta asíncrona para buscar la primera zona cuyo 'Id' coincida con el valor del parámetro 'id'. Si no se encuentra ninguna zona con ese ID, 'zona' será nulo.

            var zona = await _context.Zonas
                .Include(z => z.IdAlcaldiaNavigation)
                .Include(z => z.IdDistritoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 4. Obtención de todas las alcaldías para ViewData:
            //    - Se obtienen todas las alcaldías (municipios) de la base de datos y se almacenan en la variable 'alcaldias'.
            //    - Esto se hace probablemente para tener la lista de alcaldías disponible en la vista 'Details', aunque en este fragmento no se ve directamente su uso.

            var alcaldias = _context.Municipios.ToList();

            // 5. Pasar la lista de alcaldías a la vista a través de ViewData:
            //    - Se crea un objeto 'SelectList' a partir de la lista de 'alcaldias', utilizando la propiedad "Id" como valor y la propiedad "Nombre" como el texto visible en un posible dropdown en la vista.
            //    - Este 'SelectList' se almacena en el 'ViewData' con la clave "AlcaldiaId", lo que permite que la vista acceda a esta información.

            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            // 6. Verificación si la zona fue encontrada:
            //    - Se verifica si la variable 'zona' es nula.
            //    - Si es nula, significa que no se encontró ninguna zona con el ID proporcionado en la base de datos, por lo que se devuelve un resultado 'NotFound'.

            if (zona == null)
            {
                return NotFound();
            }

            // 7. Retorno de la vista con el modelo:
            //    - Si la zona fue encontrada, se devuelve la vista asociada a esta acción ('Details').
            //    - El objeto 'zona' se pasa como modelo a la vista, lo que permite que la vista acceda a las propiedades de la zona para mostrar sus detalles.

            return View(zona);
        }

        // GET: Zona/Create
        public IActionResult Create()
        {
            // 1. Inicialización de listas para DropDownList:
            //    - Se crean listas genéricas de objetos 'Distrito' y 'Municipio'.
            //    - Cada lista se inicializa con un único objeto que representa una opción predeterminada "SELECCIONAR" con un Id de 0. Esto se utiliza para mostrar una opción inicial en los dropdowns de la vista.
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];

            // 2. Obtención de departamentos desde la base de datos:
            //    - Se accede al DbSet 'Departamentos' del contexto de la base de datos (_context) y se utiliza 'ToList()' para obtener todos los registros de la tabla 'Departamentos' como una lista en memoria.
            var departamentos = _context.Departamentos.ToList();

            // 3. Adición de la opción "SELECCIONAR" a la lista de departamentos:
            //    - Se crea un nuevo objeto 'Departamento' con Nombre "SELECCIONAR" y Id 0.
            //    - Este objeto se añade a la lista 'departamentos'. Esto asegura que el dropdown de departamentos también tenga una opción inicial no seleccionada.
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // 4. Preparación de SelectList para los DropDownList en la vista:
            //    - Se crea un objeto 'SelectList' para cada uno de los dropdowns que se mostrarán en la vista de creación del formulario.
            //    - 'ViewData["MunicipioId"]': Se crea un SelectList utilizando la lista 'municipios'. El segundo argumento ("Id") especifica la propiedad del objeto 'Municipio' que se utilizará como el valor de cada opción en el dropdown. El tercer argumento ("Nombre") especifica la propiedad que se mostrará como el texto de cada opción. El cuarto argumento (0) establece el valor que estará seleccionado por defecto (en este caso, la opción "SELECCIONAR").
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);

            //    - 'ViewData["DistritoId"]': Similar al anterior, se crea un SelectList para los distritos utilizando la lista 'distritos'.
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);

            //    - 'ViewData["DepartamentoId"]': Similar a los anteriores, se crea un SelectList para los departamentos utilizando la lista 'departamentos'.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // 5. Retorno de la vista:
            //    - Se llama al método 'View()', que devuelve la vista asociada a la acción 'Create'. Como no se pasa ningún modelo explícitamente, la vista se renderizará con los datos disponibles en 'ViewData'. La vista contendrá los dropdowns para seleccionar el municipio, distrito y departamento, pre-cargados con las opciones configuradas.
            return View();
        }

        // POST: Zona/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,IdDistrito,IdAlcaldia,Descripcion")] Zona zona)
        {
            // 1. Bloque try-catch para manejar posibles errores durante la creación:
            try
            {
                // 2. Obtener el Id de la Alcaldía (Municipio):
                //    - Se accede al DbSet 'Alcaldias' del contexto.
                //    - Se utiliza 'FirstOrDefault' con una expresión lambda para buscar la primera Alcaldía cuyo 'IdMunicipio' coincida con el 'IdAlcaldia' que viene en el objeto 'zona' recibido del formulario.
                //    - Se asume que el valor que viene en 'zona.IdAlcaldia' desde el formulario es el Id del Municipio seleccionado. Se busca la Alcaldía correspondiente a ese Municipio y se asigna su 'Id' a la propiedad 'zona.IdAlcaldia'. Esto sugiere que en la base de datos, la relación podría ser Zona -> Distrito -> Municipio -> Alcaldía, y el formulario inicialmente permite seleccionar el Municipio.
                zona.IdAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.IdMunicipio == zona.IdAlcaldia).Id;

                // 3. Agregar la nueva entidad 'Zona' al contexto:
                //    - Se llama al método 'Add()' del contexto, pasando el objeto 'zona' recibido del formulario. Esto marca la entidad como 'Agregada' en el rastreador de cambios del contexto.
                _context.Add(zona);

                // 4. Guardar los cambios en la base de datos de forma asíncrona:
                //    - Se llama al método 'SaveChangesAsync()' del contexto. Esto persiste los cambios rastreados (en este caso, la adición de la nueva 'Zona') en la base de datos. 'await' asegura que la ejecución de la acción se pause hasta que la operación de guardado se complete.
                await _context.SaveChangesAsync();

                // 5. Redireccionar a la acción 'Index':
                //    - Si la creación de la zona fue exitosa, se llama al método 'RedirectToAction()' para enviar una respuesta de redirección al navegador del cliente.
                //    - 'nameof(Index)' obtiene el nombre de la acción 'Index' dentro del mismo controlador ('ZonaController'), lo que evita errores de escritura si el nombre de la acción se refactoriza en el futuro. Esto redirige al usuario a la página que muestra la lista de zonas.
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // 6. Manejo de errores:
                //    - Si ocurre alguna excepción dentro del bloque 'try', el control pasa al bloque 'catch'.
                //    - En este caso, en lugar de simplemente mostrar un mensaje de error genérico, el código vuelve a preparar los SelectList para los dropdowns de la misma manera que en la acción 'Create' (GET).

                // 7. Re-inicialización de listas para DropDownList en caso de error:
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                var departamentos = _context.Departamentos.ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

                // 8. Retorno de la vista con el modelo (con errores):
                //    - Se llama al método 'View(zona)', pasando el objeto 'zona' que se intentó crear como modelo a la vista. Esto permite que la vista muestre los valores que el usuario ya había ingresado en el formulario, junto con cualquier mensaje de validación o error que se haya generado. Al re-cargar los SelectList, se asegura que los dropdowns sigan funcionando correctamente en la vista de edición con errores.
                return View(zona);
            }
        }

        // GET: Zona/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // 1. Recepción del parámetro ID:
            //    - La acción recibe un parámetro entero nullable llamado 'id' desde la ruta (por ejemplo, "/Zona/Edit/5"). El '?' indica que puede ser nulo.

            // 2. Verificación de ID nulo:
            //    - Se verifica si el valor del parámetro 'id' es nulo.
            //    - Si es nulo, significa que no se proporcionó un ID válido para editar la zona, por lo que se devuelve un resultado 'NotFound' (código de estado HTTP 404).
            if (id == null)
            {
                return NotFound();
            }

            // 3. Búsqueda de la entidad 'Zona' en la base de datos:
            //    - Se accede al DbSet 'Zonas' del contexto de la base de datos (_context).
            //    - 'FindAsync(id)' es un método asíncrono que busca una entidad con la clave primaria especificada (en este caso, el 'id'). Es una forma eficiente de buscar por clave primaria.
            var zona = await _context.Zonas.FindAsync(id);

            // 4. Verificación si la zona fue encontrada:
            //    - Se verifica si la variable 'zona' es nula.
            //    - Si es nula, significa que no se encontró ninguna zona con el ID proporcionado en la base de datos, por lo que se devuelve un resultado 'NotFound'.
            if (zona == null)
            {
                return NotFound();
            }

            // 5. Inicialización de listas para los DropDownList:
            //    - Se crean listas genéricas de objetos 'Distrito' y 'Municipio', inicializadas con una opción "SELECCIONAR".
            List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
            List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];

            // 6. Obtención de todos los departamentos desde la base de datos:
            //    - Se obtienen todos los departamentos para el DropDownList de departamentos.
            var departamentos = _context.Departamentos.ToList();
            departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });

            // 7. Obtención de la Alcaldía y el Municipio asociado a la Zona a editar:
            //    - Se busca la Alcaldía cuyo 'Id' coincide con el 'IdAlcaldia' de la 'zona' que se va a editar.
            var miAlcaldia = _context.Alcaldias.FirstOrDefault(a => a.Id == zona.IdAlcaldia);
            //    - Se busca el Municipio cuyo 'Id' coincide con el 'IdMunicipio' de la 'miAlcaldia' encontrada.
            var miMunicipio = _context.Municipios.FirstOrDefault(a => a.Id == miAlcaldia.IdMunicipio);
            //    - Se añade el Municipio encontrado a la lista de 'municipios'. Esto se hace para que este municipio aparezca seleccionado en el DropDownList.
            municipios.Add(new Municipio { Nombre = miMunicipio.Nombre, Id = miMunicipio.Id, IdDepartamento = miMunicipio.IdDepartamento });

            // 8. Obtención del Distrito asociado a la Zona a editar:
            //    - Se busca el Distrito cuyo 'Id' coincide con el 'IdDistrito' de la 'zona' que se va a editar.
            var miDistrito = _context.Distritos.FirstOrDefault(a => a.Id == zona.IdDistrito);
            //    - Se añade el Distrito encontrado a la lista de 'distritos' para que aparezca seleccionado en el DropDownList.
            distritos.Add(new Distrito { Nombre = miDistrito.Nombre, Id = miDistrito.Id, IdMunicipio = miDistrito.IdMunicipio });

            // 9. Preparación de SelectList para los DropDownList en la vista de edición:
            //    - 'ViewData["MunicipioId"]': Se crea un SelectList para los municipios, utilizando la lista 'municipios'. El cuarto argumento ('miMunicipio.Id') especifica el valor que estará seleccionado por defecto en el dropdown (el municipio actual de la zona).
            ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", miMunicipio.Id);
            //    - 'ViewData["DistritoId"]': Se crea un SelectList para los distritos, utilizando la lista 'distritos'. El cuarto argumento ('zona.IdDistrito') especifica el valor que estará seleccionado por defecto (el distrito actual de la zona).
            ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", zona.IdDistrito);
            //    - 'ViewData["DepartamentoId"]': Se crea un SelectList para los departamentos. En este caso, se establece 0 como valor seleccionado por defecto. Podría necesitar lógica adicional si se quisiera preseleccionar un departamento basado en la zona.
            ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

            // 10. Retorno de la vista con el modelo:
            //     - Se llama al método 'View(zona)', pasando el objeto 'zona' encontrado como modelo a la vista 'Edit'. La vista utilizará este modelo para mostrar los datos actuales de la zona en el formulario de edición.
            return View(zona);
        }

        // POST: Zona/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,IdDistrito,IdAlcaldia,Descripcion")] Zona zona)
        {
            // 1. Verificación de la coincidencia del ID:
            //    - Se compara el 'id' recibido en la ruta con el 'Id' de la 'zona' que se recibe en el cuerpo de la solicitud (a través del formulario).
            //    - Si no coinciden, significa que el usuario podría estar intentando modificar una entidad diferente a la que se solicitó editar, por lo que se devuelve un 'NotFound'.
            if (id != zona.Id)
            {
                return NotFound();
            }

            // 2. Bloque try-catch para manejar posibles errores durante la edición:
            try
            {
                // 3. Obtener el Id de la Alcaldía (Municipio):
                //    - Similar a la acción 'Create' (POST), se busca la Alcaldía cuyo 'IdMunicipio' coincide con el 'IdAlcaldia' que viene en el objeto 'zona' desde el formulario y se actualiza 'zona.IdAlcaldia' con el Id de la Alcaldía encontrada.
                zona.IdAlcaldia = _context.Alcaldias.FirstOrDefault(s => s.IdMunicipio == zona.IdAlcaldia).Id;

                // 4. Marcar la entidad 'Zona' como modificada en el contexto:
                //    - Se llama al método 'Update()' del contexto, pasando el objeto 'zona' modificado. Esto marca la entidad como 'Modificada' en el rastreador de cambios del contexto.
                _context.Update(zona);

                // 5. Guardar los cambios en la base de datos de forma asíncrona:
                //    - Se llama a 'SaveChangesAsync()' para persistir los cambios realizados en la base de datos.
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 6. Manejo de errores:
                //    - Si ocurre alguna excepción durante la actualización, el control pasa al bloque 'catch'.
                //    - Al igual que en el 'Create' (POST), se vuelven a preparar los SelectList para los dropdowns. Sin embargo, en este caso, los valores seleccionados por defecto se restablecen a 0, lo que podría no ser ideal para una experiencia de edición (sería mejor intentar mantener los valores previamente seleccionados).

                // 7. Re-inicialización de listas para DropDownList en caso de error:
                List<Distrito> distritos = [new Distrito { Nombre = "SELECCIONAR", Id = 0, IdMunicipio = 0 }];
                List<Municipio> municipios = [new Municipio { Nombre = "SELECCIONAR", Id = 0, IdDepartamento = 0 }];
                var departamentos = _context.Departamentos.ToList();
                departamentos.Add(new Departamento { Nombre = "SELECCIONAR", Id = 0 });
                ViewData["MunicipioId"] = new SelectList(municipios, "Id", "Nombre", 0);
                ViewData["DistritoId"] = new SelectList(distritos, "Id", "Nombre", 0);
                ViewData["DepartamentoId"] = new SelectList(departamentos, "Id", "Nombre", 0);

                // 8. Retorno de la vista con el modelo (con errores):
                //    - Se devuelve la vista 'Edit' con el objeto 'zona' que se intentó modificar. Esto permite mostrar los errores de validación y los datos que el usuario ya había ingresado.
                return View(zona);
            }

            // 9. Retorno a la vista index (en caso de éxito):
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Zona/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // 1. Recepción del parámetro ID:
            //    - La acción recibe un parámetro entero nullable llamado 'id' desde la ruta (por ejemplo, "/Zona/Delete/5"). El '?' indica que puede ser nulo.

            // 2. Verificación de ID nulo:
            //    - Se verifica si el valor del parámetro 'id' es nulo.
            //    - Si es nulo, significa que no se proporcionó un ID válido para eliminar la zona, por lo que se devuelve un resultado 'NotFound' (código de estado HTTP 404).
            if (id == null)
            {
                return NotFound();
            }

            // 3. Búsqueda de la entidad 'Zona' en la base de datos con carga anticipada:
            //    - Se accede al DbSet 'Zonas' del contexto de la base de datos (_context).
            //    - 'Include(z => z.IdAlcaldiaNavigation)' carga de forma anticipada la entidad relacionada 'IdAlcaldiaNavigation' (presumiblemente la información de la alcaldía asociada a la zona).
            //    - 'Include(z => z.IdDistritoNavigation)' carga de forma anticipada la entidad relacionada 'IdDistritoNavigation' (presumiblemente la información del distrito asociado a la zona).
            //    - 'FirstOrDefaultAsync(m => m.Id == id)' ejecuta una consulta asíncrona para buscar la primera zona cuyo 'Id' coincida con el valor del parámetro 'id'. Si no se encuentra ninguna zona con ese ID, 'zona' será nulo.
            var zona = await _context.Zonas
                .Include(z => z.IdAlcaldiaNavigation)
                .Include(z => z.IdDistritoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 4. Obtención de todas las alcaldías para ViewData (aunque no parece ser utilizado directamente en esta acción):
            //    - Se obtienen todos los registros de la tabla 'Municipios' y se almacenan en la variable 'alcaldias'.
            var alcaldias = _context.Municipios.ToList();
            //    - Se crea un 'SelectList' a partir de la lista de alcaldías y se almacena en 'ViewData' con la clave "AlcaldiaId". Esto podría ser utilizado en la vista de confirmación de eliminación para mostrar información relacionada, aunque no es evidente en este fragmento.
            ViewData["AlcaldiaId"] = new SelectList(alcaldias, "Id", "Nombre");

            // 5. Verificación si la zona fue encontrada:
            //    - Se verifica si la variable 'zona' es nula.
            //    - Si es nula, significa que no se encontró ninguna zona con el ID proporcionado en la base de datos, por lo que se devuelve un resultado 'NotFound'.
            if (zona == null)
            {
                return NotFound();
            }

            // 6. Retorno de la vista con el modelo:
            //    - Se llama al método 'View(zona)', pasando el objeto 'zona' encontrado como modelo a la vista 'Delete'. Esta vista probablemente mostrará los detalles de la zona que se va a eliminar y pedirá confirmación al usuario.
            return View(zona);
        }

        // POST: Zona/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Búsqueda de la entidad 'Zona' en la base de datos por su ID:
            //    - Se accede al DbSet 'Zonas' del contexto.
            //    - 'FindAsync(id)' busca de forma asíncrona la entidad 'Zona' con la clave primaria especificada.
            var zona = await _context.Zonas.FindAsync(id);

            // 2. Verificación si la zona fue encontrada:
            //    - Se verifica si la variable 'zona' no es nula (es decir, se encontró la zona a eliminar).
            if (zona != null)
            {
                // 3. Marcar la entidad 'Zona' para su eliminación:
                //    - Se llama al método 'Remove()' del DbSet 'Zonas', pasando el objeto 'zona' encontrado. Esto marca la entidad como 'Eliminada' en el rastreador de cambios del contexto.
                _context.Zonas.Remove(zona);
            }
            // 4. Guardar los cambios en la base de datos de forma asíncrona:
            //    - Se llama al método 'SaveChangesAsync()' del contexto. Esto persiste los cambios rastreados (en este caso, la eliminación de la 'Zona', si se encontró) en la base de datos.
            await _context.SaveChangesAsync();

            // 5. Redireccionar a la acción 'Index':
            //    - Después de eliminar la zona (o intentar eliminarla sin encontrarla), se llama a 'RedirectToAction(nameof(Index))' para redirigir al usuario a la página que muestra la lista de zonas.
            return RedirectToAction(nameof(Index));
        }

        private bool ZonaExists(int id)
        {
            // 1. Consulta a la base de datos para verificar la existencia de una Zona con el ID especificado:
            //    - Se accede al DbSet 'Zonas' del contexto.
            //    - 'Any(e => e.Id == id)' es un método LINQ que devuelve 'true' si al menos un elemento en la colección 'Zonas' cumple la condición especificada (en este caso, que su propiedad 'Id' sea igual al 'id' pasado como argumento), y 'false' en caso contrario.

            // 2. Retorno del resultado de la verificación:
            //    - La función devuelve un valor booleano que indica si existe alguna entidad 'Zona' en la base de datos con el 'id' proporcionado. Esta función se utiliza típicamente para realizar validaciones antes de realizar operaciones como editar o eliminar.
            return _context.Zonas.Any(e => e.Id == id);
        }


        [HttpGet]
        public IActionResult GetZonas()
        {
            // 1. [HttpGet]:
            //    - Este atributo indica que esta acción (método) 'GetZonas' responderá a las solicitudes HTTP GET. Cuando una solicitud GET se dirige a la ruta configurada para esta acción, este método será ejecutado.

            // 2. public IActionResult GetZonas():
            //    - 'public': Este modificador de acceso indica que este método puede ser accedido desde cualquier otra parte del código.
            //    - 'IActionResult': Este es el tipo de retorno de la acción. 'IActionResult' es una interfaz que representa el resultado de una acción del controlador. Permite devolver diferentes tipos de respuestas HTTP, como JSON, vistas, redirecciones, etc.
            //    - 'GetZonas()': Este es el nombre del método de la acción. Por convención, las acciones que recuperan datos suelen tener nombres descriptivos que indican su propósito.

            // 3. var zonas = _context.Zonas
            //    - 'var zonas': Se declara una variable local llamada 'zonas'. El tipo de esta variable será inferido por el compilador basándose en el resultado de la expresión a la derecha del signo igual.
            //    - '_context.Zonas': Se accede a la propiedad 'Zonas' del objeto '_context', que es una instancia de 'SgarDbContext' (inyectada a través del constructor del controlador). Se asume que 'Zonas' es un DbSet<Zona>, representando la tabla 'Zonas' en la base de datos.
            var zonas = _context.Zonas

                // 4. .Select(z => new { id = z.Id, nombre = z.Nombre })
                //    - '.Select(...)': Este es un método de extensión LINQ (Language Integrated Query) que proyecta cada elemento de la colección 'Zonas' en un nuevo formulario.
                //    - 'z => new { id = z.Id, nombre = z.Nombre }': Esta es una expresión lambda que define la transformación para cada objeto 'Zona' (representado por 'z'). Para cada 'Zona', se crea un nuevo objeto anónimo (un objeto sin un nombre de clase explícito) con dos propiedades:
                //        - 'id': Se asigna el valor de la propiedad 'Id' del objeto 'Zona'.
                //        - 'nombre': Se asigna el valor de la propiedad 'Nombre' del objeto 'Zona'.
                //    - En resumen, esta parte del código selecciona solo las propiedades 'Id' y 'Nombre' de cada objeto 'Zona' y crea una nueva secuencia de objetos anónimos conteniendo solo estas dos propiedades.
                .Select(z => new { id = z.Id, nombre = z.Nombre })

                // 5. .ToList();
                //    - '.ToList()': Este es otro método de extensión LINQ que convierte la secuencia de objetos anónimos resultante del '.Select()' en una lista en memoria. La variable 'zonas' ahora contendrá una lista de objetos anónimos, donde cada objeto tiene las propiedades 'id' y 'nombre' de una 'Zona' de la base de datos.
                .ToList();

            // 6. return Json(zonas);
            //    - 'return Json(zonas);': Este método de la clase 'Controller' (de la cual 'ZonaController' hereda) toma un objeto (en este caso, la lista 'zonas') y lo serializa a formato JSON (JavaScript Object Notation).
            //    - El resultado de esta acción es una respuesta HTTP con el tipo de contenido configurado como 'application/json' y el cuerpo de la respuesta conteniendo la representación JSON de la lista de zonas (cada zona con su 'id' y 'nombre'). Esto es comúnmente utilizado para enviar datos desde el servidor a una aplicación cliente (como una página web con JavaScript) de manera estructurada y fácil de consumir.
            return Json(zonas);

            // En resumen, esta acción 'GetZonas' consulta la base de datos, obtiene todas las zonas, selecciona solo sus IDs y nombres, convierte estos datos en una lista y luego devuelve esa lista como una respuesta JSON a la solicitud HTTP GET.
        }

    }
}
