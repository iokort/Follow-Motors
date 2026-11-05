
import { DashboardService } from "../services/DashboardService.js";
import { DashboardCard } from "../components/DashboardCard.js";

// Servicio con datos ficticios
const dashboardService = new DashboardService();

// Construir la pantalla del cliente
export function ClienteView() {
    return `
        <main class="dashboard" id="panel-cliente">

            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Panel del Cliente</h1>

                    <p>
                        Bienvenido a tu espacio personal.
                        Consulta tus motocicletas, citas,
                        servicios y garantías.
                    </p>
                </header>

                <section aria-labelledby="cliente-resumen">
                    <h2 id="cliente-resumen" class="dashboard-subtitulo">
                        Resumen de mi cuenta
                    </h2>

                    <p
                        id="cliente-estado"
                        class="dashboard-estado"
                        role="status"
                        aria-live="polite">
                        Cargando información...
                    </p>

                    <div
                        id="cliente-grid"
                        class="dashboard-grid">
                    </div>
                </section>

                <section class="dashboard-acciones">
                    <h2>Acciones rápidas</h2>

                    <p>
                        Administra tus solicitudes y consulta
                        la información de tus motocicletas.
                    </p>

                    <div class="dashboard-acciones-botones">
                        <button
                            type="button"
                            class="dashboard-boton"
                            data-accion="solicitar-cita">
                            Solicitar cita
                        </button>

                        <button
                            type="button"
                            class="dashboard-boton-secundario"
                            data-accion="consultar-historial">
                            Consultar historial
                        </button>
                    </div>
                </section>

            </section>
        </main>
    `;
}

// Obtener y mostrar los datos
async function cargarDashboardCliente() {
    const contenedor = document.querySelector("#cliente-grid");
    const estado = document.querySelector("#cliente-estado");

    if (!contenedor || !estado) return;

    estado.textContent = "Cargando información...";
    contenedor.innerHTML = "";

    try {
        const datos =
            await dashboardService.obtenerDashboardCliente();

        // Evitar modificar otra pantalla
        if (!contenedor.isConnected) return;

        if (datos.length === 0) {
            estado.textContent =
                "No tienes información registrada.";
            return;
        }

        contenedor.innerHTML = datos.map(DashboardCard).join("");
        estado.textContent = "";

    } catch (error) {
        if (!contenedor.isConnected) return;

        estado.textContent =
            "No se pudo cargar la información del cliente.";

        console.error("Error al cargar el panel:", error);
    }
}

// Activar botones del cliente
export function iniciarCliente() {
    const panel = document.querySelector("#panel-cliente");

    if (!panel) return;

    panel.addEventListener("click", (evento) => {
        const boton = evento.target.closest(
            "[data-modulo], [data-accion]"
        );

        if (!boton) return;

        const opcion =
            boton.dataset.modulo || boton.dataset.accion;

        // Las funciones se implementarán posteriormente
        window.alert(
            `Módulo "${opcion}" en desarrollo. ` +
            "Esta es una demostración del frontend."
        );
    });

    cargarDashboardCliente();
}
const rutas = {

    "mis-motos": "mis-motos",
    "mis-citas": "mis-citas",
    "solicitar-cita": "solicitar-cita",
    "historial": "historial",
    "consultar-historial": "historial",
    "garantias": "garantias"
};
