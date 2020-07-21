-- Table: public."CookingAssistant.RecipesIngredients"

-- DROP TABLE public."CookingAssistant.RecipesIngredients";

CREATE TABLE public."CookingAssistant.RecipesIngredients"
(
    "RecipeId" integer NOT NULL,
    "IngredientId" integer NOT NULL,
    "Amount" decimal(7,2),
    "Unit" character varying(4) COLLATE pg_catalog."default",
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_CA.RecipesIngredients" PRIMARY KEY ("RecipeId", "IngredientId"),
    CONSTRAINT "FK_CA.RecipesIngredients_CA.Recipes_RecipeId" FOREIGN KEY ("RecipeId")
    REFERENCES public."CookingAssistant.Recipes" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CA.RecipesIngredients_CA.Ingredients_IngredientId" FOREIGN KEY ("IngredientId")
    REFERENCES public."CookingAssistant.Ingredients" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.RecipesIngredients"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.RecipesIngredients_RecipeId

-- DROP INDEX public."IX_CA.RecipesIngredients_RecipeId";

CREATE INDEX "IX_CA.RecipesIngredients_RecipeId"
    ON public."CookingAssistant.RecipesIngredients" USING btree
    ("RecipeId")
    TABLESPACE pg_default;

-- Index: IX_CA.RecipesIngredients_IngredientId

-- DROP INDEX public."IX_CA.RecipesIngredients_IngredientId";

CREATE INDEX "IX_CA.RecipesIngredients_IngredientId"
    ON public."CookingAssistant.RecipesIngredients" USING btree
    ("IngredientId")
    TABLESPACE pg_default;