
/* Pantalla para solicitar citas */

export function SolicitarCitaView() {
    return `
        <main class="auth">
            <section class="auth-contenedor"
                     aria-labelledby="cita-titulo">

                <div class="auth-encabezado">
                    <h1 id="cita-titulo">Solicitar cita</h1>
                    <p>
                        Programa una solicitud de servicio
                        para tu motocicleta.
                    </p>
                </div>

                <form id="cita-form">

                    <div class="auth-campo">
                        <label for="cita-moto">Mi motocicleta</label>
                        <select id="cita-moto" required>
                            <option value="">Selecciona una motocicleta</option>
                            <option value="1">CFMOTO 450 SR</option>
                            <option value="2">CFMOTO 700 MT</option>
                        </select>
                    </div>

                    <div class="auth-campo">
                        <label for="cita-servicio">Tipo de servicio</label>
                        <select id="cita-servicio" required>
                            <option value="">Selecciona un servicio</option>
                            <option value="mantenimiento">
                                Mantenimiento preventivo
                            </option>
                            <option value="aceite">Cambio de aceite</option>
                            <option value="frenos">Revisión de frenos</option>
                            <option value="diagnostico">Diagnóstico general</option>
                            <option value="garantia">Servicio de garantía</option>
                        </select>
                    </div>

                    <div class="auth-campo">
                        <label for="cita-fecha">Fecha solicitada</label>
                        <input type="date" id="cita-fecha" required>
                    </div>

                    <div class="auth-campo">
                        <label for="cita-hora">Hora solicitada</label>
                        <select id="cita-hora" required>
                            <option value="">Selecciona un horario</option>
                            <option value="09:00">09:00</option>
                            <option value="10:00">10:00</option>
                            <option value="11:00">11:00</option>
                            <option value="12:00">12:00</option>
                            <option value="13:00">13:00</option>
                            <option value="15:00">15:00</option>
                            <option value="16:00">16:00</option>
                        </select>
                    </div>

                    <div class="auth-campo">
                        <label for="cita-observaciones">
                            Observaciones (opcional)
                        </label>
                        <textarea
                            id="cita-observaciones"
                            rows="4"
                            maxlength="500"
                            placeholder="Describe el motivo de tu cita"
                        ></textarea>
                    </div>

                    <p id="cita-estado"
                       role="status"
                       aria-live="polite"></p>

                    <button type="submit" class="auth-boton">
                        Solicitar cita
                    </button>
                </form>

                <p class="auth-pie">
                    <a href="#cliente">Volver a mi panel</a>
                </p>

            </section>
        </main>
    `;
}

// Activar el formulario
export function iniciarSolicitarCita() {
    const formulario = document.querySelector("#cita-form");
    const estado = document.querySelector("#cita-estado");
    const fecha = document.querySelector("#cita-fecha");

    if (!formulario || !estado || !fecha) return;

    // No permitir fechas anteriores a hoy
    const hoy = new Date();
    const anio = hoy.getFullYear();
    const mes = String(hoy.getMonth() + 1).padStart(2, "0");
    const dia = String(hoy.getDate()).padStart(2, "0");

    fecha.min = `${anio}-${mes}-${dia}`;

    formulario.addEventListener("submit", (evento) => {
        evento.preventDefault();

        if (!formulario.reportValidity()) return;

        if (fecha.value < fecha.min) {
            estado.textContent = "Selecciona una fecha válida.";
            return;
        }

        const datos = {
            moto: formulario.querySelector("#cita-moto").value,
            servicio: formulario.querySelector("#cita-servicio").value,
            fecha: fecha.value,
            hora: formulario.querySelector("#cita-hora").value,
            observaciones:
                formulario.querySelector("#cita-observaciones").value.trim()
        };

        // Demostración: no se envían datos a un servidor
        console.log("Solicitud de demostración:", datos);

        estado.textContent =
            "Formulario validado correctamente. " +
            "Esta es una simulación: la cita no ha sido registrada.";

        formulario.reset();
    });
}
