
import { historial } from "../mocks/historial.js";

export class HistorialService {

    // Recibir los registros ficticios
    constructor(datos = historial) {
        this.datos = datos;
    }

    // Obtener todo el historial
    async obtenerTodos() {
        return this.datos.map(registro => ({ ...registro }));
    }

    // Buscar un registro por ID
    async obtenerPorId(id) {
        const registro = this.datos.find(
            registro => registro.id === Number(id)
        );

        if (!registro) {
            throw new Error("Registro no encontrado");
        }

        return { ...registro };
    }

    // Consultar historial por motocicleta
    async filtrarPorMotocicleta(motocicleta) {
        const registros = await this.obtenerTodos();

        if (motocicleta === "Todas") {
            return registros;
        }

        return registros.filter(
            registro => registro.motocicleta === motocicleta
        );
    }
}
