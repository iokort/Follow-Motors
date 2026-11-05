
import { GestionServiciosService } from "../../services/GestionServiciosService.js";

// Servicio de gestion
const servicioService = new GestionServiciosService();

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

// Construir pantalla
export function GestionServiciosView() {
    return `
        <main class="dashboard" id="gestion-servicios">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Gestión de servicios</h1>

                    <p>
                        Consulta los servicios de mantenimiento
                        y administra su estado.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-servicios-gerente">
                        Filtrar por estado:
                    </label>

                    <select
                        id="filtro-servicios-gerente"
                        class="dashboard-buscador"
                    >
                        <option value="Todos">Todos</option>
                        <option value="Pendiente">Pendientes</option>
                        <option value="En proceso">En proceso</option>
                        <option value="Completado">Completados</option>
                        <option value="Cancelado">Cancelados</option>
                    </select>
                </div>

                <p
                    id="servicios-gerente-estado"
                    role="status"
                    aria-live="polite"
                ></p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Cliente</th>
                                <th scope="col">Motocicleta</th>
                                <th scope="col">Servicio</th>
                                <th scope="col">Fecha</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acciones</th>
                            </tr>
                        </thead>

                        <tbody id="servicios-gerente-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Detalles del servicio</h2>

                    <p
                        id="servicio-gerente-detalles"
                        role="status"
                        aria-live="polite"
                    >
                        Selecciona un servicio para ver sus detalles.
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

// Botones permitidos segun el estado
function obtenerAcciones(servicio) {
    const id = Number(servicio.id);

    let acciones = `
        <button
            type="button"
            class="dashboard-boton-secundario"
            data-servicio-id="${id}"
            data-accion-servicio="detalles"
        >
            Ver detalles
        </button>
    `;

    if (servicio.estado === "Pendiente") {
        acciones += `
            <button
                type="button"
                class="dashboard-boton"
                data-servicio-id="${id}"
                data-accion-servicio="estado"
                data-nuevo-estado="En proceso"
            >
                Iniciar
            </button>

            <button
                type="button"
                class="dashboard-boton-secundario"
                data-servicio-id="${id}"
                data-accion-servicio="estado"
                data-nuevo-estado="Cancelado"
            >
                Cancelar
            </button>
        `;
    }

    if (servicio.estado === "En proceso") {
        acciones += `
            <button
                type="button"
                class="dashboard-boton"
                data-servicio-id="${id}"
                data-accion-servicio="estado"
                data-nuevo-estado="Completado"
            >
                Completar
            </button>
        `;
    }

    return acciones;
}

// Mostrar servicios
async function cargarServicios(estado = "Todos") {
    const tabla = document.querySelector("#servicios-gerente-tabla");
    const mensaje = document.querySelector("#servicios-gerente-estado");

    if (!tabla || !mensaje) return;

    mensaje.textContent = "Cargando servicios...";
    tabla.innerHTML = "";

    try {
        const servicios = await servicioService.filtrarPorEstado(estado);

        if (!tabla.isConnected) return;

        if (servicios.length === 0) {
            mensaje.textContent =
                "No se encontraron servicios con ese estado.";
            return;
        }

        tabla.innerHTML = servicios.map(servicio => `
            <tr>
                <td>${escaparHTML(servicio.cliente)}</td>
                <td>${escaparHTML(servicio.motocicleta)}</td>
                <td>${escaparHTML(servicio.tipoServicio)}</td>
                <td>${escaparHTML(servicio.fecha)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(servicio.estado)}
                    </span>
                </td>
                <td>
                    <div class="gestion-citas-acciones">
                        ${obtenerAcciones(servicio)}
                    </div>
                </td>
            </tr>
        `).join("");

        mensaje.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        mensaje.textContent =
            "No se pudieron cargar los servicios.";

        console.error("Error al cargar servicios:", error);
    }
}

// Inicializar eventos de la pantalla
export function iniciarGestionServicios() {
    const panel = document.querySelector("#gestion-servicios");
    const filtro = document.querySelector("#filtro-servicios-gerente");
    const mensaje = document.querySelector("#servicios-gerente-estado");
    const detalles = document.querySelector("#servicio-gerente-detalles");

    if (!panel || !filtro || !mensaje || !detalles) return;

    // Filtrar servicios
    filtro.addEventListener("change", () => {
        detalles.textContent =
            "Selecciona un servicio para ver sus detalles.";

        cargarServicios(filtro.value);
    });

    // Gestionar los botones
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest(
            "[data-servicio-id][data-accion-servicio]"
        );

        if (!boton || !panel.contains(boton)) return;

        const id = boton.dataset.servicioId;
        const accion = boton.dataset.accionServicio;

        boton.disabled = true;

        try {
            const servicio = await servicioService.obtenerPorId(id);

            if (accion === "detalles") {
                if (!detalles.isConnected) return;

                detalles.textContent =
                    `Cliente: ${servicio.cliente}. ` +
                    `Motocicleta: ${servicio.motocicleta}. ` +
                    `Servicio: ${servicio.tipoServicio}. ` +
                    `Fecha: ${servicio.fecha}. ` +
                    `Descripción: ${servicio.descripcion}. ` +
                    `Estado: ${servicio.estado}.`;

                return;
            }

            if (accion === "estado") {
                const nuevoEstado = boton.dataset.nuevoEstado;

                if (nuevoEstado === "Cancelado") {
                    const confirmado = window.confirm(
                        `¿Deseas cancelar el servicio de ${servicio.cliente}?`
                    );

                    if (!confirmado) return;
                }

                await servicioService.cambiarEstado(id, nuevoEstado);

                if (!panel.isConnected) return;

                await cargarServicios(filtro.value);

                if (detalles.isConnected) {
                    detalles.textContent =
                        `El servicio de ${servicio.cliente} ` +
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

            console.error("Error de gestión:", error);

        } finally {
            boton.disabled = false;
        }
    });

    cargarServicios();
}
