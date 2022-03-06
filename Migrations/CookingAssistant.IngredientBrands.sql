-- Table: public.CookingAssistant.IngredientBrands

-- DROP TABLE IF EXISTS public."CookingAssistant.IngredientBrands";

CREATE TABLE public."CookingAssistant.IngredientBrands"
(
    "Id" serial NOT NULL,
    "Name" character varying(15) COLLATE pg_catalog."default",
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.IngredientBrands" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.IngredientBrands"
    OWNER to personalassistant;
