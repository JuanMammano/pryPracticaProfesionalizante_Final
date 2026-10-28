namespace pryPracticaProfesionalizante_Final
{
    public partial class Form1 : Form
    {
        private readonly AgendaRepository _repository = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dtpFecha.MinDate = DateTime.Today;
            dtpFecha.Value = DateTime.Today;
            dtpVista.Value = DateTime.Today;
            dtpHora.Value = DateTime.Today.AddHours(10);
            RefrescarAgenda();
        }

        private void btnGuardar_Click(object? sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string telefono = txtTelefono.Text.Trim();
            string detalles = txtDetalles.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono) || string.IsNullOrWhiteSpace(detalles))
            {
                MessageBox.Show("Complete nombre, teléfono y detalles del diseño.", "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime inicio = dtpFecha.Value.Date.Add(dtpHora.Value.TimeOfDay);
            int duracion = (int)nudDuracion.Value;
            if (inicio < DateTime.Now)
            {
                MessageBox.Show("No se puede registrar un turno en una fecha u hora pasada.", "Fecha inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_repository.IsAvailable(inicio, duracion))
            {
                MessageBox.Show("El horario seleccionado está ocupado o se superpone con otro turno.", "Horario no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cliente cliente = _repository.AddOrGetCliente(nombre, telefono, txtEmail.Text.Trim());
            var turno = new Turno
            {
                ClienteId = cliente.Id,
                Inicio = inicio,
                DuracionMinutos = duracion,
                Presupuesto = nudPresupuesto.Value,
                DetallesDiseno = detalles
            };

            try
            {
                _repository.AddTurno(turno);
                lblEstado.Text = $"Turno confirmado para {cliente.NombreCompleto}: {inicio:dd/MM/yyyy HH:mm} hs.";
                lblEstado.ForeColor = Color.DarkGreen;
                MessageBox.Show($"Turno confirmado.\n\nCliente: {cliente.NombreCompleto}\nFecha: {inicio:dd/MM/yyyy}\nHorario: {inicio:HH:mm} a {turno.Fin:HH:mm} hs.", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                dtpVista.Value = inicio.Date;
                RefrescarAgenda();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear(); txtTelefono.Clear(); txtEmail.Clear(); txtDetalles.Clear(); nudPresupuesto.Value = 0;
        }

        private void InitializeComponent()
        {

        }

        private void RefrescarAgenda()
        {
            if (dgvAgenda is null || cmbVista.SelectedIndex < 0) return;
            DateTime referencia = dtpVista.Value.Date;
            DateTime desde = referencia;
            DateTime hasta = referencia.AddDays(1);
            string vista = cmbVista.SelectedItem?.ToString() ?? "Diaria";
            if (vista == "Semanal")
            {
                int diferencia = ((int)referencia.DayOfWeek + 6) % 7;
                desde = referencia.AddDays(-diferencia);
                hasta = desde.AddDays(7);
            }
            else if (vista == "Mensual")
            {
                desde = new DateTime(referencia.Year, referencia.Month, 1);
                hasta = desde.AddMonths(1);
            }

            var filas = _repository.Data.Turnos
                .Where(t => t.Inicio >= desde && t.Inicio < hasta)
                .OrderBy(t => t.Inicio)
                .Select(t =>
                {
                    Cliente? cliente = _repository.Data.Clientes.FirstOrDefault(c => c.Id == t.ClienteId);
                    return new
                    {
                        Fecha = t.Inicio.ToString("dd/MM/yyyy"),
                        Horario = $"{t.Inicio:HH:mm} - {t.Fin:HH:mm}",
                        Cliente = cliente?.NombreCompleto ?? "Sin cliente",
                        Telefono = cliente?.Telefono ?? "",
                        Duracion = $"{t.DuracionMinutos} min",
                        Presupuesto = t.Presupuesto.ToString("C"),
                        Detalles = t.DetallesDiseno
                    };
                }).ToList();
            dgvAgenda.DataSource = filas;
            lblEstado.Text = $"Vista {vista.ToLowerInvariant()}: {filas.Count} turno(s).";
            lblEstado.ForeColor = Color.DarkSlateGray;
        }

        private Button button1;
    }
}
