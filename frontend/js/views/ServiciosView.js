
import { ServicioService } from "../services/ServicioService.js";
import { ServicioCard } from "../components/ServicioCard.js";

// Servicio encargado de obtener los datos
const servicioService = new ServicioService();

// Construir la pantalla
export function ServiciosView() {
    return `
        <main class="servicios" id="servicios">
            <section class="servicios-contenedor">

                <header class="servicios-encabezado">
                    <span class="servicios-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Nuestros servicios</h1>

                    <p>
                        Conoce nuestros servicios de mantenimiento,
                        diagnóstico, garantía y seguimiento
                        para tu motocicleta.
                    </p>
                </header>

                <div class="servicios-filtros">
                    <label for="filtro-servicio">
                        Filtrar por categoría:
                    </label>

                    <select id="filtro-servicio">
                        <option value="Todas">Todas</option>
                        <option value="Mantenimiento">Mantenimiento</option>
                        <option value="Seguridad">Seguridad</option>
                        <option value="Garantía">Garantía</option>
                        <option value="Diagnóstico">Diagnóstico</option>
                        <option value="Seguimiento">Seguimiento</option>
                    </select>
                </div>

                <p
                    id="servicios-estado"
                    role="status"
                    aria-live="polite">
                </p>

                <div
                    id="servicios-grid"
                    class="servicios-grid">
                </div>

            </section>
        </main>
    `;
}

// Obtener y mostrar servicios
async function cargarServicios(categoria = "Todas") {
    const contenedor = document.querySelector("#servicios-grid");
    const estado = document.querySelector("#servicios-estado");

    if (!contenedor || !estado) return;

    estado.textContent = "Cargando servicios...";
    contenedor.innerHTML = "";

    try {
        const servicios =
            await servicioService.filtrarPorCategoria(categoria);

        // Evitar actualizar una pantalla que ya cambió
        if (!contenedor.isConnected) return;

        if (servicios.length === 0) {
            estado.textContent =
                "No hay servicios en esta categoría.";
            return;
        }

        contenedor.innerHTML = servicios.map(ServicioCard).join("");
        estado.textContent = "";

    } catch (error) {
        if (!contenedor.isConnected) return;

        estado.textContent =
            "No se pudieron cargar los servicios.";

        console.error("Error al consultar servicios:", error);
    }
}

// Activar los eventos de la pantalla
export function iniciarServicios() {
    const filtro = document.querySelector("#filtro-servicio");
    const contenedor = document.querySelector("#servicios-grid");

    if (!filtro || !contenedor) return;

    // Cambiar categoría
    filtro.addEventListener("change", () => {
        cargarServicios(filtro.value);
    });

    // Mostrar información del servicio seleccionado
    contenedor.addEventListener("click", async (evento) => {
        const boton = evento.target.closest(".servicio-boton");

        if (!boton) return;

        const id = Number(boton.dataset.servicioId);

        try {
            const servicio = await servicioService.obtenerPorId(id);

            window.alert(
                `${servicio.nombre}\n\n${servicio.descripcion}`
            );

        } catch (error) {
            window.alert("No se encontró el servicio seleccionado.");
        }
    });

    // Mostrar todos los servicios inicialmente
    cargarServicios();
}
