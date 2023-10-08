import { PLATFORM } from "aurelia-pal";

import { Language } from "../../shared/src/models/enums/language";

export default [
  {
    route: ["", Language.English, Language.Macedonian],
    redirect: "recipes",
  },
  {
    route: "recipes",
    name: "recipes",
    moduleId: PLATFORM.moduleName("./pages/recipes"),
  },
  {
    route: "recipes/:editedId",
    name: "recipesEdited",
    moduleId: PLATFORM.moduleName("./pages/recipes"),
  },
  {
    route: "recipe/:id",
    name: "recipe",
    moduleId: PLATFORM.moduleName("./pages/recipe"),
  },
  {
    route: "edit-recipe/:id",
    name: "editRecipe",
    moduleId: PLATFORM.moduleName("./pages/editRecipe"),
  },
  {
    route: "share-recipe/:id",
    name: "shareRecipe",
    moduleId: PLATFORM.moduleName("./pages/shareRecipe"),
  },
  {
    route: "send-recipe/:id",
    name: "sendRecipe",
    moduleId: PLATFORM.moduleName("./pages/sendRecipe"),
  },
  {
    route: "dietary-profile",
    name: "dietaryProfile",
    moduleId: PLATFORM.moduleName("./pages/dietaryProfile"),
  },
  {
    route: "ingredients",
    name: "ingredients",
    moduleId: PLATFORM.moduleName("./pages/ingredients"),
  },
  {
    route: "ingredients/:editedId",
    name: "ingredientsEdited",
    moduleId: PLATFORM.moduleName("./pages/ingredients"),
  },
  {
    route: "edit-ingredient/:id",
    name: "editIngredient",
    moduleId: PLATFORM.moduleName("./pages/editIngredient"),
  },
  {
    route: "view-ingredient/:id",
    name: "viewIngredient",
    moduleId: PLATFORM.moduleName("./pages/viewIngredient"),
  },
  {
    route: "share-requests",
    name: "shareRequests",
    moduleId: PLATFORM.moduleName("./pages/shareRequests"),
  },
  {
    route: "inbox",
    name: "inbox",
    moduleId: PLATFORM.moduleName("./pages/inbox"),
  },
  {
    route: "review-ingredients/:id",
    name: "reviewIngredients",
    moduleId: PLATFORM.moduleName("./pages/reviewIngredients"),
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
