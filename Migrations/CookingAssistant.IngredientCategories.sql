-- Table: public.CookingAssistant.IngredientCategories

-- DROP TABLE IF EXISTS public."CookingAssistant.IngredientCategories";

CREATE TABLE public."CookingAssistant.IngredientCategories"
(
    "Id" serial NOT NULL,
    "ParentId" integer,
    "Name" character varying(15) COLLATE pg_catalog."default",
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.IngredientCategories" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.IngredientCategories"
    OWNER to personalassistant;


-- Seed
-- Root categories
INSERT INTO public."CookingAssistant.IngredientCategories"("Id", "ParentId", "Name") VALUES
(1, NULL, 'grain'),
(2, NULL, 'vegetables'),
(3, NULL, 'fruit'),
(4, NULL, 'dairy'),
(5, NULL, 'protein'),
(6, NULL, 'nuts_legumes'),
(7, NULL, 'seasoning'),
(9, NULL, 'alcohol');

-- Child categories
INSERT INTO public."CookingAssistant.IngredientCategories"("Id", "ParentId", "Name") VALUES
(10, 4, 'cheese'),
(11, 5, 'seafood');
