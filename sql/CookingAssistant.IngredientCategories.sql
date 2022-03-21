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
