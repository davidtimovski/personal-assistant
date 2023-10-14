# Architecture

## C# APIs

The APIs built with C# follow a common theme.

* [AppName].Api

  ASP.NET Core Web API. Top layer which can call into any of the other projects but does so using the contracts defined in the Application project.

* [AppName].Application

  Class library that contains all contracts and business logic except any logic that pertains to third-party dependencies.

* [AppName].Infrastructure

  Class library that contains the implementation for any third-party dependencies. For example Cloudinary which is the image CDN provider for Personal Assistant. Depends on the contracts defined in the Application project.

* [AppName].Persistence

  Class library that contains all code relating to storage. Depends on the repository contracts defined in the Application project.

## F# APIs

### Accountant

* [Accountant.Api](../src/Accountant/Accountant.Api)

  Giraffe Web API. Validates and maps requests into entities. Calls into the Accountant.Persistence project to store and retrieve data.

* [Accountant.Persistence](../src/Accountant/Accountant.Persistence)

  Class library that contains all code relating to storage.

### Weatherman

* [Weatherman.Api](../src/Weatherman/Weatherman.Api)

  Giraffe Web API. Validates and maps requests into entities. Calls into the Weatherman.Application C# project to retrieve data.

* [Weatherman.Application](../src/Weatherman/Weatherman.Application)

  C# class library that defines the contracts for weather data retrieval, but none of the logic since the data comes from a third-party API.

* [Weatherman.Infrastructure](../src/Weatherman/Weatherman.Infrastructure)

  C# class library that contains the implementation for weather data retrieval. Depends on the contracts in the Weatherman.Application project.

* [Weatherman.Persistence](../src/Weatherman/Weatherman.Persistence)

  C# class library that contains all code relating to storage. Depends on the repository contracts defined in the Weatherman.Application project.

## Front-end apps

The front-end apps are build with SvelteKit and follow a common structure. They share code contained in the [shared2](../src/Core/shared2) folder which supercedes the v1 [shared](../src/Core/shared) folder that was used when the apps were built with Aurelia.