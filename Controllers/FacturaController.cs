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
    public class FacturaController : ControllerBase
    {
        private readonly ILogger<FacturaController> _logger;
        private readonly IFacturaRepositorio _facturaRepo;
        private readonly IMapper _mapper;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IProductoRepositorio _productoRepo;
        protected Response _response;

        public FacturaController(ILogger<FacturaController> logger, IFacturaRepositorio facturaRepo, IUsuarioRepositorio usuarioRepo, IProductoRepositorio productoRepo,IMapper mapper)
        {
            _logger = logger;
            _facturaRepo = facturaRepo;
            _usuarioRepo = usuarioRepo;
            _productoRepo = productoRepo;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response>> GetFactura()
        {
            try
            {
                _logger.LogInformation("Obtener las facturas");
                IEnumerable<Factura> facturaList = await _facturaRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<FacturaDto>>(facturaList);
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

        
        [HttpGet("{id:int}", Name = "GetFactura")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> GetFactura(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer la factura con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                var factura = await _facturaRepo.Obtener(c => c.IdFactura == id);
                if (factura == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<Factura>(factura);
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

        /* [HttpGet("ultimaFactura")]
         [ProducesResponseType(StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         [ProducesResponseType(StatusCodes.Status500InternalServerError)]
         public async Task<ActionResult<Response>> ObtenerUltimaFactura()
         {
             try
             {
                 // Esperar la finalización de la tarea para obtener la lista de facturas
                 List<Factura> facturas = await _facturaRepo.ObtenerTodos();

                 // Obtener la última factura ordenada por ID de forma descendente
                 var ultimaFactura = facturas.OrderByDescending(f => f.IdFactura).FirstOrDefault();

                 if (ultimaFactura == null)
                 {
                     // Manejar el caso en que no hay facturas
                     _response.statusCode = HttpStatusCode.NotFound;
                     _response.IsExitoso = false;
                     return NotFound(_response);
                 }

                 // Devolver la última factura
                 _response.Resultado = _mapper.Map<FacturaDto>(ultimaFactura);
                 _response.statusCode = HttpStatusCode.OK;

                 return Ok(_response);
             }
             catch (Exception ex)
             {
                 // Manejar errores, loggear, etc.
                 _response.IsExitoso = false;
                 _response.ErrorMessages = new List<string> { ex.ToString() };
                 return StatusCode(StatusCodes.Status500InternalServerError, _response);
             }
         }*/

        [HttpGet("ultimaFactura")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Tuple<int, int>> ObtenerUltimaFactura()
        {
            try
            {
                // Esperar la finalización de la tarea para obtener la lista de facturas
                List<Factura> facturas = await _facturaRepo.ObtenerTodos();

                // Obtener la última factura ordenada por ID de forma descendente
                var ultimaFactura = facturas.OrderByDescending(f => f.IdFactura).FirstOrDefault();

                if (ultimaFactura == null)
                {
                    // Manejar el caso en que no hay facturas
                    return Tuple.Create(-1, -1);
                }

                // Devolver el idFactura y el idUsuario de la última factura
                return Tuple.Create(ultimaFactura.IdFactura, ultimaFactura.IdUsuario ?? -1);
            }
            catch (Exception ex)
            {
                // Manejar errores, loggear, etc.
                return Tuple.Create(-1, -1);
            }
        }






        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response>> CrearFactura([FromBody] FacturaCreateDto createDto)
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
    
                Factura factura = _mapper.Map<Factura>(createDto);

              
                factura.FechaCreacion = DateTime.Now;
                factura.FechaActualizacion = DateTime.Now;

                // Guardar Factura
                await _facturaRepo.Crear(factura);

                // Calculo del IGV
                decimal igv = factura.Subtotal * (factura.PorcentajeIgv/100);
                decimal total = factura.Subtotal + igv;
                factura.Igv = igv;
                factura.Total = total;

                
                await _facturaRepo.Grabar();

                _response.Resultado = factura;
                _response.statusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetFactura", new { id = factura.IdFactura }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return _response;
            }
        }



        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> DeleteFactura(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var factura = await _facturaRepo.Obtener(v => v.IdFactura == id);
                if (factura == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _facturaRepo.Remover(factura);
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

        /*[HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFactura(int id, [FromBody] FacturaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdFactura)
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            if (await _usuarioRepo.Obtener(v => v.IdUsuario == updateDto.IdUsuario) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Id de Usuario no existe");
                return BadRequest(ModelState);
            }


            Factura modelo = _mapper.Map<Factura>(updateDto);

            await _facturaRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }*/

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> EditarFactura(int id, [FromBody] FacturaUpdateDto editDto)
        {
            try
            {
                // Validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Retrieve factura
                Factura factura = await _facturaRepo.Obtener(v => v.IdFactura == id);
                if (factura == null)
                {
                    return NotFound();
                }

                // Check for valid user (if applicable)
                if (await _usuarioRepo.Obtener(v => v.IdUsuario == editDto.IdUsuario) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El Id de usuario no existe");
                    return BadRequest(ModelState);
                }

                // Map editable fields to factura
                _mapper.Map(editDto, factura);

                // Update fields that shouldn't be mapped directly
                factura.FechaActualizacion = DateTime.Now;

                // Ensure consistency with IGV and total
                decimal igv = Math.Round(factura.Subtotal * (factura.PorcentajeIgv / 100), 2);
                decimal total = Math.Round(factura.Subtotal + igv, 2);
                factura.Igv = igv;
                factura.Total = total;

                // Save factura
                await _facturaRepo.Grabar();

                // Prepare response
                _response.Resultado = factura;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }




    }
}
