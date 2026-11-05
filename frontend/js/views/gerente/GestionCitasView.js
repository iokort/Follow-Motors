
import { GestionCitasService } from "../../services/GestionCitasService.js";

// Servicio para gestionar las citas ficticias
const citasService = new GestionCitasService();

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

// Construir pantalla de Gestion de Citas
export function GestionCitasView() {
    return `
        <main class="dashboard" id="gestion-citas">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Gestión de citas</h1>

                    <p>
                        Consulta y administra las citas
                        de servicio de los clientes.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-citas-gerente">
                        Filtrar por estado:
                    </label>

                    <select
                        id="filtro-citas-gerente"
                        class="dashboard-buscador"
                    >
                        <option value="Todas">Todas</option>
                        <option value="Pendiente">Pendientes</option>
                        <option value="Confirmada">Confirmadas</option>
                        <option value="En proceso">En proceso</option>
                        <option value="Completada">Completadas</option>
                        <option value="Cancelada">Canceladas</option>
                    </select>
                </div>

                <p
                    id="citas-gerente-estado"
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
                                <th scope="col">Hora</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acciones</th>
                            </tr>
                        </thead>

                        <tbody id="citas-gerente-tabla"></tbody>
                    </table>
                </div>

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

// Crear botones segun el estado de la cita
function obtenerAcciones(cita) {
    if (cita.estado === "Pendiente") {
        return `
            <button
                type="button"
                class="dashboard-boton"
                data-cita-id="${Number(cita.id)}"
                data-nuevo-estado="Confirmada"
            >
                Confirmar
            </button>

            <button
                type="button"
                class="dashboard-boton-secundario"
                data-cita-id="${Number(cita.id)}"
                data-nuevo-estado="Cancelada"
            >
                Cancelar
            </button>
        `;
    }

    if (cita.estado === "Confirmada") {
        return `
            <button
                type="button"
                class="dashboard-boton"
                data-cita-id="${Number(cita.id)}"
                data-nuevo-estado="En proceso"
            >
                Iniciar servicio
            </button>

            <button
                type="button"
                class="dashboard-boton-secundario"
                data-cita-id="${Number(cita.id)}"
                data-nuevo-estado="Cancelada"
            >
                Cancelar
            </button>
        `;
    }

    if (cita.estado === "En proceso") {
        return `
            <button
                type="button"
                class="dashboard-boton"
                data-cita-id="${Number(cita.id)}"
                data-nuevo-estado="Completada"
            >
                Completar
            </button>
        `;
    }

    return `<span>Sin acciones disponibles</span>`;
}

// Mostrar las citas en la tabla
async function cargarCitas(estado = "Todas") {
    const tabla = document.querySelector("#citas-gerente-tabla");
    const mensaje = document.querySelector("#citas-gerente-estado");

    if (!tabla || !mensaje) return;

    mensaje.textContent = "Cargando citas...";
    tabla.innerHTML = "";

    try {
        const citas = await citasService.filtrarPorEstado(estado);

        if (!tabla.isConnected) return;

        if (citas.length === 0) {
            mensaje.textContent =
                "No se encontraron citas con ese estado.";
            return;
        }

        tabla.innerHTML = citas.map(cita => `
            <tr>
                <td>${escaparHTML(cita.cliente)}</td>
                <td>${escaparHTML(cita.motocicleta)}</td>
                <td>${escaparHTML(cita.servicio)}</td>
                <td>${escaparHTML(cita.fecha)}</td>
                <td>${escaparHTML(cita.hora)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(cita.estado)}
                    </span>
                </td>
                <td>
                    <div class="gestion-citas-acciones">
                        ${obtenerAcciones(cita)}
                    </div>
                </td>
            </tr>
        `).join("");

        mensaje.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        mensaje.textContent =
            "No se pudieron cargar las citas.";

        console.error("Error al cargar citas:", error);
    }
}

// Activar filtros y botones
export function iniciarGestionCitas() {
    const panel = document.querySelector("#gestion-citas");
    const filtro = document.querySelector("#filtro-citas-gerente");
    const mensaje = document.querySelector("#citas-gerente-estado");

    if (!panel || !filtro || !mensaje) return;

    // Filtrar citas por estado
    filtro.addEventListener("change", () => {
        cargarCitas(filtro.value);
    });

    // Cambiar estado de una cita
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest(
            "[data-cita-id][data-nuevo-estado]"
        );

        if (!boton || !panel.contains(boton)) return;

        const id = boton.dataset.citaId;
        const nuevoEstado = boton.dataset.nuevoEstado;

        boton.disabled = true;

        try {
            const cita = await citasService.obtenerPorId(id);

            // Confirmar antes de cancelar
            if (nuevoEstado === "Cancelada") {
                const confirmado = window.confirm(
                    `¿Deseas cancelar la cita de ${cita.cliente}?`
                );

                if (!confirmado) return;
            }

            await citasService.cambiarEstado(id, nuevoEstado);

            if (!panel.isConnected) return;

            await cargarCitas(filtro.value);

            if (mensaje.isConnected) {
                mensaje.textContent =
                    `Estado de la cita actualizado a: ${nuevoEstado}.`;
            }

        } catch (error) {
            if (mensaje.isConnected) {
                mensaje.textContent =
                    "No se pudo actualizar la cita.";
            }

            console.error("Error al actualizar cita:", error);

        } finally {
            boton.disabled = false;
        }
    });

    cargarCitas();
}
