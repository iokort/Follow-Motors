
import { HistorialService } from "../../services/HistorialService.js";

// Servicio de historial
const historialService = new HistorialService();

// Evitar interpretar datos como HTML
function escaparHTML(valor) {
    return String(valor).replace(/[&<>"']/g, caracter => {
        const caracteres = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        };

        return caracteres[caracter];
    });
}

// Construir la pantalla
export function HistorialView() {
    return `
        <main class="dashboard" id="historial-cliente">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Historial de servicios</h1>

                    <p>
                        Consulta los mantenimientos y servicios
                        realizados a tus motocicletas.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-historial">
                        Seleccionar motocicleta:
                    </label>

                    <select id="filtro-historial">
                        <option value="Todas">Todas</option>
                        <option value="CFMOTO 450 SR">CFMOTO 450 SR</option>
                        <option value="CFMOTO 700 MT">CFMOTO 700 MT</option>
                    </select>
                </div>

                <p id="historial-estado"
                   class="dashboard-estado"
                   role="status"
                   aria-live="polite">
                </p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Fecha</th>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Servicio</th>
                                <th scope="col">Kilometraje</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acción</th>
                            </tr>
                        </thead>

                        <tbody id="historial-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Detalles del servicio</h2>

                    <p id="historial-detalles"
                       role="status"
                       aria-live="polite">
                        Selecciona un registro para ver sus detalles.
                    </p>
                </section>

                <div class="dashboard-acciones">
                    <a href="#cliente"
                       class="dashboard-boton-secundario">
                        Volver a mi panel
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Mostrar registros del historial
async function cargarHistorial(motocicleta = "Todas") {
    const tabla = document.querySelector("#historial-tabla");
    const estado = document.querySelector("#historial-estado");

    if (!tabla || !estado) return;

    estado.textContent = "Cargando historial...";
    tabla.innerHTML = "";

    try {
        const registros =
            await historialService.filtrarPorMotocicleta(motocicleta);

        if (!tabla.isConnected) return;

        if (registros.length === 0) {
            estado.textContent = "No hay servicios registrados.";
            return;
        }

        tabla.innerHTML = registros.map(registro => `
            <tr>
                <td>${escaparHTML(registro.fecha)}</td>
                <td>${escaparHTML(registro.motocicleta)}</td>
                <td>${escaparHTML(registro.servicio)}</td>
                <td>${registro.kilometraje.toLocaleString("es-MX")} km</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(registro.estado)}
                    </span>
                </td>
                <td>
                    <button
                        type="button"
                        class="dashboard-boton"
                        data-historial-id="${Number(registro.id)}">
                        Ver detalles
                    </button>
                </td>
            </tr>
        `).join("");

        estado.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        estado.textContent = "No se pudo cargar el historial.";
        console.error("Error al cargar historial:", error);
    }
}

// Activar filtros y botones
export function iniciarHistorial() {
    const panel = document.querySelector("#historial-cliente");
    const filtro = document.querySelector("#filtro-historial");
    const detalles = document.querySelector("#historial-detalles");

    if (!panel || !filtro || !detalles) return;

    filtro.addEventListener("change", () => {
        detalles.textContent =
            "Selecciona un registro para ver sus detalles.";

        cargarHistorial(filtro.value);
    });

    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest("[data-historial-id]");

        if (!boton || !panel.contains(boton)) return;

        try {
            const registro = await historialService.obtenerPorId(
                boton.dataset.historialId
            );

            if (!detalles.isConnected) return;

            detalles.textContent =
                `${registro.servicio} - ${registro.motocicleta}. ` +
                `Observaciones: ${registro.observaciones}`;

        } catch (error) {
            if (!detalles.isConnected) return;

            detalles.textContent =
                "No se encontraron los detalles del servicio.";
        }
    });

    cargarHistorial();
}
