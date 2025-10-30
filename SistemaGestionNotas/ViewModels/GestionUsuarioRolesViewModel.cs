namespace SistemaGestionNotas.Models
{
    public class GestionUsuarioRoles
    {
        public string UserId { get; set; }
        public string UserName { get; set; }   // Nombre de usuario
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}
