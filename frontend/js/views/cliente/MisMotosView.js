
/* Pantalla de motocicletas del cliente */

// Datos ficticios de las motocicletas del cliente
const misMotos = [
    {
        id: 1,
        marca: "CFMOTO",
        modelo: "450 SR",
        anio: 2025,
        placa: "ABC-123-A",
        kilometraje: 5200,
        imagen: "./assets/images/motos/450-sr.png"
    },
    {
        id: 2,
        marca: "CFMOTO",
        modelo: "700 MT",
        anio: 2024,
        placa: "XYZ-456-B",
        kilometraje: 12800,
        imagen: "./assets/images/motos/700-mt.png"
    }
];

// Construir la pantalla
export function MisMotosView() {
    return `
        <main class="dashboard" id="mis-motos">
            <section class="dashboard-contenedor">

                <header class="dashboard-encabezado">
                    <span class="dashboard-etiqueta">
                        FOLLOW MOTORS
                    </span>

                    <h1>Mis motocicletas</h1>

                    <p>
                        Consulta las motocicletas registradas
                        en tu cuenta.
                    </p>
                </header>

                <div
                    id="mis-motos-estado"
                    class="dashboard-estado"
                    role="status"
                    aria-live="polite">
                </div>

                <div class="catalogo-grid">
                    ${misMotos.map(moto => `
                        <article class="moto-card">

                            <div class="moto-imagen">
                                <img
                                    src="${moto.imagen}"
                                    alt="${moto.marca} ${moto.modelo}"
                                    loading="lazy"
                                >
                            </div>

                            <div class="moto-informacion">
                                <span class="moto-categoria">
                                    ${moto.marca}
                                </span>

                                <h2>${moto.modelo}</h2>

                                <p>Año: ${moto.anio}</p>
                                <p>Placa: ${moto.placa}</p>
                                <p>
                                    Kilometraje:
                                    ${moto.kilometraje.toLocaleString("es-MX")} km
                                </p>

                                <button
                                    type="button"
                                    class="moto-boton"
                                    data-moto-id="${moto.id}">
                                    Ver detalles
                                </button>
                            </div>

                        </article>
                    `).join("")}
                </div>

                <div class="dashboard-acciones">
                    <h2>Gestión de motocicletas</h2>

                    <p>
                        Consulta la información de tus unidades
                        y sus servicios registrados.
                    </p>

                    <a href="#cliente"
                       class="dashboard-boton-secundario"
                       style="display:inline-block;text-decoration:none">
                        Volver a mi panel
                    </a>
                </div>

            </section>
        </main>
    `;
}

// Activar los botones
export function iniciarMisMotos() {
    const contenedor = document.querySelector("#mis-motos");
    const estado = document.querySelector("#mis-motos-estado");

    if (!contenedor || !estado) return;

    contenedor.addEventListener("click", evento => {
        const boton = evento.target.closest("[data-moto-id]");

        if (!boton) return;

        const moto = misMotos.find(
            moto => moto.id === Number(boton.dataset.motoId)
        );

        if (!moto) {
            estado.textContent = "Motocicleta no encontrada.";
            return;
        }

        estado.textContent =
            `${moto.marca} ${moto.modelo} | ` +
            `Placa: ${moto.placa} | ` +
            `Kilometraje: ${moto.kilometraje} km`;
    });
}
