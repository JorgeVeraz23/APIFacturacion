using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI1.Models.Dto
{
    public class UsuarioCreateDto
    {
        [Required]
        public int IdUsuario { get; set; }


        public string Usuario1 { get; set; } = null!;

        public string Contraseña { get; set; } = null!;

        public int? IntentosFallidos { get; set; }

        public bool? Bloqueado { get; set; }

  
    }
}
