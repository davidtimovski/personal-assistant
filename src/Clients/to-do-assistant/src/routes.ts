import { PLATFORM } from "aurelia-pal";

import { Language } from "../../shared/src/models/enums/language";

export default [
  { route: ["", Language.English, Language.Macedonian], redirect: "lists" },
  {
    route: "lists",
    name: "lists",
    moduleId: PLATFORM.moduleName("./pages/lists"),
  },
  {
    route: "lists/:editedId",
    name: "listsEdited",
    moduleId: PLATFORM.moduleName("./pages/lists"),
  },
  {
    route: "list/:id",
    name: "list",
    moduleId: PLATFORM.moduleName("./pages/list"),
  },
  {
    route: "list/:id/:editedId",
    name: "listEdited",
    moduleId: PLATFORM.moduleName("./pages/list"),
  },
  {
    route: "computed-list/:type",
    name: "computedList",
    moduleId: PLATFORM.moduleName("./pages/computedList"),
  },
  {
    route: "edit-list/:id",
    name: "editList",
    moduleId: PLATFORM.moduleName("./pages/editList"),
  },
  {
    route: "share-list/:id",
    name: "shareList",
    moduleId: PLATFORM.moduleName("./pages/shareList"),
  },
  {
    route: "copy-list/:id",
    name: "copyList",
    moduleId: PLATFORM.moduleName("./pages/copyList"),
  },
  {
    route: "archive-list/:id",
    name: "archiveList",
    moduleId: PLATFORM.moduleName("./pages/archiveList"),
  },
  {
    route: "uncomplete-tasks/:id",
    name: "uncompleteTasks",
    moduleId: PLATFORM.moduleName("./pages/uncompleteTasks"),
  },
  {
    route: "bulk-add-tasks/:id",
    name: "bulkAddTasks",
    moduleId: PLATFORM.moduleName("./pages/bulkAddTasks"),
  },
  {
    route: "task/:id",
    name: "editTask",
    moduleId: PLATFORM.moduleName("./pages/editTask"),
  },
  {
    route: "notifications",
    name: "notifications",
    moduleId: PLATFORM.moduleName("./pages/notifications"),
  },
  {
    route: "notifications/:id",
    name: "notificationsHighlighted",
    moduleId: PLATFORM.moduleName("./pages/notifications"),
  },
  {
    route: "share-requests",
    name: "shareRequests",
    moduleId: PLATFORM.moduleName("./pages/shareRequests"),
  },
  {
    route: "archived-lists",
    name: "archivedLists",
    moduleId: PLATFORM.moduleName("./pages/archivedLists"),
  },
  {
    route: "archived-lists/:editedId",
    name: "archivedListsEdited",
    moduleId: PLATFORM.moduleName("./pages/archivedLists"),
  },
  {
    route: "help",
    name: "help",
    moduleId: PLATFORM.moduleName("./pages/help"),
  },
  {
    route: "menu",
    name: "menu",
    moduleId: PLATFORM.moduleName("./pages/menu"),
  },
  {
    route: "preferences",
    name: "preferences",
    moduleId: PLATFORM.moduleName("./pages/preferences"),
  },
  {
    route: "not-found",
    name: "notFound",
    moduleId: PLATFORM.moduleName("./pages/notFound"),
  },
  {
    route: "signin-oidc",
    name: "signinOidc",
    moduleId: PLATFORM.moduleName("./pages/auth/signinOidc"),
    settings: {
      noAuth: true,
    },
  },
];
