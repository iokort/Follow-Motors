
import { GestionGarantiasService } from "../../services/GestionGarantiasService.js";

// Servicio de garantias
const garantiaService = new GestionGarantiasService();

// Evitar interpretar datos como HTML
function escaparHTML(valor) {
    return String(valor).replace(/[&<>"']/g, caracter => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    }[caracter]));
}

// Construir la pantalla
export function GestionGarantiasView() {
    return `
        <main class="dashboard" id="gestion-garantias">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Gestión de garantías</h1>

                    <p>
                        Consulta las solicitudes de garantía
                        y administra su estado.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-garantias-gerente">
                        Filtrar por estado:
                    </label>

                    <select
                        id="filtro-garantias-gerente"
                        class="dashboard-buscador"
                    >
                        <option value="Todas">Todas</option>
                        <option value="Pendiente">Pendientes</option>
                        <option value="En revisión">En revisión</option>
                        <option value="Aprobada">Aprobadas</option>
                        <option value="Rechazada">Rechazadas</option>
                    </select>
                </div>

                <p
                    id="garantias-gerente-estado"
                    role="status"
                    aria-live="polite"
                ></p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Cliente</th>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Fecha</th>
                                <th scope="col">Motivo</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acciones</th>
                            </tr>
                        </thead>

                        <tbody id="garantias-gerente-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Detalles de la garantía</h2>

                    <p
                        id="garantia-gerente-detalles"
                        role="status"
                        aria-live="polite"
                    >
                        Selecciona una solicitud para consultar sus detalles.
                    </p>
                </section>

                <div class="dashboard-acciones">
                    <a
                        href="#gerente"
                        class="dashboard-boton-secundario"
                    >
                        Volver al panel del Gerente
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Obtener botones segun el estado de la garantia
function obtenerAcciones(garantia) {
    const id = Number(garantia.id);

    let botones = `
        <button
            type="button"
            class="dashboard-boton-secundario"
            data-garantia-id="${id}"
            data-accion-garantia="detalles"
        >
            Ver detalles
        </button>
    `;

    if (garantia.estado === "Pendiente") {
        botones += `
            <button
                type="button"
                class="dashboard-boton"
                data-garantia-id="${id}"
                data-accion-garantia="estado"
                data-nuevo-estado="En revisión"
            >
                Iniciar revisión
            </button>
        `;
    }

    if (garantia.estado === "En revisión") {
        botones += `
            <button
                type="button"
                class="dashboard-boton"
                data-garantia-id="${id}"
                data-accion-garantia="estado"
                data-nuevo-estado="Aprobada"
            >
                Aprobar
            </button>

            <button
                type="button"
                class="dashboard-boton-secundario"
                data-garantia-id="${id}"
                data-accion-garantia="estado"
                data-nuevo-estado="Rechazada"
            >
                Rechazar
            </button>
        `;
    }

    return botones;
}

// Mostrar solicitudes en la tabla
async function cargarGarantias(estado = "Todas") {
    const tabla = document.querySelector("#garantias-gerente-tabla");
    const mensaje = document.querySelector("#garantias-gerente-estado");

    if (!tabla || !mensaje) return;

    mensaje.textContent = "Cargando garantías...";
    tabla.innerHTML = "";

    try {
        const garantias = await garantiaService.filtrarPorEstado(estado);

        if (!tabla.isConnected) return;

        if (garantias.length === 0) {
            mensaje.textContent =
                "No se encontraron garantías con ese estado.";
            return;
        }

        tabla.innerHTML = garantias.map(garantia => `
            <tr>
                <td>${escaparHTML(garantia.cliente)}</td>
                <td>${escaparHTML(garantia.motocicleta)}</td>
                <td>${escaparHTML(garantia.fecha)}</td>
                <td>${escaparHTML(garantia.motivo)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(garantia.estado)}
                    </span>
                </td>
                <td>
                    <div class="gestion-citas-acciones">
                        ${obtenerAcciones(garantia)}
                    </div>
                </td>
            </tr>
        `).join("");

        mensaje.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        mensaje.textContent =
            "No se pudieron cargar las garantías.";

        console.error("Error al cargar garantías:", error);
    }
}

// Inicializar filtros y botones
export function iniciarGestionGarantias() {
    const panel = document.querySelector("#gestion-garantias");
    const filtro = document.querySelector("#filtro-garantias-gerente");
    const mensaje = document.querySelector("#garantias-gerente-estado");
    const detalles = document.querySelector("#garantia-gerente-detalles");

    if (!panel || !filtro || !mensaje || !detalles) return;

    // Filtrar solicitudes
    filtro.addEventListener("change", () => {
        detalles.textContent =
            "Selecciona una solicitud para consultar sus detalles.";

        cargarGarantias(filtro.value);
    });

    // Administrar las solicitudes
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest(
            "[data-garantia-id][data-accion-garantia]"
        );

        if (!boton || !panel.contains(boton)) return;

        const id = boton.dataset.garantiaId;
        const accion = boton.dataset.accionGarantia;

        boton.disabled = true;

        try {
            const garantia = await garantiaService.obtenerPorId(id);

            if (accion === "detalles") {
                if (!detalles.isConnected) return;

                detalles.textContent =
                    `Cliente: ${garantia.cliente}. ` +
                    `Motocicleta: ${garantia.motocicleta}. ` +
                    `Fecha: ${garantia.fecha}. ` +
                    `Motivo: ${garantia.motivo}. ` +
                    `Descripción: ${garantia.descripcion}. ` +
                    `Estado: ${garantia.estado}.`;

                return;
            }

            if (accion === "estado") {
                const nuevoEstado = boton.dataset.nuevoEstado;

                if (
                    nuevoEstado === "Aprobada" ||
                    nuevoEstado === "Rechazada"
                ) {
                    const confirmado = window.confirm(
                        `¿Deseas cambiar la solicitud de ` +
                        `${garantia.cliente} a ${nuevoEstado}?`
                    );

                    if (!confirmado) return;
                }

                await garantiaService.cambiarEstado(id, nuevoEstado);

                if (!panel.isConnected) return;

                await cargarGarantias(filtro.value);

                if (detalles.isConnected) {
                    detalles.textContent =
                        `La garantía de ${garantia.cliente} ` +
                        `cambió a: ${nuevoEstado}.`;
                }

                if (mensaje.isConnected) {
                    mensaje.textContent =
                        "Estado actualizado correctamente.";
                }
            }

        } catch (error) {
            if (mensaje.isConnected) {
                mensaje.textContent =
                    "No se pudo realizar la operación.";
            }

            console.error("Error al gestionar garantía:", error);

        } finally {
            boton.disabled = false;
        }
    });

    cargarGarantias();
}
