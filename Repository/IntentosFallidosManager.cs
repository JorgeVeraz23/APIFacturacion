namespace FacturacionAPI1.Repository
{
    public class IntentosFallidosManager
    {
        private Dictionary<string, int> intentosFallidos = new Dictionary<string, int>();
        private Dictionary<string, DateTime?> ultimoIntentoFallido = new Dictionary<string, DateTime?>();

        public void RegistrarIntentoFallido(string usuario)
        {
            if (!intentosFallidos.ContainsKey(usuario))
            {
                intentosFallidos[usuario] = 1;
            }
            else
            {
                intentosFallidos[usuario]++;
            }

            ultimoIntentoFallido[usuario] = DateTime.UtcNow;
        }

        public int ObtenerIntentosFallidos(string usuario)
        {
            return intentosFallidos.TryGetValue(usuario, out var intentos) ? intentos : 0;
        }

        public DateTime? ObtenerUltimoIntentoFallido(string usuario)
        {
            return ultimoIntentoFallido.TryGetValue(usuario, out var ultimoIntento) ? ultimoIntento : (DateTime?)null;
        }

        public void ReiniciarIntentosFallidos(string usuario)
        {
            intentosFallidos.Remove(usuario);
            ultimoIntentoFallido.Remove(usuario);
        }
        public void BloquearUsuario(string usuario)
        {
            // Aquí puedes agregar la lógica para bloquear al usuario según tus requisitos.
            // Podrías desencadenar un evento, registrar el bloqueo en algún lugar, etc.
            // En este ejemplo, simplemente imprimimos un mensaje.
            Console.WriteLine($"El usuario {usuario} está bloqueado.");
        }
        public void RestablecerIntentosFallidos(string usuario)
        {
            if (intentosFallidos.ContainsKey(usuario))
            {
                // Restablecer intentos fallidos a 0
                intentosFallidos[usuario] = 0;
            }

            if (ultimoIntentoFallido.ContainsKey(usuario))
            {
                // Restablecer la fecha del último intento fallido
                ultimoIntentoFallido[usuario] = null;
            }
        }
    }
}
