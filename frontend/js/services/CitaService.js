
import { citas } from "../mocks/citas.js";

export class CitaService {

    // Recibir datos de demostración
    constructor(datos = citas) {
        this.datos = datos;
    }

    // Obtener todas las citas
    async obtenerTodas() {
        return this.datos.map(cita => ({ ...cita }));
    }

    // Buscar cita por identificador
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

        return citas.filter(
            cita => cita.estado === estado
        );
    }
}
