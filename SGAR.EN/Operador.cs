

namespace SGAR.EN
{
    public class Operador
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string TelefonoPersonal { get; set; } = string.Empty;
        public string CorreoPersonal { get; set; } = string.Empty;
        public string DUI { get; set; } = string.Empty;
        public byte[] Foto { get; set; } = new byte[0];
        public string Ayudantes { get; set; } = string.Empty;
        public string CodigoOperador { get; set; } = string.Empty;
        public string TelefonoLaboral { get; set; } = string.Empty;
        public string CorreoLaboral { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public byte[] LicenciaDoc {  get; set; } = new byte[0];
        public byte[] AntecedentesDoc { get; set; } = new byte[0];
        public byte[] SolvenciaDoc { get; set; } = new byte[0];
        public string Password { get; set; } = string.Empty;
        public int IdAlcaldia {  get; set; }
    }
}
