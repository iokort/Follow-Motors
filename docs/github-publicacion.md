# Uso de Tags, Wiki, Packages y Releases — GitHub SSC

Todos los comandos siguientes están pensados para **Git CMD** y son independientes. No ejecutar una publicación que todavía no tenga versión aprobada en `main`.

## Topics (etiquetas del repositorio)

GitHub organiza repositorios con Topics; son diferentes de las etiquetas de Issues y de los Git tags de versiones.

```cmd
gh repo edit iokort/Follow-Motors --add-topic "follow-motors,ssc,software-engineering,after-sales,motorcycles,aspnetcore,csharp,mysql,javascript,scrum,kanban,mvvm"
```

Este comando añade Topics sin sustituir los existentes.

## Wiki

La Wiki se guarda en su propio repositorio. Abre [Follow-Motors Wiki](https://github.com/iokort/Follow-Motors/wiki), pulsa **Create the first page**, deja el título **Home** y pega el contenido versionado de [docs/wiki/Home.md](wiki/Home.md). Pulsa **Save Page**.

Después de crear Home, los cambios posteriores podrán actualizarse desde Git CMD:

```cmd
cd ..
git clone https://github.com/iokort/Follow-Motors.wiki.git
xcopy Follow-Motors\docs\wiki\*.md Follow-Motors.wiki\ /Y
cd Follow-Motors.wiki
git add .
git commit -m "docs(wiki): portal academico Follow Motors"
git push
```

**Importante:** primero debe existir la página en GitHub. La Wiki no es sinónimo de `docs/`; se emplea como portada navegable.

## Packages — API C# como contenedor

La aplicación ya tiene un `Dockerfile`; publicar un contenedor es un uso legítimo de GitHub Packages, a diferencia de inventar una librería NuGet sin código reutilizable.

El workflow [publicar-ghcr.yml](../.github/workflows/publicar-ghcr.yml) publica la imagen `ghcr.io/iokort/follow-motors-api` cuando:
- se ejecuta manualmente desde la rama `main`, o
- se publica una Release.

El flujo realiza una compilación .NET antes de enviar el contenedor. Requiere permisos `packages:write` para su trabajo. La publicación de imagen **no** despliega a Hostinger ni a producción.

**Para que lo vea el profesor:** tras la primera publicación correcta, abre [Packages de ioKort](https://github.com/orgs/iokort/packages), selecciona la imagen y cambia **Package settings → Change visibility → Public** si aparece privada. Comprueba acceso en sesión anónima.

## Git tags y Releases

Un **Git tag** identifica el commit de una versión. Una **Release** agrega notas y descarga del código. El docente podrá navegar ambas.

Tras aprobar los PR, publicar en `main` y comprobar CI, se recomienda la primera versión de **base técnica** como *pre-release*, no una falsa versión comercial:

```cmd
gh release create v0.1.0-alpha.1 -R iokort/Follow-Motors --target main --prerelease --latest=false --title "v0.1.0-alpha.1 - Base técnica" --notes-file docs\releases\v0.1.0-alpha.1.md
```

GitHub CLI crea el tag automáticamente si no existe. El workflow de GHCR escucha cuando se publica la Release. La versión debe referenciar un commit real en `main`.

Revisiones:
```cmd
gh release list -R iokort/Follow-Motors
git fetch --tags
git tag --list
gh run list -R iokort/Follow-Motors --limit 8
```

## Revisión pública

- [Código](https://github.com/iokort/Follow-Motors) · [Wiki](https://github.com/iokort/Follow-Motors/wiki) · [Releases](https://github.com/iokort/Follow-Motors/releases) · [Packages](https://github.com/orgs/iokort/packages)
- No publicar imágenes con secretos, expedientes o datos reales.
- No declarar releases de producción ni tests concluidos cuando solo existe una base técnica.

## Fuentes

GitHub. (s. f.). *Adding or editing wiki pages*. https://docs.github.com/en/communities/documenting-your-project-with-wikis/adding-or-editing-wiki-pages

GitHub. (s. f.). *Publishing Docker images*. https://docs.github.com/en/actions/tutorials/publish-packages/publish-docker-images

GitHub CLI. (s. f.). *gh release create*. https://cli.github.com/manual/gh_release_create
