
import { servicios } from "../mocks/servicios.js";

export class ServicioService {

    // Recibir los datos ficticios
    constructor(datos = servicios) {
        this.datos = datos;
    }

    // Obtener todos los servicios
    async obtenerTodos() {
        return this.datos.map(servicio => ({ ...servicio }));
    }

    // Buscar un servicio por ID
    async obtenerPorId(id) {
        const servicio = this.datos.find(
            servicio => servicio.id === Number(id)
        );

        if (!servicio) {
            throw new Error("Servicio no encontrado");
        }

        return { ...servicio };
    }

    // Filtrar servicios por categoría
    async filtrarPorCategoria(categoria) {
        const servicios = await this.obtenerTodos();

        if (categoria === "Todas") {
            return servicios;
        }

        return servicios.filter(
            servicio => servicio.categoria === categoria
        );
    }

    // Obtener servicios disponibles
    async obtenerDisponibles() {
        const servicios = await this.obtenerTodos();

        return servicios.filter(
            servicio => servicio.disponible === true
        );
    }
}
