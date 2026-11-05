
import { garantias } from "../mocks/garantias.js";

export class GarantiaService {

    constructor(datos = garantias) {
        this.datos = datos;
    }

    // Obtener todas las garantías
    async obtenerTodas() {
        return this.datos.map(garantia => ({ ...garantia }));
    }

    // Buscar garantía por ID
    async obtenerPorId(id) {
        const garantia = this.datos.find(
            garantia => garantia.id === Number(id)
        );

        if (!garantia) {
            throw new Error("Garantía no encontrada");
        }

        return { ...garantia };
    }

    // Filtrar garantías por estado
    async filtrarPorEstado(estado) {
        const garantias = await this.obtenerTodas();

        if (estado === "Todas") {
            return garantias;
        }

        return garantias.filter(
            garantia => garantia.estado === estado
        );
    }
}
