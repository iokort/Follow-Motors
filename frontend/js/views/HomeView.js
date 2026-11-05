
export function HomeView() {
    return `
        <main id="inicio">

            <!-- Presentación principal -->
            <section class="hero">
                <div class="hero-contenido">
                    <span class="hero-etiqueta">
                        Bienvenido a Follow Motors
                    </span>

                    <h1>
                        Tu próxima aventura
                        <span>comienza aquí</span>
                    </h1>

                    <p>
                        Descubre nuestra colección de motocicletas
                        y encuentra el modelo ideal para ti.
                        En Follow Motors te acompañamos en cada
                        kilómetro.
                    </p>

                    <div class="hero-botones">
                        <a href="#catalogo" class="btn-primary">
                            Explorar catálogo
                        </a>

                        <a href="#servicios" class="btn-secondary">
                            Nuestros servicios
                        </a>
                    </div>
                </div>

                <div class="hero-imagen">
                    <img
                        src="./assets/images/motos/450-sr.png"
                        alt="Motocicleta deportiva 450 SR"
                    >
                </div>
            </section>

            <!-- Características -->
            <section class="beneficios" aria-labelledby="beneficios-titulo">
                <h2 id="beneficios-titulo">
                    ¿Por qué elegir Follow Motors?
                </h2>

                <div class="beneficios-grid">

                    <article class="beneficio-card">
                        <h3>Motocicletas</h3>
                        <p>
                            Explora diferentes modelos
                            y encuentra tu motocicleta ideal.
                        </p>
                    </article>

                    <article class="beneficio-card">
                        <h3>Servicios</h3>
                        <p>
                            Consulta información sobre
                            mantenimiento y servicios de garantía.
                        </p>
                    </article>

                    <article class="beneficio-card">
                        <h3>Seguimiento</h3>
                        <p>
                            Consulta el historial y seguimiento
                            de servicios de tu motocicleta.
                        </p>
                    </article>

                </div>
            </section>

        </main>
    `;
}
