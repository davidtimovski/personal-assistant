-- Table: public.CookingAssistant.FoodCategories

-- DROP TABLE IF EXISTS public."CookingAssistant.FoodCategories";

CREATE TABLE public."CookingAssistant.FoodCategories"
(
    "Id" serial NOT NULL,
    "ParentId" integer,
    "Name" character varying(15) COLLATE pg_catalog."default",
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.FoodCategories" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.FoodCategories"
    OWNER to personalassistant;


-- Seed
-- Root categories
INSERT INTO public."CookingAssistant.FoodCategories"("Id", "ParentId", "Name") VALUES
(1, NULL, 'grain'),
(2, NULL, 'vegetables'),
(3, NULL, 'fruit'),
(4, NULL, 'dairy'),
(5, NULL, 'protein'),
(6, NULL, 'nuts_legumes'),
(7, NULL, 'seasoning'),
(9, NULL, 'alcohol');

-- Child categories
INSERT INTO public."CookingAssistant.FoodCategories"("Id", "ParentId", "Name") VALUES
(10, 1, 'cereal'),
(11, 4, 'cheese'),
(12, 5, 'seafood');
