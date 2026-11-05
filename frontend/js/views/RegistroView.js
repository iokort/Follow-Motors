
/* Pantalla de registro de clientes */

export function RegistroView() {
    return `
        <main class="auth">
            <section class="auth-contenedor"
                     aria-labelledby="registro-titulo">

                <div class="auth-encabezado">
                    <img
                        src="./assets/images/branding/logo.png"
                        alt="Follow Motors"
                        class="auth-logo"
                    >

                    <h1 id="registro-titulo">
                        Crear cuenta
                    </h1>

                    <p>
                        Regístrate para acceder a los
                        servicios de Follow Motors.
                    </p>
                </div>

                <form id="registro-form">

                    <div class="auth-campo">
                        <label for="registro-nombre">
                            Nombre completo
                        </label>

                        <input
                            type="text"
                            id="registro-nombre"
                            name="nombre"
                            autocomplete="name"
                            minlength="3"
                            maxlength="100"
                            required
                        >
                    </div>

                    <div class="auth-campo">
                        <label for="registro-correo">
                            Correo electrónico
                        </label>

                        <input
                            type="email"
                            id="registro-correo"
                            name="correo"
                            autocomplete="email"
                            required
                        >
                    </div>

                    <div class="auth-campo">
                        <label for="registro-telefono">
                            Teléfono
                        </label>

                        <input
                            type="tel"
                            id="registro-telefono"
                            name="telefono"
                            autocomplete="tel"
                            pattern="[0-9]{10}"
                            maxlength="10"
                            placeholder="10 dígitos"
                            title="Ingresa un teléfono de 10 dígitos"
                            required
                        >
                    </div>

                    <div class="auth-campo">
                        <label for="registro-password">
                            Contraseña
                        </label>

                        <input
                            type="password"
                            id="registro-password"
                            name="password"
                            minlength="8"
                            autocomplete="new-password"
                            required
                        >
                    </div>

                    <div class="auth-campo">
                        <label for="registro-confirmar">
                            Confirmar contraseña
                        </label>

                        <input
                            type="password"
                            id="registro-confirmar"
                            name="confirmar"
                            minlength="8"
                            autocomplete="new-password"
                            required
                        >
                    </div>

                    <p id="registro-estado"
                       role="status"
                       aria-live="polite">
                    </p>

                    <button type="submit"
                            class="auth-boton">
                        Crear cuenta
                    </button>
                </form>

                <p class="auth-pie">
                    ¿Ya tienes una cuenta?
                    <a href="#login">
                        Inicia sesión
                    </a>
                </p>

            </section>
        </main>
    `;
}

// Validar el formulario sin guardar credenciales
export function iniciarRegistro() {
    const formulario = document.querySelector("#registro-form");
    const estado = document.querySelector("#registro-estado");

    if (!formulario || !estado) return;

    formulario.addEventListener("submit", (evento) => {
        evento.preventDefault();

        if (!formulario.reportValidity()) return;

        const password = formulario.elements.password.value;
        const confirmar = formulario.elements.confirmar.value;

        if (password !== confirmar) {
            estado.textContent =
                "Las contraseñas no coinciden.";

            return;
        }

        // Simulación: no se almacenan datos personales
        estado.textContent =
            "Formulario validado correctamente. " +
            "El registro real estará disponible " +
            "cuando se conecte el backend.";

        // Limpiar los datos del formulario
        formulario.reset();
    });
}
