
import { GarantiaService } from "../../services/GarantiaService.js";

// Servicio de garantías
const garantiaService = new GarantiaService();

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
export function GarantiasView() {
    return `
        <main class="dashboard" id="garantias-cliente">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Mis garantías</h1>

                    <p>
                        Consulta tus solicitudes de garantía
                        y revisa el estado de seguimiento.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-garantias">
                        Filtrar por estado:
                    </label>

                    <select id="filtro-garantias">
                        <option value="Todas">Todas</option>
                        <option value="En revisión">En revisión</option>
                        <option value="Aprobada">Aprobada</option>
                        <option value="Finalizada">Finalizada</option>
                    </select>
                </div>

                <p
                    id="garantias-estado"
                    class="dashboard-estado"
                    role="status"
                    aria-live="polite">
                </p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Folio</th>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Fecha</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acción</th>
                            </tr>
                        </thead>

                        <tbody id="garantias-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Detalles de garantía</h2>

                    <p
                        id="garantia-detalles"
                        role="status"
                        aria-live="polite">
                        Selecciona una garantía para consultar sus detalles.
                    </p>
                </section>

                <div class="dashboard-acciones">
                    <a
                        href="#cliente"
                        class="dashboard-boton-secundario">
                        Volver a mi panel
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Consultar y mostrar las garantías
async function cargarGarantias(estadoFiltro = "Todas") {
    const tabla = document.querySelector("#garantias-tabla");
    const estado = document.querySelector("#garantias-estado");

    if (!tabla || !estado) return;

    estado.textContent = "Cargando garantías...";
    tabla.innerHTML = "";

    try {
        const garantias =
            await garantiaService.filtrarPorEstado(estadoFiltro);

        if (!tabla.isConnected) return;

        if (garantias.length === 0) {
            estado.textContent =
                "No se encontraron garantías con ese estado.";
            return;
        }

        tabla.innerHTML = garantias.map(garantia => `
            <tr>
                <td>${escaparHTML(garantia.folio)}</td>
                <td>${escaparHTML(garantia.motocicleta)}</td>
                <td>${escaparHTML(garantia.fechaSolicitud)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(garantia.estado)}
                    </span>
                </td>
                <td>
                    <button
                        type="button"
                        class="dashboard-boton"
                        data-garantia-id="${Number(garantia.id)}">
                        Ver detalles
                    </button>
                </td>
            </tr>
        `).join("");

        estado.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        estado.textContent =
            "No fue posible cargar las garantías.";

        console.error("Error al consultar garantías:", error);
    }
}

// Activar filtros y consulta de detalles
export function iniciarGarantias() {
    const panel = document.querySelector("#garantias-cliente");
    const filtro = document.querySelector("#filtro-garantias");
    const detalles = document.querySelector("#garantia-detalles");

    if (!panel || !filtro || !detalles) return;

    // Filtrar las garantías
    filtro.addEventListener("change", () => {
        detalles.textContent =
            "Selecciona una garantía para consultar sus detalles.";

        cargarGarantias(filtro.value);
    });

    // Mostrar detalles de una garantía
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest("[data-garantia-id]");

        if (!boton || !panel.contains(boton)) return;

        try {
            const garantia = await garantiaService.obtenerPorId(
                boton.dataset.garantiaId
            );

            if (!detalles.isConnected) return;

            detalles.textContent =
                `Folio: ${garantia.folio}. ` +
                `Motocicleta: ${garantia.motocicleta}. ` +
                `Motivo: ${garantia.motivo}. ` +
                `Estado: ${garantia.estado}.`;

        } catch (error) {
            if (!detalles.isConnected) return;

            detalles.textContent =
                "No se encontraron los detalles de la garantía.";
        }
    });

    cargarGarantias();
}
