
/* Pantalla de inicio de sesión */

export function LoginView() {
    return `
        <main class="auth">

            <section class="auth-contenedor"
                     aria-labelledby="login-titulo">

                <div class="auth-encabezado">
                    <img
                        src="./assets/images/branding/logo.png"
                        alt="Follow Motors"
                        class="auth-logo"
                    >

                    <h1 id="login-titulo">
                        Iniciar sesión
                    </h1>

                    <p>
                        Accede a tu cuenta de Follow Motors.
                    </p>
                </div>

                <form id="login-form">

                    <div class="auth-campo">
                        <label for="login-correo">
                            Correo electrónico
                        </label>

                        <input
                            type="email"
                            id="login-correo"
                            name="correo"
                            placeholder="correo@ejemplo.com"
                            autocomplete="email"
                            required
                        >
                    </div>

                    <div class="auth-campo">
                        <label for="login-password">
                            Contraseña
                        </label>

                        <input
                            type="password"
                            id="login-password"
                            name="password"
                            placeholder="Ingresa tu contraseña"
                            autocomplete="current-password"
                            required
                        >
                    </div>

                    <a href="#recuperar"
                       class="auth-enlace">
                        ¿Olvidaste tu contraseña?
                    </a>

                    <p id="login-estado"
                       role="status"
                       aria-live="polite">
                    </p>

                    <button
                        type="submit"
                        class="auth-boton">
                        Iniciar sesión
                    </button>

                </form>

                <p class="auth-pie">
                    ¿No tienes una cuenta?
                    <a href="#registro">
                        Regístrate aquí
                    </a>
                </p>

            </section>
        </main>
    `;
}

// Activar los eventos del formulario
export function iniciarLogin() {
    const formulario = document.querySelector("#login-form");
    const estado = document.querySelector("#login-estado");

    if (!formulario || !estado) return;

    formulario.addEventListener("submit", (evento) => {
        evento.preventDefault();

        if (!formulario.reportValidity()) return;

        // Demostración visual sin enviar credenciales
        estado.textContent =
            "Formulario validado. La autenticación estará disponible al conectar el backend.";
    });
}
