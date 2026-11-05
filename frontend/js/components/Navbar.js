
export function Navbar() {
    return `
        <header class="navbar">
            <nav class="navbar-container" aria-label="Navegación principal">

                <a href="#inicio" class="navbar-logo">
                    <img
                        src="./assets/images/branding/logo.png"
                        alt="Logo de Follow Motors"
                    >
                    <span>Follow Motors</span>
                </a>

                <button
                    class="navbar-toggle"
                    type="button"
                    aria-label="Abrir menú"
                    aria-expanded="false"
                    aria-controls="navbar-links"
                >
                    ☰
                </button>

                <div class="navbar-links" id="navbar-links">
                    <a href="#inicio">Inicio</a>
                    <a href="#catalogo">Catálogo</a>
                    <a href="#servicios">Servicios</a>
                    <a href="#nosotros">Nosotros</a>
                    <a href="#login" class="navbar-login">
                        Iniciar sesión
                    </a>
                </div>

            </nav>
        </header>
    `;
}

export function iniciarNavbar() {
    const boton = document.querySelector(".navbar-toggle");
    const enlaces = document.querySelector(".navbar-links");

    if (!boton || !enlaces) return;

    boton.addEventListener("click", () => {
        const abierto = enlaces.classList.toggle("active");

        boton.setAttribute("aria-expanded", String(abierto));
        boton.setAttribute(
            "aria-label",
            abierto ? "Cerrar menú" : "Abrir menú"
        );
    });

    enlaces.addEventListener("click", (evento) => {
        if (evento.target.closest("a")) {
            enlaces.classList.remove("active");
            boton.setAttribute("aria-expanded", "false");
            boton.setAttribute("aria-label", "Abrir menú");
        }
    });
}
