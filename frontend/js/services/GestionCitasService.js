
import { citasGerente } from "../mocks/citasGerente.js";

export class GestionCitasService {

    constructor(datos = citasGerente) {
        this.datos = datos.map(cita => ({ ...cita }));
    }

    // Obtener todas las citas
    async obtenerTodas() {
        return this.datos.map(cita => ({ ...cita }));
    }

    // Buscar una cita por ID
    async obtenerPorId(id) {
        const cita = this.datos.find(
            cita => cita.id === Number(id)
        );

        if (!cita) {
            throw new Error("Cita no encontrada");
        }

        return { ...cita };
    }

    // Filtrar por estado
    async filtrarPorEstado(estado) {
        const citas = await this.obtenerTodas();

        if (estado === "Todas") {
            return citas;
        }

        return citas.filter(cita => cita.estado === estado);
    }

    // Simular el cambio de estado de una cita
    async cambiarEstado(id, nuevoEstado) {
        const estadosValidos = [
            "Pendiente",
            "Confirmada",
            "En proceso",
            "Completada",
            "Cancelada"
        ];

        if (!estadosValidos.includes(nuevoEstado)) {
            throw new Error("Estado no válido");
        }

        const cita = this.datos.find(
            cita => cita.id === Number(id)
        );

        if (!cita) {
            throw new Error("Cita no encontrada");
        }

        cita.estado = nuevoEstado;

        return { ...cita };
    }
}
