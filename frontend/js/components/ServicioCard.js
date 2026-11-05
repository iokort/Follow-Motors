
export function ServicioCard(servicio) {
    // Verificar que exista el servicio
    if (!servicio) {
        return "";
    }

    // Preparar los datos para mostrarlos de forma segura
    const escaparHTML = (texto) => {
        return String(texto).replace(/[&<>"']/g, (caracter) => {
            const caracteres = {
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;"
            };

            return caracteres[caracter];
        });
    };

    const nombre = escaparHTML(servicio.nombre);
    const descripcion = escaparHTML(servicio.descripcion);
    const categoria = escaparHTML(servicio.categoria);
    const icono = escaparHTML(servicio.icono);

    const estado = servicio.disponible
        ? "Disponible"
        : "No disponible";

    return `
        <article class="servicio-card">

            <div class="servicio-icono" aria-hidden="true">
                ${icono}
            </div>

            <div class="servicio-informacion">

                <span class="servicio-categoria">
                    ${categoria}
                </span>

                <h3>${nombre}</h3>

                <p class="servicio-descripcion">
                    ${descripcion}
                </p>

                <span class="servicio-estado">
                    ${estado}
                </span>

                <button
                    type="button"
                    class="servicio-boton"
                    data-servicio-id="${Number(servicio.id)}"
                    ${servicio.disponible ? "" : "disabled"}
                >
                    Ver información
                </button>

            </div>

        </article>
    `;
}
