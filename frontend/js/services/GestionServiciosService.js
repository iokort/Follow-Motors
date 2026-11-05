
import { serviciosGerente } from "../mocks/serviciosGerente.js";

export class GestionServiciosService {

    constructor(datos = serviciosGerente) {
        this.datos = datos.map(servicio => ({ ...servicio }));
    }

    // Obtener todos los servicios
    async obtenerTodos() {
        return this.datos.map(servicio => ({ ...servicio }));
    }

    // Consultar un servicio por ID
    async obtenerPorId(id) {
        const servicio = this.datos.find(
            servicio => servicio.id === Number(id)
        );

        if (!servicio) {
            throw new Error("Servicio no encontrado");
        }

        return { ...servicio };
    }

    // Filtrar los servicios por estado
    async filtrarPorEstado(estado) {
        const servicios = await this.obtenerTodos();

        if (estado === "Todos") {
            return servicios;
        }

        return servicios.filter(
            servicio => servicio.estado === estado
        );
    }

    // Actualizar el estado de un servicio
    async cambiarEstado(id, nuevoEstado) {
        const transiciones = {
            "Pendiente": ["En proceso", "Cancelado"],
            "En proceso": ["Completado"],
            "Completado": [],
            "Cancelado": []
        };

        const servicio = this.datos.find(
            servicio => servicio.id === Number(id)
        );

        if (!servicio) {
            throw new Error("Servicio no encontrado");
        }

        const estadosPermitidos = transiciones[servicio.estado];

        if (!estadosPermitidos ||
            !estadosPermitidos.includes(nuevoEstado)) {
            throw new Error("Cambio de estado no permitido");
        }

        servicio.estado = nuevoEstado;

        return { ...servicio };
    }
}
