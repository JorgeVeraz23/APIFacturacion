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
    public class ProductoController : ControllerBase
    {
        private readonly ILogger<ProductoController> _logger;
        private readonly IProductoRepositorio _productoRepo;
        private readonly IMapper _mapper;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IFamiliaProductoRepositorio _familiaproductoRepo;
        protected Response _response;

        public ProductoController(ILogger<ProductoController> logger, IProductoRepositorio productoRepo, IUsuarioRepositorio usuarioRepo, IFamiliaProductoRepositorio familiaproductoRepo,IMapper mapper)
        {
            _logger = logger;
            _productoRepo = productoRepo;
            _usuarioRepo = usuarioRepo;
            _familiaproductoRepo = familiaproductoRepo; 
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response>> GetProducto()
        {
            try
            {
                _logger.LogInformation("Obtener los productos");
                IEnumerable<Producto> ProductoList = await _productoRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<ProductoDto>>(ProductoList);
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

        [HttpGet("{id:int}", Name = "GetProducto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> GetProducto(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer el producto con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                var producto = await _productoRepo.Obtener(c => c.IdProducto == id);
                if (producto == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<ProductoDto>(producto);
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
        public async Task<ActionResult<Response>> CrearProducto([FromBody] ProductoCreateDto createDto)
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

                if (await _familiaproductoRepo.Obtener(v => v.IdFamilia == createDto.IdFamilia) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El Id de familia de producto no existe");
                    return BadRequest(ModelState);
                }



                if (createDto == null)
                {
                    return BadRequest(createDto);
                }
                Producto modelo = _mapper.Map<Producto>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;


                await _productoRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetProducto", new { id = modelo.IdProducto }, _response);
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

        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var producto = await _productoRepo.Obtener(v => v.IdProducto == id);
                if (producto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _productoRepo.Remover(producto);
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
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] ProductoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdProducto)
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

            if (await _familiaproductoRepo.Obtener(v => v.IdFamilia == updateDto.IdFamilia) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Id de familia de producto no existe");
                return BadRequest(ModelState);
            }

            Producto modelo = _mapper.Map<Producto>(updateDto);

            await _productoRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }



    }
}
