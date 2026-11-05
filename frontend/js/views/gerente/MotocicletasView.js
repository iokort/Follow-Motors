
import { UnidadService } from "../../services/UnidadService.js";

// Servicio de motocicletas registradas
const unidadService = new UnidadService();

// Evitar interpretar los datos como HTML
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

// Construir la pantalla del gerente
export function MotocicletasView() {
    return `
        <main class="dashboard" id="gestion-motocicletas">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Gestión de motocicletas</h1>

                    <p>
                        Consulta las motocicletas registradas
                        y su información general.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="buscar-unidad">
                        Buscar motocicleta:
                    </label>

                    <input
                        type="search"
                        id="buscar-unidad"
                        class="dashboard-buscador"
                        placeholder="Propietario, modelo o placa"
                        autocomplete="off"
                    >
                </div>

                <p
                    id="unidades-estado"
                    class="dashboard-estado"
                    role="status"
                    aria-live="polite">
                </p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Propietario</th>
                                <th scope="col">Placa</th>
                                <th scope="col">Año</th>
                                <th scope="col">Kilometraje</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acción</th>
                            </tr>
                        </thead>

                        <tbody id="unidades-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Detalles de la motocicleta</h2>

                    <div
                        id="unidad-detalles"
                        role="status"
                        aria-live="polite">
                        Selecciona una motocicleta para
                        consultar sus detalles.
                    </div>
                </section>

                <div class="dashboard-acciones">
                    <a
                        href="#gerente"
                        class="dashboard-boton-secundario">
                        Volver al panel del Gerente
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Obtener y mostrar motocicletas
async function cargarUnidades(busqueda = "") {
    const tabla = document.querySelector("#unidades-tabla");
    const estado = document.querySelector("#unidades-estado");

    if (!tabla || !estado) return;

    estado.textContent = "Cargando motocicletas...";
    tabla.innerHTML = "";

    try {
        const unidades = await unidadService.buscar(busqueda);

        // Evitar actualizar una pantalla que ya no existe
        if (!tabla.isConnected) return;

        if (unidades.length === 0) {
            estado.textContent =
                "No se encontraron motocicletas.";
            return;
        }

        tabla.innerHTML = unidades.map(unidad => `
            <tr>
                <td>
                    ${escaparHTML(unidad.marca)}
                    ${escaparHTML(unidad.modelo)}
                </td>

                <td>${escaparHTML(unidad.propietario)}</td>

                <td>${escaparHTML(unidad.placa)}</td>

                <td>${escaparHTML(unidad.anio)}</td>

                <td>
                    ${Number(unidad.kilometraje).toLocaleString("es-MX")} km
                </td>

                <td>
                    <span class="cita-estado">
                        ${escaparHTML(unidad.estado)}
                    </span>
                </td>

                <td>
                    <button
                        type="button"
                        class="dashboard-boton"
                        data-unidad-id="${Number(unidad.id)}">
                        Ver detalles
                    </button>
                </td>
            </tr>
        `).join("");

        estado.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        estado.textContent =
            "No se pudieron cargar las motocicletas.";

        console.error("Error al cargar motocicletas:", error);
    }
}

// Activar búsqueda y consulta de detalles
export function iniciarMotocicletas() {
    const panel = document.querySelector("#gestion-motocicletas");
    const buscador = document.querySelector("#buscar-unidad");
    const detalles = document.querySelector("#unidad-detalles");

    if (!panel || !buscador || !detalles) return;

    // Buscar motocicletas
    buscador.addEventListener("input", () => {
        detalles.textContent =
            "Selecciona una motocicleta para consultar sus detalles.";

        cargarUnidades(buscador.value);
    });

    // Consultar motocicleta seleccionada
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest("[data-unidad-id]");

        if (!boton || !panel.contains(boton)) return;

        try {
            const unidad = await unidadService.obtenerPorId(
                boton.dataset.unidadId
            );

            if (!detalles.isConnected) return;

            // Crear la imagen mediante atributos DOM
            detalles.replaceChildren();

            const imagen = document.createElement("img");
            imagen.src = unidad.imagen;
            imagen.alt = `${unidad.marca} ${unidad.modelo}`;
            imagen.className = "unidad-detalle-imagen";

            const informacion = document.createElement("p");
            informacion.textContent =
                `Propietario: ${unidad.propietario}. ` +
                `Motocicleta: ${unidad.marca} ${unidad.modelo}. ` +
                `Placa: ${unidad.placa}. ` +
                `Año: ${unidad.anio}. ` +
                `Kilometraje: ${unidad.kilometraje} km. ` +
                `Estado: ${unidad.estado}.`;

            detalles.append(imagen, informacion);

        } catch (error) {
            if (!detalles.isConnected) return;

            detalles.textContent =
                "No se encontró la motocicleta seleccionada.";
        }
    });

    cargarUnidades();
}
