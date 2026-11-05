
import { motos } from "../mocks/motos.js";

export class MotoService {
    // Recibe los datos desde una fuente externa
    constructor(datos = motos) {
        this.datos = datos;
    }

    // Obtener todas las motocicletas
    async obtenerTodas() {
        return this.datos.map(moto => ({ ...moto }));
    }

    // Buscar motocicleta por ID
    async obtenerPorId(id) {
        const moto = this.datos.find(
            moto => moto.id === Number(id)
        );

        if (!moto) {
            throw new Error("Motocicleta no encontrada");
        }

        return { ...moto };
    }

    // Filtrar motocicletas por categoría
    async filtrarPorCategoria(categoria) {
        const motos = await this.obtenerTodas();

        if (categoria === "Todas") {
            return motos;
        }

        return motos.filter(
            moto => moto.categoria === categoria
        );
    }

    // Consultar motocicletas disponibles
    async obtenerDisponibles() {
        const motos = await this.obtenerTodas();

        return motos.filter(moto => moto.disponible);
    }
}
