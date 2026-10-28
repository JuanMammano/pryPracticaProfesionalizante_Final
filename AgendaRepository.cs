using System.Text.Json;

namespace pryPracticaProfesionalizante_Final
{
    public sealed class AgendaRepository
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "agenda.json");
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public AgendaData Data { get; private set; }

        public AgendaRepository()
        {
            Data = Load();
        }

        public Cliente AddOrGetCliente(string nombre, string telefono, string email)
        {
            Cliente? existente = Data.Clientes.FirstOrDefault(c =>
                c.Telefono.Equals(telefono, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(telefono));

            if (existente is not null)
            {
                existente.NombreCompleto = nombre;
                existente.Email = email;
                return existente;
            }

            var cliente = new Cliente
            {
                NombreCompleto = nombre,
                Telefono = telefono,
                Email = email
            };
            Data.Clientes.Add(cliente);
            return cliente;
        }

        public bool IsAvailable(DateTime inicio, int duracionMinutos, Guid? turnoId = null)
        {
            DateTime fin = inicio.AddMinutes(duracionMinutos);
            return !Data.Turnos.Any(t =>
                t.Id != turnoId &&
                inicio < t.Fin && fin > t.Inicio);
        }

        public void AddTurno(Turno turno)
        {
            if (!IsAvailable(turno.Inicio, turno.DuracionMinutos))
                throw new InvalidOperationException("El horario seleccionado se superpone con otro turno.");

            Data.Turnos.Add(turno);
            Save();
        }

        public void Save()
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_filePath, JsonSerializer.Serialize(Data, _jsonOptions));
        }

        private AgendaData Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<AgendaData>(json) ?? new AgendaData();
                }
            }
            catch (JsonException)
            {
                // Se inicia una agenda vacía si el archivo fue alterado o está incompleto.
            }

            return new AgendaData();
        }
    }
}
