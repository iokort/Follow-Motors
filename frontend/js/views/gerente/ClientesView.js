
import { ClienteService } from "../../services/ClienteService.js";

// Servicio de clientes ficticios
const clienteService = new ClienteService();

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

// Construir pantalla de gestión de clientes
export function ClientesView() {
    return `
        <main class="dashboard" id="gestion-clientes">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Gestión de clientes</h1>

                    <p>
                        Consulta los clientes registrados
                        y revisa su información.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="buscar-cliente">
                        Buscar cliente:
                    </label>

                    <input
                        type="search"
                        id="buscar-cliente"
                        placeholder="Buscar por nombre"
                        autocomplete="off"
                    >
                </div>

                <p
                    id="clientes-estado"
                    class="dashboard-estado"
                    role="status"
                    aria-live="polite">
                </p>

                <div class="tabla-contenedor">
                    <table class="tabla-citas">
                        <thead>
                            <tr>
                                <th scope="col">Nombre</th>
                                <th scope="col">Correo</th>
                                <th scope="col">Teléfono</th>
                                <th scope="col">Motocicletas</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Acción</th>
                            </tr>
                        </thead>

                        <tbody id="clientes-tabla"></tbody>
                    </table>
                </div>

                <section class="dashboard-acciones">
                    <h2>Información del cliente</h2>

                    <p
                        id="cliente-detalles"
                        role="status"
                        aria-live="polite">
                        Selecciona un cliente para consultar sus datos.
                    </p>
                </section>

                <div class="dashboard-acciones">
                    <a href="#gerente"
                       class="dashboard-boton-secundario">
                        Volver al panel del Gerente
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Obtener y mostrar clientes
async function cargarClientes(nombre = "") {
    const tabla = document.querySelector("#clientes-tabla");
    const estado = document.querySelector("#clientes-estado");

    if (!tabla || !estado) return;

    estado.textContent = "Cargando clientes...";
    tabla.innerHTML = "";

    try {
        const clientes = await clienteService.buscarPorNombre(nombre);

        if (!tabla.isConnected) return;

        if (clientes.length === 0) {
            estado.textContent = "No se encontraron clientes.";
            return;
        }

        tabla.innerHTML = clientes.map(cliente => `
            <tr>
                <td>${escaparHTML(cliente.nombre)}</td>
                <td>${escaparHTML(cliente.correo)}</td>
                <td>${escaparHTML(cliente.telefono)}</td>
                <td>${escaparHTML(cliente.motocicletas)}</td>
                <td>
                    <span class="cita-estado">
                        ${escaparHTML(cliente.estado)}
                    </span>
                </td>
                <td>
                    <button
                        type="button"
                        class="dashboard-boton"
                        data-cliente-id="${Number(cliente.id)}">
                        Ver detalles
                    </button>
                </td>
            </tr>
        `).join("");

        estado.textContent = "";

    } catch (error) {
        if (!tabla.isConnected) return;

        estado.textContent = "No se pudieron cargar los clientes.";
        console.error("Error al cargar clientes:", error);
    }
}

// Activar búsqueda y detalles
export function iniciarClientes() {
    const panel = document.querySelector("#gestion-clientes");
    const buscador = document.querySelector("#buscar-cliente");
    const detalles = document.querySelector("#cliente-detalles");

    if (!panel || !buscador || !detalles) return;

    // Buscar mientras se escribe
    buscador.addEventListener("input", () => {
        cargarClientes(buscador.value);
    });

    // Consultar información del cliente
    panel.addEventListener("click", async (evento) => {
        const boton = evento.target.closest("[data-cliente-id]");

        if (!boton || !panel.contains(boton)) return;

        try {
            const cliente = await clienteService.obtenerPorId(
                boton.dataset.clienteId
            );

            if (!detalles.isConnected) return;

            detalles.textContent =
                `Nombre: ${cliente.nombre}. ` +
                `Correo: ${cliente.correo}. ` +
                `Teléfono: ${cliente.telefono}. ` +
                `Motocicletas: ${cliente.motocicletas}. ` +
                `Estado: ${cliente.estado}.`;

        } catch (error) {
            if (!detalles.isConnected) return;

            detalles.textContent = "Cliente no encontrado.";
        }
    });

    cargarClientes();
}
