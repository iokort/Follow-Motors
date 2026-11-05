
import { clientes } from "../mocks/clientes.js";

export class ClienteService {

    constructor(datos = clientes) {
        this.datos = datos;
    }

    // Obtener todos los clientes
    async obtenerTodos() {
        return this.datos.map(cliente => ({ ...cliente }));
    }

    // Buscar cliente por ID
    async obtenerPorId(id) {
        const cliente = this.datos.find(
            cliente => cliente.id === Number(id)
        );

        if (!cliente) {
            throw new Error("Cliente no encontrado");
        }

        return { ...cliente };
    }

    // Buscar clientes por nombre
    async buscarPorNombre(nombre) {
        const clientes = await this.obtenerTodos();

        return clientes.filter(cliente =>
            cliente.nombre.toLowerCase().includes(
                nombre.trim().toLowerCase()
            )
        );
    }
}
