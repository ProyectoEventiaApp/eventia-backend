* **Usuario de pruebas activo**: `admin@eventia.com / Admin123!`
* **Rama activa del repo**: `dev`

# 📘 Eventia API – Documentación

### 🌍 URL base
```

[https://eventia-api-dev-ecdbcphtdzg0fbd8.centralus-01.azurewebsites.net](https://eventia-api-dev-ecdbcphtdzg0fbd8.centralus-01.azurewebsites.net)

```

### 📑 Documentación interactiva (Swagger)
👉 [Swagger UI](https://eventia-api-dev-ecdbcphtdzg0fbd8.centralus-01.azurewebsites.net/swagger/index.html)

---

## 🔑 Autenticación
Todas las rutas (excepto `/api/auth/login`) requieren **Bearer Token (JWT)**.

### Credenciales de prueba (usuario activo)
```

email: [admin@eventia.com](mailto:admin@eventia.com)
password: Admin123!

````

### Login
```http
POST /api/auth/login
````

**Body (JSON):**

```json
{
  "email": "admin@eventia.com",
  "password": "Admin123!"
}
```

**Respuesta (200):**

```json
{
  "token": "jwt_token",
  "user": {
    "id": 1,
    "name": "Admin",
    "email": "admin@eventia.com",
    "roles": ["Admin"],
    "permissions": ["CrearEvento", "EditarEvento"]
  }
}
```

---

## 📋 Auditoría

* `GET /api/audit/recent?count=50` → Últimos registros de actividad.
* `GET /api/audit/entity/{entityType}/{entityId}` → Historial de cambios de una entidad.
* `GET /api/audit/user/{userId}` → Acciones realizadas por un usuario.

---

## 🎟️ Eventos

* `POST /api/events` → Crear evento.
* `GET /api/events` → Listar eventos.
* `GET /api/events/{id}` → Detalle de un evento.
* `PUT /api/events/{id}` → Editar evento.
* `DELETE /api/events/{id}` → Eliminar evento.

**Ejemplo creación:**

```json
{
  "title": "Conferencia Tech",
  "description": "Evento de tecnología",
  "startDate": "2025-09-01T10:00:00Z",
  "endDate": "2025-09-01T18:00:00Z",
  "location": "Auditorio Medellín",
  "maxAttendees": 200
}
```

---

## 🎫 Tickets

* `GET /api/tickets` → Lista de tickets.
* `POST /api/tickets` → Crear ticket.
* `PATCH /api/tickets/{id}` → Editar ticket.
* `PATCH /api/tickets/{id}/status` → Cambiar estado.
* `DELETE /api/tickets/{id}` → Eliminar ticket.

---

## 🔐 Seguridad

### Roles

* `GET /api/security/roles` → Listar roles.
* `POST /api/security/roles/{name}` → Crear rol.
* `PUT /api/security/roles/{id}` → Editar rol.
* `GET /api/security/roles/{roleName}/users` → Usuarios con un rol.

### Permisos

* `GET /api/security/permissions` → Listar permisos.
* `POST /api/security/permissions` → Crear permiso.
* `PUT /api/security/permissions/{id}` → Actualizar permiso.
* `DELETE /api/security/permissions/{id}` → Eliminar permiso.

### Asignación

* `POST /api/security/roles/{roleId}/permissions` → Asignar permisos a un rol.
* `DELETE /api/security/roles/{roleId}/permissions` → Remover permisos de un rol.

---

## 👥 Usuarios

* `POST /api/users` → Crear usuario.
* `GET /api/users` → Listar usuarios.
* `GET /api/users/{id}` → Detalle de usuario.
* `PUT /api/users/{id}` → Editar usuario.
* `DELETE /api/users/{id}` → Eliminar usuario.
* `POST /api/users/{email}/disable` → Desactivar usuario.
* `POST /api/users/{email}/enable` → Activar usuario.
* `POST /api/users/{email}/assign-role/{roleId}` → Asignar rol.
* `DELETE /api/users/{email}/role` → Remover rol.

**Ejemplo creación:**

```json
{
  "name": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "password": "secreto123"
}
```

---

## 🚀 Cómo probar

1. **Autenticarse** con `/api/auth/login` usando las credenciales de prueba.
2. Copiar el **token JWT** recibido.
3. En Postman/Swagger, ir a **Authorize** e ingresar:

   ```
   Bearer <token>
   ```
4. Probar los endpoints protegidos.

---

## 🛠️ Recursos útiles

* Swagger JSON:

  ```
  https://eventia-api-dev-ecdbcphtdzg0fbd8.centralus-01.azurewebsites.net/swagger/v1/swagger.json
  ```
* Swagger UI:
  👉 [Abrir Documentación](https://eventia-api-dev-ecdbcphtdzg0fbd8.centralus-01.azurewebsites.net/swagger/index.html)

---

## 🌱 Rama activa del proyecto

El desarrollo actual se encuentra en la rama:

```
dev
```
