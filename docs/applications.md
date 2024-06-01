# Applications

## Core

Applications that contain core functionality and are not specific to any app.

* [PersonalAssistant.Web](../src/Core/PersonalAssistant.Web)

  Web application that deals with user accounts. User registration, profile settings, password changing, deleting account, etc.

* [Core.Api](../src/Core/Core.Api)

  Web API that deals with core functionalities used across the apps. Like returning a user's profile picture, currency rates, tooltips, preferences, etc.

* [Gateway](../src/Core/Gateway)

  Gateway API that acts as a single point of authentication for the apps.

* [Sender](../src/Core/Sender)

  Background service that asynchronously sends emails and push notifications that have been enqueued in RabbitMQ.

* [Jobs](../src/Core/Jobs)

  Console application that does cleanup, generates records, database backups, etc. Runs every night by cron job.

## To Do Assistant

* [ToDoAssistant.Api](../src/ToDoAssistant/ToDoAssistant.Api)

  Web API for To Do Assistant.

* [to-do-assistant2](../src/ToDoAssistant/to-do-assistant2)

  To Do Assistant app. A PWA built with SvelteKit. The [original v1](../src/ToDoAssistant/to-do-assistant) was done with Aurelia.

## Chef

* [Chef.Api](../src/Chef/Chef.Api)

  Web API for Chef.

* [chef](../src/Chef/chef)

  Chef app. A PWA built with SvelteKit. The [original v1 called Cooking Assistant](../src/Chef/cooking-assistant) was done with Aurelia.

## Accountant

* [Accountant.Api](../src/Accountant/Accountant.Api)

  Web API for Accountant.

* [accountant2](../src/Accountant/accountant2)

  Accountant app. A PWA built with SvelteKit. The [original v1](../src/Accountant/accountant) was done with Aurelia.

## Weatherman

* [Weatherman.Api](../src/Weatherman/Weatherman.Api)

  Web API for Weatherman.

* [weatherman](../src/Weatherman/weatherman)

  Weatherman app. A PWA built with SvelteKit. Currently the only app that includes a dark theme.
