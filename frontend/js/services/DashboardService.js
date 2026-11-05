
import {
    dashboardCliente,
    dashboardGerente
} from "../mocks/dashboard.js";

export class DashboardService {

    // Recibir los datos ficticios de cada rol
    constructor(
        datosCliente = dashboardCliente,
        datosGerente = dashboardGerente
    ) {
        this.datosCliente = datosCliente;
        this.datosGerente = datosGerente;
    }

    // Obtener información del cliente
    async obtenerDashboardCliente() {
        return this.datosCliente.map(
            dato => ({ ...dato })
        );
    }

    // Obtener información del gerente
    async obtenerDashboardGerente() {
        return this.datosGerente.map(
            dato => ({ ...dato })
        );
    }
}
