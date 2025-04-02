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
    public class VehiculoController : Controller
    {
        // Contexto de la base de datos para interactuar con las entidades.
        private readonly SgarDbContext _context;

        // Constructor que recibe el contexto de la base de datos por inyección de dependencias.
        public VehiculoController(SgarDbContext context)
        {
            _context = context;
        }

        // GET: Vehiculo
        // Acción para mostrar la lista de vehículos, con paginación y filtrado.
        public async Task<IActionResult> Index(Vehiculo vehiculo, int? pag, int topRegistro = 10)
        {
            // Obtiene todos los vehículos, incluyendo sus relaciones con Operador y Marca.
            var vehiculos =  _context.Vehiculos.Include(s=>s.IdOperadorNavigation).Include(s=>s.IdMarcaNavigation);

            // Convierte la colección 'vehiculos' (ya filtrada por alcaldía y con relaciones incluidas) a un IQueryable,
            // permitiendo la construcción dinámica de consultas LINQ para aplicar filtros, ordenamientos y paginación.
            var query = vehiculos.AsQueryable();

            // Si se proporciona una placa para filtrar, aplica el filtro.
            if (!string.IsNullOrWhiteSpace(vehiculo.Placa))
                query = query.Where(s => s.Placa.Contains(vehiculo.Placa));

            // Ordena los vehículos por Id descendente.
            query = query.OrderByDescending(s => s.Id);

            // Retorna la vista con los vehículos paginados.

            return View(await PaginatedList<Vehiculo>.CreateAsync(query, pag ?? 1, topRegistro));
        }

        // GET: Vehiculo/Details/5   
        // Acción para mostrar los detalles de un vehículo específico.
        public async Task<IActionResult> Details(int? id)
        {
            // Si el Id es nulo, retorna NotFound.
            if (id == null)
            {
                return NotFound();
            }
            // Obtiene el vehículo con las relaciones Marca, Operador y TipoVehiculo.
            var vehiculo = await _context.Vehiculos
                .Include(v => v.IdMarcaNavigation)
                .Include(v => v.IdOperadorNavigation)
                .Include(v => v.IdTipoVehiculoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si el vehículo no se encuentra, retorna NotFound.
            if (vehiculo == null)
            {
                return NotFound();
            }
            // Retorna la vista con los detalles del vehículo.
            return View(vehiculo);
        }

        // GET: Vehiculo/Create
        // Acción para mostrar el formulario de creación de un nuevo vehículo.
        public IActionResult Create()
        {
            // Prepara las listas desplegables para Marca, Operador y TipoVehiculo.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo");
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre");
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion");
            // Retorna la vista con un nuevo objeto Vehiculo.
            return View(new Vehiculo());
        }

        
        // Método auxiliar para convertir un IFormFile (imagen) a un array de bytes.
        public async Task<byte[]?> GenerarByteImage(IFormFile? file, byte[]? bytesImage = null)
        {
            byte[]? bytes = bytesImage;
            // Si el archivo no es nulo y tiene longitud mayor a 0, se procesa.
            if (file != null && file.Length > 0)
            {
                // Crea un MemoryStream para almacenar los bytes del archivo.
                using (var memoryStream = new MemoryStream())
                {
                    // Copia el contenido del archivo al MemoryStream.
                    await file.CopyToAsync(memoryStream);
                    // Convierte el MemoryStream a un array de bytes.
                    bytes = memoryStream.ToArray();
                }
            }
            // Retorna el array de bytes.
            return bytes;
        }


        // POST: Vehiculo/Create
        // Acción para procesar la creación de un nuevo vehículo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehiculo vehiculo)
        {
            try
            {
                // Si se proporciona un archivo de imagen, lo convierte a bytes y lo asigna al vehículo.
                if (vehiculo.fotofile != null)
                {
                    vehiculo.Foto = await GenerarByteImage(vehiculo.fotofile);
                }

                // Agrega el vehículo al contexto y guarda los cambios.
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                // Redirige a la lista de vehículos.
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Si ocurre un error, prepara las listas desplegables y retorna la vista con el vehículo.
                ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
                ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);
                ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
                return View(vehiculo);
            }
        }
        // GET: Vehiculo/Edit/5
        // Acción para mostrar el formulario de edición de un vehículo existente.
        public async Task<IActionResult> Edit(int? id)
        {
            // Si el Id es nulo, retorna NotFound.
            if (id == null)
            {
                return NotFound();
            }

            // Obtiene el vehículo a editar.
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            // Si el vehículo no se encuentra, retorna NotFound.
            if (vehiculo == null)
            {
                return NotFound();
            }

            // Prepara las listas desplegables para Marca, Operador y TipoVehiculo.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Nombre", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            // Retorna la vista con el vehículo a editar.
            return View(vehiculo);
        }
        // POST: Vehiculo/Edit/5
        // Acción para procesar la edición de un vehículo existente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            // Si el Id del vehículo no coincide con el Id proporcionado, retorna NotFound.
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            // Obtiene el vehículo a actualizar.
            var vehiculoUpdate = await _context.Vehiculos
                .FirstOrDefaultAsync(o => o.Id == vehiculo.Id);

            // Si el vehículo no se encuentra, retorna NotFound.
            if (vehiculoUpdate == null)
            {
                return NotFound();
            }

            try
            {
                // Actualiza las propiedades del vehículo.
                vehiculoUpdate.IdMarca = vehiculo.IdMarca;
                vehiculoUpdate.Mecanico = vehiculo.Mecanico;
                vehiculoUpdate.Taller = vehiculo.Taller;
                vehiculoUpdate.Estado = vehiculo.Estado;
                vehiculoUpdate.Codigo = vehiculo.Codigo;
                vehiculoUpdate.IdOperador = vehiculo.IdOperador;
                vehiculoUpdate.IdTipoVehiculo = vehiculo.IdTipoVehiculo;
                vehiculoUpdate.Placa = vehiculo.Placa;
                vehiculoUpdate.Descripcion = vehiculo.Descripcion;

                // Obtiene la foto anterior del vehículo.
                var fotoAnterior = await _context.Vehiculos
                    .Where(s => s.Id == vehiculo.Id)
                    .Select(s => s.Foto).FirstOrDefaultAsync();
                // Actualiza la foto del vehículo, si se proporciona un nuevo archivo.
                vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, fotoAnterior);

                if (vehiculo.fotofile != null)
                {
                    vehiculoUpdate.Foto = await GenerarByteImage(vehiculo.fotofile, vehiculoUpdate.Foto);
                }

                // Guarda los cambios en la base de datos.
                await _context.SaveChangesAsync();
                // Redirige a la lista de vehículos.
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si ocurre un error de concurrencia, verifica si el vehículo existe.
                if (!VehiculoExists(vehiculo.Id))
                {
                    return NotFound();
                }
                else
                {
                    // Agrega un error al modelo.
                    ModelState.AddModelError("", "Ocurrió un error de concurrencia. Por favor, intente de nuevo.");
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error inesperado, agrega un error al modelo.
                ModelState.AddModelError("", "Ocurrió un error inesperado, por favor intente de nuevo.");
            }

            // Prepara las listas desplegables y retorna la vista con el vehículo.
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "Id", "Modelo", vehiculo.IdMarca);
            ViewData["IdOperador"] = new SelectList(_context.Operadores, "Id", "Id", vehiculo.IdOperador);
            ViewData["IdTipoVehiculo"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.IdTipoVehiculo);
            return View(vehiculo);
        }

        // GET: Vehiculo/Delete/5
        // Acción para mostrar la confirmación de eliminación de un vehículo.
        public async Task<IActionResult> Delete(int? id)
        {
            // Si el Id es nulo, retorna NotFound.
            if (id == null)
            {
                return NotFound();
            }

            // Obtiene el vehículo con sus relaciones.
            var vehiculo = await _context.Vehiculos
                .Include(v => v.IdMarcaNavigation)
                .Include(v => v.IdOperadorNavigation)
                .Include(v => v.IdTipoVehiculoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            // Si el vehículo no se encuentra, retorna NotFound.
            if (vehiculo == null)
            {
                return NotFound();
            }

            // Retorna la vista con el vehículo a eliminar.
            return View(vehiculo);
        }

        // POST: Vehiculo/Delete/5
        // Acción para procesar la eliminación de un vehículo.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Obtiene el vehículo a eliminar.
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            // Si el vehículo no es nulo, lo elimina.
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
            }

            // Guarda los cambios en la base de datos.
            await _context.SaveChangesAsync();
            // Redirige a la lista de vehículos.
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para verificar si un vehículo existe.
        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}