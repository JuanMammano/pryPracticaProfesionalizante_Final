namespace pryPracticaProfesionalizante_Final
{
    public sealed class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public override string ToString() => $"{NombreCompleto} - {Telefono}";
    }

    public sealed class Turno
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClienteId { get; set; }
        public DateTime Inicio { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Presupuesto { get; set; }
        public string DetallesDiseno { get; set; } = string.Empty;
        public string Estado { get; set; } = "Confirmado";

        public DateTime Fin => Inicio.AddMinutes(DuracionMinutos);
    }

    public sealed class AgendaData
    {
        public List<Cliente> Clientes { get; set; } = new();
        public List<Turno> Turnos { get; set; } = new();
    }
}
