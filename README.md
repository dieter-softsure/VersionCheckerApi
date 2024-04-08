# Introductie
This Project reads the package references and pipeline data from all the repositories. Vulnerabilities are taken from the Github advisory database.

Supported hosts:
- Azure devops
- Github

Supported languages and filetypes:
- .net (Nuget)
	- Framework
	- Core
	- Standard
	- .net 5+
- nodejs (NPM) 
	- All frameworks
	- package.lock.json v1. v2+ is untested
- PHP (Composer) 
	- composer.json en composer.lock
	- Packagist, WPackagist and Drupal packages

# Project opstarten
Dit project is geschreven in .net 6. Zorg er voor dat je een editor hebt die dat ondersteunt.

1. Clone de repository naar een map naar keuze
2. Open VersionCheckerApi.sln
3. Druk op de startknop

# Build en deploy
Bij elke pull request in azure devops word een build gemaakt. Als je wilt deployen hoef je alleen op de ploy te klikken.

1. Navigeer naar deze repository in devops
2. Druk links op Pipelines -> Releases
3. Klik een release aan en druk dan op deploy

# Basisprincipes
De code is modulair opgezet. Omdat de repositories die worden uitgelezen een speciale structuur bevatten is deze structuur geprobeerd na te maken in de code.

In devops is de hierarchie
```
- Organisatie
	- Project
		- Repository
```

Die repository is in principe gewoon de code. Dat ziet er zo uit:
```
- Repository
	- Module (oftewel map met project er in aangeduid met bv. een .csproj file)
		- Pakket
```

In de code van dit project is deze structuur aangehouden:
```
(Organisatie word overgeslagen)
- Project (is eigenlijk een repository)
	- Actionables (acties om het project te verbeteren)
	- Pipelines
	- Module
		- Pakket
			- Vulnerabilities
```

# Code aanpassen?
Als het goed is hoef je bijna geen code aan te passen als je een nieuwe taal wilt ondersteunen of bijvoorbeeld github uit wilt lezen.

## Nieuwe taal toevoegen
We beginnen op het laagste niveau - de vulnerabilities.

1. Zorg er voor dat de github advisory database je nieuwe taal ondersteunt [https://github.com/advisories](https://github.com/advisories)
	- Als dat niet zo is moet er wat code worden aangepast in SecurityService.cs
Voeg je taal toe aan de **PackageType enum in project.cs** en zorg er voor dat de naam overeen komt met die in de advisory database
2. Maak onder Analysing->Packages->LatestVersionGetters een nieuwe class aan die **derived van LatestVersionGetter.cs**.
Deze class moet er voor zorgen dat de laatste versie en de tags worden opgehaald uit de packagemanager. Kijk naar NpmService.cs voor een voorbeeld.
Voeg hem toe aan de LatestVersionGetterFactory.cs.
3. Maak onder Analysing->Modules een modulebuilder class aan voor je nieuwe taal. Deze moet de **IModuleBuilder interface implementeren**.
Als dat is gebeurd moet je hem nog **toevoegen aan de ModuleBuilderFactory** in dezelfde map. Vergeet ook niet de fileending toe te voegen daar.
Voor voorbeelden kan je kijken in NetModuleBuilder en NodeModuleBuilder. Ze zijn vij simpel.
4. Je bent al klaar

## Nieuwe Host toevoegen (Bv. Bitbucket)

1. Maak onder Analysing->RepoGetter een mapje met de naam van de nieuwe host(niet verplicht)
2. Maak een **class aan die IRepositoryGetter implementeert**. Hier hier zitten alle functies in die de data uit de repositories haalt.
Kijk naar DevopsRepositoryGetter.cs voor een voorbeeld. Dit kan best uitgebreid en moeilijk zijn aan de hand van hoe goed de api is
gedocumenteerd van die nieuwe host.
3. Maak in dezelfde map een **class aan die IRepository implementeert**. Deze fungeert als lijm tussen de code van die api en zelfgemaakte code.
Het enige wat dit doet is wat properties overhevelen.
4. Nu moet er nog wat code bij hier en daar om de data van meerdere hosts samen te voegen(het moeilijke stuk)
5. Klaar

## Een nieuwe property de database in jenken
1. Voeg de property toe in de code en dus in een van de objecten onder hep mapje Persistence
2. In de packagemanagerconsole tiep je `Add-Migration [naam]`
3. tiep `Update-Database`
