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
        protected Response _response;

        public FacturaController(ILogger<FacturaController> logger, IFacturaRepositorio facturaRepo, IUsuarioRepositorio usuarioRepo, IMapper mapper)
        {
            _logger = logger;
            _facturaRepo = facturaRepo;
            _usuarioRepo = usuarioRepo;
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


                if (createDto == null)
                {
                    return BadRequest(createDto);
                }
                Factura modelo = _mapper.Map<Factura>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;


                await _facturaRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetFactura", new { id = modelo.IdFactura }, _response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };


            }
            return _response;
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

        [HttpPut("{id:int}")]
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
        }



    }
}
