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
    public class FamiliaProductoController : ControllerBase
    {

        private readonly ILogger<FamiliaProductoController> _logger;
        private readonly IFamiliaProductoRepositorio _familiaProductoRepo;
        private readonly IMapper _mapper;
        private readonly IUsuarioRepositorio _usuarioRepo;
        protected Response _response;

        public FamiliaProductoController(ILogger<FamiliaProductoController> logger, IFamiliaProductoRepositorio familiaProductoRepo, IUsuarioRepositorio usuarioRepo, IMapper mapper)
        {
            _logger = logger;
            _familiaProductoRepo = familiaProductoRepo;
            _usuarioRepo = usuarioRepo;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response>> GetFamiliaProducto()
        {
            try
            {
                _logger.LogInformation("Obtener las familias de productos");
                IEnumerable<FamiliaProducto> familiaProductoList = await _familiaProductoRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<FamiliaProductoDto>>(familiaProductoList);
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

        [HttpGet("{id:int}", Name = "GetFamiliaProducto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> GetFamiliaProducto(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer la familia del producto con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                var familiaproducto = await _familiaProductoRepo.Obtener(c => c.IdFamilia == id);
                if (familiaproducto == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<FamiliaProductoDto>(familiaproducto);
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
        public async Task<ActionResult<Response>> CrearFamiliaProducto([FromBody] FamiliaProductoCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                if (await _usuarioRepo.Obtener(v => v.IdUsuario== createDto.IdUsuario) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El Id de usuario no existe");
                    return BadRequest(ModelState);
                }


                if (createDto == null)
                {
                    return BadRequest(createDto);
                }
                FamiliaProducto modelo = _mapper.Map<FamiliaProducto>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;


                await _familiaProductoRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetFamiliaProducto", new { id = modelo.IdFamilia }, _response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };


            }
            return _response;
        }

        /*   [HttpDelete("{id:int}")]
           [ProducesResponseType(StatusCodes.Status204NoContent)]
           [ProducesResponseType(StatusCodes.Status400BadRequest)]
           [ProducesResponseType(StatusCodes.Status404NotFound)]

           public async Task<IActionResult> DeleteFamiliaProducto(int id)
           {
               try
               {
                   if (id == 0)
                   {
                       _response.IsExitoso = false;
                       _response.statusCode = HttpStatusCode.BadRequest;
                       return BadRequest(_response);
                   }
                   var familiaproducto = await _familiaProductoRepo.Obtener(v => v.IdFamilia == id);
                   if (familiaproducto == null)
                   {
                       _response.IsExitoso = false;
                       _response.statusCode = HttpStatusCode.NotFound;
                       return NotFound(_response);
                   }
                   await _familiaProductoRepo.Remover(familiaproducto);
                   _response.statusCode = HttpStatusCode.NoContent;
                   return BadRequest(_response);
               }
               catch (Exception ex)
               {
                   _response.IsExitoso = false;
                   _response.ErrorMessages = new List<string>() { ex.ToString() };

                   throw;
               }
           }*/

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFamiliaProducto(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var familiaproducto = await _familiaProductoRepo.Obtener(v => v.IdFamilia == id);

                if (familiaproducto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                // Cambiar el valor de la propiedad "Activo" a false en lugar de eliminar físicamente
                familiaproducto.Activo = false;

                await _familiaProductoRepo.Actualizar(familiaproducto); // Asumo que tienes un método para actualizar

                _response.statusCode = HttpStatusCode.NoContent;
                return NoContent(); // Devolver NoContent en lugar de BadRequest
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
        public async Task<IActionResult> UpdateFamiliaProducto(int id, [FromBody] FamiliaProductoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdFamilia)
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


            FamiliaProducto modelo = _mapper.Map<FamiliaProducto>(updateDto);

            await _familiaProductoRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }




    }
}
