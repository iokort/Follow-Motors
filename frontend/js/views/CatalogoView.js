
import { MotoService } from "../services/MotoService.js";
import { MotoCard } from "../components/MotoCard.js";

// Servicio de motocicletas
const motoService = new MotoService();

// Estructura de la pantalla
export function CatalogoView() {
    return `
        <main class="catalogo" id="catalogo">
            <section class="catalogo-contenedor">

                <header class="catalogo-encabezado">
                    <span class="catalogo-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Catálogo de motocicletas</h1>

                    <p>
                        Explora nuestros modelos y encuentra
                        la motocicleta ideal para ti.
                    </p>
                </header>

                <div class="catalogo-filtros">
                    <label for="filtro-categoria">
                        Filtrar por categoría:
                    </label>

                    <select id="filtro-categoria">
                        <option value="Todas">Todas</option>
                        <option value="Deportiva">Deportiva</option>
                        <option value="Naked">Naked</option>
                        <option value="Aventura">Aventura</option>
                        <option value="Retro">Retro</option>
                        <option value="Doble propósito">
                            Doble propósito
                        </option>
                    </select>
                </div>

                <div
                    id="catalogo-estado"
                    role="status"
                    aria-live="polite">
                </div>

                <div
                    id="catalogo-grid"
                    class="catalogo-grid">
                </div>

            </section>
        </main>
    `;
}

// Mostrar las motocicletas
async function cargarMotos(categoria = "Todas") {
    const contenedor = document.querySelector("#catalogo-grid");
    const estado = document.querySelector("#catalogo-estado");

    if (!contenedor || !estado) return;

    estado.textContent = "Cargando motocicletas...";
    contenedor.innerHTML = "";

    try {
        const motos = await motoService.filtrarPorCategoria(categoria);

        if (motos.length === 0) {
            estado.textContent =
                "No hay motocicletas en esta categoría.";
            return;
        }

        contenedor.innerHTML = motos.map(MotoCard).join("");
        estado.textContent = "";

    } catch (error) {
        estado.textContent =
            "Ocurrió un error al cargar el catálogo.";
        console.error(error);
    }
}

// Activar los eventos de la pantalla
export function iniciarCatalogo() {
    const filtro = document.querySelector("#filtro-categoria");
    const contenedor = document.querySelector("#catalogo-grid");

    if (!filtro || !contenedor) return;

    filtro.addEventListener("change", () => {
        cargarMotos(filtro.value);
    });

    contenedor.addEventListener("click", (evento) => {
        const boton = evento.target.closest(".moto-boton");

        if (!boton) return;

        const id = Number(boton.dataset.motoId);

        // Funcionalidad de demostración
        window.alert(
            `Seleccionaste la motocicleta número ${id}.`
        );
    });

    cargarMotos();
}
