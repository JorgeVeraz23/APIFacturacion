using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using FacturacionAPI1.Models;
using FacturacionAPI1.Repository;
using FacturacionAPI1.Repository.IRepositorio;
using System.Net;
using FacturacionAPI1.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data.Common;


namespace FacturacionAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleFacturaController : ControllerBase
    {
        private readonly ILogger<DetalleFacturaController> _logger;
        private readonly IDetalleFacturaRepositorio _detallefacturaRepo;
        private readonly IMapper _mapper;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IFacturaRepositorio _facturaRepo;
        private readonly IProductoRepositorio _productoRepo;
        protected Response _response;

        public DetalleFacturaController(ILogger<DetalleFacturaController> logger, IDetalleFacturaRepositorio detallefacturaRepo, IUsuarioRepositorio usuarioRepo, IFacturaRepositorio facturaRepo,IProductoRepositorio productoRepo, IMapper mapper)
        {
            _logger = logger;
            _detallefacturaRepo = detallefacturaRepo;
            _usuarioRepo = usuarioRepo;
            _facturaRepo = facturaRepo;
            _productoRepo = productoRepo;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response>> GetDetalleFactura()
        {
            try
            {
                _logger.LogInformation("Obtener los detalles de facturas");
                IEnumerable<DetalleFactura> detallefacturaList = await _detallefacturaRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<DetalleFacturaDto>>(detallefacturaList);
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;

        }

        [HttpGet("{id:int}", Name = "GetDetalleFactura")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> GetDetalleFactura(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer el detalle de factura con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                var detallefactura = await _detallefacturaRepo.Obtener(c => c.IdItem == id);
                if (detallefactura == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<DetalleFactura>(detallefactura);
                _response.statusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };

                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }

        }

        [HttpGet("ultimoDetalleFacturaId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<int> ObtenerUltimoDetalleFacturaId()
        {
            try
            {
                // Esperar la finalización de la tarea para obtener la lista de detalles de factura
                List<DetalleFactura> detallesFactura = await _detallefacturaRepo.ObtenerTodos();

                // Obtener el último detalle de factura ordenado por IdItem de forma descendente
                var ultimoDetalleFactura = detallesFactura.OrderByDescending(df => df.IdItem).FirstOrDefault();

                if (ultimoDetalleFactura == null)
                {
                    // Manejar el caso en que no hay detalles de factura
                    return -1;
                }

                // Devolver el IdItem del último detalle de factura
                return ultimoDetalleFactura.IdItem;
            }
            catch (Exception ex)
            {
                // Manejar errores, loggear, etc.
                return -1;
            }
        }

        [HttpGet("BuscarPorIdFactura/{idFactura}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DetalleFacturaDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DetalleFacturaDto>>> BuscarDetallesPorIdFactura(int idFactura)
        {
            try
            {
                if (idFactura <= 0)
                {
                    return BadRequest("El ID de factura debe ser un número positivo.");
                }

                var detallesFactura = await _detallefacturaRepo.ObtenerTodos(df => df.IdFactura == idFactura);

                if (detallesFactura == null)
                {
                    return NotFound($"No se encontraron detalles de factura para la factura con ID {idFactura}.");
                }

                var detallesFacturaDto = _mapper.Map<IEnumerable<DetalleFacturaDto>>(detallesFactura);

                return Ok(detallesFacturaDto);
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al buscar detalles de factura por ID.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error de base de datos al buscar detalles de factura.");
            }
        }








        [HttpPost("AddItem/{idFactura}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> AddItem(int idFactura, [FromBody] DetalleFacturaCreateDto itemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                Factura factura = await _facturaRepo.Obtener(v => v.IdFactura == idFactura);
                if (factura == null)
                {
                    return NotFound();
                }

                Producto producto = await _productoRepo.Obtener(v => v.Codigo == itemDto.CodigoProducto);
                if (producto == null)
                {
                    ModelState.AddModelError("Producto", "Producto no encontrado");
                    return BadRequest(ModelState);
                }
                if (producto.Stock < itemDto.Cantidad)
                {
                    ModelState.AddModelError("Producto", "Stock insuficiente");
                    return BadRequest(ModelState);
                }

               
                DetalleFactura item = _mapper.Map<DetalleFactura>(itemDto);
                item.IdFactura = factura.IdFactura;
                item.Precio = producto.Precio; 
                item.Subtotal = item.Precio * item.Cantidad;

                // añadimos el item a la factura
                factura.DetalleFacturas.Add(item);

                // validamos que cuando se genere un detalle factura se reduzca el stock
                producto.Stock -= item.Cantidad;

                // guardamos la factura y el producto
                await _facturaRepo.Grabar();
                await _productoRepo.Grabar(); // Con este codigo nosotros garantizamos la actualizacion del producto

                // actualizamos el total de la factura
                decimal igv = factura.Subtotal * factura.PorcentajeIgv;
                decimal total = factura.Subtotal + igv;
                factura.Igv = igv;
                factura.Total = total;

                _response.Resultado = item;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtAction("GetDetalleFactura", new { idFactura = idFactura, idItem = item.IdItem }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }



        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDetalleFactura(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var detallefactura = await _detallefacturaRepo.Obtener(v => v.IdItem == id);
                if (detallefactura == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                Producto producto = await _productoRepo.Obtener(v => v.Codigo == detallefactura.CodigoProducto);

                // Actualizamos Stock
                producto.Stock += detallefactura.Cantidad;

                await _detallefacturaRepo.Remover(detallefactura);

                // Devuelve correctamente un 204 NoContent en caso de éxito
                _response.statusCode = HttpStatusCode.NoContent;
                return NoContent();
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };

                throw;
            }
        }


        







    }
}
