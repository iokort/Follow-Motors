
export function MotoCard(moto) {
    // Validar que exista información
    if (!moto) {
        return "";
    }

    // Mostrar disponibilidad
    const disponibilidad = moto.disponible
        ? "Disponible"
        : "No disponible";

    const claseEstado = moto.disponible
        ? "disponible"
        : "no-disponible";

    return `
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
                    ${moto.categoria}
                </span>

                <h3>${moto.marca} ${moto.modelo}</h3>

                <p class="moto-estado ${claseEstado}">
                    ${disponibilidad}
                </p>

                <button
                    type="button"
                    class="moto-boton"
                    data-moto-id="${moto.id}"
                    aria-label="Ver detalles de ${moto.modelo}"
                >
                    Ver detalles
                </button>

            </div>

        </article>
    `;
}
