# Talaria Links API

API REST de **Talaria Links**, un acortador de enlaces con cuentas de usuario, roles y registro de clics. Permite crear, editar, eliminar y compartir enlaces cortos, redirigir a su destino contabilizando cada visita y administrar usuarios y enlaces desde un rol de administrador.

El frontend se encuentra en el repositorio [talaria-links-web](https://github.com/ArmandoReyes105/talaria-links-web).

## Stack

- .NET 10 y ASP.NET Core (Web API con controladores)
- Entity Framework Core 10 con el proveedor de PostgreSQL (Npgsql)
- PostgreSQL
- Autenticación con JWT almacenado en una cookie `httpOnly`
- OpenAPI con Scalar como interfaz de documentación

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (local o en la nube)
- Herramienta de Entity Framework Core:

```bash
dotnet tool install --global dotnet-ef
```

- Certificado de desarrollo HTTPS de .NET:

```bash
dotnet dev-certs https --trust
```

## Configuración

Los archivos `appsettings*.json` están excluidos del repositorio para no publicar credenciales. Se incluye una plantilla con las claves necesarias.

1. Copia la plantilla dentro de la carpeta del proyecto:

```bash
cp talaria-links-api/appsettings.Template.json talaria-links-api/appsettings.Development.json
```

2. Completa los valores:

| Clave | Descripción | Ejemplo |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a PostgreSQL | `Host=localhost;Port=5432;Database=talaria_links;Username=postgres;Password=postgres` |
| `AppSettings:Token` | Clave para firmar los JWT. **Debe tener al menos 32 caracteres** (HMAC-SHA256) | Cadena aleatoria larga |
| `AppSettings:Issuer` | Emisor del token | `TalariaLinksAPI` |
| `AppSettings:Audience` | Audiencia del token | `TalariaLinksAudience` |
| `Cors:AllowedOrigins` | Orígenes autorizados para consumir la API | `["http://localhost:3000"]` |
| `Links:BaseUrl` | URL base con la que se generan los enlaces cortos | `https://localhost:7151` |

### Administrador inicial (opcional)

Al iniciar, la API crea una cuenta de administrador si todavía no existe ninguna y se definen las siguientes claves. Si no se configuran, no se crea ninguna cuenta.

| Clave | Descripción |
|---|---|
| `AdminSeed:Email` | Email del administrador |
| `AdminSeed:Password` | Contraseña del administrador |
| `AdminSeed:Username` | Nombre de usuario (por defecto `admin`) |

```json
"AdminSeed": {
  "Email": "admin@talaria.dev",
  "Password": "Admin12345",
  "Username": "admin"
}
```

En producción estas claves se configuran como variables de entorno (`AdminSeed__Email`, `AdminSeed__Password`, `AdminSeed__Username`) y se recomienda eliminar la contraseña una vez creada la cuenta.

## Base de datos

Crea una base de datos vacía en PostgreSQL y aplica las migraciones desde la raíz del repositorio:

```bash
dotnet ef database update --project talaria-links-api
```

Las migraciones crean las tablas y registran los roles `Admin` y `User`.

## Ejecución

```bash
dotnet run --project talaria-links-api --launch-profile https
```

La API queda disponible en:

- `https://localhost:7151`
- `http://localhost:5276`

### Documentación de la API

En el entorno de desarrollo, la documentación interactiva de los endpoints está disponible en:

```
https://localhost:7151/scalar
```

Desde Scalar se pueden probar todos los endpoints. Al ejecutar `POST /api/auth/login`, el navegador guarda la cookie de sesión y las peticiones siguientes se envían autenticadas.

## Docker

El proyecto incluye un `Dockerfile`. Desde la raíz del repositorio:

```bash
docker build -f talaria-links-api/Dockerfile -t talaria-links-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="<cadena de conexión>" \
  -e AppSettings__Token="<clave de al menos 32 caracteres>" \
  -e AppSettings__Issuer="TalariaLinksAPI" \
  -e AppSettings__Audience="TalariaLinksAudience" \
  -e Cors__AllowedOrigins__0="http://localhost:3000" \
  -e Links__BaseUrl="http://localhost:8080" \
  talaria-links-api
```

## Esquema de la base de datos

La base de datos está normalizada en tercera forma normal (3FN) y se compone de cuatro tablas:

- **Roles**: catálogo de roles del sistema (`Admin` y `User`). El nombre del rol es único.
- **Accounts**: cuentas de usuario. Guarda el nombre de usuario, el email, el hash de la contraseña y la fecha de registro. Cada cuenta referencia a un rol mediante `RoleId`.
- **ShortUrls**: enlaces cortos. Guarda el código único, la URL de destino, un título opcional, la visibilidad (público o privado) y las fechas de creación y actualización. Cada enlace pertenece a una cuenta mediante `AccountId`.
- **Clicks**: registro de cada visita a un enlace corto, con la fecha, el referrer y el user agent. Cada clic referencia a un enlace mediante `ShortUrlId`.

Relaciones:

- Un rol tiene muchas cuentas (1:N).
- Una cuenta tiene muchos enlaces (1:N). Al eliminar una cuenta se eliminan sus enlaces.
- Un enlace tiene muchos clics (1:N). Al eliminar un enlace se eliminan sus clics.

El nombre del rol se almacena una sola vez en `Roles` y el número de clics de cada enlace se calcula a partir de la tabla `Clicks`, por lo que no existen datos duplicados ni dependencias transitivas.

## Arquitectura

```
talaria-links-api/
├── Common/          # Utilidades compartidas, constantes y excepciones de la aplicación
├── Controllers/     # Endpoints HTTP
├── DTOs/            # Objetos de entrada y salida de la API
├── Entities/        # Entidades del dominio
├── Enums/           # Enumeraciones (visibilidad y ordenamiento)
├── Middleware/      # Manejo global de excepciones
├── Migrations/      # Migraciones de Entity Framework Core
├── Options/         # Opciones de configuración tipadas
├── Persistence/     # DbContext, configuración de entidades y seeder del administrador
└── Services/        # Lógica de negocio (interfaces e implementaciones)
```

Los controladores reciben la petición y delegan en los servicios, que contienen la lógica de negocio y el acceso a datos. Las entidades nunca se exponen directamente: las respuestas se proyectan a DTOs.

## Seguridad

- Contraseñas almacenadas con hash mediante `PasswordHasher` de ASP.NET Core Identity.
- JWT enviado en una cookie `httpOnly` y `Secure`, inaccesible desde JavaScript.
- Autorización por roles (`Admin` y `User`) en los endpoints protegidos.
- Límite de intentos en el inicio de sesión (rate limiting).
- Validación de datos de entrada con Data Annotations y validación de URLs (solo `http` y `https`).
- Búsquedas con caracteres especiales escapados en consultas `LIKE`.
- CORS restringido a los orígenes configurados.
- Manejo global de excepciones con respuestas en formato `ProblemDetails`.
