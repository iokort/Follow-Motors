
/* Tarjeta reutilizable para Cliente y Gerente */

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

// Construir tarjeta del dashboard
export function DashboardCard(dato) {
    if (!dato) {
        return "";
    }

    const id = escaparHTML(dato.id);
    const titulo = escaparHTML(dato.titulo);
    const descripcion = escaparHTML(dato.descripcion);
    const cantidad = escaparHTML(dato.cantidad);
    const icono = escaparHTML(dato.icono);

    return `
        <article class="dashboard-card">

            <div class="dashboard-card-encabezado">

                <span class="dashboard-icono"
                      aria-hidden="true">
                    ${icono}
                </span>

                <span class="dashboard-cantidad">
                    ${cantidad}
                </span>

            </div>

            <h2>${titulo}</h2>

            <p>${descripcion}</p>

            <button
                type="button"
                class="dashboard-boton"
                data-modulo="${id}">
                Ver módulo
            </button>

        </article>
    `;
}
