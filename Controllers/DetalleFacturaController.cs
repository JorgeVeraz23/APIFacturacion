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
        /*
                [HttpPost]
                [ProducesResponseType(StatusCodes.Status201Created)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
                [ProducesResponseType(StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<Response>> CrearDetalleFactura([FromBody] DetalleFacturaCreateDto createDto)
                {
                    try
                    {
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        if (await _usuarioRepo.Obtener(v => v.IdUsuario == createDto.IdUsuario) == null)
                        {
                            ModelState.AddModelError("ClaveForanea", "El Id de usuario no existe");
                            return BadRequest(ModelState);
                        }

                        if (await _facturaRepo.Obtener(v => v.IdFactura == createDto.IdFactura) == null)
                        {
                            ModelState.AddModelError("ClaveForanea", "El Id de factura no existe");
                            return BadRequest(ModelState);
                        }

                       var codigoProducto = createDto.CodigoProducto.ToUpperInvariant(); // Convierte a mayúsculas para comparación sin distinción de mayúsculas y minúsculas
                        var productosEnMemoria = await _productoRepo.ObtenerTodos();
                        var productoEnMemoria = productosEnMemoria.FirstOrDefault(p => p.Codigo.ToUpperInvariant() == codigoProducto);

                        if (productoEnMemoria == null)
                        {
                            ModelState.AddModelError(nameof(createDto.CodigoProducto), "El código del producto no existe");
                            return BadRequest(ModelState);
                        }

                        DetalleFactura modelo = _mapper.Map<DetalleFactura>(createDto);
                        modelo.FechaCreacion = DateTime.Now;
                        modelo.FechaActualizacion = DateTime.Now;

                        await _detallefacturaRepo.Crear(modelo);
                        _response.Resultado = modelo;
                        _response.statusCode = HttpStatusCode.Created;

                        return CreatedAtRoute("GetDetalleFactura", new { id = modelo.IdItem }, _response);
                    }
                    catch (Exception ex)
                    {
                        _response.IsExitoso = false;
                        _response.ErrorMessages = new List<string>() { ex.ToString() };
                    }
                    return _response;
                }
        */

        [HttpPost("AddItem/{idFactura}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> AddItem(int idFactura, [FromBody] DetalleFacturaCreateDto itemDto)
        {
            try
            {
                // Validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Retrieve factura
                Factura factura = await _facturaRepo.Obtener(v => v.IdFactura == idFactura);
                if (factura == null)
                {
                    return NotFound();
                }

                // Validate product existence and stock availability
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

                // Create DetalleFactura entity
                DetalleFactura item = _mapper.Map<DetalleFactura>(itemDto);
                item.IdFactura = factura.IdFactura;
                item.Precio = producto.Precio; // Ensure consistent pricing
                item.Subtotal = item.Precio * item.Cantidad;

                // Add item to factura
                factura.DetalleFacturas.Add(item);

                // Update product stock
                producto.Stock -= item.Cantidad;

                // Save factura and product
                await _facturaRepo.Grabar();
                await _productoRepo.Grabar(); // Ensure product update

                // Update factura total
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
                await _detallefacturaRepo.Remover(detallefactura);
                _response.statusCode = HttpStatusCode.NoContent;
                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };

                throw;
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDetalleFactura(int id, [FromBody] DetalleFacturaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdItem)
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            if (await _usuarioRepo.Obtener(v => v.IdUsuario == updateDto.IdUsuario) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Id de usuario no existe");
                return BadRequest(ModelState);
            }

            if (await _facturaRepo.Obtener(v => v.IdFactura == updateDto.IdFactura) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Id de factura no existe");
                return BadRequest(ModelState);
            }

            var codigoProducto = updateDto.CodigoProducto.ToUpperInvariant(); // Convierte a mayúsculas para comparación sin distinción de mayúsculas y minúsculas
            var productosEnMemoria = await _productoRepo.ObtenerTodos();
            var productoEnMemoria = productosEnMemoria.FirstOrDefault(p => p.Codigo.ToUpperInvariant() == codigoProducto);

            if (productoEnMemoria == null)
            {
                ModelState.AddModelError(nameof(updateDto.CodigoProducto), "El código del producto no existe");
                return BadRequest(ModelState);
            }


            DetalleFactura modelo = _mapper.Map<DetalleFactura>(updateDto);

            await _detallefacturaRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }




    }
}
