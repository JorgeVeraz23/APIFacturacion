﻿using Microsoft.AspNetCore.Http;
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
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IntentosFallidosManager _intentosFallidosManager;
        private readonly IMapper _mapper;
        protected Response _response;

        public UsuarioController(ILogger<UsuarioController> logger, IUsuarioRepositorio usuarioRepo,IntentosFallidosManager intentosFallidosManager, IMapper mapper)
        {
            _logger = logger;
            _usuarioRepo = usuarioRepo;
            _intentosFallidosManager = intentosFallidosManager;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response>> GetUsuario()
        {
            try
            {
                _logger.LogInformation("Obtener los usuarios");
                IEnumerable<Usuario> usuarioList = await _usuarioRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<UsuarioDto>>(usuarioList);
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

        [HttpGet("{id:int}", Name = "GetUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response>> GetUsuario(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer Usuario con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                var usuario = await _usuarioRepo.Obtener(c => c.IdUsuario == id);
                if (usuario == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    return NotFound(_response);
                }

                _response.Resultado = _mapper.Map<UsuarioDto>(usuario);
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
        public async Task<ActionResult<Response>> CrearUsuario([FromBody] UsuarioCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                if (createDto == null)
                {
                    return BadRequest(createDto);
                }
                Usuario modelo = _mapper.Map<Usuario>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;


                await _usuarioRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetUsuario", new { id = modelo.IdUsuario }, _response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };


            }
            return _response;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Response>> Login([FromBody] Usuario usuarioLogin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var intentosFallidos = _intentosFallidosManager.ObtenerIntentosFallidos(usuarioLogin.Usuario1);
                var ultimoIntentoFallido = _intentosFallidosManager.ObtenerUltimoIntentoFallido(usuarioLogin.Usuario1);

                if (intentosFallidos >= 3 && ultimoIntentoFallido.HasValue)
                {
                    var tiempoTranscurrido = DateTime.UtcNow - ultimoIntentoFallido.Value;
                    if (tiempoTranscurrido.TotalMinutes < 1)
                    {
                        _response.IsExitoso = false;
                        _response.ErrorMessages = new List<string> { "Usuario bloqueado. Espere 1 minuto." };
                        _response.statusCode = HttpStatusCode.Unauthorized;
                        return Unauthorized(_response);
                    }
                    else
                    {
                        // Si han pasado más de 1 minuto, reiniciamos los intentos fallidos
                        _intentosFallidosManager.ReiniciarIntentosFallidos(usuarioLogin.Usuario1);
                    }
                }

                // Verificar si el usuario existe
                var usuario = await _usuarioRepo.ObtenerUsuarioPorCredenciales(usuarioLogin.Usuario1, usuarioLogin.Contraseña);

                if (usuario == null)
                {
                    _intentosFallidosManager.RegistrarIntentoFallido(usuarioLogin.Usuario1);

                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Credenciales inválidas" };
                    _response.statusCode = HttpStatusCode.Unauthorized;
                    if (_intentosFallidosManager.ObtenerIntentosFallidos(usuarioLogin.Usuario1) >= 3)
                    {                  
                        _intentosFallidosManager.BloquearUsuario(usuarioLogin.Usuario1);

                        _response.ErrorMessages.Add("Usuario bloqueado. Contacte al administrador.");
                    }

                    return Unauthorized(_response);
                }     
                _intentosFallidosManager.ReiniciarIntentosFallidos(usuarioLogin.Usuario1);

                _response.Resultado = new { Usuario = usuario.Usuario1 };
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












        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var usuario = await _usuarioRepo.Obtener(v => v.IdUsuario == id);
                if (usuario == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _usuarioRepo.Remover(usuario);
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
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdUsuario)
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            Usuario modelo = _mapper.Map<Usuario>(updateDto);

            await _usuarioRepo.Actualizar(modelo);
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }


    }
}
