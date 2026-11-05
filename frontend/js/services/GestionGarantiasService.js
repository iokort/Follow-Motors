
import { garantiasGerente } from "../mocks/garantiasGerente.js";

export class GestionGarantiasService {

    constructor(datos = garantiasGerente) {
        this.datos = datos.map(garantia => ({ ...garantia }));
    }

    // Obtener todas las solicitudes
    async obtenerTodas() {
        return this.datos.map(garantia => ({ ...garantia }));
    }

    // Consultar una solicitud por ID
    async obtenerPorId(id) {
        const garantia = this.datos.find(
            garantia => garantia.id === Number(id)
        );

        if (!garantia) {
            throw new Error("Garantía no encontrada");
        }

        return { ...garantia };
    }

    // Filtrar solicitudes por estado
    async filtrarPorEstado(estado) {
        const garantias = await this.obtenerTodas();

        if (estado === "Todas") {
            return garantias;
        }

        return garantias.filter(
            garantia => garantia.estado === estado
        );
    }

    // Simular el cambio de estado
    async cambiarEstado(id, nuevoEstado) {
        const transiciones = {
            "Pendiente": ["En revisión"],
            "En revisión": ["Aprobada", "Rechazada"],
            "Aprobada": [],
            "Rechazada": []
        };

        const garantia = this.datos.find(
            garantia => garantia.id === Number(id)
        );

        if (!garantia) {
            throw new Error("Garantía no encontrada");
        }

        if (!transiciones[garantia.estado]?.includes(nuevoEstado)) {
            throw new Error("Cambio de estado no permitido");
        }

        garantia.estado = nuevoEstado;

        return { ...garantia };
    }
}
