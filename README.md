# .NET 8 Web API – Dockerized

Este proyecto es una API robusta y modernizada para la gestión de películas y categorías, completamente dockerizada.

## ¿Qué hemos hecho? (Mejoras Recientes)
- **Persistencia de Logs:** Ahora los logs de la aplicación se guardan automáticamente en tu carpeta local `backend/logs/peliculas.log`. No se pierden al reiniciar los contenedores.
- **Trazabilidad "Humana":** Hemos mejorado los mensajes de log para que sean fáciles de entender (ej: "Nueva película registrada: Inception").
- **Infraestructura Docker:** Separación limpia entre Frontend y Backend usando Docker Compose.
- **Base de Datos:** Uso de SQLite para una ejecución inmediata sin configuraciones complejas.

## Requisitos
- [Docker](https://www.docker.com/)

## Cómo empezar
Solo necesitas un comando para levantar todo el sistema:

```bash
docker compose up --build
```

## Enlaces útiles
- **Frontend:** [http://localhost:3000](http://localhost:3000)
- **Documentación API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Archivo de Logs:** Revisa la carpeta `./backend/logs/peliculas.log` para ver la actividad en tiempo real.

## Comandos rápidos
- **Detener todo:** `docker compose down`
- **Ver logs en consola:** `docker compose logs -f backend`
- **Correr Pruebas Unitarias:** 
  ```bash
  docker compose run --rm backend-test
  ```
