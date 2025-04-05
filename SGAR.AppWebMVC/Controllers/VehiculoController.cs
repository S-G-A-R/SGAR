using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGAR.AppWebMVC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace SGAR.AppWebMVC.Controllers
{
    // 1. public class VehiculoController : Controller: Declara una clase pública llamada VehiculoController
    //    que hereda de la clase base Controller. Los controladores son responsables de manejar las
    //    solicitudes entrantes y devolver respuestas al cliente.
    public class VehiculoController : Controller
    {
        // 2. private readonly SgarDbContext _context;: Declara una variable privada de solo lectura
        //    llamada _context que contendrá una instancia de la clase SgarDbContext. Se asume que
        //    SgarDbContext es el contexto de Entity Framework Core utilizado para interactuar con la base de datos.
        private readonly SgarDbContext _context;

        // 3. public VehiculoController(SgarDbContext context): Este es el constructor del controlador.
        //    Recibe una instancia de SgarDbContext a través de la inyección de dependencias.
        //    La instancia recibida se asigna a la variable privada _context, permitiendo que el
        //    controlador acceda a la base de datos.
        public VehiculoController(SgarDbContext context)
        {
            _context = context;
        }

        // 4. public async Task<IActionResult> Index(Vehiculo vehiculo, int? pag, int topRegistro = 10):
        //    Declara un método de acción asíncrono llamado Index que devuelve un IActionResult.
        //    Este método se utiliza para mostrar una lista paginada de vehículos.
        //    Recibe opcionalmente un objeto Vehiculo (para filtros), un número de página 'pag' (nullable int),
        //    y la cantidad de registros por página 'topRegistro' (con un valor predeterminado de 10).
        public async Task<IActionResult> Index(Vehiculo vehiculo, int? pag, int topRegistro = 10)
        {
            // 5. var vehiculos =  _context.Vehiculos.Include(s=>s.IdOperadorNavigation).Include(s=>s.IdMarcaNavigation);:
            //    Obtiene un DbSet de la tabla Vehiculos desde el contexto de la base de datos (_context).
            //    Utiliza el método Include para realizar una carga "eager" de las propiedades de navegación
            //    IdOperadorNavigation e IdMarcaNavigation. Esto significa que cuando se carguen los vehículos,
            //    también se cargarán los datos de sus respectivos operadores y marcas en la misma consulta a la base de datos.
            var vehiculos = _context.Vehiculos.Include(s => s.IdOperadorNavigation).Include(s => s.IdMarcaNavigation);

            // 6. var query = vehiculos.AsQueryable();: Convierte el DbSet 'vehiculos' a un IQueryable.
            //    IQueryable representa una consulta que se puede construir paso a paso y se ejecutará
            //    en la base de datos solo cuando sea necesario (por ejemplo, al llamar a ToListAsync o al
            //    utilizarla en PaginatedList.CreateAsync).
            var query = vehiculos.AsQueryable();

            // 7. if (!string.IsNullOrWhiteSpace(vehiculo.Placa)): Comprueba si la propiedad 'Placa' del
            //    objeto 'vehiculo' (que podría contener criterios de filtro) no es nula, vacía o solo contiene espacios en blanco.
            if (!string.IsNullOrWhiteSpace(vehiculo.Placa))
                // 8. query = query.Where(s => s.Placa.Contains(vehiculo.Placa));: Si la 'Placa' no está vacía,
                //    agrega una cláusula WHERE a la consulta para filtrar los vehículos cuya propiedad 'Placa'
                //    contenga el valor de 'vehiculo.Placa'.
                query = query.Where(s => s.Placa.Contains(vehiculo.Placa));

            // 9. query = query.OrderByDescending(s => s.Id);: Agrega una cláusula ORDER BY a la consulta
            //    para ordenar los vehículos de forma descendente según su propiedad 'Id'. Esto mostrará
            //    los vehículos más recientes primero.
            query = query.OrderByDescending(s => s.Id);

            // 10. return View(await PaginatedList<Vehiculo>.CreateAsync(query, pag ?? 1, topRegistro));:
            //     Utiliza la clase estática PaginatedList<Vehiculo> (se asume que existe en el proyecto)
            //     para crear una lista paginada de vehículos de forma asíncrona.
            //     - 'query': La consulta IQueryable de vehículos que se va a paginar.
            //     - 'pag ?? 1': El número de página actual. Utiliza el operador de coalescencia nula (??)
            //       para establecer el número de página en 1 si el parámetro 'pag' es nulo.
            //     - 'topRegistro': La cantidad de registros que se mostrarán en cada página.
            //     El resultado (una instancia de PaginatedList<Vehiculo>) se pasa a la vista asociada a la
            //     acción Index para ser mostrada al usuario.
            return View(await PaginatedList<Vehiculo>.CreateAsync(query, pag ?? 1, topRegistro));
        }

        // 11. public async Task<IActionResult> Details(int? id): Declara un método de acción asíncrono
        //     llamado Details que devuelve un IActionResult. Este método se utiliza para mostrar los
        //     detalles de un vehículo específico. Recibe un parámetro 'id' de tipo entero nullable (int?),
        //     que representa el identificador del vehículo a mostrar.
        public async Task<IActionResult> Details(int? id)
        {
            // 12. if (id == null): Comprueba si el parámetro 'id' es nulo. Esto podría ocurrir si no se
            //     proporciona un ID en la ruta.
            if (id == null)
            {
                // 13. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound (código de
                //     estado HTTP 404), indicando que el recurso solicitado no se encontró.
                return NotFound();
            }
            // 14. var vehiculo = await _context.Vehiculos
            //     Busca de forma asíncrona en la tabla 'Vehiculos' del contexto de la base de datos.
            var vehiculo = await _context.Vehiculos
                // 15. .Include(v => v.IdMarcaNavigation): Realiza una carga "eager" de la propiedad de
                //     navegación IdMarcaNavigation, cargando los datos de la marca del vehículo.
                .Include(v => v.IdMarcaNavigation)
                // 16. .Include(v => v.IdOperadorNavigation): Realiza una carga "eager" de la propiedad de
                //     navegación IdOperadorNavigation, cargando los datos del operador del vehículo.
                .Include(v => v.IdOperadorNavigation)
                // 17. .Include(v => v.IdTipoVehiculoNavigation): Realiza una carga "eager" de la propiedad
                //     de navegación IdTipoVehiculoNavigation, cargando los datos del tipo de vehículo.
                .Include(v => v.IdTipoVehiculoNavigation)
                // 18. .FirstOrDefaultAsync(m => m.Id == id);: Busca el primer vehículo que cumpla con la
                //     condición de que su propiedad 'Id' sea igual al 'id' recibido. FirstOrDefaultAsync
                //     devuelve el primer elemento encontrado o null si no se encuentra ninguno.
                .FirstOrDefaultAsync(m => m.Id == id);

            // 19. if (vehiculo == null): Comprueba si se encontró un vehículo con el 'id' proporcionado
            //     en la base de datos.
            if (vehiculo == null)
            {
                // 20. return NotFound(): Si no se encuentra ningún vehículo con ese 'id', devuelve un
                //     resultado NotFound.
                return NotFound();
            }
            // 21. return View(vehiculo);: Si se encontró el vehículo, devuelve la vista asociada a esta
            //     acción ('Details.cshtml' por convención), pasando el objeto 'vehiculo' como modelo a la vista.
            //     La vista mostrará los detalles del vehículo, incluyendo la información de su marca, operador
            //     y tipo de vehículo (gracias a las cargas "eager").
            return View(vehiculo);
        }

        // 1. public IActionResult Create(): Declara un método de acción público llamado Create que
        //    devuelve un IActionResult. Este método se utiliza para mostrar el formulario de creación
        //    de un nuevo vehículo.
        public IActionResult Create()
        {
            // 2. ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo");:
            //    Crea un objeto SelectList a partir de la colección de Marcas obtenida del contexto de
            //    la base de datos (_context.Marcas). Este SelectList se utilizará para poblar un control
            //    desplegable (dropdown) en la vista para seleccionar la marca del vehículo.
            //    - "Id": Especifica la propiedad de la entidad Marca que se utilizará como el valor del
            //      elemento en el dropdown.
            //    - "Modelo": Especifica la propiedad de la entidad Marca que se mostrará como el texto del
            //      elemento en el dropdown.
            //    El SelectList se almacena en el ViewData con la clave "IdMarca" para que pueda ser accedido
            //    en la vista.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo");
            // 3. ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre");:
            //    Similar al paso anterior, crea un SelectList a partir de la colección de Operadores para
            //    poblar un dropdown para seleccionar el operador del vehículo.
            //    - "Id": Valor del elemento del dropdown.
            //    - "Nombre": Texto visible del elemento del dropdown.
            //    Se almacena en ViewData con la clave "IdOperador".
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre");
            // 4. ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion");:
            //    Crea un SelectList a partir de la colección de TiposVehiculos para poblar un dropdown
            //    para seleccionar el tipo de vehículo.
            //    - "Id": Valor del elemento del dropdown.
            //    - "Descripcion": Texto visible del elemento del dropdown.
            //    Se almacena en ViewData con la clave "IdTipoVehiculo".
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion");
            // 5. return View(new Vehiculo());: Devuelve la vista asociada a la acción "Create"
            //    ("Create.cshtml" por convención), pasando una nueva instancia vacía del modelo Vehiculo
            //    para que el formulario pueda enlazar sus campos a las propiedades de este modelo.
            return View(new Vehiculo());
        }

        // 6. public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null):
        //    Declara un método asíncrono que devuelve un array de bytes nullable (byte[]?). Este método
        //    se encarga de convertir un archivo de imagen (representado por IFormFile) a un array de bytes.
        //    Recibe un parámetro 'file' de tipo IFormFile nullable (que representa el archivo subido) y
        //    un parámetro opcional 'bytesImage' de tipo byte array nullable (que podría contener una imagen
        //    existente).
        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            // 7. byte[]? bytes = bytesImage;: Inicializa una variable 'bytes' con el valor de 'bytesImage'.
            //    Esto permite mantener una imagen existente si no se proporciona un nuevo archivo.
            byte[]? bytes = bytesImage;
            // 8. if (file != null && file.Length > 0): Comprueba si se proporcionó un archivo ('file' no
            //    es nulo) y si el archivo tiene un tamaño mayor que cero.
            if (file != null && file.Length > 0)
            {
                // 9. using (var memoryStream = new MemoryStream()): Crea una instancia de MemoryStream dentro
                //    de un bloque 'using'. MemoryStream proporciona una secuencia en memoria para trabajar con
                //    datos sin necesidad de acceder directamente al disco. El bloque 'using' asegura que el
                //    MemoryStream se libere correctamente una vez que se complete el bloque.
                using (var memoryStream = new MemoryStream())
                {
                    // 10. await file.CopyToAsync(memoryStream);: Copia de forma asíncrona el contenido del
                    //     archivo subido ('file') al MemoryStream.
                    await file.CopyToAsync(memoryStream);
                    // 11. bytes = memoryStream.ToArray();: Convierte el contenido del MemoryStream (que ahora
                    //     contiene los datos del archivo) a un array de bytes y lo asigna a la variable 'bytes'.
                    //     Esto sobrescribe cualquier valor previo que 'bytes' pudiera haber tenido.
                    bytes = memoryStream.ToArray();
                }
            }
            // 12. return bytes;: Devuelve el array de bytes que representa la imagen (ya sea el nuevo archivo
            //     convertido o la imagen existente si no se subió un nuevo archivo). Si no se proporcionó
            //     ningún archivo nuevo y 'bytesImage' también era nulo, entonces 'bytes' será nulo.
            return bytes;
        }

        // 13. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //     Esto significa que se espera que un formulario HTML envíe datos a esta URL utilizando el método POST.
        [HttpPost]
        // 14. [ValidateAntiForgeryToken]: Este atributo habilita la protección contra la falsificación de
        //     solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // 15. public async Task<IActionResult> Create(Vehiculo vehiculo): Declara un método de acción
        //     asíncrono que devuelve un IActionResult. Este método se encarga de procesar el formulario
        //     de creación de un nuevo vehículo. Recibe un objeto Vehiculo que se espera que contenga los
        //     datos del formulario.
        public async Task<IActionResult> Create(Vehiculo vehiculo)
        {
            // 16. try: Inicia un bloque try para envolver el código que podría generar excepciones durante
            //     el proceso de creación del vehículo.
            try
            {
                // 17. if (vehiculo.fotofile != null): Comprueba si la propiedad 'fotofile' del objeto
                //     'vehiculo' (que se espera que esté enlazada al control de carga de archivos en el
                //     formulario) no es nula.
                if (vehiculo.fotofile != null)
                {
                    // 18. vehiculo.Foto = await GenerarByteImage(vehiculo.fotofile);: Llama al método
                    //     asíncrono GenerarByteImage para convertir el archivo subido (vehiculo.fotofile)
                    //     a un array de bytes y asigna el resultado a la propiedad 'Foto' del objeto 'vehiculo'.
                    vehiculo.Foto = await GenerarByteImage(vehiculo.fotofile);
                }

                // 19. _context.Add(vehiculo);: Agrega la entidad 'vehiculo' al contexto de la base de datos,
                //     marcándola para ser insertada en la tabla Vehiculos al guardar los cambios.
                _context.Add(vehiculo);
                // 20. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios
                //     realizados en el contexto (en este caso, la adición del nuevo vehículo) en la base de datos.
                await _context.SaveChangesAsync();
                // 21. return RedirectToAction(nameof(Index));: Si el proceso de creación del vehículo se
                //     completa con éxito, redirige al usuario a la acción "Index" de este controlador, que
                //     probablemente muestra la lista de vehículos.
                return RedirectToAction(nameof(Index));
            }
            // 22. catch: Captura cualquier excepción que pueda ocurrir dentro del bloque try.
            catch
            {
                // 23. ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);:
                //     Si ocurre una excepción (por ejemplo, un error de validación o de base de datos),
                //     re-crea el SelectList para las Marcas y lo almacena en ViewData. El tercer argumento
                //     ('vehiculo.IdMarca') se utiliza para preseleccionar la marca que el usuario había
                //     seleccionado en el formulario antes de que ocurriera el error.
                ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
                // 24. ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);:
                //     Re-crea el SelectList para los Operadores, preseleccionando el operador previamente seleccionado.
                ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);
                // 25. ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);:
                //     Re-crea el SelectList para los Tipos de Vehículos, preseleccionando el tipo previamente seleccionado.
                ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
                // 26. return View(vehiculo);: Devuelve la vista asociada a la acción "Create" ("Create.cshtml"),
                //     pasando el objeto 'vehiculo' original a la vista. Esto permite mostrar los datos que el
                //     usuario intentó ingresar y cualquier mensaje de error de validación que se haya generado.
                //     Los SelectLists se re-pueblan para que el usuario pueda corregir los errores y volver a intentar enviar el formulario.
                return View(vehiculo);
            }
        }

        // 1. public async Task<IActionResult> Edit(int? id): Declara un método de acción asíncrono
        //    llamado Edit que devuelve un IActionResult. Este método se utiliza para mostrar el formulario
        //    de edición de un vehículo específico. Recibe un parámetro 'id' de tipo entero nullable (int?),
        //    que representa el identificador del vehículo a editar.
        public async Task<IActionResult> Edit(int? id)
        {
            // 2. if (id == null): Comprueba si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 3. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound (código de
                //    estado HTTP 404), indicando que el recurso solicitado no se encontró.
                return NotFound();
            }

            // 4. var vehiculo = await _context.Vehiculos.FindAsync(id);: Busca de forma asíncrona la
            //    entidad Vehiculo en la base de datos utilizando su clave primaria ('Id'). FindAsync es
            //    una forma eficiente de buscar por clave primaria.
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            // 5. if (vehiculo == null): Comprueba si se encontró un vehículo con el 'id' proporcionado.
            if (vehiculo == null)
            {
                // 6. return NotFound(): Si no se encuentra ningún vehículo con ese 'id', devuelve un
                //    resultado NotFound.
                return NotFound();
            }

            // 7. ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);:
            //    Crea un objeto SelectList a partir de la colección de Marcas obtenida del contexto de
            //    la base de datos (_context.Marcas). El cuarto argumento ('vehiculo.IdMarca') se utiliza
            //    para preseleccionar la marca actual del vehículo en el dropdown.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            // 8. ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);:
            //    Crea un SelectList a partir de la colección de Operadores, preseleccionando el operador
            //    actual del vehículo.
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);
            // 9. ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);:
            //    Crea un SelectList a partir de la colección de TiposVehiculos, preseleccionando el tipo
            //    de vehículo actual.
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            // 10. return View(vehiculo);: Devuelve la vista asociada a la acción "Edit" ("Edit.cshtml"),
            //     pasando el objeto 'vehiculo' como modelo a la vista para que se muestren los datos actuales
            //     en el formulario de edición.
            return View(vehiculo);
        }

        // 11. [HttpPost]: Este atributo indica que este método de acción responde a solicitudes HTTP POST.
        //     Esto significa que se espera que un formulario HTML envíe datos a esta URL utilizando el método POST.
        [HttpPost]
        // 12. [ValidateAntiForgeryToken]: Este atributo habilita la protección contra la falsificación de
        //     solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // 13. public async Task<IActionResult> Edit(int id, Vehiculo vehiculo): Declara un método de acción
        //     asíncrono que devuelve un IActionResult. Este método se encarga de procesar el envío del
        //     formulario de edición de un vehículo. Recibe el 'id' del vehículo a editar y el objeto Vehiculo
        //     que contiene los datos modificados del formulario.
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            // 14. if (id != vehiculo.Id): Comprueba si el 'id' recibido en la ruta o consulta no coincide
            //     con el 'Id' del objeto 'vehiculo' que se intentó enlazar desde el formulario. Esto es una
            //     medida de seguridad para evitar la manipulación del ID.
            if (id != vehiculo.Id)
            {
                // 15. return NotFound(): Si los IDs no coinciden, devuelve un resultado NotFound.
                return NotFound();
            }

            // 16. var vehiculoUpdate = await _context.Vehiculos
            //     Busca de forma asíncrona la entidad Vehiculo existente en la base de datos cuyo 'Id'
            //     coincida con el 'Id' del objeto 'vehiculo' recibido.
            var vehiculoUpdate = await _context.Vehiculos
                // 17. .FirstOrDefaultAsync(o => o.Id == vehiculo.Id);: Utiliza FirstOrDefaultAsync para obtener
                //     el primer vehículo que cumpla la condición (su ID coincida).
                .FirstOrDefaultAsync(o => o.Id == vehiculo.Id);

            // 18. if (vehiculoUpdate == null): Comprueba si se encontró un vehículo con el 'id' proporcionado
            //     en la base de datos. Si no se encuentra, significa que el vehículo que se intenta editar
            //     ya no existe.
            if (vehiculoUpdate == null)
            {
                // 19. return NotFound(): Si no se encuentra el vehículo a actualizar, devuelve NotFound.
                return NotFound();
            }

            // 20. try: Inicia un bloque try para envolver el código que podría generar excepciones durante
            //     el proceso de actualización del vehículo.
            try
            {
                // 21. vehiculoUpdate.IdMarca = vehiculo.IdMarca;: Actualiza la propiedad IdMarca de la
                //     entidad existente con el valor del objeto 'vehiculo' recibido.
                vehiculoUpdate.IdMarca = vehiculo.IdMarca;
                // 22. vehiculoUpdate.Mecanico = vehiculo.Mecanico;: Actualiza la propiedad Mecanico.
                vehiculoUpdate.Mecanico = vehiculo.Mecanico;
                // 23. vehiculoUpdate.Taller = vehiculo.Taller;: Actualiza la propiedad Taller.
                vehiculoUpdate.Taller = vehiculo.Taller;
                // 24. vehiculoUpdate.Estado = vehiculo.Estado;: Actualiza la propiedad Estado.
                vehiculoUpdate.Estado = vehiculo.Estado;
                // 25. vehiculoUpdate.Codigo = vehiculo.Codigo;: Actualiza la propiedad Codigo.
                vehiculoUpdate.Codigo = vehiculo.Codigo;
                // 26. vehiculoUpdate.IdOperador = vehiculo.IdOperador;: Actualiza la propiedad IdOperador.
                vehiculoUpdate.IdOperador = vehiculo.IdOperador;
                // 27. vehiculoUpdate.IdTipoVehiculo = vehiculo.IdTipoVehiculo;: Actualiza la propiedad IdTipoVehiculo.
                vehiculoUpdate.IdTipoVehiculo = vehiculo.IdTipoVehiculo;
                // 28. vehiculoUpdate.Placa = vehiculo.Placa;: Actualiza la propiedad Placa.
                vehiculoUpdate.Placa = vehiculo.Placa;
                // 29. vehiculoUpdate.Descripcion = vehiculo.Descripcion;: Actualiza la propiedad Descripcion.
                vehiculoUpdate.Descripcion = vehiculo.Descripcion;

                // 30. var fotoAnterior = await _context.Vehiculos
                //     Obtiene la foto anterior del vehículo desde la base de datos.
                var fotoAnterior = await _context.Vehiculos
                    // 31. .Where(s => s.Id == vehiculo.Id): Filtra los vehículos por el ID actual.
                    .Where(s => s.Id == vehiculo.Id)
                    // 32. .Select(s => s.Foto).FirstOrDefaultAsync();: Selecciona solo la propiedad Foto y
                    //     obtiene el primer resultado (o null si no hay ninguno).
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                // 33. vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, fotoAnterior);: Llama
                //     al método GenerarByteImage para convertir el archivo subido (si hay uno) a bytes. Si
                //     no se subió un nuevo archivo, se conservará la 'fotoAnterior'.
                vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, fotoAnterior);

                // 34. if (vehiculo.fotofile != null): Comprueba nuevamente si se subió un nuevo archivo.
                if (vehiculo.fotofile != null)
                {
                    // 35. vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, vehiculoUpdate.Foto);:
                    //     Vuelve a llamar a GenerarByteImage. Aunque parece redundante con la línea 33, podría
                    //     haber una intención de asegurar que si 'fotofile' no es nulo, se procese la imagen.
                    //     En la práctica, la línea 33 ya debería haber manejado este caso.
                    vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, vehiculoUpdate.Foto);
                }

                // 36. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios
                //     realizados en el contexto en la base de datos.
                await _context.SaveChangesAsync();
                // 37. return RedirectToAction(nameof(Index));: Si la actualización fue exitosa, redirige
                //     al usuario a la acción "Index" de este controlador, que probablemente muestra la lista de vehículos.
                return RedirectToAction(nameof(Index));
            }
            // 38. catch (DbUpdateConcurrencyException): Captura la excepción DbUpdateConcurrencyException,
            //     que ocurre cuando se detecta un conflicto de concurrencia al intentar guardar los cambios
            //     en la base de datos (otro usuario ha modificado la misma entidad).
            catch (DbUpdateConcurrencyException)
            {
                // 39. if (!VehiculoExists(vehiculo.Id)): Llama a un método (que debes implementar) para
                //     verificar si el Vehiculo con el 'id' proporcionado todavía existe en la base de datos.
                if (!VehiculoExists(vehiculo.Id))
                {
                    // 40. return NotFound(): Si el Vehiculo ya no existe, devuelve NotFound.
                    return NotFound();
                }
                // 41. else: Si el Vehiculo todavía existe, significa que hubo un conflicto de concurrencia.
                else
                {
                    // 42. ModelState.AddModelError("", "Ocurrió un error de concurrencia. Por favor, intente de nuevo.");:
                    //     Agrega un error al ModelState con un mensaje indicando que ocurrió un error de
                    //     concurrencia. Este mensaje se mostrará al usuario en la vista.
                    ModelState.AddModelError("", "Ocurrió un error de concurrencia. Por favor, intente de nuevo.");
                }
            }
            // 43. catch (Exception ex): Captura cualquier otra excepción que pueda ocurrir durante el proceso
            //     de actualización. Es una captura genérica para errores inesperados.
            catch (Exception ex)
            {
                // 44. ModelState.AddModelError("", "Ocurrió un error inesperado, por favor intente de nuevo.");:
                //     Agrega un error genérico al ModelState para errores inesperados.
                ModelState.AddModelError("", "Ocurrió un error inesperado, por favor intente de nuevo.");
            }

            // 45. ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);:
            //     Re-crea el SelectList para las Marcas, preseleccionando la marca del vehículo. Esto se hace
            //     en caso de que haya ocurrido un error y se necesite volver a mostrar el formulario de edición.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            // 46. ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);:
            //     Re-crea el SelectList para los Operadores, preseleccionando el operador del vehículo.
            //     **Ojo:** Aquí se está usando "Id" como el texto a mostrar en el dropdown, lo cual probablemente
            //     no es lo deseado (debería ser "Nombre" como en el método Create).
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            // 47. ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);:
            //     Re-crea el SelectList para los Tipos de Vehículos, preseleccionando el tipo del vehículo.
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            // 48. return View(vehiculo);: Devuelve la vista asociada a la acción "Edit" ("Edit.cshtml"),
            //     pasando el objeto 'vehiculo' a la vista para que se muestren los datos y los mensajes de error
            //     al usuario.
            return View(vehiculo);
        }

        // 1. public async Task<IActionResult> Delete(int? id): Declara un método de acción asíncrono
        //    llamado Delete que devuelve un IActionResult. Este método se utiliza para mostrar la vista
        //    de confirmación de eliminación de un vehículo específico. Recibe un parámetro 'id' de tipo
        //    entero nullable (int?), que representa el identificador del vehículo a eliminar.
        public async Task<IActionResult> Delete(int? id)
        {
            // 2. if (id == null): Comprueba si el parámetro 'id' es nulo.
            if (id == null)
            {
                // 3. return NotFound(): Si el 'id' es nulo, devuelve un resultado NotFound (código de
                //    estado HTTP 404), indicando que el recurso solicitado no se encontró.
                return NotFound();
            }

            // 4. var vehiculo = await _context.Vehiculos
            //    Busca de forma asíncrona en la tabla 'Vehiculos' del contexto de la base de datos.
            var vehiculo = await _context.Vehiculos
                // 5. .Include(v => v.IdMarcaNavigation): Realiza una carga "eager" de la propiedad de
                //    navegación IdMarcaNavigation, cargando los datos de la marca del vehículo.
                .Include(v => v.IdMarcaNavigation)
                // 6. .Include(v => v.IdOperadorNavigation): Realiza una carga "eager" de la propiedad de
                //    navegación IdOperadorNavigation, cargando los datos del operador del vehículo.
                .Include(v => v.IdOperadorNavigation)
                // 7. .Include(v => v.IdTipoVehiculoNavigation): Realiza una carga "eager" de la propiedad
                //    de navegación IdTipoVehiculoNavigation, cargando los datos del tipo de vehículo.
                .Include(v => v.IdTipoVehiculoNavigation)
                // 8. .FirstOrDefaultAsync(m => m.Id == id);: Busca el primer vehículo que cumpla con la
                //    condición de que su propiedad 'Id' sea igual al 'id' recibido. FirstOrDefaultAsync
                //    devuelve el primer elemento encontrado o null si no se encuentra ninguno.
                .FirstOrDefaultAsync(m => m.Id == id);
            // 9. if (vehiculo == null): Comprueba si se encontró un vehículo con el 'id' proporcionado
            //    en la base de datos.
            if (vehiculo == null)
            {
                // 10. return NotFound(): Si no se encuentra ningún vehículo con ese 'id', devuelve un
                //     resultado NotFound.
                return NotFound();
            }

            // 11. return View(vehiculo);: Devuelve la vista asociada a la acción "Delete" ("Delete.cshtml"
            //     por convención), pasando el objeto 'vehiculo' como modelo a la vista. Esta vista
            //     generalmente muestra los detalles del vehículo y pregunta al usuario si está seguro de
            //     que desea eliminarlo.
            return View(vehiculo);
        }

        // 12. [HttpPost, ActionName("Delete")]: Estos atributos indican que este método de acción responde
        //     a solicitudes HTTP POST y que su nombre de acción es "Delete", aunque el nombre del método
        //     es DeleteConfirmed. Esto es una convención común para separar la acción que muestra la
        //     confirmación de eliminación de la acción que realmente realiza la eliminación.
        [HttpPost, ActionName("Delete")]
        // 13. [ValidateAntiForgeryToken]: Este atributo habilita la protección contra la falsificación de
        //     solicitudes entre sitios (CSRF).
        [ValidateAntiForgeryToken]
        // 14. public async Task<IActionResult> DeleteConfirmed(int id): Declara un método de acción
        //     asíncrono llamado DeleteConfirmed que devuelve un IActionResult. Este método se encarga
        //     de realizar la eliminación del vehículo una vez que el usuario ha confirmado la acción.
        //     Recibe el 'id' del vehículo a eliminar.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 15. var vehiculo = await _context.Vehiculos.FindAsync(id);: Busca de forma asíncrona la
            //     entidad Vehiculo en la base de datos utilizando su clave primaria ('Id').
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            // 16. if (vehiculo != null): Comprueba si se encontró un vehículo con el 'id' proporcionado.
            if (vehiculo != null)
            {
                // 17. _context.Vehiculos.Remove(vehiculo);: Marca la entidad 'vehiculo' para ser eliminada
                //     del contexto de la base de datos. Los cambios no se guardan en la base de datos hasta
                //     que se llama a SaveChangesAsync().
                _context.Vehiculos.Remove(vehiculo);
            }
            // 18. await _context.SaveChangesAsync();: Guarda de forma asíncrona todos los cambios realizados
            //     en el contexto (en este caso, la eliminación del vehículo) en la base de datos.
            await _context.SaveChangesAsync();
            // 19. return RedirectToAction(nameof(Index));: Después de eliminar el vehículo con éxito,
            //     redirige al usuario a la acción "Index" de este controlador, que probablemente muestra
            //     la lista de vehículos actualizada.
            return RedirectToAction(nameof(Index));
        }

        // 20. private bool VehiculoExists(int id): Declara un método privado que devuelve un valor booleano.
        //     Este método se utiliza internamente para verificar si un Vehiculo existe en la base de datos
        //     por su 'Id'.
        private bool VehiculoExists(int id)
        {
            // 21. return _context.Vehiculos.Any(e => e.Id == id);: Utiliza el método LINQ 'Any' para
            //     verificar si existe alguna entidad Vehiculo en la tabla 'Vehiculos' del contexto cuya
            //     propiedad 'Id' coincida con el 'id' proporcionado. Devuelve 'true' si existe al menos
            //     un Vehiculo con ese 'id', y 'false' en caso contrario.
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}