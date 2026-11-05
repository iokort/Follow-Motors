
import { unidades } from "../mocks/unidades.js";

export class UnidadService {

    constructor(datos = unidades) {
        this.datos = datos;
    }

    // Obtener todas las motocicletas
    async obtenerTodas() {
        return this.datos.map(unidad => ({ ...unidad }));
    }

    // Buscar motocicleta por ID
    async obtenerPorId(id) {
        const unidad = this.datos.find(
            unidad => unidad.id === Number(id)
        );

        if (!unidad) {
            throw new Error("Motocicleta no encontrada");
        }

        return { ...unidad };
    }

    // Buscar por propietario, modelo o placa
    async buscar(texto) {
        const unidades = await this.obtenerTodas();
        const busqueda = texto.trim().toLowerCase();

        return unidades.filter(unidad =>
            unidad.propietario.toLowerCase().includes(busqueda) ||
            unidad.modelo.toLowerCase().includes(busqueda) ||
            unidad.placa.toLowerCase().includes(busqueda)
        );
    }

    // Filtrar por estado
    async filtrarPorEstado(estado) {
        const unidades = await this.obtenerTodas();

        if (estado === "Todas") {
            return unidades;
        }

        return unidades.filter(
            unidad => unidad.estado === estado
        );
    }
}
