
import { getHealth } from "./api.js";

// Componentes generales
import {
    Navbar,
    iniciarNavbar
} from "./components/Navbar.js";

import { Footer } from "./components/Footer.js";

// Pantallas principales
import { HomeView } from "./views/HomeView.js";

import {
    CatalogoView,
    iniciarCatalogo
} from "./views/CatalogoView.js";

import {
    ServiciosView,
    iniciarServicios
} from "./views/ServiciosView.js";

import {
    LoginView,
    iniciarLogin
} from "./views/LoginView.js";

import {
    RegistroView,
    iniciarRegistro
} from "./views/RegistroView.js";

// Paneles de usuarios
import {
    ClienteView,
    iniciarCliente
} from "./views/ClienteView.js";

import {
    GerenteView,
    iniciarGerente
} from "./views/GerenteView.js";

// Pantallas internas del Cliente
import {
    MisMotosView,
    iniciarMisMotos
} from "./views/cliente/MisMotosView.js";

import {
    SolicitarCitaView,
    iniciarSolicitarCita
} from "./views/cliente/SolicitarCitaView.js";

import {
    MisCitasView,
    iniciarMisCitas
} from "./views/cliente/MisCitasView.js";

import {
    HistorialView,
    iniciarHistorial
} from "./views/cliente/HistorialView.js";

import {
    GarantiasView,
    iniciarGarantias
} from "./views/cliente/GarantiasView.js";

// Pantallas internas del Gerente
import {
    ClientesView,
    iniciarClientes
} from "./views/gerente/ClientesView.js";

import {
    MotocicletasView,
    iniciarMotocicletas
} from "./views/gerente/MotocicletasView.js";

import {
    GestionCitasView,
    iniciarGestionCitas
} from "./views/gerente/GestionCitasView.js";

import {
    GestionServiciosView,
    iniciarGestionServicios
} from "./views/gerente/GestionServiciosView.js";

// Ultimo modulo: Gestion de Garantias del Gerente
import {
    GestionGarantiasView,
    iniciarGestionGarantias
} from "./views/gerente/GestionGarantiasView.js";

// Contenedores principales
const navbar = document.querySelector("#navbar");
const app = document.querySelector("#app");
const footer = document.querySelector("#footer");

// Registro de pantallas y sus eventos
const vistas = {
    inicio: {
        mostrar: HomeView
    },
    catalogo: {
        mostrar: CatalogoView,
        iniciar: iniciarCatalogo
    },
    servicios: {
        mostrar: ServiciosView,
        iniciar: iniciarServicios
    },
    login: {
        mostrar: LoginView,
        iniciar: iniciarLogin
    },
    registro: {
        mostrar: RegistroView,
        iniciar: iniciarRegistro
    },
    cliente: {
        mostrar: ClienteView,
        iniciar: iniciarCliente
    },
    gerente: {
        mostrar: GerenteView,
        iniciar: iniciarGerente
    },

    // Modulos del Cliente
    "mis-motos": {
        mostrar: MisMotosView,
        iniciar: iniciarMisMotos
    },
    "solicitar-cita": {
        mostrar: SolicitarCitaView,
        iniciar: iniciarSolicitarCita
    },
    "mis-citas": {
        mostrar: MisCitasView,
        iniciar: iniciarMisCitas
    },
    historial: {
        mostrar: HistorialView,
        iniciar: iniciarHistorial
    },
    garantias: {
        mostrar: GarantiasView,
        iniciar: iniciarGarantias
    },

    // Modulos del Gerente
    "gestion-clientes": {
        mostrar: ClientesView,
        iniciar: iniciarClientes
    },
    "gestion-motocicletas": {
        mostrar: MotocicletasView,
        iniciar: iniciarMotocicletas
    },
    "gestion-citas": {
        mostrar: GestionCitasView,
        iniciar: iniciarGestionCitas
    },
    "gestion-servicios": {
        mostrar: GestionServiciosView,
        iniciar: iniciarGestionServicios
    },
    "gestion-garantias": {
        mostrar: GestionGarantiasView,
        iniciar: iniciarGestionGarantias
    }
};

// Mostrar la pantalla seleccionada
function mostrarVista() {
    const ruta = window.location.hash.slice(1) || "inicio";

    // Seleccionar la pantalla correspondiente
    const vista = vistas[ruta] || vistas.inicio;

    // Renderizar el contenido
    app.innerHTML = vista.mostrar();

    // Inicializar los eventos de la pantalla
    if (vista.iniciar) {
        vista.iniciar();
    }

    window.scrollTo(0, 0);
}

// Inicializar la aplicacion
function iniciarAplicacion() {
    navbar.innerHTML = Navbar();
    footer.innerHTML = Footer();

    iniciarNavbar();
    mostrarVista();

    // Detectar cambios en la navegacion
    window.addEventListener("hashchange", mostrarVista);
}

// Comprobar conexion HTTP con la API
async function checkApi() {
    const status = document.querySelector("#api-status");
    const retry = document.querySelector("#retry-api");

    status.textContent = "Comprobando conexión…";
    retry.disabled = true;

    try {
        const data = await getHealth();

        status.textContent = data.status === "ok"
            ? "API disponible."
            : "La API respondió con un estado inesperado.";

    } catch {
        status.textContent =
            "API sin conexión. La interfaz de demostración sigue disponible.";

    } finally {
        retry.disabled = false;
    }
}

// Iniciar la aplicacion
iniciarAplicacion();

// Boton para reintentar la conexion
document.querySelector("#retry-api")
    .addEventListener("click", checkApi);

checkApi();
