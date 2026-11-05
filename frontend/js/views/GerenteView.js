
import { DashboardService } from "../services/DashboardService.js";
import { DashboardCard } from "../components/DashboardCard.js";

// Servicio con datos ficticios
const dashboardService = new DashboardService();

// Construir pantalla del gerente
export function GerenteView() {
    return `
        <main class="dashboard" id="panel-gerente">

            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Panel del Gerente</h1>

                    <p>
                        Administra y consulta la información
                        general de Follow Motors.
                    </p>
                </header>

                <section aria-labelledby="gerente-resumen">

                    <h2 id="gerente-resumen"
                        class="dashboard-subtitulo">
                        Resumen general
                    </h2>

                    <p
                        id="gerente-estado"
                        class="dashboard-estado"
                        role="status"
                        aria-live="polite">
                        Cargando información...
                    </p>

                    <div
                        id="gerente-grid"
                        class="dashboard-grid">
                    </div>

                </section>

                <section class="dashboard-acciones">

                    <h2>Acciones administrativas</h2>

                    <p>
                        Accede a los módulos de gestión
                        de clientes, unidades y servicios.
                    </p>

                    <div class="dashboard-acciones-botones">

                        <button
                            type="button"
                            class="dashboard-boton"
                            data-accion="gestionar-clientes">
                            Gestionar clientes
                        </button>

                        <button
                            type="button"
                            class="dashboard-boton-secundario"
                            data-accion="gestionar-citas">
                            Gestionar citas
                        </button>

                    </div>
                </section>

            </section>
        </main>
    `;
}

// Obtener y mostrar los datos del gerente
async function cargarDashboardGerente() {
    const contenedor = document.querySelector("#gerente-grid");
    const estado = document.querySelector("#gerente-estado");

    if (!contenedor || !estado) return;

    estado.textContent = "Cargando información...";
    contenedor.innerHTML = "";

    try {
        const datos =
            await dashboardService.obtenerDashboardGerente();

        // Evitar actualizar una pantalla diferente
        if (!contenedor.isConnected) return;

        if (datos.length === 0) {
            estado.textContent =
                "No hay información disponible.";
            return;
        }

        contenedor.innerHTML = datos.map(DashboardCard).join("");
        estado.textContent = "";

    } catch (error) {
        if (!contenedor.isConnected) return;

        estado.textContent =
            "No se pudo cargar la información del gerente.";

        console.error("Error al cargar el panel:", error);
    }
}

// Activar botones del gerente

export function iniciarGerente() {
    const panel = document.querySelector("#panel-gerente");

    if (!panel) return;

    // Rutas de los módulos del gerente
    const rutas = {
        "clientes": "gestion-clientes",
        "gestionar-clientes": "gestion-clientes",
        "unidades": "gestion-motocicletas",
        "citas": "gestion-citas",
    "gestionar-citas": "gestion-citas",
    "servicios": "gestion-servicios",
    "garantias": "gestion-garantias"
    };

    panel.addEventListener("click", (evento) => {
        const boton = evento.target.closest(
            "[data-modulo], [data-accion]"
        );

        if (!boton || !panel.contains(boton)) return;

        const opcion =
            boton.dataset.modulo || boton.dataset.accion;

        if (rutas[opcion]) {
            window.location.hash = rutas[opcion];
        } else {
            window.alert(
                "Este módulo todavía está en desarrollo."
            );
        }
    });

    cargarDashboardGerente();
}
