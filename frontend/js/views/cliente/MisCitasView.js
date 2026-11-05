
import { CitaService } from "../../services/CitaService.js";

// Servicio para consultar las citas
const citaService = new CitaService();

// Construir pantalla
export function MisCitasView() {
    return `
        <main class="dashboard" id="mis-citas">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Mis citas</h1>
                    <p>
                        Consulta tus citas de servicio
                        y su estado de seguimiento.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-citas">
                        Filtrar por estado:
                    </label>

                    <select id="filtro-citas">
                        <option value="Todas">Todas</option>
                        <option value="Pendiente">Pendiente</option>
                        <option value="Confirmada">Confirmada</option>
                        <option value="En proceso">En proceso</option>
                        <option value="Completada">Completada</option>
                        <option value="Cancelada">Cancelada</option>
                    </select>
                </div>

                <p id="citas-estado"
                   class="dashboard-estado"
                   role="status"
                   aria-live="polite">
                </p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Servicio</th>
                                <th scope="col">Fecha</th>
                                <th scope="col">Hora</th>
                                <th scope="col">Estado</th>
                            </tr>
                        </thead>

                        <tbody id="citas-tabla"></tbody>
                    </table>
                </div>

                <div class="dashboard-acciones">
                    <h2>Gestión de citas</h2>

                    <div class="dashboard-acciones-botones">
                        <a href="#solicitar-cita"
                           class="dashboard-boton">
                            Solicitar nueva cita
                        </a>

                        <a href="#cliente"
                           class="dashboard-boton-secundario">
                            Volver a mi panel
                        </a>
                    </div>
                </div>

            </section>
        </main>
    `;
}

// Escapar los datos antes de mostrarlos
function escaparHTML(valor) {
    const elemento = document.createElement("span");
    elemento.textContent = String(valor);
    return elemento.innerHTML;
}

// Mostrar citas en la tabla
async function cargarCitas(estadoFiltro = "Todas") {
    const tabla = document.querySelector("#citas-tabla");
    const estado = document.querySelector("#citas-estado");

    if (!tabla || !estado) return;

    estado.textContent = "Cargando citas...";
    tabla.innerHTML = "";

    try {
        const citas = await citaService.filtrarPorEstado(
            estadoFiltro
        );

        if (!tabla.isConnected) return;

        if (citas.length === 0) {
            estado.textContent =
                "No se encontraron citas con ese estado.";
            return;
        }

        tabla.innerHTML = citas.map(cita => `
            <tr>
                <td>${escaparHTML(cita.motocicleta)}</td>
                <td>${escaparHTML(cita.servicio)}</td>
                <td>${escaparHTML(cita.fecha)}</td>
                <td>${escaparHTML(cita.hora)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(cita.estado)}
                    </span>
                </td>
            </tr>
        `).join("");

        estado.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        estado.textContent =
            "No fue posible cargar tus citas.";

        console.error("Error al cargar citas:", error);
    }
}

// Activar filtros
export function iniciarMisCitas() {
    const filtro = document.querySelector("#filtro-citas");

    if (!filtro) return;

    filtro.addEventListener("change", () => {
        cargarCitas(filtro.value);
    });

    cargarCitas();
}
